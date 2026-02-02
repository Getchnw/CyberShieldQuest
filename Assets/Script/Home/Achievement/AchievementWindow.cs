using UnityEngine;
using System.Collections.Generic;

public class AchievementWindow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentContainer; // จุดที่จะเสกกล่องลงไป (Content ใน ScrollView)
    [SerializeField] private GameObject achievementPrefab; // ตัว Prefab "Achievement box"

    void OnEnable()
    {
        CreateUI();
    }

    public void CreateUI()
    {
        // เคลียร์ของเก่า
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        // ดึงรายการ Achievement ทั้งหมดจาก Manager
        var allStaticData = GameContentDatabase.Instance.GetAllAchievementData();
        var userSaveData = GameManager.Instance.CurrentGameData.achievements;

        foreach (var staticData in allStaticData)
        {
            // หาข้อมูลเซฟของอันนี้ (ถ้าไม่มีให้สร้าง Dummy ขึ้นมาโชว์ก่อน)
            var saveData = userSaveData.Find(x => x.achievementID == staticData.id);
            if (saveData == null)
            {
                saveData = new PlayerAchievementData
                {
                    achievementID = staticData.id,
                    isUnlocked = false,
                    isClaimed = false
                };
            }

            // เสก Prefab
            GameObject obj = Instantiate(achievementPrefab, contentContainer);
            AchievementItemUI ui = obj.GetComponent<AchievementItemUI>();

            // ตั้งค่า
            ui.Setup(staticData, saveData);
        }
    }
}