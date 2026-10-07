using UnityEngine;
using UnityEngine.UI;

public class ItemListButton : MonoBehaviour
{
    public string itemId;
    public void ItemClick()
    {
        EditMode manager = GameObject.Find("SettingsManager").GetComponent<EditMode>();
        manager.itemClick(GetComponent<Image>().sprite, itemId);
    }
}