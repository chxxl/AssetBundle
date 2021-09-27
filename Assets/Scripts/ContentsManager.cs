using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ContentsManager : MonoBehaviour
{
    public Image image;
    public Transform prefabSpawnParent;
    public VideoPlayer videoPlayer;

    public Text imageCount;
    public Text prefabCount;
    public Text videoCount;

    // Start is called before the first frame update
    void Start()
    {
        // ���� ����
        imageCount.text = DataContainer.GetInstance().Images.Count.ToString() + "��";
        prefabCount.text = DataContainer.GetInstance().Prefabs.Count.ToString() + "��";
        videoCount.text = DataContainer.GetInstance().Videos.Count.ToString() + "��";

        // �̹��� ����
        image.sprite = DataContainer.GetInstance().GetRandomImage();

        // ������ ����
        GameObject getObj = DataContainer.GetInstance().GetRandomPrefab();
        Transform obj = Instantiate(getObj).transform;
        obj.parent = prefabSpawnParent;
        obj.localPosition = new Vector3(0,-170, -10);
        obj.localScale = Vector3.one * 144;

        // ���� ����
        videoPlayer.clip = DataContainer.GetInstance().GetRandomVideo();
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.Play();
    }
}
