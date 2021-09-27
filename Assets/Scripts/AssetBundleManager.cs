using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class AssetBundleManager : MonoBehaviour
{
    // UI ���
    [Header("UI")]
    public Text stateText;
    public GameObject newResourcePopUpObject;
    public Button newResourceDownloadButton;
    public GameObject downloadProgressBarImage;
    public Text downloadPercentageText;
    public Text downloadCountText;

    private string _localVersionFilePath;               // ���� �������� ���
    private string _serverVersionFilePath;              // ���� �������� ���

    private AssetBundleInfoContainer _localAssetBundleInfo;
    private Queue<System.Tuple<UnityWebRequest, string>> downloadWaitQueue = new Queue<System.Tuple<UnityWebRequest, string>>();

    // �ٿ�ε� �� ���¹���
    private AssetBundle imageBundle;
    private AssetBundle prefabBundle;
    private AssetBundle videoBundle;

    void Start()
    {
        // ���ͳ� ���� üũ
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            stateText.text = "���ͳ� ������ Ȯ�����ּ���.";
            return;
        }

        _localVersionFilePath = Application.streamingAssetsPath + "/AssetBundleInfo.json";
        _serverVersionFilePath = "https://drive.google.com/u/0/uc?id=1wIO34OS5Wm_s9ItQJr6hOgb-Ec2YKOcp&export=download";

        StartCoroutine(StartVersionChecking());
    }

    // ���� üũ
    IEnumerator StartVersionChecking()
    {
        AssetBundleInfoContainer serverAssetBundleInfo;
        byte[] data;

        stateText.text = "���ҽ� ���� Ȯ�� ��...";

        // ���� ���¹������� �ε�
        if (File.Exists(_localVersionFilePath))
        {
            string localJsonData = File.ReadAllText(_localVersionFilePath);
            _localAssetBundleInfo = JsonUtility.FromJson<AssetBundleInfoContainer>(localJsonData);
        }       

        using (UnityWebRequest uwr = UnityWebRequest.Get(_serverVersionFilePath))
        {
            yield return uwr.SendWebRequest();

            // ���� ���¹������� �ε�
            serverAssetBundleInfo = JsonUtility.FromJson<AssetBundleInfoContainer>(uwr.downloadHandler.text);

            // ĳ�̵Ȱ��� ������ ĳ�̷ε�
            // ������ URL�� �ٿ�ε�
            for (int i = 0; i < serverAssetBundleInfo.assetBundleInfoArray.Length; i++)
            {
                string filename = serverAssetBundleInfo.assetBundleInfoArray[i].filename;
                string version = serverAssetBundleInfo.assetBundleInfoArray[i].version;
                string url = serverAssetBundleInfo.assetBundleInfoArray[i].url;

                var unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, uint.Parse(version), 0);
                System.Tuple<UnityWebRequest, string> tp = new System.Tuple<UnityWebRequest, string>(unityWebRequest, filename);
                downloadWaitQueue.Enqueue(tp);
            }

           // ���� ������ ���� �������� ���
            data = uwr.downloadHandler.data;            
        }

        // ���ð� ���� ������ ������ ĳ�� �ε� �� �Ѿ
        for (int i = 0; i < serverAssetBundleInfo.assetBundleInfoArray.Length; i++)
        {
            if (_localAssetBundleInfo == null ||
                serverAssetBundleInfo.assetBundleInfoArray[i].version != _localAssetBundleInfo.assetBundleInfoArray[i].version)
                break;

            yield return PatchProcess();

            stateText.text = "����� ȭ���� ��ȯ�˴ϴ�.";
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(1);
            yield break;
        }

         // ���� ���� ���̺��� �����޾� ���� �������̺� �����
        FileStream fs = new FileStream(_localVersionFilePath, FileMode.Create);
        fs.Write(data, 0, data.Length);
        fs.Dispose();

        // �ٿ�ε� �ʿ� - �˾� ���, ��ư ��� �߰�
        AddDownloadForButton();
        newResourcePopUpObject.SetActive(true);
    }

    // ��ġ ����
    IEnumerator PatchProcess()
    {
        int maxDownloadCount = 4;       // ���� �ٿ�ε� �ִ� ����

        // �ٿ�ε� ���� ���� ����Ʈ
        List<System.Tuple<UnityWebRequest, string>> downloadingList = new List<System.Tuple<UnityWebRequest, string>>();

        // �ٿ�ε� ���� ���� �ۼ�Ʈ
        float[] downloadPercentage = new float[downloadWaitQueue.Count];
        int downloadEnd = 0;

        SetDownloadCountText(downloadWaitQueue.Count, downloadingList.Count, downloadEnd);

        // �ٿ�ε� ��� ť�� �ٿ�ε� �Ϸ� ����Ʈ�� �ε����� ������ �ݺ�
        while (downloadWaitQueue.Count > 0 || downloadingList.Count > 0)
        {
            // ���� �ٿ�ε�� ���ǿ� �����Ѵٸ� �ٿ�ε��û �� ����Ʈ�� ����
            if (downloadingList.Count < maxDownloadCount && downloadWaitQueue.Count > 0)
            {
                var download = downloadWaitQueue.Dequeue();
                download.Item1.SendWebRequest();

                downloadingList.Add(download);

                SetDownloadCountText(downloadWaitQueue.Count, downloadingList.Count, downloadEnd);
            }

            for (int i = 0; i < downloadingList.Count; i++)
            {
                // �ٿ�ε� �̿Ϸ��
                if (!downloadingList[i].Item1.isDone)
                {
                    downloadPercentage[i] = downloadingList[i].Item1.downloadProgress * 100;
                 
                    downloadPercentageText.text = SumListAllIndex(downloadPercentage).ToString("f1") + "%";
                    continue;
                }
                else
                {
                    print(downloadingList[i].Item2 + " �ٿ�ε� �Ϸ�");

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

        stateText.text = "����� ȭ���� ��ȯ�˴ϴ�.";
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(1);
        yield break;
    }

    // ��ư�� ��ġ ��� �߰� �Լ�
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

    // �迭 �� ��� ���ϱ�
    private float SumListAllIndex(float[] array)
    {
       float value = 0;

        for (int i = 0; i < array.Length; i++)
            value += array[i];        

        return value / array.Length;
    }

    // �ٿ�ε� ���� ���� ������Ʈ
    private void SetDownloadCountText(int wait, int ing, int end)
    {
        downloadCountText.text = string.Format("�ٿ�ε� ��� : {0}��\n�ٿ�ε� �� : {1}��\n�ٿ�ε� �Ϸ� : {2}��\n", wait, ing, end);
    }
}


// ���¹������� Ŭ����
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
