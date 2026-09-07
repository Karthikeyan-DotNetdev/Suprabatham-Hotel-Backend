using System.Data;

namespace AppSettings
{
    public class JwtConfig
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string TimeOutMinutes { get; set; }
        public string TimeOutMinutesDevice { get; set; }
        public string TimeOutMinutesCaptain { get; set; }
        public string TimeOutMinutesPasswordReset { get; set; }
    }

    public class OTPConfig
    {
        public string FromEmail { get; set; }
        public string FromEmailPassword { get; set; }
        public string ToEmail { get; set; }
    }

    public class MailConfig
    {
        public string SenderMailID { get; set; }
        public string FromMailID { get; set; }
        public string ReplyToMailID { get; set; }
        public string FriendlyName { get; set; }
        public string SmtpClient { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AppSetting
    {
        public static string ServerAppPath = "";
        public static string ServerwwwwrootPath = "";

        public static string ApiVersion = "";
        public static bool UseHttpsRedirection = false;
        public static string CipherKey = "";
        public static string IsDataLogEnabled = "";
        public static bool IsProduction = true;
        public static string AccountActivationUrl = "";

        public static JwtConfig Jwt;

        public static OTPConfig otp;

        public static string AdminUsername = "";
        public static string AdminPassword = "";
        public static string AdminMailID = "";
        public static string SignupNotificationMailID = "";

        public static MailConfig MailConfig;

        public static DataTable TenantDatabaseServerMaster = null;

        public static string MySQLConnectionString = "Server=xxxxx;Port=3306;UserID=xxxxx;Password=xxxxx;Database=xxxxx;ConnectionProtocol=Socket;SslMode=None;Pooling=true;ConnectionLifeTime=5;";

    }
}
