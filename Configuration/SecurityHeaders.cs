namespace Saigor.Configuration {
    public class SecurityHeaders {
        public bool EnableXContentTypeOptions { get; set; } = true;
        public bool EnableXFrameOptions { get; set; } = true;
        public bool EnableXssProtection { get; set; } = true;
    }
} 