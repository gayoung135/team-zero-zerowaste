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

    private List<TrashGuideData> trashList = new List<TrashGuideData>();
    private int currentPairIndex = 0;

    public void OpenCollection()
    {
        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB == null)
        {
            Debug.LogError("GameDBManager 또는 TrashGuideMasterDB를 찾을 수 없습니다.");
            return;
        }
        trashList = GameDBManager.Instance.trashGuideMasterDB;
        currentPairIndex = 0;

        // 새 씬의 UI 캔버스에 정렬 수치 각인
        UpdateBookUI();
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
}
