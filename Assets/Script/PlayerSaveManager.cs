// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;

// public class PlayerSaveManager : MonoBehaviour
// {
//     public static PlayerSaveManager Instance; // Singleton เรียกใช้ได้ทุกที่

//     [System.Serializable]
//     public class CardInventoryData
//     {
//         public string cardId;
//         public int amount;
//     }

//     [System.Serializable]
//     public class PlayerSaveData
//     {
//         public List<CardInventoryData> inventory = new List<CardInventoryData>();
//         // อนาคตเพิ่ม Gold, Gem, Level ตรงนี้ได้
//     }

//     public PlayerSaveData currentData;

//     void Awake()
//     {
//         if (Instance == null) Instance = this;
//         else Destroy(gameObject);

//         LoadData();
//     }

//     // --- ฟังก์ชันใช้งานหลัก ---

//     // เพิ่มการ์ด (ใช้ตอนเปิดกาชา / จบด่าน)
//     public void AddCard(string id, int count)
//     {
//         CardInventoryData found = currentData.inventory.Find(x => x.cardId == id);
//         if (found != null)
//         {
//             found.amount += count;
//         }
//         else
//         {
//             currentData.inventory.Add(new CardInventoryData { cardId = id, amount = count });
//         }
//         SaveData();
//     }

//     // เช็คจำนวนการ์ดที่มี
//     public int GetCardAmount(string id)
//     {
//         CardInventoryData found = currentData.inventory.Find(x => x.cardId == id);
//         return (found != null) ? found.amount : 0;
//     }

//     // --- ระบบ Save/Load ---
//     public void SaveData()
//     {
//         string json = JsonUtility.ToJson(currentData);
//         PlayerPrefs.SetString("PlayerSaveData", json);
//         PlayerPrefs.Save();
//     }

//     public void LoadData()
//     {
//         if (PlayerPrefs.HasKey("PlayerSaveData"))
//         {
//             string json = PlayerPrefs.GetString("PlayerSaveData");
//             currentData = JsonUtility.FromJson<PlayerSaveData>(json);
//         }
//         else
//         {
//             currentData = new PlayerSaveData();
//             // แจกการ์ดเริ่มต้นให้ (Starter Deck)
//             AddStarterCards(); 
//         }
//     }

//     // แจกการ์ดฟรีตอนเริ่มเกมใหม่ (แก้ ID ให้ตรงกับที่คุณมี)
//     void AddStarterCards()
//     {
//         Debug.Log("Creating New Save...");
//         AddCard("M_A01_01", 3); // Data Snoop x3
//         AddCard("S_A01_01", 2); // ID Enumerator x2
//         // ... เพิ่มใบอื่นตามใจชอบ
//     }

    
// }