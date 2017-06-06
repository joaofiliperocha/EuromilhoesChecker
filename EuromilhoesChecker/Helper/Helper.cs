using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Net;
using Java.IO;
using Android.Util;

namespace EuromilhoesChecker.Helper
{
  public  class Helper
    {
        static String stream;
        public Helper()
        {

        }

        public string GetHTTPData(string URLString)
        {
            try
            {
                URL url = new URL(URLString);
                using (var urlconnection = (HttpURLConnection)url.OpenConnection())
                {
                    if (urlconnection.ResponseCode == HttpStatus.Ok)
                    {
                        BufferedReader r = new BufferedReader(new InputStreamReader(urlconnection.InputStream));
                        StringBuilder sb = new StringBuilder();
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            sb.AppendLine(line);
                        }
                        stream = sb.ToString();
                        urlconnection.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Helper", ex.ToString());
            }

            return stream;
        }
    }
}