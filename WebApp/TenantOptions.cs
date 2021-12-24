namespace WebApp
{
    public class TenantOptions
    {
        public string Id { get; set; } = null!;
        public string Startup { get; set; } = null!;
        public string[] Urls { get; set; } = Array.Empty<string>();
    }
}
