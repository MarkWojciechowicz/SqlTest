namespace SqlTest
{
    internal class Setting
    {
        public required string ServerName { get; set; }
        public required string DatabaseName { get; set; }
        public required bool IntegratedSecurity { get; set; }
        public bool TrustServerCertificate { get; set; }
        public required string User { get; set; }
        public required string Password { get; set; }
    }
}


