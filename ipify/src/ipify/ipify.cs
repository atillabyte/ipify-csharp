using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// This project can output the Class library as a NuGet Package.
// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
public static class ipify
{
    public static class Settings
    {
        public static bool ALLOW_USER_AGENT = true;
        public static string API_URL = "https://api.ipify.org";
        public static uint MAX_TRIES = 3;
        public static IWebProxy PROXY = null;
        public static int TRIES_DURATION = 0x3e8;
        public static string USER_AGENT = string.Format("csharp-ipify/{0} csharp/{1} {2}", VERSION, Environment.Version, Environment.OSVersion);
        public static bool USING_PROXY = false;
        private static string VERSION = "1.0.0";
    }


    public enum Format
    {
        Raw, JSON, JSONP
    }

    public static string Resolve(Format format = Format.Raw, string additional_args = null)
    {
        string result = null;
        object last_response = null;

        using (OWebClient webClient = new OWebClient() { Proxy = (Settings.USING_PROXY) ? Settings.PROXY : null, Headers = new WebHeaderCollection() })
        {
            if (Settings.ALLOW_USER_AGENT)
                webClient.Headers.Add("User-Agent", Settings.USER_AGENT);

            for (int tries = 0; tries < Settings.MAX_TRIES; tries++)
            {
                try {
                    result = webClient.DownloadString(string.Format("{0}/?format={1}&{2}", Settings.API_URL, format.ToString(), additional_args));
                }
                catch (WebException e) {
                    last_response = e;
                    Task.Delay(Settings.TRIES_DURATION);
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