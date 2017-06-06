using System;
using Android.App;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using static Android.Gms.Vision.Detector;
using System.Text;
using Android.Speech.Tts;
using Java.Util;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using EuromilhoesChecker.Model;
using Newtonsoft.Json;

namespace EuromilhoesChecker
{
    [Activity(Label = "EuromilhoesChecker", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, TextToSpeech.IOnInitListener, ISurfaceHolderCallback, IProcessor
    {
        private SurfaceView cameraView;
        private TextView textView;
        private TextView resultView;
        private CameraSource cameraSource;
        private Button btnCheckResult;
        private const int RequestCameraPremissionID = 1001;
        private TextToSpeech tts;
        private EuroMilionsResults bet = new EuroMilionsResults();
        string dataSort = string.Empty;
        List<string> numeros = new List<string>();
        List<string> estrelas = new List<string>();

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCameraPremissionID:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(cameraView.Holder);
                        }
                    }
                    break;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            cameraView = FindViewById<SurfaceView>(Resource.Id.surface_view);
            textView = FindViewById<TextView>(Resource.Id.text_view);
            btnCheckResult = FindViewById<Button>(Resource.Id.checkResult);
            resultView = FindViewById<TextView>(Resource.Id.result_view);

            TextRecognizer textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!textRecognizer.IsOperational)
            {
                Log.Error("Main Activity", "Dependencias não estao prontas");
            }
            else
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();

                cameraView.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);
                tts = new TextToSpeech(this, this);
                btnCheckResult.Click += BtnCheckResult_Click;
            }
        }

        private void BtnCheckResult_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dataSort))
            {
                if (numeros != null)
                {
                    bet.drawns = new List<Drawn>();
                    Drawn drawn = new Drawn
                    {
                        date = DateTime.Today.Date.ToString(),
                        numbers = string.Join(" ", numeros.ToArray()),
                        stars = string.Join(" ", estrelas.ToArray())
                    };
                    bet.drawns.Add(drawn);
                }
                new GetResults(this, bet).Execute(Common.Common.APIRequest(DateTime.Parse(dataSort)));
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {

        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera
                }, RequestCameraPremissionID);
                return;
            }
            cameraSource.Start(cameraView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            cameraSource.Stop();
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if (items.Size() != 0)
            {
                textView.Post(() =>
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < items.Size(); ++i)
                    {

                        sb.AppendFormat("{0}\n", ((TextBlock)items.ValueAt(i)).Value);

                    }

                    Regex rgx = new Regex(@"N\s([O]\d+|\d+[O]+|\d+)\s([O]\d+|\d+[O]+|\d+)\s([O]\d+|\d+[O]+|\d+)\s([O]\d+|\d+[O]+|\d+)\s([O]\d+|\d+[O]+|\d+)", RegexOptions.IgnoreCase);
                    MatchCollection matchesNum = rgx.Matches(sb.ToString());
                    if (matchesNum.Count > 0)
                    {
                        numeros.Clear();
                        for (int i = 0; i < matchesNum[0].Groups.Count; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string nt = matchesNum[0].Groups[i].Value;
                            nt = nt.Replace("O", "0");
                            numeros.Add(nt);
                        }
                    }

                    rgx = new Regex(@"E\s([O]\d+|\d+[O]+|\d+)\s([O]\d+|\d+[O]+|\d+)", RegexOptions.IgnoreCase);
                    MatchCollection matchesStar = rgx.Matches(sb.ToString());
                    if (matchesStar.Count > 0)
                    {
                        estrelas.Clear();
                        for (int i = 0; i < matchesStar[0].Groups.Count; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string et = matchesStar[0].Groups[i].Value;
                            et = et.Replace("O", "0");
                            estrelas.Add(et);
                        }
                    }

                    rgx = new Regex(@"(\d+\/\d+\/\d+)", RegexOptions.IgnoreCase);
                    MatchCollection matchesDTS = rgx.Matches(sb.ToString());
                    if (matchesDTS.Count > 0)
                    {
                        dataSort = string.Empty;
                        for (int i = 0; i < matchesDTS[0].Groups.Count; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string dt = matchesDTS[0].Groups[i].Value;
                            dt = dt.Replace("O", "0");
                            dataSort = dt;
                        }
                    }

                    textView.Text = string.Format("Data Sorteio:{2} \nNumeros: {0} \nEstrelas {1}", string.Join(" ", numeros.ToArray())
                        , string.Join(" ", estrelas.ToArray()), dataSort);
                    //textView.Text = sb.ToString();
                    //SpeakOut(textView.Text);
                });


            }
        }

        public void Release()
        {

        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status != OperationResult.Success)
            {
                Log.Error("Main Actitvity", "No TTS enable");
            }
            else
            {
                tts.SetLanguage(Locale.Us);
            }
        }

        private void SpeakOut(string text)
        {
            //if (!string.IsNullOrEmpty(text))
            //{
            //    tts.Speak(text, QueueMode.Flush, null);
            //}
        }

        private class GetResults : AsyncTask<string, Java.Lang.Void, string>
        {
            private ProgressDialog pd = new ProgressDialog(Application.Context);
            private MainActivity activity;
            EuroMilionsResults bet;

            public GetResults(MainActivity activity, EuroMilionsResults bet)
            {
                this.activity = activity;
                this.bet = bet;
            }

            protected override void OnPreExecute()
            {
                base.OnPreExecute();
                pd.Window.SetType(Android.Views.WindowManagerTypes.SystemAlert);
                pd.SetTitle("Aguarde ...");
                pd.Show();

            }

            protected override void OnPostExecute(string result)
            {
                base.OnPostExecute(result);
                if (!result.Contains("numbers"))
                {
                    pd.Dismiss();
                    return;
                }
                EuroMilionsResults results = JsonConvert.DeserializeObject<EuroMilionsResults>(result);

                List<string> numerosCertos = new List<string>();
                List<string> estrelasCertas = new List<string>();
                foreach (Drawn betDrawn in bet.drawns)
                {
                    List<string> bn = new List<string>(betDrawn.numbers.Split(new[] { ' ' }));
                    List<string> bs = new List<string>(betDrawn.stars.Split(new[] { ' ' }));
                    foreach (Drawn resultDrawn in results.drawns)
                    {
                        List<string> rn = new List<string>(resultDrawn.numbers.Split(new[] { ' ' }));
                        List<string> rs = new List<string>(resultDrawn.stars.Split(new[] { ' ' }));

                        for (int i = 0; i < bn.Count; i++)
                        {
                            if (Convert.ToInt32(rn[i]) == Convert.ToInt32(bn[i]))
                            {
                                numerosCertos.Add(bn[i]);
                            }

                        }
                        for (int i = 0; i < bs.Count; i++)
                        {
                            if (rs[i] == bs[i])
                            {
                                estrelasCertas.Add(bs[i]);
                            }

                        }
                    }
                }

                activity.resultView = activity.FindViewById<TextView>(Resource.Id.result_view);
                activity.resultView.Text = string.Format("Chave:{2} + {3} \nNumeros Certos:{0} \nEstrelas Certas:{1} ", numerosCertos.Count, estrelasCertas.Count, results.drawns[0].numbers, results.drawns[0].stars);


                pd.Dismiss();


            }

            protected override string RunInBackground(params string[] @params)
            {
                string stream = null;
                string urlString = @params[0];
                Helper.Helper http = new Helper.Helper();
                //urlstring = Common.Common.APIRequest()
                stream = http.GetHTTPData(urlString);
                return stream;
            }
        }
    }
}

