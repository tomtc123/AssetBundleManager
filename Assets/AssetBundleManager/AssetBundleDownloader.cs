using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace AssetBundles {
    public class AssetBundleDownloader : MonoBehaviour
    {
        public enum LoaderSpace
        {
            Streaming,//Application.streamingAssetsPath
            Persistent,//Application.persistentDataPath
            Server,
        }

        public class AssetBundleVersion
        {
            public string name;
            public string hash;
            public long size;

            public LoaderSpace loaderSpace = LoaderSpace.Streaming;
        }

        [HideInInspector]
        public string assetBundleVersionFileName = "AssetBundleVersion.bytes";

        public Dictionary<string, AssetBundleVersion> streamingVersion = new Dictionary<string, AssetBundleVersion>();
        public Dictionary<string, AssetBundleVersion> persistentVersion = new Dictionary<string, AssetBundleVersion>();
        public Dictionary<string, AssetBundleVersion> serverVersion = new Dictionary<string, AssetBundleVersion>();

        public long DownloadTotalSize { get; set; }

        private List<AssetBundleVersion> mDownloadAssetBundleVersion = new List<AssetBundleVersion>();

        private static AssetBundleDownloader mInstance;
        public static AssetBundleDownloader Instance { get { return mInstance; } }

        
        private void Awake()
        {
            mInstance = this;
            DontDestroyOnLoad(this);
        }

        public IEnumerator CollectDownloadFile()
        {
            UnityWebRequest request = UnityWebRequest.Get(GetVersionFileUrl(LoaderSpace.Streaming));
            yield return request.SendWebRequest();
            string versionText = DownloadHandlerBuffer.GetContent(request);
            streamingVersion = GetAssetBundleVersion(versionText, LoaderSpace.Streaming);

            print(Application.persistentDataPath);
            print(GetVersionFileUrl(LoaderSpace.Persistent));
            request = UnityWebRequest.Get(GetVersionFileUrl(LoaderSpace.Persistent));
            yield return request.SendWebRequest();
            print(request.error);
            if (string.IsNullOrEmpty(request.error))
            {
                versionText = DownloadHandlerBuffer.GetContent(request);
                print(versionText);
                persistentVersion = GetAssetBundleVersion(versionText, LoaderSpace.Persistent);
            }

            AssetBundleManager.SetDevelopmentAssetBundleServer();

            request = UnityWebRequest.Get(GetVersionFileUrl(LoaderSpace.Server));
            print(GetVersionFileUrl(LoaderSpace.Server));
            yield return request.SendWebRequest();
            versionText = DownloadHandlerBuffer.GetContent(request);
            serverVersion = GetAssetBundleVersion(versionText, LoaderSpace.Server);

            DownloadTotalSize = 0;
            foreach (var item in serverVersion)
            {
                if (persistentVersion.ContainsKey(item.Key))
                {
                    if (!string.Equals(item.Value.hash, persistentVersion[item.Key].hash))
                    {
                        DownloadTotalSize += item.Value.size;
                        mDownloadAssetBundleVersion.Add(item.Value);
                    }
                }
                else
                {
                    AssetBundleVersion streamInfo = null;
                    streamingVersion.TryGetValue(item.Key, out streamInfo);
                    if (streamInfo == null || !string.Equals(item.Value.hash, streamInfo.hash))
                    {
                        DownloadTotalSize += item.Value.size;
                        mDownloadAssetBundleVersion.Add(item.Value);
                    }
                }
            }

        }

        public static void WriteAllBytes(string rPath, byte[] rBytes)
        {
            string rDir = Path.GetDirectoryName(rPath);
            if (!Directory.Exists(rDir)) Directory.CreateDirectory(rDir);
            File.WriteAllBytes(rPath, rBytes);
        }
        
        public IEnumerator DownloadAsync(UnityAction<long, long> dowanloadingCallback, UnityAction<long> finishedCallback)
        {
            long downloadedSize = 0;
            foreach (var bundleVersion in mDownloadAssetBundleVersion)
            {
                UnityWebRequest request = UnityWebRequest.Get(Path.Combine(AssetBundleManager.BaseDownloadingURL, bundleVersion.name));
                yield return request.SendWebRequest();
                var bytes = request.downloadHandler.data;
                string savePath = Path.Combine(Application.persistentDataPath, bundleVersion.name);
                WriteAllBytes(savePath, bytes);
                Debug.Log("====Save AssetBundle" + savePath);
                persistentVersion.Add(bundleVersion.name, bundleVersion);
                downloadedSize += bundleVersion.size;
                if (dowanloadingCallback != null)
                {
                    dowanloadingCallback(downloadedSize, DownloadTotalSize);
                }
            }
            if (finishedCallback != null)
            {
                finishedCallback(DownloadTotalSize);
            }
            string versions = "";
            foreach (var item in persistentVersion)
            {
                versions += string.Format("{0},{1},{2}\n", item.Key, item.Value.hash, item.Value.size);
            }
            //File.WriteAllText(Path.Combine(Application.persistentDataPath, "AssetBundleVersion.bytes"), versions);

        }

        public string GetVersionFileUrl(LoaderSpace loaderspace)
        {
            switch (loaderspace)
            {
                case LoaderSpace.Streaming:
                    return Path.Combine(Utility.GetStreamingAssetPath(true), assetBundleVersionFileName);
                case LoaderSpace.Persistent:
                    return Path.Combine(Utility.GetPlatformPrefix(Application.platform), Application.persistentDataPath, assetBundleVersionFileName);
                default:
                    return Path.Combine(AssetBundleManager.BaseDownloadingURL, assetBundleVersionFileName);
            }
        }

        Dictionary<string, AssetBundleVersion> GetAssetBundleVersion(string text, LoaderSpace loaderSpace)
        {
            Dictionary<string, AssetBundleVersion> assetBundleVersion = new Dictionary<string, AssetBundleVersion>();
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] info = line.Split(',');
                    AssetBundleVersion version = new AssetBundleVersion();
                    version.name = info[0];
                    version.hash = info[1];
                    version.size = long.Parse(info[2]);
                    version.loaderSpace = loaderSpace;
                    assetBundleVersion.Add(version.name, version);

                }
            }
            return assetBundleVersion;
        }
    }
    
}
