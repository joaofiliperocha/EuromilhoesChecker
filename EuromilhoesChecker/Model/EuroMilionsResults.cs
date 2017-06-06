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

namespace EuromilhoesChecker.Model
{
    
    public class Drawn
    {
        public string date { get; set; }
        public string numbers { get; set; }
        public string stars { get; set; }
    }

    public class EuroMilionsResults
    {
        public List<Drawn> drawns { get; set; }
    }
}