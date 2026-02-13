using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // อยู่ยาวไปทุกฉาก
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(int localeID)
    {
        StartCoroutine(SwitchLanguageRoutine(localeID));
    }

    private IEnumerator SwitchLanguageRoutine(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];

        // อัปเดตข้อมูลใน GameManager
        GameManager.Instance.CurrentGameData.isTranstale = (localeID == 0);
        GameManager.Instance.SaveCurrentGame();
    }
}