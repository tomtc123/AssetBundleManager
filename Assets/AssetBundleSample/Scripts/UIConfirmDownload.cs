using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmDownload : MonoBehaviour
{
    public Text label;

    
    IEnumerator Start()
    {
        yield return StartCoroutine(AssetBundles.AssetBundleDownloader.Instance.CollectDownloadFile());
        long size = AssetBundles.AssetBundleDownloader.Instance.DownloadTotalSize;
        label.text = string.Format("Download {0} assets?", FormatSize(size));
    }

    public void OnClickDownload()
    {
        StartCoroutine(AssetBundles.AssetBundleDownloader.Instance.DownloadAsync((long cursize, long totalsize)=>
        {
            Debug.LogFormat("downloaded size={0}, total size={1}", cursize, totalsize);
        }, null));
    }

    public static string FormatSize(long byteCount)
    {
        if (byteCount < 1024)
            return string.Format("{0}B", byteCount);
        if (byteCount < 1024 * 1024)
            return string.Format("{0:0.0}KB", byteCount / 1024.0);
        return string.Format("{0:0.0}MB", byteCount / (1024 * 1024.0));
    }
}
