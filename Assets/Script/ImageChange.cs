using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageChange : MonoBehaviour
{
    public Sprite hover_img;
    public Sprite no_hover_img;
    Image thisImg;
    void Start()
    {
        thisImg = GetComponent<Image>();
    }

    public void ChangeButtonUI_In()
    {
        thisImg.sprite = hover_img;
    }

    public void ChangeButtonUI_out()
    {
        thisImg.sprite = no_hover_img;
    }
}
