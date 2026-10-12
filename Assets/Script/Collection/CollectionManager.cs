using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    [Header("[UI Panels]")]
    public GameObject collectionPanel;
    [Header("[Pages Component]")]
    public CollectionPage leftPage;
    public CollectionPage rightPage;

    [Header("[인게임 쓰레기 프리팹 등록 리스트]")]
    public List<GameObject> trashPrefabs = new List<GameObject>();

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
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (collectionPanel != null) collectionPanel.SetActive(false);
        LoadCollectionData();
    }

    // 도감 해금
    public void UnlockTrash(string trashName)
    {
        if (string.IsNullOrEmpty(trashName)) return;

        if (PlayerPrefs.HasKey(SaveKey))
        {
            string savedData = PlayerPrefs.GetString(SaveKey);
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] ids = savedData.Split(',');
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id)) unlockedTrashIds.Add(id.Trim());
                }
            }
        }

        // 새 쓰레기 이름을 기입
        if (!unlockedTrashIds.Contains(trashName))
        {
            unlockedTrashIds.Add(trashName);
            Debug.Log($"<color=green>[도감 해금 성공]</color> 목록 누적 완료: {trashName}");
            SaveCollectionData();
        }
    }

    // 해금 검사
    public bool IsUnlocked(string trashName)
    {
        if (string.IsNullOrEmpty(trashName)) return false;

        // 저장소 데이터와 실시간 최종 싱크(Sync) 맞춤
        LoadCollectionData();

        string targetName = trashName.ToLower().Trim();

        foreach (string unlockedId in unlockedTrashIds)
        {
            string savedName = unlockedId.ToLower().Trim();

            if (savedName == targetName || savedName.Contains(targetName) || targetName.Contains(savedName))
            {
                return true;
            }
        }
        return false;
    }

    // 도감 열기 버튼
    public void OpenCollection()
    {
        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB == null)
        {
            Debug.LogError("GameDBManager 또는 TrashGuideMasterDB를 찾을 수 없습니다.");
            return;
        }

        LoadCollectionData();

        trashList = GameDBManager.Instance.trashGuideMasterDB;
        currentPairIndex = 0;
        UpdateBookUI();
        collectionPanel.SetActive(true);
    }

    public void CloseCollection()
    {
        if (collectionPanel != null) collectionPanel.SetActive(false);
    }

    public void NextPage()
    {
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
        int leftIndex = currentPairIndex * 2;
        int rightIndex = leftIndex + 1;

        if (leftIndex < trashList.Count)
        {
            TrashGuideData tData = trashList[leftIndex];
            Sprite trashSprite = GetTrashSpriteFromPrefabs(tData.trash_name); 
            leftPage.UpdatePage(tData, trashSprite);
        }
        else
        {
            leftPage.UpdatePage(null, null);
        }

        if (rightIndex < trashList.Count)
        {
            TrashGuideData tData = trashList[rightIndex];
            Sprite trashSprite = GetTrashSpriteFromPrefabs(tData.trash_name); 
            rightPage.UpdatePage(tData, trashSprite);
        }
        else
        {
            rightPage.UpdatePage(null, null);
        }
    }

    private Sprite GetTrashSpriteFromPrefabs(string trashName)
    {
        if (string.IsNullOrEmpty(trashName)) return null;
        
        string target = trashName.ToLower().Trim();

        foreach (GameObject prefab in trashPrefabs)
        {
            if (prefab == null) continue;

            string prefabName = prefab.name.ToLower().Trim();

            if (prefabName == target || prefabName.Contains(target) || target.Contains(prefabName))
            {
                SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) return spriteRenderer.sprite;

                Image uiImage = prefab.GetComponent<Image>();
                if (uiImage != null) return uiImage.sprite;
            }
        }
        return null; 
    }

    private void SaveCollectionData()
    {
        string joinedIds = string.Join(",", unlockedTrashIds);
        PlayerPrefs.SetString(SaveKey, joinedIds);
        PlayerPrefs.Save();
        Debug.Log($"[도감 하드 세이브 완료] 최종 데이터 보존 목록: {joinedIds}");
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
