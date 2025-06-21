using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HandleHealthUI : MonoBehaviour
{
    [SerializeField] Sprite defaultHealthPotionIMG;
    [SerializeField] Sprite emptyHealthPotionIMG;

    [SerializeField] Image HealthPotionImage;

    [SerializeField] TextMeshProUGUI HealthPotionCountText;

    public void UpdateHealthPotionUI(int currentCount)
    {
        if (currentCount > 0)
        {
            HealthPotionImage.sprite = defaultHealthPotionIMG;
        }
        else
        {
            HealthPotionImage.sprite = emptyHealthPotionIMG;
        }
    }

    public void UpdateHealthPotionCount(int currentCount)
    {
        HealthPotionCountText.text = currentCount.ToString();
    }
}
