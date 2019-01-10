# AssetBundleManager

NOTE:This project is forked from https://bitbucket.org/Unity-Technologies/assetbundledemo, which is no longer actively maintained by Unity.

# Requirements
Unity 2018.3.0f2 or NEWER.

# UnityWebRequest vs WWW

```
//Unity 5.4+
Obsolete public static WWW LoadFromCacheOrDownload(string url, Hash128 hash, uint crc);

//Unity 2018.3
public static Networking.UnityWebRequest GetAssetBundle(Uri uri, Hash128 hash, uint crc);
```
