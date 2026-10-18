using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    [Header("[UI Panels]")]
    public GameObject collectionPanel;

    [Header("[Pages Component]")]
    public CollectionPage leftPage;
    public CollectionPage rightPage;

    private List<TrashGuideData> trashList = new List<TrashGuideData>();
    private int currentPairIndex = 0;
    private HashSet<string> unlockedTrashIds = new HashSet<string>();
    private const string SaveKey = "UnlockedTrashData";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        ForceClearBindings();
        RefreshSceneBinding();
        if (collectionPanel != null) collectionPanel.SetActive(false);
        LoadCollectionData();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 즉시 이전 씬의 유령 망령 주소 전수 소멸 처리
        ForceClearBindings();
        StartCoroutine(Co_DeferredRefreshBinding());
    }

    private IEnumerator Co_DeferredRefreshBinding()
    {
        // 오브젝트들이 하이Hierarchy에 무결하게 로드될 프레임 확보
        yield return new WaitForSecondsRealtime(0.1f);
        RefreshSceneBinding();
    }

    private void ForceClearBindings()
    {
        collectionPanel = null;
        leftPage = null;
        rightPage = null;
    }

    private void RefreshSceneBinding()
    {
        if (this == null || gameObject == null) return;

        // 씬 최상위 UI 루트 추적
        GameObject canvasObj = GameObject.Find("Terrarium_UI");
        if (canvasObj == null) return;

        // 꺼져있는(Inactive) 오브젝트까지 찾아서 부품 색출
        Transform[] childs = canvasObj.GetComponentsInChildren<Transform>(true);
        GameObject foundPanel = null;
        Button openBookButton = null;
        Button closeBookButton = null;
        Button nextBtn = null;
        Button prevBtn = null;

        foreach (var child in childs)
        {
            if (child == null) continue;

            if (child.gameObject.name == "Collection_Book_Panel")
            {
                foundPanel = child.gameObject;
            }
            if (child.gameObject.name == "CollectionButton" && child.GetComponent<Button>() != null)
            {
                openBookButton = child.GetComponent<Button>();
            }
            if (child.gameObject.name == "Invisible_Close_Area" && child.GetComponent<Button>() != null)
            {
                closeBookButton = child.GetComponent<Button>();
            }
            if (child.gameObject.name == "Next_Button" && child.GetComponent<Button>() != null)
            {
                nextBtn = child.GetComponent<Button>();
            }
            if (child.gameObject.name == "Prev_Button" && child.GetComponent<Button>() != null)
            {
                prevBtn = child.GetComponent<Button>();
            }
        }

        // 꺼져 있는 자식 페이지 오브젝트 주소 낚아채는 배열식 마스터 바인딩
        if (foundPanel != null)
        {
            collectionPanel = foundPanel;

            // GetComponentsInChildren(true), 꺼져 있는 자식 컴포넌트 찾음
            CollectionPage[] pages = foundPanel.GetComponentsInChildren<CollectionPage>(true);
            foreach (var page in pages)
            {
                if (page == null) continue;
                
                // 문자열 오차, 공백 지우고 이름 검사
                string pageName = page.gameObject.name.Trim();
                if (pageName == "Left_Page") leftPage = page;
                if (pageName == "Right_Page") rightPage = page;
            }
        }

        // 새 씬의 실물 버튼 컴포넌트들에 리스너 무결성 재조립
        if (openBookButton != null)
        {
            openBookButton.onClick.RemoveAllListeners();
            openBookButton.onClick.AddListener(() => { if (Instance != null) Instance.OpenCollection(); });
        }

        if (closeBookButton != null)
        {
            closeBookButton.onClick.RemoveAllListeners();
            closeBookButton.onClick.AddListener(() => { if (Instance != null) Instance.CloseCollection(); });
        }

        if (nextBtn != null)
        {
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(() => { if (Instance != null) Instance.NextPage(); });
        }

        if (prevBtn != null)
        {
            prevBtn.onClick.RemoveAllListeners();
            prevBtn.onClick.AddListener(() => { if (Instance != null) Instance.PreviousPage(); });
        }
    }

    public void OpenCollection()
    {
        // 도감 열기 직전, 유령 주소라면 조립 수색
        if (collectionPanel == null || leftPage == null || rightPage == null)
        {
            RefreshSceneBinding();
        }

        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB == null)
        {
            Debug.LogError("GameDBManager 또는 TrashGuideMasterDB를 찾을 수 없습니다.");
            return;
        }

        LoadCollectionData();
        trashList = GameDBManager.Instance.trashGuideMasterDB;
        currentPairIndex = 0;

        // 새 씬의 UI 캔버스에 정렬 수치 각인
        UpdateBookUI();

        if (collectionPanel != null)
        {
            collectionPanel.SetActive(true);
        }
    }

    public void CloseCollection()
    {
        if (collectionPanel != null) 
        {
            collectionPanel.SetActive(false);
        }
    }

    public void NextPage()
    {
        if (trashList == null || trashList.Count == 0) return;

        if ((currentPairIndex + 1) * 2 < trashList.Count)
        {
            currentPairIndex++;
            UpdateBookUI();
        }
    }

    public void PreviousPage()
    {
        if (currentPairIndex > 0)
        {
            currentPairIndex--;
            UpdateBookUI();
        }
    }

    private void UpdateBookUI()
    {
        if (leftPage == null || rightPage == null) RefreshSceneBinding();

        int leftIndex = currentPairIndex * 2;
        int rightIndex = leftIndex + 1;

        if (trashList == null || trashList.Count == 0) return;

        // 왼쪽 페이지 데이터 인젝션
        if (leftIndex < trashList.Count)
        {
            TrashGuideData tData = trashList[leftIndex];
            if (leftPage != null) leftPage.UpdatePage(tData);
        }
        else
        {
            if (leftPage != null) leftPage.UpdatePage(null);
        }

        // 오른쪽 페이지 데이터 인젝션
        if (rightIndex < trashList.Count)
        {
            TrashGuideData tData = trashList[rightIndex];
            if (rightPage != null) rightPage.UpdatePage(tData);
        }
        else
        {
            if (rightPage != null) rightPage.UpdatePage(null);
        }
    }

    public void UnlockTrash(string trashID)
    {
        if (string.IsNullOrEmpty(trashID)) return;
        LoadCollectionData();
        if (!unlockedTrashIds.Contains(trashID))
        {
            unlockedTrashIds.Add(trashID);
            SaveCollectionData();
        }
    }

    public bool IsUnlocked(string trashID)
    {
        if (string.IsNullOrEmpty(trashID)) return false;
        LoadCollectionData();
        return unlockedTrashIds.Contains(trashID.Trim());
    }

    private void SaveCollectionData()
    {
        string joinedIds = string.Join(",", unlockedTrashIds);
        PlayerPrefs.SetString(SaveKey, joinedIds);
        PlayerPrefs.Save();
    }

    private void LoadCollectionData()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string savedData = PlayerPrefs.GetString(SaveKey);
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] ids = savedData.Split(',');
                unlockedTrashIds.Clear();
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id)) unlockedTrashIds.Add(id.Trim());
                }
            }
        }
    }
}
