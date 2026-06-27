namespace Authentication.DTOs
{
    public class AuthResponseDTO
    {
        public required bool IsSuccess { get; set; }
        public required string Response { get; set; }
        public required string Message { get; set; }
    }
}