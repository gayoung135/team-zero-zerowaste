using UnityEngine;
using UnityEngine.UI;

public class ItemListButton : MonoBehaviour
{
    public void ItemClick()
    {
        EditMode manager = FindObjectOfType<EditMode>();
        if (manager == null)
        {
            GameObject settings = GameObject.Find("SettingsManager");
            if (settings != null) manager = settings.GetComponent<EditMode>();
        }

        if (manager != null)
        {
            Image img = GetComponent<Image>();
            if (img == null) img = GetComponentInChildren<Image>();

            if (img != null)
            {
                manager.itemClick(img.sprite);
            }
        }
    }
}