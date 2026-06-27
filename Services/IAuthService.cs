using Authentication.DTOs;

namespace Authentication.Services
{
    public interface IAuthService
    {
        Task<bool> IsEmailRegisteredAsync(string email, CancellationToken cancellationToken = default);

        Task<AuthResponseDTO> Register(RegisterDTO registerDTO);

        Task<AuthResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        Task<AuthResponseDTO> verifyTOTP(VerifyTotpDTO verifyTOTPRequestDTO);
    }


}
