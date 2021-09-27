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
        // 개수 셋팅
        imageCount.text = DataContainer.GetInstance().Images.Count.ToString() + "개";
        prefabCount.text = DataContainer.GetInstance().Prefabs.Count.ToString() + "개";
        videoCount.text = DataContainer.GetInstance().Videos.Count.ToString() + "개";

        // 이미지 셋팅
        image.sprite = DataContainer.GetInstance().GetRandomImage();

        // 프리팹 생성
        GameObject getObj = DataContainer.GetInstance().GetRandomPrefab();
        Transform obj = Instantiate(getObj).transform;
        obj.parent = prefabSpawnParent;
        obj.localPosition = new Vector3(0,-170, -10);
        obj.localScale = Vector3.one * 144;

        // 비디오 셋팅
        videoPlayer.clip = DataContainer.GetInstance().GetRandomVideo();
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.Play();
    }
}
