using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUISlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image cardImage;
    public Image frameImage; // 🔥 กรอบการ์ด
    public Button btn;
    public TextMeshProUGUI amountText;

    [Header("Card Frame")]
    public Sprite commonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    public Sprite legendaryFrame;

    private CardData _data;
    private UnityAction<CardData> _onLeftClick;
    private UnityAction<CardData> _onRightClick;

    private Vector3 originalScale;
    private Coroutine currentAnim;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(CardData data, int amount, UnityAction<CardData> leftClick, UnityAction<CardData> rightClick)
    {
        _data = data;
        _onLeftClick = leftClick;
        _onRightClick = rightClick;

        if (currentAnim != null) StopCoroutine(currentAnim);
        transform.localScale = originalScale;

        if (data.artwork != null) {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        } else {
            cardImage.color = Color.red; 
        }

        ApplyFrameByRarity();

        if (amountText != null) amountText.text = (amount >= 0) ? $"x{amount}" : "";
        if (amount == 0) cardImage.color = Color.gray;

        btn.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale * 1.05f, 0.1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale, 0.1f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        StartCoroutine(ClickEffect());

        if (eventData.button == PointerEventData.InputButton.Left) _onLeftClick?.Invoke(_data);
        else if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke(_data);
    }

    IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t); 
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator ClickEffect()
    {
        yield return StartCoroutine(AnimateScale(originalScale * 0.9f, 0.05f));
        yield return StartCoroutine(AnimateScale(originalScale * 1.1f, 0.05f));
    }

    void EnsureFrameImage()
    {
        // ถ้ามี frameImage อยู่แล้ว ไม่ต้องสร้างใหม่
        if (frameImage != null) return;

        // ลบ CardFrame เก่าทั้งหมดก่อน (ป้องกันการสร้างซ้ำ)
        foreach (Transform child in transform)
        {
            if (child.name == "CardFrame")
            {
                Destroy(child.gameObject);
            }
        }

        // สร้าง CardFrame ใหม่
        GameObject frameObj = new GameObject("CardFrame");
        frameObj.transform.SetParent(transform, false);
        frameObj.transform.SetAsFirstSibling();

        frameImage = frameObj.AddComponent<Image>();
        frameImage.raycastTarget = false;
        frameImage.color = new Color(0f, 0f, 0f, 0f); // โปร่งใสสนิท
        frameImage.sprite = null; // ไม่มี sprite

        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;
    }

    void ApplyFrameByRarity()
    {
        if (_data == null) return;
        EnsureFrameImage();
        if (frameImage == null) return;

        Sprite rarityFrame = null;
        switch (_data.rarity)
        {
            case Rarity.Common:
                rarityFrame = commonFrame;
                break;
            case Rarity.Rare:
                rarityFrame = rareFrame;
                break;
            case Rarity.Epic:
                rarityFrame = epicFrame;
                break;
            case Rarity.Legendary:
                rarityFrame = legendaryFrame;
                break;
        }

        if (rarityFrame != null)
        {
            frameImage.sprite = rarityFrame;
            frameImage.color = Color.white;
        }
        else
        {
            frameImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}