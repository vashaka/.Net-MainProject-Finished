namespace CasinoApi.Models
{
    public class UserInfoResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal CurrentBalance { get; set; }
        public int Currency { get; set; }
    }
}
