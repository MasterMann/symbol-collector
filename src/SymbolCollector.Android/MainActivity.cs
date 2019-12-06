using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SymbolCollector.Core;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace SymbolCollector.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private class GridAdapter : BaseAdapter
        {
            public override Object GetItem(int position)
            {
                return new[] {(String) "a", (String) "b", (String) "c"};
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
//                TextView dummyTextView = new TextView(mContext);
//                dummyTextView.setText(String.valueOf(position));
//                return dummyTextView;

                LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
                var inf = layoutInflater.Inflate(Resource.Layout.gridview_item, null);
                var textView = (TextView)inf.FindViewById(Resource.Id.gridViewItem);
                textView.Text = "aisdbasidbas";
                return inf;
            }

            public override int Count { get; } = 3;
        }

        private const string Tag = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var a = (GridView)base.FindViewById(Resource.Id.mainGridView);
            a.Adapter = new GridAdapter();

//            _ = StartUpload();
        }

        private Task StartUpload()
        {
#if DEBUG
            // For local testing on macOS: https://docs.microsoft.com/en-US/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#unable-to-start-aspnet-core-grpc-app-on-macos
            // 'HTTP/2 over TLS is not supported on macOS due to missing ALPN support.'.
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
            var bundle = PackageManager.GetApplicationInfo(PackageName, global::Android.Content.PM.PackageInfoFlags.MetaData).MetaData;
            var url = bundle.GetString("io.sentry.symbol-collector");
            Log.Info(Tag, "Using Symbol Collector endpoint: " + url);

            return Task.Run(async () =>
            {
                var paths = new[] {"/system/lib", "/system/lib64", "/system/"};
                var client = new Client(new Uri(url), logger: new LoggerAdapter<Client>());
                try
                {
                    await client.UploadAllPathsAsync(paths, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Log.Error(Tag, Throwable.FromException(e), "Failed uploading.");
                }
            });
        }
    }
}
