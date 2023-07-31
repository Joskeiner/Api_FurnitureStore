namespace Api.FurnitureStore.Api.Configuration
{
    public class JwtConfig
    {
        public string? Secret { get;set; }

        public TimeSpan ExparyTime { get; set; }
    }
}
