using System;
using System.Net;

public static class ipify
{
    public class Settings
    {
        public bool ALLOW_USER_AGENT = true;
        public string API_URL = "https://api.ipify.org";
        public uint MAX_TRIES = 3;
        public IWebProxy PROXY = null;
        public int TRIES_DURATION = 0x3e8;
        public string USER_AGENT = string.Format("csharp-ipify/{0} csharp/{1} {2}", VERSION, Environment.Version, Environment.OSVersion);
        public bool USING_PROXY = false;
        private static string VERSION = "1.0.0";
    }
    
    public enum Format
    {
        Raw, JSON, JSONP
    }

    public static Settings settings = new Settings();

    public static string Resolve(Format format = Format.Raw, string additional_args = null)
    {
        string result = null;
        object last_response = null;

        using (OWebClient webClient = new OWebClient() { Proxy = (settings.USING_PROXY) ? settings.PROXY : null, Headers = new WebHeaderCollection() })
        {
            if (settings.ALLOW_USER_AGENT)
                webClient.Headers.Add("User-Agent", settings.USER_AGENT);

            for (int tries = 0; tries < settings.MAX_TRIES; tries++)
            {
                try {
                    result = webClient.DownloadString(string.Format("{0}/?format={1}&{2}", settings.API_URL, format.ToString(), additional_args));
                }
                catch (WebException e) {
                    last_response = e;
                    System.Threading.Thread.Sleep(settings.TRIES_DURATION);
                }
            }
        }

        if (string.IsNullOrEmpty(result))
            throw new Exception("The request failed. Check the inner exception details for more information.", (WebException)last_response);
        else
            return result;
    }

    private class OWebClient : WebClient
    {
        public int Timeout;
        public OWebClient() : this(10000) { }

        public OWebClient(int timeout)
        {
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            if (w != null)
            {
                w.Timeout = this.Timeout;
            }
            return w;
        }
    }

}