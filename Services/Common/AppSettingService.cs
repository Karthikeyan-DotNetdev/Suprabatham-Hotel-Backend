namespace CommonServices
{
    public class AppSettingService
    {
        private static AppSettingService _instance;
        private static readonly object ObjLocked = new object();
        private IConfiguration _configuration;

        public AppSettingService()
        {
        }

        public static AppSettingService Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (ObjLocked)
                    {
                        if (null == _instance)
                            _instance = new AppSettingService();
                    }
                }
                return _instance;
            }
        }
        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetValue(string key, string defaultValue = "")
        {
            try
            {
                return _configuration.GetValue<string>(key);
            }
            catch
            {
                return defaultValue;
            }
        }
        public bool GetValue(string key, bool defaultValue = false)
        {
            try
            {
                return _configuration.GetValue<bool>(key);
            }
            catch
            {
                return defaultValue;
            }
        }
        public int GetValue(string key, int defaultValue = 0)
        {
            try
            {
                return _configuration.GetValue<int>(key);
            }
            catch
            {
                return defaultValue;
            }
        }
        public decimal GetValue(string key, decimal defaultValue = 0)
        {
            try
            {
                return _configuration.GetValue<decimal>(key);
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetConnection(string key, string defaultValue = "")
        {
            try
            {
                return _configuration.GetConnectionString(key);
            }
            catch
            {
                return defaultValue;
            }
        }
        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                return bool.Parse(_configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key).Value);
            }
            catch
            {
                return defaultValue;
            }
        }
        public int GetInt32(string key, int defaultValue = 0)
        {
            try
            {
                return Int32.Parse(_configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key).Value);
            }
            catch
            {
                return defaultValue;
            }
        }
        public long GetInt64(string key, long defaultValue = 0L)
        {
            try
            {
                return Int64.Parse(_configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key).Value);
            }
            catch
            {
                return defaultValue;
            }
        }
        public string GetString(string key, string defaultValue = "")
        {
            try
            {
                var value = _configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }
        public T Get<T>(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return _configuration.Get<T>();
            else
                return _configuration.GetSection(key).Get<T>();
        }
        public T Get<T>(string key, T defaultValue)
        {
            if (_configuration.GetSection(key) == null)
                return defaultValue;

            if (string.IsNullOrWhiteSpace(key))
                return _configuration.Get<T>();
            else
                return _configuration.GetSection(key).Get<T>();
        }
        public static T GetObject<T>(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Instance._configuration.Get<T>();
            else
            {
                var section = Instance._configuration.GetSection(key);
                return section.Get<T>();
            }
        }
        public static T GetObject<T>(string key, T defaultValue)
        {
            if (Instance._configuration.GetSection(key) == null)
                return defaultValue;

            if (string.IsNullOrWhiteSpace(key))
                return Instance._configuration.Get<T>();
            else
                return Instance._configuration.GetSection(key).Get<T>();
        }
    }
}

/*
 * How To use ?
 * Step 1: Configure service in "Startup" class & "ConfigureServices" method.
 * Ex:
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ......
            // Read App Settings
            CommonServices.AppSettings.Instance.SetConfiguration(Configuration);
        }
    }
 * Step 2: How to call from other controller or class ?
 * Ex:
    string connectionString = CommonServices.AppSettings.Instance.GetConnection("MySQL");
    string connectionString2 = CommonServices.AppSettings.Instance.GetValue("ConnectionStrings:MySQL","");
    bool connectionString3 = CommonServices.AppSettings.Instance.GetValue("Lockout:AllowedForNewUsers",false);
    int connectionString4 = CommonServices.AppSettings.Instance.GetValue("Lockout:DefaultLockoutTimeSpanInMins",0);
    decimal connectionString5 = CommonServices.AppSettings.Instance.GetValue("Lockout:Ratio",0);
*/

