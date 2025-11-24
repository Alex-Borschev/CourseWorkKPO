namespace KursuchServ.Token
{
    public class SessionData
    {
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}