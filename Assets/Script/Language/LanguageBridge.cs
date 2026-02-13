using UnityEngine.Localization.Settings;

public static class LanguageBridge
{
    public static string Get(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return "";

        // ถ้าปัจจุบันเป็นภาษาไทย (th) คืนค่าเดิมทันที
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "th") return originalText;

        // "MyGameTable" คือชื่อ String Table ที่คุณต้องสร้างใน Localization Window
        var localized = LocalizationSettings.StringDatabase.GetLocalizedString("MyGameTable", originalText);

        // ถ้าหาคำแปลไม่เจอ จะคืนค่าเดิม (ภาษาไทยใน SO) กลับไป เกมจะได้ไม่พัง
        return string.IsNullOrEmpty(localized) ? originalText : localized;
    }
}