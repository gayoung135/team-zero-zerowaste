using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("--- Spawn Prefab ---")]
    [Tooltip("씬에 생성될 진짜 개구리 프리팹 (Project 창의 프리팹 파일 연결)")]
    public GameObject worldItemPrefab; 

    [Header("--- Spawn Settings ---")]
    [Tooltip("배치될 아이템의 크기 (원하는 크기로 자유롭게 조절하세요)")]
    public Vector3 spawnScale = new Vector3(0.3f, 0.3f, 1f);

    private GameObject currentSpawnedItem;
    private bool isDraggingSuccess = false;

    void Awake()
    {
        // 게임 시작 시 이 슬롯 및 자식에 붙어있는 모든 잔여 텍스트 컴포넌트 강제 삭제
        RemoveAllTextComponents(gameObject);
    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 잔여 텍스트 정리
        RemoveAllTextComponents(gameObject);
    }

    // 1. 드래그 시작 시점
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (worldItemPrefab == null)
        {
            Debug.LogError("[InventorySlotDrag] worldItemPrefab이 비어있습니다! Inspector를 확인하세요.");
            return;
        }

        isDraggingSuccess = false;

        // 마우스 화면 좌표를 2D 월드 좌표로 변환
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // 개구리 소환
        currentSpawnedItem = Instantiate(worldItemPrefab, mouseWorldPos, Quaternion.identity);

        if (currentSpawnedItem != null)
        {
            // 소환된 월드 개구리 오브젝트 내부 텍스트 삭제
            RemoveAllTextComponents(currentSpawnedItem);

            // UI Canvas 영향 제거를 위해 최상위 Root 월드로 계층 이동
            currentSpawnedItem.transform.SetParent(null);

            // 크기 설정
            RectTransform rectTransform = currentSpawnedItem.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = spawnScale;
            }
            else
            {
                currentSpawnedItem.transform.localScale = spawnScale;
            }

            // 강제 활성화 및 렌더링 레이어 순서 상향
            currentSpawnedItem.SetActive(true);

            SpriteRenderer sr = currentSpawnedItem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 20;
            }

            // 소환 직후 TerrariumManager에 자동 선택 등록 및 조작 UI 활성화
            PlaceableItem placeable = currentSpawnedItem.GetComponent<PlaceableItem>();
            if (placeable != null && TerrariumManager.Instance != null)
            {
                TerrariumManager.Instance.SelectPlacedItem(placeable);
            }
        }
    }

    // 2. 드래그 중 시점
    public void OnDrag(PointerEventData eventData)
    {
        if (currentSpawnedItem != null)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -Camera.main.transform.position.z;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            currentSpawnedItem.transform.position = mouseWorldPos;
            currentSpawnedItem.transform.localScale = spawnScale;

            isDraggingSuccess = true;
        }
    }

    // 3. 드래그 종료 시점
    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentSpawnedItem != null && isDraggingSuccess)
        {
            currentSpawnedItem.transform.localScale = spawnScale;

            // 하단 인벤토리 슬롯 비활성화
            gameObject.SetActive(false);

            // UI 상태 매니저를 통해 Empty 문구 갱신
            EditModeManager editManager = FindObjectOfType<EditModeManager>();
            if (editManager != null)
            {
                editManager.UpdateEmptyMessage();
            }
        }
    }

    // -------------------------------------------------------------
    // 강제로 텍스트 관련 컴포넌트를 모두 찾아 파괴하는 함수
    // -------------------------------------------------------------
    private void RemoveAllTextComponents(GameObject target)
    {
        if (target == null) return;

        Text[] legacyTexts = target.GetComponentsInChildren<Text>(true);
        foreach (Text t in legacyTexts)
        {
            Destroy(t);
        }

        TextMeshProUGUI[] tmpTexts = target.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI tmp in tmpTexts)
        {
            Destroy(tmp);
        }

        TextMeshPro[] tmp3DTexts = target.GetComponentsInChildren<TextMeshPro>(true);
        foreach (TextMeshPro tmp3d in tmp3DTexts)
        {
            Destroy(tmp3d);
        }
    }
}