using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffSlot : MonoBehaviour
{
    public string currentBuffName;
    public Image iconImage;
    public TextMeshProUGUI countText;

    public void SetupBuff(string name, Sprite icon, int count)
    {
        currentBuffName = name;
        iconImage.sprite = icon;
        countText.text = count.ToString();
        gameObject.SetActive(true);
    }

    public void UpdateCount(int count)
    {
        countText.text = count.ToString();
    }

    public void ClearBuff()
    {
        currentBuffName = "";
        gameObject.SetActive(false);
    }
}