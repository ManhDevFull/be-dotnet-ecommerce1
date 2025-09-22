namespace be_dotnet_ecommerce1.Model
{
    public class Account
    {
        public int _id { get; set; }              // Khóa chính (PK)

        public string email { get; set; } = null!;

        public string password { get; set; } = null!;

        public string? first_name { get; set; }

        public string? last_name { get; set; }

        public int rule { get; set; }          

        public string? avatar_img { get; set; }

        // Token quản lý refresh
        public string? refresh_token { get; set; }

        public DateTime? refresh_token_expires { get; set; }
    }
}
