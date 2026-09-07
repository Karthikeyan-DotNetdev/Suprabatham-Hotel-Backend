using System.Net.Http.Headers;

namespace CommonServices
{
    public class HttpHeaders
    {
        public HttpHeaders()
        {
            headers = new List<Header>();
        }
        public string AuthorizationBasicToken { get; set; }
        public string AuthorizationBearerToken { get; set; }
        public string AcceptHeader { get; set; }
        public string ContentType { get; set; }
        public string UserAgent { get; set; }
        public List<Header> headers { get; set; }
    }
    public class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public static class HeadersAcceptHeader
    {
        public static string application_json
        {
            get { return "application/json"; }
        }
        public static string application_xml
        {
            get { return "application/xml"; }
        }
    }
    public static class HeadersContentType
    {
        public static string application_json
        {
            get { return "application/json"; }
        }
        public static string application_xml
        {
            get { return "application/xml"; }
        }
    }

    public class HttpResponseResult
    {
        public HttpResponseMessage httpResponseMessage { get; set; }
        public string httpResponseData { get; set; }
    }

    public class HttpService
    {
        /// <summary>
        /// This is custom maximum timeout (120) seconds for http request.
        /// </summary>
        public static double customTimeoutSeconds = 120;


        #region HTTP GET Methods ------------------------------------------------------------------------------
        public static async Task<HttpResponseResult> GetHttpResponseMessage(string url, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = httpResponseMessage = await httpClient.GetAsync(url);

                HttpResponseResult httpResponseResult = new HttpResponseResult();
                httpResponseResult.httpResponseMessage = httpResponseMessage;
                httpResponseResult.httpResponseData = await httpResponseMessage.Content.ReadAsStringAsync();

                return httpResponseResult;
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }
        public static async Task<string> GetReadAsStringAsync(string url, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);
                httpResponseMessage.EnsureSuccessStatusCode();

                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }
        public static async Task<byte[]> GetByteArrayAsync(string url, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                byte[] imageBytes = await httpClient.GetByteArrayAsync(url);

                //string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //string localFilename = "favicon.ico";
                //string localPath = Path.Combine(documentsPath, localFilename);
                //File.WriteAllBytes(localPath, imageBytes);

                return imageBytes;
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        #endregion HTTP GET Methods ------------------------------------------------------------------------------


        #region HTTP POST Methods ------------------------------------------------------------------------------
        public static async Task<HttpResponseResult> PostHttpResponseMessage(string url, HttpContent requestStringContent, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, requestStringContent);

                HttpResponseResult httpResponseResult = new HttpResponseResult();
                httpResponseResult.httpResponseMessage = httpResponseMessage;
                httpResponseResult.httpResponseData = await httpResponseMessage.Content.ReadAsStringAsync();

                return httpResponseResult;
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }
        public static async Task<string> PostReadAsStringAsync(string url, HttpContent requestStringContent, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, requestStringContent);
                httpResponseMessage.EnsureSuccessStatusCode();

                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        #endregion HTTP POST Methods ------------------------------------------------------------------------------


        #region HTTP PUT Methods ------------------------------------------------------------------------------
        public static async Task<HttpResponseResult> PutHttpResponseMessage(string url, HttpContent requestStringContent, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = await httpClient.PutAsync(url, requestStringContent);

                HttpResponseResult httpResponseResult = new HttpResponseResult();
                httpResponseResult.httpResponseMessage = httpResponseMessage;
                httpResponseResult.httpResponseData = await httpResponseMessage.Content.ReadAsStringAsync();

                return httpResponseResult;
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }
        public static async Task<string> PutReadAsStringAsync(string url, HttpContent requestStringContent, HttpHeaders httpHeaders = null, double timeoutSeconds = 0)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = null;
            httpClientHandler.UseProxy = false;
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                HttpReqAddTimeout(httpClient, timeoutSeconds);
                HttpReqAddHeader(httpClient, httpHeaders);

                HttpResponseMessage httpResponseMessage = await httpClient.PutAsync(url, requestStringContent);
                httpResponseMessage.EnsureSuccessStatusCode();

                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch
            {
                httpClient.Dispose();
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        #endregion HTTP PUT Methods ------------------------------------------------------------------------------


        #region Http Req Utility Methods
        public static void HttpReqAddTimeout(HttpClient httpClient, double timeoutSeconds = 0)
        {
            if (timeoutSeconds < 0)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(100); //default timeoutSeconds of library
            }
            else if (timeoutSeconds == 0)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(customTimeoutSeconds);
            }
            else if (timeoutSeconds > 0)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }
        }

        public static void HttpReqAddHeader(HttpClient httpClient, HttpHeaders httpHeaders)
        {
            if (httpHeaders != null)
            {
                if (!string.IsNullOrWhiteSpace(httpHeaders.AuthorizationBasicToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", httpHeaders.AuthorizationBasicToken);
                }

                if (!string.IsNullOrWhiteSpace(httpHeaders.AuthorizationBearerToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpHeaders.AuthorizationBearerToken);
                }

                if (!string.IsNullOrWhiteSpace(httpHeaders.UserAgent))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("User-Agent", httpHeaders.UserAgent);
                }

                if (!string.IsNullOrWhiteSpace(httpHeaders.ContentType))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Content-Type", httpHeaders.ContentType);
                }

                if (!string.IsNullOrWhiteSpace(httpHeaders.AcceptHeader))
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(httpHeaders.AcceptHeader));
                }

                if (httpHeaders.headers != null && httpHeaders.headers.Count > 0)
                {
                    foreach (Header header in httpHeaders.headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);
                    }
                }
            }
        }

        #endregion Http Req Utility Methods

    }
}
