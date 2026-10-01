using UnityEngine;
using UnityEngine.UI;

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false;

    [Header("--- UI Group Containers ---")]
    public GameObject editUI;            // Terrarium_UI -> Edit_UI
    public GameObject basicUI;           // Terrarium_UI -> Basic_UI
    
    [Header("--- Preview UI ---")]
    public GameObject previewEditButton; // Terrarium_UI -> PreviewEditButton (우측 상단 독립 버튼)

    [Header("--- Item Spawn Settings ---")]
    public GameObject itemPrefab;   
    public GameObject itemParent;   

    [Header("--- Edit UI Elements ---")]
    public GameObject itemBarPanel; // Edit_UI -> Panel
    public GameObject emptyText;    // Empty... 텍스트

    void Start()
    {
        //rectTransform = GetComponent<RectTransform>();
        //나중에 아이템 불러오는 코드 이 아래에 추가하기
    }

    // 1. 메인 화면
    public void ShowBasicUI()
    {
        isEditMode = false;
        
        if (basicUI != null) basicUI.SetActive(true);
        if (editUI != null) editUI.SetActive(false);
        if (previewEditButton != null) previewEditButton.SetActive(false);
    }

    // 2. 편집 모드 진입 ([EDIT] 버튼 누를 때)
    public void EditModeOn()
    {
        isEditMode = true;
    }
    
    public void save() //저장버튼
    {
        Debug.Log("저장");
        //tempbox에 들어있는 아이템들의 종류와 좌표를 데이터베이스에 저장해야 합니다
        isEditMode = false;
    }

    // 3. [PREVIEW] 버튼 클릭 시 (관상 모드)
    public void OnClickPreview()
    {
        Debug.Log("취소");
        //데이터베이스에 저장된 아이템을 tempbox로 불러오는 기능을 구현해야 합니다
        isEditMode = false;

        if (basicUI != null) basicUI.SetActive(false);
        if (editUI != null) editUI.SetActive(false);
        if (previewEditButton != null) previewEditButton.SetActive(true); 
    }
}