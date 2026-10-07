using UnityEngine;
using UnityEngine.UI;

public class ItemListButton : MonoBehaviour
{
    private EditMode manager;
    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    public void ItemClick()
    {
        if (manager == null)
        {
            manager = EditMode.Instance != null ? EditMode.Instance : FindObjectOfType<EditMode>(true);
        }

        if (manager != null && buttonImage != null)
        {
            manager.itemClick(buttonImage.sprite);
        }
    }
}