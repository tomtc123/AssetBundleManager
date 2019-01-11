using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloaderTest : MonoBehaviour
{
    public Text label;
    public Slider slider;
    public Text sliderText;
    
    IEnumerator Start()
    {
        yield return StartCoroutine(AssetBundles.AssetBundleDownloader.Instance.CollectDownloadFile());
        long size = AssetBundles.AssetBundleDownloader.Instance.DownloadTotalSize;
        label.text = string.Format("Download {0} assets?", FormatSize(size));
    }

    public void OnClickDownload()
    {
        StartCoroutine(AssetBundles.AssetBundleDownloader.Instance.DownloadAsync(
        (long cursize, long totalsize)=>
        {
            slider.value = cursize / (totalsize * 1.0f);
            sliderText.text = string.Format("{0:N2}%", 100 * cursize / (totalsize * 1.0f));
            Debug.LogFormat("downloaded size={0}, total size={1}", cursize, totalsize);
        }, 
        (long size) =>
        {
            label.text = "Download finished.";
        }));
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
