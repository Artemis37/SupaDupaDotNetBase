namespace Vehicle.Domain.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid PersonSyncId { get; set; }
    }
}
