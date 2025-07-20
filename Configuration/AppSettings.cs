namespace Saigor.Configuration {
    public class AppSettings {
        public string ExampleSetting { get; set; } = string.Empty;
        public LoggingSettings Logging { get; set; } = new();
        public DatabaseSettings Database { get; set; } = new();
        public QuartzSettings Quartz { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
    }

    public class LoggingSettings {
        public bool EnableConsole { get; set; }
        public bool EnableDebug { get; set; }
    }

    public class DatabaseSettings {
        public string ConnectionString { get; set; } = string.Empty;
        public bool EnableSensitiveDataLogging { get; set; }
        public bool EnableDetailedErrors { get; set; }
    }

    public class QuartzSettings {
        public int MaxConcurrency { get; set; }
        public bool WaitForJobsToComplete { get; set; }
        public bool AwaitApplicationStarted { get; set; }
    }

    public class SecuritySettings {
        public SecurityHeaders Headers { get; set; } = new();
        public bool EnableHsts { get; set; }
        public bool EnableHttpsRedirection { get; set; }
    }
} 