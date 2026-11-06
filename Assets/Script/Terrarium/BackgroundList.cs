using UnityEngine;
using UnityEngine.UI;

public class BackgroundList : MonoBehaviour
{
    public string saveKey; //저장용 키 이름
    public Image Icon; 
    private GameObject[] objects;
    private int currentIndex = 0; //바꾼게 몇번째인지
    private int savedIndex = 0; //저장용

    void Start()
    {
        objects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) objects[i] = transform.GetChild(i).gameObject;
        savedIndex = PlayerPrefs.GetInt(saveKey, 0);
        if (savedIndex < 0 || savedIndex >= objects.Length) savedIndex = 0;
        currentIndex = savedIndex;
        objects[currentIndex].SetActive(true);
        Icon.sprite = objects[currentIndex].GetComponent<Image>().sprite;
    }

    public void Next()   // 오른쪽 화살표 버튼
    {
        objects[currentIndex].SetActive(false);
        currentIndex++;
        if (currentIndex >= objects.Length) currentIndex = 0;
        objects[currentIndex].SetActive(true);
        Icon.sprite = objects[currentIndex].GetComponent<Image>().sprite;
    }

    public void Prev()   // 왼쪽 화살표 버튼
    {
        objects[currentIndex].SetActive(false);
        currentIndex--;
        if (currentIndex < 0) currentIndex = objects.Length - 1;
        objects[currentIndex].SetActive(true);
        Icon.sprite = objects[currentIndex].GetComponent<Image>().sprite;
    }

    public void Save()
    {
        savedIndex = currentIndex;
        PlayerPrefs.SetInt(saveKey, savedIndex);
        PlayerPrefs.Save();
        Icon.sprite = objects[currentIndex].GetComponent<Image>().sprite;
    }

    public void cancel()
    {
        objects[currentIndex].SetActive(false);
        currentIndex = savedIndex;
        objects[currentIndex].SetActive(true);
        Icon.sprite = objects[currentIndex].GetComponent<Image>().sprite;
    }
}
