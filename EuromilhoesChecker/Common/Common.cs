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

namespace EuromilhoesChecker.Common
{
    class Common
    {
        public static string API_LINK = "https://nunofcguerreiro.com/api-euromillions-json?";

        public static string APIRequest (DateTime DataSorteio)
        {
            StringBuilder sb = new StringBuilder(API_LINK);
            sb.AppendFormat("result={0}", DataSorteio.ToString("yyyy-MM-dd"));
            return sb.ToString();
        }
    }
}