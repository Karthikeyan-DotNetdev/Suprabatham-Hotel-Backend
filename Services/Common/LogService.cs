using AppSettings;

namespace CommonServices
{
    public class LogService
    {
        public static void DataLog(ILogger logger, string eventName, string eventUrl, string eventPayload, string responseStatusCode, string responseData)
        {
            if (AppSetting.IsDataLogEnabled == "1")
            {
                try
                {
                    string tmp = "\r\n------------------------------------------------";
                    tmp += "\r\nRequest For : " + eventName;
                    tmp += "\r\nRequest Url : " + eventUrl;
                    tmp += "\r\nRequest Payload :";
                    tmp += "\r\n" + eventPayload;
                    tmp += "\r\nResponse Status :" + responseStatusCode;
                    tmp += "\r\nResponse Payload :";
                    tmp += "\r\n" + responseData;
                    tmp += "\r\n------------------------------------------------";
                    logger.LogInformation(tmp);
                }
                catch { }
            }
        }
        public static void DataLog(ILogger logger, string eventName, string eventUrl, Newtonsoft.Json.Linq.JObject eventPayload, string responseStatusCode, string responseData)
        {
            if (AppSetting.IsDataLogEnabled == "1")
            {
                try
                {
                    string tmp = "\r\n------------------------------------------------";
                    tmp += "\r\nRequest For : " + eventName;
                    tmp += "\r\nRequest Url : " + eventUrl;
                    tmp += "\r\nRequest Payload :";
                    tmp += "\r\n" + UtilityService.JObjectToString(eventPayload);
                    tmp += "\r\nResponse Status :" + responseStatusCode;
                    tmp += "\r\nResponse Payload :";
                    tmp += "\r\n" + responseData;
                    tmp += "\r\n------------------------------------------------";
                    logger.LogInformation(tmp);
                }
                catch { }
            }
        }

    }
}
