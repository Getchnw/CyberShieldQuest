using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class LanguageSettingsController : MonoBehaviour
{
    public GameObject ThaiButton;
    public GameObject EnglishButton;

    // ทำงานทุกครั้งที่หน้า Settings ถูกเปิดขึ้นมา (SetActive(true))
    private void OnEnable()
    {
        // เช็คจาก GameManager ว่าตอนนี้ตั้งค่าอะไรไว้
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "th")
        {
            GameManager.Instance.CurrentGameData.isTranstale = false;
        }
        else
        {
            GameManager.Instance.CurrentGameData.isTranstale = true;
        }
        int currentID = GameManager.Instance.CurrentGameData.isTranstale ? 0 : 1;
        UpdateButtonVisuals(currentID);
    }

    public void OnClickSelectLanguage(int localeID)
    {
        // สั่งให้ Manager ตัวจริงทำงาน
        LanguageManager.Instance.SetLanguage(localeID);
        UpdateButtonVisuals(localeID);
    }

    void UpdateButtonVisuals(int selectedID)
    {
        Color activeColor = Color.white;
        Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // ปรับสีทั้งปุ่ม Icon และ Text
        ApplyColor(EnglishButton, selectedID == 0 ? activeColor : inactiveColor);
        ApplyColor(ThaiButton, selectedID == 1 ? activeColor : inactiveColor);
    }

    void ApplyColor(GameObject btnObj, Color targetColor)
    {
        foreach (var img in btnObj.GetComponentsInChildren<Image>()) img.color = targetColor;
        foreach (var txt in btnObj.GetComponentsInChildren<TextMeshProUGUI>()) txt.color = targetColor;
    }
}