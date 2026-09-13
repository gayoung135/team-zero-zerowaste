using UnityEngine;
using UnityEngine.UI;

public class ItemListButton : MonoBehaviour
{
    public string itemName;
    public Sprite image;
    
    public void ItemClick()
    {
        EditMode manager = GameObject.Find("SettingsManager").GetComponent<EditMode>();
        manager.itemClick(image, itemName);
    }
}
