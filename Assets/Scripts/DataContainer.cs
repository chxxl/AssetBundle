using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DataContainer : MonoBehaviour
{
    #region singleton
    private static DataContainer instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static DataContainer GetInstance()
    {
        if (instance == null)
        {
            print("DataContainer Instance is NULL");
            return null;
        }

        return instance;
    }
    #endregion

    // 에셋 번들에서 추출된 데이터
    private List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> Prefabs { get { return prefabs; } }

    private List<Sprite> images = new List<Sprite>();
    public List<Sprite> Images { get { return images; } }

    private List<VideoClip> videos = new List<VideoClip>();
    public List<VideoClip> Videos { get { return videos; } }

   
    // 데이터를 추출
    // 다른 오브젝트가 같이 들어오는 경우가 있기때문에 is 키워드로 추출
    // ex) png를 에셋번들로 뽑으면 texture, sprite가 한 셋트로 저장됨
    public void SetData(Object[] assets, string filename)
    {
        foreach (Object obj in assets)
        {
            if (filename == "prefab" && obj is GameObject)
                prefabs.Add(obj as GameObject);
            else if (filename == "image" && obj is Sprite)
                images.Add(obj as Sprite);
            else if (filename == "video" && obj is VideoClip)
                videos.Add(obj as VideoClip);
         }
    }

    public Sprite GetRandomImage() => images[Random.Range(0, images.Count)];
    public GameObject GetRandomPrefab() => prefabs[Random.Range(0, prefabs.Count)];
    public VideoClip GetRandomVideo() => videos[Random.Range(0, videos.Count)];

}
