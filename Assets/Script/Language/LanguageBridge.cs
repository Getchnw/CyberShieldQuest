using UnityEngine.Localization.Settings;

public static class LanguageBridge
{
    public static string Get(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return "";

        // ถ้าปัจจุบันเป็นภาษาไทย (th) คืนค่าเดิมทันที
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "th") return originalText;

        // "MyGameTable" คือชื่อ String Table ที่คุณต้องสร้างใน Localization Window
        var localized = LocalizationSettings.StringDatabase.GetLocalizedString("MyGameData", originalText);

        // ถ้าหาคำแปลไม่เจอ จะคืนค่าเดิม (ภาษาไทยใน SO) กลับไป เกมจะได้ไม่พัง
        return string.IsNullOrEmpty(localized) ? originalText : localized;
    }

    // เพิ่มต่อจากฟังก์ชัน Get() เดิมเลยครับ
    public static string GetForceEnglish(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return "";

        // 1. ค้นหา Locale ที่เป็นภาษาอังกฤษ ("en") 
        var enLocale = LocalizationSettings.AvailableLocales.GetLocale("en");

        // 2. สั่งดึงคำแปลโดยบังคับใช้ enLocale ทันที
        var localized = LocalizationSettings.StringDatabase.GetLocalizedString("MyGameData", originalText, enLocale);

        // ถ้าหาไม่เจอจริงๆ ค่อยคืนค่าเดิมกลับไป
        return string.IsNullOrEmpty(localized) ? originalText : localized;
    }
}