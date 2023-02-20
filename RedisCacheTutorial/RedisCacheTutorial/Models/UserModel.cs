namespace RedisCacheTutorial.Models
{
    public class UserModel
    {
        public string userName { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool isAdmin { get; set; }
    }
}
