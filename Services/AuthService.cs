using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Authentication.Data;
using Authentication.DTOs;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Authentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly dbContext _context;
        private readonly IDistributedCache _redisDb;

        public AuthService(dbContext context, IDistributedCache redisDb)
        {
            _context = context;
            _redisDb = redisDb;
        }

        public string TOTPGenerator(string secret, int timeStep = 30, int digits = 6)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);

            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long counter = unixTime / timeStep;

            byte[] timeBuffer = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeBuffer, counter);

            byte[] hash = HMACSHA256.HashData(keyBytes, timeBuffer);

            int offset = hash[hash.Length - 1] & 0x0f;

            int binaryCode = (hash[offset] & 0x7f) << 24
                           | (hash[offset + 1] & 0xff) << 16
                           | (hash[offset + 2] & 0xff) << 8
                           | (hash[offset + 3] & 0xff);

            int mod  = (int)Math.Pow(10, digits);
            int totp = binaryCode % mod;

            return totp.ToString().PadLeft(digits, '0');    
        }

        public async Task<bool> IsEmailRegisteredAsync(string email, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"EmailCheck:{email}";
            var cachedResult = await _redisDb.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                return bool.Parse(cachedResult);
            }

            bool isRegistered = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _redisDb.SetStringAsync(cacheKey, isRegistered.ToString(), options, cancellationToken);

            return isRegistered;
        }

        public async Task<AuthResponseDTO> Register(RegisterDTO registerDTO)
        {

            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (await IsEmailRegisteredAsync(registerDTO.Email))
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email is already registered.",
                    Response = "400 Bad Request"
                };
            }

            var user = new userModel
            {
                Email = registerDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                ToptSecret = RandomNumberGenerator.GetString(allowedChars, 16)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "User registered successfully.",
                Response = "200 OK"
            };
        }

        public async Task<AuthResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDTO.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequestDTO.Password, user.PasswordHash))
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Invalid email or password.",
                    Response = "401 Unauthorized"
                };
            }

            string generatedTOTP = TOTPGenerator(user.ToptSecret);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            };

            _redisDb.SetString($"TOTP:{user.Email}", generatedTOTP, options);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Login successful.",
                Response = "200 OK"
            };
        }

        public async Task<AuthResponseDTO> verifyTOTP(VerifyTotpDTO verifyTOTPRequestDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyTOTPRequestDTO.Email);

            if (user == null)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "User not found.",
                    Response = "404 Not Found"
                };
            }

            var cachedTOTP = await _redisDb.GetStringAsync($"TOTP:{user.Email}", default);

            if (cachedTOTP != verifyTOTPRequestDTO.totpCode)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Invalid TOTP.",
                    Response = "401 Unauthorized"
                };
            }

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "TOTP verified successfully.",
                Response = "200 OK"
            };
        }
    }
}
