using System;
using UnityEditor;
using UnityEngine;

namespace sc.modeling.splines.editor
{
    public static class AssetInfo
    {
        public const int ASSET_ID = 280289;
        public const string ASSET_NAME = "Spline Mesher";
        public const string VERSION = "1.4.0";
        public const string DOC_URL = "http://staggart.xyz/sm-docs";
        public const string FORUM_URL = "https://forum.unity.com/threads/1565389";
        public const string DISCORD_INVITE_URL = "https://staggart.xyz/support/discord/";

        private const string MIN_SPLINES_VERSION = "2.8.1";
        
        public static void OpenInPackageManager()
        {
            Application.OpenURL("com.unity3d.kharma:content/" + ASSET_ID);
        }
        
        public static void OpenReviewsPage()
        {
            Application.OpenURL($"https://assetstore.unity.com/packages/slug/{ASSET_ID}?aid=1011l7Uk8&pubref=sm1editor#reviews");
        }
        
        internal static class VersionChecking
        {
            public static bool UPDATE_AVAILABLE
            {
                get => SessionState.GetBool("SPLINE_MESHER_UPDATE_AVAILABLE", false);
                set => SessionState.SetBool("SPLINE_MESHER_UPDATE_AVAILABLE", value);
            }
            public static string LATEST_AVAILABLE
            {
                get => SessionState.GetString("SPLINE_MESHER_1_LATEST_AVAILABLE", AssetInfo.VERSION);
                set => SessionState.SetString("SPLINE_MESHER_1_LATEST_AVAILABLE", value);
            }

            private static string apiResult;

            public static void CheckForUpdate()
            {
                //UPDATE_AVAILABLE = true; return;
                
                //Default, in case of a fail
                UPDATE_AVAILABLE = false;
                
                //Offline
                if (Application.internetReachability == NetworkReachability.NotReachable) return;
                
                //Debug.Log("Checking for version update");
                
                var url = $"https://api.assetstore.unity3d.com/package/latest-version/{ASSET_ID}";

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    webClient.DownloadStringCompleted += OnRetrievedAPIContent;
                    webClient.DownloadStringAsync(new System.Uri(url), apiResult);
                }
            }

            private class AssetStoreItem
            {
                public string name;
                public string version;
            }

            private static void OnRetrievedAPIContent(object sender, System.Net.DownloadStringCompletedEventArgs e)
            {
                if (e.Error == null && !e.Cancelled)
                {
                    string result = e.Result;

                    AssetStoreItem asset = (AssetStoreItem)JsonUtility.FromJson(result, typeof(AssetStoreItem));

                    LATEST_AVAILABLE = asset.version;

                    Version remoteVersion = new Version(asset.version);
                    Version installedVersion = new Version(VERSION);

                    UPDATE_AVAILABLE = remoteVersion > installedVersion;

                    if (UPDATE_AVAILABLE)
                    {
                        //Debug.Log($"[{asset.name} v{installedVersion}] New version ({asset.version}) is available");
                    }
                }
            }
        }
    }
}