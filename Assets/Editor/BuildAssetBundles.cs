using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundles : MonoBehaviour
{
    [MenuItem("Bundles/Build AssetBundles")]
    static public void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundle", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    [MenuItem("Bundles/Clear Cache")]
    static public void ClearingCache()
    {
        Caching.ClearCache();
        print("cache clearing done");
    }
}
