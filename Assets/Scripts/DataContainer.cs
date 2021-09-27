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

    // ���� ���鿡�� ����� ������
    private List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> Prefabs { get { return prefabs; } }

    private List<Sprite> images = new List<Sprite>();
    public List<Sprite> Images { get { return images; } }

    private List<VideoClip> videos = new List<VideoClip>();
    public List<VideoClip> Videos { get { return videos; } }

   
    // �����͸� ����
    // �ٸ� ������Ʈ�� ���� ������ ��찡 �ֱ⶧���� is Ű����� ����
    // ex) png�� ���¹���� ������ texture, sprite�� �� ��Ʈ�� �����
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
