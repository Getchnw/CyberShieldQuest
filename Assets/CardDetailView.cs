using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailView : MonoBehaviour
{
    [Header("UI Components")]
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI flavorText;

    // ฟังก์ชันเปิดหน้าต่าง
    public void Open(CardData data)
    {
        gameObject.SetActive(true); // โชว์หน้าต่าง

        // 1. ใส่รูป
        if (data.artwork != null) 
        {
            artworkImage.sprite = data.artwork;
        }

        // 2. ใส่ชื่อ (เพิ่มหัวข้อ Name:)
        nameText.text = $"<b>Name:</b> {data.cardName}";

        // 3. ใส่ประเภท (เพิ่มหัวข้อ Type:)
        // เช็คว่ามี SubCategory ไหม (ถ้าไม่ใช่ None/General ให้โชว์ด้วย)
        string subCat = (data.subCategory != SubCategory.General && data.subCategory != SubCategory.General) 
                        ? $" / [{data.subCategory}]" 
                        : "";
        typeText.text = $"<b>Type:</b> {data.type}{subCat}";

        // 4. ใส่ค่าพลัง (เพิ่มหัวข้อ Stats:)
        string stats = $"Cost: {data.cost}";
        if (data.type == CardType.Monster || data.type == CardType.Token)
        {
            stats += $" | Atk: {data.atk}";
            // if (data.hp > 0) stats += $" | HP: {data.hp}"; // ถ้ามี HP ให้เปิดบรรทัดนี้
        }
        statsText.text = $"<b>Stats:</b> {stats}";

        // 5. ใส่สกิล (เพิ่มหัวข้อ Skill: และขึ้นบรรทัดใหม่)
        abilityText.text = $"<b>Skill:</b>\n{data.abilityText}";

        // 6. ใส่คำบรรยาย (เพิ่มหัวข้อ Description: และทำตัวเอียง)
        flavorText.text = $"<b>Description:</b>\n<i>{data.flavorText}</i>";
    }

    // ฟังก์ชันปิดหน้าต่าง (ผูกกับปุ่ม Close)
    public void Close()
    {
        gameObject.SetActive(false);
    }
}