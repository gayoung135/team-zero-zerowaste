using UnityEngine;
using System.Linq;

public class TrashObject : MonoBehaviour
{
    public string TrashID;
    public TrashGuideData item;

    void Start()
    {
        item = GameDBManager.Instance.GetTrashData(TrashID); //정보 불러오기
        //태그 지정
        if(item.correct_bin_id == 1) gameObject.tag = "Recyclable";
        else gameObject.tag = "GeneralTrash";
        this.gameObject.name = item.name;
    }
}
