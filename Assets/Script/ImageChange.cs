using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageChange : MonoBehaviour
{
    public Sprite hover_img;
    public Sprite no_hover_img;
    public Sprite normalRobot;
    public Sprite goodRobot;
    public Sprite perfectRobot;
    public Sprite badRobot;
    public Sprite fatalRobot;
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

    public void RobotChange(int combo)
    {
        switch (combo)
        {
            case 0: thisImg.sprite = normalRobot; break;
            case 1: thisImg.sprite = goodRobot; break;
            case 2: thisImg.sprite = perfectRobot; break;
            case -1: thisImg.sprite = badRobot; break;
            case -2: thisImg.sprite = fatalRobot; break;
        }
    }

    // 말풍선이 꺼질 때 로봇 표정을 기본 표정으로 되돌리는 함수
    public void ResetRobot()
    {
        if (thisImg != null && normalRobot != null)
        {
            thisImg.sprite = normalRobot;
        }
    }
}