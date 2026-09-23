using UnityEngine;

public class TrashObject : MonoBehaviour
{
    [Header("🔗 DB 연동 Key")]
    [Tooltip("유니티 인스펙터창에 입력한 쓰레기 Key(id)를 적어줍니다. 스패너가 자동 주입해 주므로 프리팹에는 비워두거나 기본값 세팅만 해도 됩니다.")]
    public string trashId;

    /// <summary>
    /// 이 쓰레기 오브젝트에 매칭된 정적 가이드 데이터를 GameDBManager에서 가져옵니다.
    /// </summary>
    public TrashGuideData GetMyData()
    {
        if (GameDBManager.Instance == null) return null;
        return GameDBManager.Instance.GetTrashData(trashId);
    }
}
