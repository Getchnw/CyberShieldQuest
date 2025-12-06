using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GachaManager : MonoBehaviour
{
    [Header("Settings")]
    public int costPerPull = 100; // ราคาต่อการสุ่ม 1 ครั้ง

    [Header("Drop Rates (รวมกันต้องได้ 100)")]
    public int commonRate = 60;
    public int rareRate = 30;
    public int epicRate = 8;
    public int legendaryRate = 2;

    [Header("UI References")]
    public TextMeshProUGUI goldText;     // ข้อความแสดงเงิน
    public GameObject resultPanel;       // หน้าต่างผลลัพธ์ (Panel สีดำ)
    public Transform resultGrid;         // พื้นที่วางการ์ด (Content ใน ScrollView)
    public GameObject cardDisplayPrefab; // Prefab CardSlot

    // ตัวแปรเก็บข้อมูลการ์ดทั้งหมดในเกม
    private List<CardData> allCards;

    void Start()
    {
        // 1. โหลดการ์ดทั้งหมดจาก Resources มารอไว้
        allCards = Resources.LoadAll<CardData>("GameContent/Cards").ToList();
        
        // 2. ปิดหน้าต่างผลลัพธ์ก่อนเริ่ม
        if(resultPanel != null) resultPanel.SetActive(false);
    }

    void Update()
    {
        // 3. อัปเดตตัวเลขเงินตลอดเวลา
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"Gold: {GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }

    // =========================================================
    // 🔥 ฟังก์ชันสำหรับปุ่มกด (Public void ไม่มีพารามิเตอร์)
    // =========================================================

    public void PullOne()
    {
        // เช็คเงิน
        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;
        
        if (currentGold >= costPerPull)
        {
            // 1. หักเงิน
            GameManager.Instance.DecreaseGold(costPerPull); 

            // 2. สุ่มการ์ด
            CardData pulledCard = RandomCard();

            // 3. เพิ่มเข้ากระเป๋า
            GameManager.Instance.AddCardToInventory(pulledCard.card_id, 1);
            
            // 4. บันทึกเกมทันที
            GameManager.Instance.SaveCurrentGame();

            // 5. โชว์ผล
            ShowResult(new List<CardData> { pulledCard });
        }
        else
        {
            Debug.Log("เงินไม่พอ! (Not enough Gold)");
        }
    }

    public void PullTen()
    {
        int totalCost = costPerPull * 10;
        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;

        if (currentGold >= totalCost)
        {
            // 1. หักเงิน
            GameManager.Instance.DecreaseGold(totalCost);

            // 2. สุ่ม 10 ใบ
            List<CardData> pulledList = new List<CardData>();
            for (int i = 0; i < 10; i++)
            {
                CardData c = RandomCard();
                pulledList.Add(c);
                GameManager.Instance.AddCardToInventory(c.card_id, 1);
            }

            // 3. บันทึกเกม
            GameManager.Instance.SaveCurrentGame();

            // 4. โชว์ผล
            ShowResult(pulledList);
        }
        else
        {
            Debug.Log("เงินไม่พอ! (Not enough Gold)");
        }
    }

    // ปุ่มปิดหน้าต่างผลลัพธ์
    public void CloseResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    // =========================================================
    // 🎲 Logic การสุ่ม (System)
    // =========================================================

    CardData RandomCard()
    {
        int rng = Random.Range(0, 100); // สุ่มเลข 0-99
        Rarity targetRarity = Rarity.Common;

        // คำนวณเรท
        if (rng < legendaryRate) targetRarity = Rarity.Legendary;
        else if (rng < legendaryRate + epicRate) targetRarity = Rarity.Epic;
        else if (rng < legendaryRate + epicRate + rareRate) targetRarity = Rarity.Rare;
        else targetRarity = Rarity.Common;

        // คัดกรองการ์ดเฉพาะระดับที่สุ่มได้
        List<CardData> pool = allCards.FindAll(x => x.rarity == targetRarity);

        // กันเหนียว: ถ้าไม่มีการ์ดระดับนั้นเลย ให้ไปสุ่ม Common แทน
        if (pool.Count == 0) pool = allCards.FindAll(x => x.rarity == Rarity.Common);

        // สุ่มใบหนึ่งจากใน Pool
        if (pool.Count > 0)
            return pool[Random.Range(0, pool.Count)];
        
        return allCards[0]; // กรณีฉุกเฉิน (ไม่ควรเกิด)
    }

    // =========================================================
    // 🖼️ แสดงผล (UI)
    // =========================================================

    void ShowResult(List<CardData> cards)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);
        
        // ล้างการ์ดเก่าทิ้งก่อน
        foreach(Transform child in resultGrid) Destroy(child.gameObject);

        // สร้างการ์ดใหม่ตามลิสต์ที่ได้
        foreach(var card in cards)
        {
            GameObject obj = Instantiate(cardDisplayPrefab, resultGrid);
            
            // เรียกใช้ Setup ของ CardUISlot
            // ส่ง -1 เพื่อซ่อนตัวเลขจำนวน
            // ส่ง null เพื่อไม่ให้คลิกได้
            var slot = obj.GetComponent<CardUISlot>();
            if(slot != null) 
            {
                slot.Setup(card, -1, null, null);
            }
        }
    }
}