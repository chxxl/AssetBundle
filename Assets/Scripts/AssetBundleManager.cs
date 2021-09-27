using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class AssetBundleManager : MonoBehaviour
{
    // UI 요소
    [Header("UI")]
    public Text stateText;
    public GameObject newResourcePopUpObject;
    public Button newResourceDownloadButton;
    public GameObject downloadProgressBarImage;
    public Text downloadPercentageText;
    public Text downloadCountText;

    private string _localVersionFilePath;               // 로컬 버전파일 경로
    private string _serverVersionFilePath;              // 서버 버전파일 경로

    private AssetBundleInfoContainer _localAssetBundleInfo;
    private Queue<System.Tuple<UnityWebRequest, string>> downloadWaitQueue = new Queue<System.Tuple<UnityWebRequest, string>>();

    // 다운로드 한 에셋번들
    private AssetBundle imageBundle;
    private AssetBundle prefabBundle;
    private AssetBundle videoBundle;

    void Start()
    {
        // 인터넷 연결 체크
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            stateText.text = "인터넷 연결을 확인해주세요.";
            return;
        }

        _localVersionFilePath = Application.streamingAssetsPath + "/AssetBundleInfo.json";
        _serverVersionFilePath = "https://drive.google.com/u/0/uc?id=1wIO34OS5Wm_s9ItQJr6hOgb-Ec2YKOcp&export=download";

        StartCoroutine(StartVersionChecking());
    }

    // 버전 체크
    IEnumerator StartVersionChecking()
    {
        AssetBundleInfoContainer serverAssetBundleInfo;
        byte[] data;

        stateText.text = "리소스 버전 확인 중...";

        // 로컬 에셋번들인포 로드
        if (File.Exists(_localVersionFilePath))
        {
            string localJsonData = File.ReadAllText(_localVersionFilePath);
            _localAssetBundleInfo = JsonUtility.FromJson<AssetBundleInfoContainer>(localJsonData);
        }       

        using (UnityWebRequest uwr = UnityWebRequest.Get(_serverVersionFilePath))
        {
            yield return uwr.SendWebRequest();

            // 서버 에셋번들인포 로드
            serverAssetBundleInfo = JsonUtility.FromJson<AssetBundleInfoContainer>(uwr.downloadHandler.text);

            // 캐싱된것이 있으면 캐싱로드
            // 없으면 URL로 다운로드
            for (int i = 0; i < serverAssetBundleInfo.assetBundleInfoArray.Length; i++)
            {
                string filename = serverAssetBundleInfo.assetBundleInfoArray[i].filename;
                string version = serverAssetBundleInfo.assetBundleInfoArray[i].version;
                string url = serverAssetBundleInfo.assetBundleInfoArray[i].url;

                var unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, uint.Parse(version), 0);
                System.Tuple<UnityWebRequest, string> tp = new System.Tuple<UnityWebRequest, string>(unityWebRequest, filename);
                downloadWaitQueue.Enqueue(tp);
            }

           // 추후 저장할 서버 버전정보 담기
            data = uwr.downloadHandler.data;            
        }

        // 로컬과 서버 버전이 같으면 캐싱 로드 후 넘어감
        for (int i = 0; i < serverAssetBundleInfo.assetBundleInfoArray.Length; i++)
        {
            if (_localAssetBundleInfo == null ||
                serverAssetBundleInfo.assetBundleInfoArray[i].version != _localAssetBundleInfo.assetBundleInfoArray[i].version)
                break;

            yield return PatchProcess();

            stateText.text = "잠시후 화면이 전환됩니다.";
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(1);
            yield break;
        }

         // 서버 버전 테이블을 내려받아 로컬 버전테이블에 덮어쓰기
        FileStream fs = new FileStream(_localVersionFilePath, FileMode.Create);
        fs.Write(data, 0, data.Length);
        fs.Dispose();

        // 다운로드 필요 - 팝업 띄움, 버튼 기능 추가
        AddDownloadForButton();
        newResourcePopUpObject.SetActive(true);
    }

    // 패치 진행
    IEnumerator PatchProcess()
    {
        int maxDownloadCount = 4;       // 동시 다운로드 최대 개수

        // 다운로드 중인 파일 리스트
        List<System.Tuple<UnityWebRequest, string>> downloadingList = new List<System.Tuple<UnityWebRequest, string>>();

        // 다운로드 진행 정도 퍼센트
        float[] downloadPercentage = new float[downloadWaitQueue.Count];
        int downloadEnd = 0;

        SetDownloadCountText(downloadWaitQueue.Count, downloadingList.Count, downloadEnd);

        // 다운로드 대기 큐와 다운로드 완료 리스트에 인덱스가 있으면 반복
        while (downloadWaitQueue.Count > 0 || downloadingList.Count > 0)
        {
            // 동시 다운로드수 조건에 만족한다면 다운로드요청 후 리스트에 삽입
            if (downloadingList.Count < maxDownloadCount && downloadWaitQueue.Count > 0)
            {
                var download = downloadWaitQueue.Dequeue();
                download.Item1.SendWebRequest();

                downloadingList.Add(download);

                SetDownloadCountText(downloadWaitQueue.Count, downloadingList.Count, downloadEnd);
            }

            for (int i = 0; i < downloadingList.Count; i++)
            {
                // 다운로드 미완료시
                if (!downloadingList[i].Item1.isDone)
                {
                    downloadPercentage[i] = downloadingList[i].Item1.downloadProgress * 100;
                 
                    downloadPercentageText.text = SumListAllIndex(downloadPercentage).ToString("f1") + "%";
                    continue;
                }
                else
                {
                    print(downloadingList[i].Item2 + " 다운로드 완료");

                    UnityWebRequest uwr = downloadingList[i].Item1;
                    string filename = downloadingList[i].Item2;

                    if (filename == "image")
                    {
                        imageBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                        DataContainer.GetInstance().SetData(imageBundle.LoadAllAssets(), filename);
                    }
                    else if (filename == "prefab")
                    {
                        prefabBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                        DataContainer.GetInstance().SetData(prefabBundle.LoadAllAssets(), filename);
                    }
                    else if(filename == "video")
                    {
                        videoBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                        DataContainer.GetInstance().SetData(videoBundle.LoadAllAssets(), filename);
                    }

                    downloadingList.RemoveAt(i);
                    downloadEnd++;

                    SetDownloadCountText(downloadWaitQueue.Count, downloadingList.Count, downloadEnd);
                }
            }

            yield return null;
        }

        newResourcePopUpObject.SetActive(false);

        stateText.text = "잠시후 화면이 전환됩니다.";
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(1);
        yield break;
    }

    // 버튼에 패치 기능 추가 함수
    private void AddDownloadForButton()
    {
        newResourceDownloadButton.onClick.RemoveAllListeners();
        newResourceDownloadButton.onClick.AddListener(() =>
        {
            newResourceDownloadButton.gameObject.SetActive(false);
            downloadProgressBarImage.SetActive(true);
            StartCoroutine(PatchProcess());
        });
    }

    // 배열 값 모두 더하기
    private float SumListAllIndex(float[] array)
    {
       float value = 0;

        for (int i = 0; i < array.Length; i++)
            value += array[i];        

        return value / array.Length;
    }

    // 다운로드 파일 개수 업데이트
    private void SetDownloadCountText(int wait, int ing, int end)
    {
        downloadCountText.text = string.Format("다운로드 대기 : {0}개\n다운로드 중 : {1}개\n다운로드 완료 : {2}개\n", wait, ing, end);
    }
}


// 에셋번들인포 클래스
public class AssetBundleInfoContainer
{
    public AssetBundleInfo[] assetBundleInfoArray;
}

[System.Serializable]
public class AssetBundleInfo
{
    public string filename;
    public string version;
    public string url;
}
