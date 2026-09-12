using UnityEngine;

public class TerrariumManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static TerrariumManager Instance { get; private set; }

    [Header("--- Current Selected Item ---")]
    public PlaceableItem currentSelectedItem;

    [Header("--- UI Elements ---")]
    [Tooltip("개구리 클릭 시 켜질 조작 UI 패널")]
    public GameObject itemControlUI; 

    [Tooltip("하단 인벤토리 Empty 메시지 게임 오브젝트")]
    public GameObject emptyMessageText;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 배치 아이템 선택 (핵심)
    public void SelectPlacedItem(PlaceableItem item)
    {
        currentSelectedItem = item;

        if (itemControlUI != null)
        {
            itemControlUI.SetActive(true);
        }

        Debug.Log($"<color=cyan>[TerrariumManager] 아이템 선택 완료: {item.gameObject.name}</color>");
    }

    // 기존 스크립트 호환용 (단일 정의)
    public void SelectItem(PlaceableItem item)
    {
        SelectPlacedItem(item);
    }

    // 선택 해제
    public void DeselectItem()
    {
        Debug.Log("<color=gray>[TerrariumManager] 아이템 선택 해제됨</color>");
        currentSelectedItem = null;

        if (itemControlUI != null)
        {
            itemControlUI.SetActive(false);
        }
    }

    // --------------------------------------------------
    // UI 버튼 연결용 함수 (Preview / Save / Cancel)
    // --------------------------------------------------

    // 1. Preview / Rotate 버튼 클릭
    public void OnClickPreview()
    {
        Debug.Log("[TerrariumManager] Preview 버튼 클릭됨!");
        if (currentSelectedItem != null)
        {
            currentSelectedItem.PreviewItem();
        }
        else
        {
            Debug.LogWarning("[TerrariumManager] 선택된 아이템(currentSelectedItem)이 NULL 입니다!");
        }
    }

    // 2. Save (완료/저장) 버튼 클릭
    public void OnClickSave()
    {
        Debug.Log("[TerrariumManager] Save 버튼 클릭됨!");
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SaveItem();
            DeselectItem(); 
        }
        else
        {
            Debug.LogWarning("[TerrariumManager] 선택된 아이템(currentSelectedItem)이 NULL 입니다!");
        }
    }

    // 3. Cancel (취소/보관) 버튼 클릭
    public void OnClickCancel()
    {
        Debug.Log("[TerrariumManager] Cancel 버튼 클릭됨!");
        if (currentSelectedItem != null)
        {
            currentSelectedItem.CancelItem();
            DeselectItem(); 
        }
        else
        {
            Debug.LogWarning("[TerrariumManager] 선택된 아이템(currentSelectedItem)이 NULL 입니다!");
        }
    }

    // TerrariumManager.cs 내부에 추가

// 1. 테라리움 전체 배치 상태 저장 (SAVE 버튼 연동)
public void SaveTerrariumLayout()
{
    // 배치되어 있는 모든 개구리/아이템의 ID, Position, Rotation 정보 수집 후 저장
    // 예시: PlayerPrefs 또는 JSON 저장
    Debug.Log("[TerrariumManager] 현재 배치된 개구리/아이템 데이터가 저장되었습니다.");
    
    // TODO: 프로젝트의 SaveSystem이 있다면 이곳에서 호출
    // SaveSystem.Save(placedItemsList);
}

// 2. 테라리움 전체 배치 취소 (CANCEL 버튼 연동)
public void CancelTerrariumLayout()
{
    // 편집 시작 직전 저장된 데이터로 다시 로드하거나 배치 상태 취소
    Debug.Log("[TerrariumManager] 편집 이전 데이터로 복구합니다.");
    
    // TODO: 저장되어 있던 원래 데이터 다시 로드
    // LoadTerrariumLayout();
}
}
