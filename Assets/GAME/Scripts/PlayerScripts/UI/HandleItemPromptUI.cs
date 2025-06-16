using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class HandleItemPromptUI : MonoBehaviour
{

    [SerializeField] Image itemInteractOverlayImage;
    [SerializeField] TMP_Text itemUseText;

    [SerializeField] Image ItemPromptImage;
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text ItemNameText;
    [SerializeField] TMP_Text ItemDescriptionText;
    private bool isFadeOutAnimPlaying;
    private bool isFadeInAnimPlaying;
    private bool isItemPromptShowing = false;

    public bool IsItemPromptShowing => isItemPromptShowing;

    public void SetItemPickUpUI(ItemPickUp itemPickUp)
    {
        ItemNameText.text = itemPickUp.itemName;
        ItemDescriptionText.text = itemPickUp.itemDescription;
        itemImage.sprite = itemPickUp.itemIcon;
    }

    

    public void FadeOutItemPromptUI(float waitTime, Action onFadeOutComplete = null)
    {
        if (isFadeOutAnimPlaying) return;
        StartCoroutine(FadeOutImage(waitTime, ItemPromptImage, itemInteractOverlayImage, itemImage, ItemNameText,
         ItemDescriptionText, itemUseText, () => isItemPromptShowing = false));
    }

    public void FadeInItemPromptUI(float waitTime,Action onFadeInComplete = null)
    {
        if (isFadeInAnimPlaying) return;
        ItemPromptImage.gameObject.SetActive(true);
        isItemPromptShowing = true;
         StartCoroutine(FadeInImage(waitTime, ItemPromptImage, itemInteractOverlayImage, itemImage, ItemNameText,
         ItemDescriptionText, itemUseText, onFadeInComplete));
    }

    

    IEnumerator FadeOutImage(float waitTime, Image overlayImage,Image itemInteractOverlayImage, Image iconImage, TMP_Text itemNameText
    , TMP_Text itemDescriptionText,TMP_Text itemUseText, Action onFadeOutComplete = null)
    {

        isFadeOutAnimPlaying = true;
        float elapsedTime = 0f;
        Color startOverlayImageColor = overlayImage.color;
        Color startitemInteractOverlayImageColor = itemInteractOverlayImage.color;
        Color startIconImageColor = iconImage.color;

        Color startItemNameTextColor = itemNameText.color;
        Color startItemDescriptionTextColor = itemDescriptionText.color;
        Color startitemUseTextColor = itemUseText.color;

        Color endOverlayImageColor = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 0f);
        Color enditemInteractOverlayImageColor = new Color(itemInteractOverlayImage.color.r, itemInteractOverlayImage.color.g, itemInteractOverlayImage.color.b, 0f);
        Color endIconImageColor = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);

        Color endItemNameTextColor = new Color(itemNameText.color.r, itemNameText.color.g, itemNameText.color.b, 0f);
        Color endItemDescriptionTextColor = new Color(itemDescriptionText.color.r, itemDescriptionText.color.g, itemDescriptionText.color.b, 0f);
        Color enditemUseTextColor = new Color(itemUseText.color.r, itemUseText.color.g, itemUseText.color.b, 0f);

        while (elapsedTime < waitTime)
        {
            float t = elapsedTime / waitTime;
            overlayImage.color = Color.Lerp(startOverlayImageColor, endOverlayImageColor, t);
            iconImage.color = Color.Lerp(startIconImageColor, endIconImageColor, t);
            itemInteractOverlayImage.color = Color.Lerp(startitemInteractOverlayImageColor, enditemInteractOverlayImageColor, t);

            if (itemNameText.text != "")
            {
                itemNameText.color = Color.Lerp(startItemNameTextColor, endItemNameTextColor, t);

            }
            if (itemDescriptionText.text != "")
            {
                itemDescriptionText.color = Color.Lerp(startItemDescriptionTextColor, endItemDescriptionTextColor, t);
            }
            if (itemDescriptionText.text != "")
            {
                itemUseText.color = Color.Lerp(startitemUseTextColor, enditemUseTextColor, t);
            }


            elapsedTime += Time.deltaTime;

            yield return null;
        }

        overlayImage.color = endOverlayImageColor;
        itemInteractOverlayImage.color = enditemInteractOverlayImageColor;
        iconImage.color = endIconImageColor;

        itemNameText.color = endItemNameTextColor;
        itemDescriptionText.color = endItemDescriptionTextColor;
        itemUseText.color = enditemUseTextColor;

        overlayImage.gameObject.SetActive(false);
        isFadeOutAnimPlaying = false;

        onFadeOutComplete?.Invoke();

    }

    IEnumerator FadeInImage(float waitTime, Image overlayImage,Image itemInteractOverlayImage, Image iconImage, TMP_Text itemNameText
    , TMP_Text itemDescriptionText,TMP_Text itemUseText, Action onFadeInComplete = null)
    {

        isFadeInAnimPlaying = true;
        float elapsedTime = 0f;
        Color startOverlayImageColor = overlayImage.color;
        Color startitemInteractOverlayImageColor = itemInteractOverlayImage.color;
        Color startIconImageColor = iconImage.color;

        Color startItemNameTextColor = itemNameText.color;
        Color startItemDescriptionTextColor = itemDescriptionText.color;
        Color startitemUseTextColor = itemUseText.color;

        Color endOverlayImageColor = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 1f);
        Color enditemInteractOverlayImageColor = new Color(itemInteractOverlayImage.color.r, itemInteractOverlayImage.color.g, itemInteractOverlayImage.color.b, 1f);
        Color endIconImageColor = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        Color endItemNameTextColor = new Color(itemNameText.color.r, itemNameText.color.g, itemNameText.color.b, 1f);
        Color endItemDescriptionTextColor = new Color(itemDescriptionText.color.r, itemDescriptionText.color.g, itemDescriptionText.color.b, 1f);
        Color enditemUseTextColor = new Color(itemUseText.color.r, itemUseText.color.g, itemUseText.color.b, 1f);

        while (elapsedTime < waitTime)
        {
            float t = elapsedTime / waitTime;
            overlayImage.color = Color.Lerp(startOverlayImageColor, endOverlayImageColor, t);
            iconImage.color = Color.Lerp(startIconImageColor, endIconImageColor, t);
            itemInteractOverlayImage.color = Color.Lerp(startitemInteractOverlayImageColor, enditemInteractOverlayImageColor, t);

            if (itemNameText.text != "")
            {
                itemNameText.color = Color.Lerp(startItemNameTextColor, endItemNameTextColor, t);

            }
            if (itemDescriptionText.text != "")
            {
                itemDescriptionText.color = Color.Lerp(startItemDescriptionTextColor, endItemDescriptionTextColor, t);
            }
            if (itemDescriptionText.text != "")
            {
                itemUseText.color = Color.Lerp(startitemUseTextColor, enditemUseTextColor, t);
            }


            elapsedTime += Time.deltaTime;

            yield return null;
        }

        overlayImage.color = endOverlayImageColor;
        itemInteractOverlayImage.color = enditemInteractOverlayImageColor;
        iconImage.color = endIconImageColor;

        itemNameText.color = endItemNameTextColor;
        itemDescriptionText.color = endItemDescriptionTextColor;
        itemUseText.color = enditemUseTextColor;
        
        isFadeInAnimPlaying = false;

        onFadeInComplete?.Invoke();

    }

    
}
