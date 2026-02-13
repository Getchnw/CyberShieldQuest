using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailView : MonoBehaviour
{
    [Header("UI Components")]
    public Image artworkImage;
    public Image frameImage; // 🔥 กรอบการ์ด
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI flavorText;

    [Header("Card Frame")]
    public Sprite commonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    public Sprite legendaryFrame;

    private CardData currentCard; // จำการ์ดที่กำลังแสดงอยู่

    // เช็คว่ากำลังแสดงการ์ดนี้อยู่หรือไม่
    public bool IsShowingCard(CardData card)
    {
        return gameObject.activeSelf && currentCard == card;
    }

    // ฟังก์ชันเปิดหน้าต่าง
    public void Open(CardData data)
    {
        currentCard = data; // จำการ์ดที่กำลังแสดง
        gameObject.SetActive(true); // โชว์หน้าต่าง

        // 1. ใส่รูป
        if (data.artwork != null) 
        {
            artworkImage.sprite = data.artwork;
        }
        ApplyFrameByRarity(data);

        //// 2. ใส่ชื่อ (เพิ่มหัวข้อ Name:)
        //nameText.text = $"<b>Name:</b> {data.cardName}";

        //// 3. ใส่ประเภท (เพิ่มหัวข้อ Type:)
        //// เช็คว่ามี SubCategory ไหม (ถ้าไม่ใช่ None/General ให้โชว์ด้วย)
        //string subCat = (data.subCategory != SubCategory.General && data.subCategory != SubCategory.General) 
        //                ? $" / [{data.subCategory}]" 
        //                : "";
        //typeText.text = $"<b>Type:</b> {data.type}{subCat}";

        //// 4. ใส่ค่าพลัง (เพิ่มหัวข้อ Stats:)
        //string stats = $"Cost: {data.cost}";
        //if (data.type == CardType.Monster || data.type == CardType.Token)
        //{
        //    stats += $" | Atk: {data.atk}";
        //    // if (data.hp > 0) stats += $" | HP: {data.hp}"; // ถ้ามี HP ให้เปิดบรรทัดนี้
        //}
        //statsText.text = $"<b>Stats:</b> {stats}";

        //// 5. ใส่สกิล (เพิ่มหัวข้อ Skill: และขึ้นบรรทัดใหม่)
        //abilityText.text = $"<b>Skill:</b>\n{data.abilityText}";

        //// 6. ใส่คำบรรยาย (เพิ่มหัวข้อ Description: และทำตัวเอียง)
        //flavorText.text = $"<b>Description:</b>\n<i>{data.flavorText}</i>";

        if (GameManager.Instance.CurrentGameData.isTranstale)
        {
            //English
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
            abilityText.text = $"<b>Skill:</b>\n{LanguageBridge.Get(data.abilityText)}";

            // 6. ใส่คำบรรยาย (เพิ่มหัวข้อ Description: และทำตัวเอียง)
            flavorText.text = $"<b>Description:</b>\n<i>{LanguageBridge.Get(data.flavorText)}</i>";
        }
        else
        {
            //Thai
            // 2. ใส่ชื่อ (เพิ่มหัวข้อ Name:)
            nameText.text = $"<b>ชื่อ:</b> {data.cardName}";

            // 3. ใส่ประเภท (เพิ่มหัวข้อ Type:)
            // เช็คว่ามี SubCategory ไหม (ถ้าไม่ใช่ None/General ให้โชว์ด้วย)
            string subCat = (data.subCategory != SubCategory.General && data.subCategory != SubCategory.General)
                            ? $" / [{data.subCategory}]"
                            : "";
            typeText.text = $"<b>ประเภท:</b> {data.type}{subCat}";

            // 4. ใส่ค่าพลัง (เพิ่มหัวข้อ Stats:)
            string stats = $"Cost: {data.cost}";
            if (data.type == CardType.Monster || data.type == CardType.Token)
            {
                stats += $" | Atk: {data.atk}";
                // if (data.hp > 0) stats += $" | HP: {data.hp}"; // ถ้ามี HP ให้เปิดบรรทัดนี้
            }
            statsText.text = $"<b>ค่าสถานะ:</b> {stats}";

            // 5. ใส่สกิล (เพิ่มหัวข้อ Skill: และขึ้นบรรทัดใหม่)
            abilityText.text = $"<b>สกิล:</b>\n{data.abilityText}";

            // 6. ใส่คำบรรยาย (เพิ่มหัวข้อ Description: และทำตัวเอียง)
            flavorText.text = $"<b>คำอธิบาย:</b>\n<i>{data.flavorText}</i>";
        }
    }

    // ฟังก์ชันปิดหน้าต่าง (ผูกกับปุ่ม Close)
    public void Close()
    {
        currentCard = null; // ล้างการ์ดที่จำไว้
        gameObject.SetActive(false);
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

    void ApplyFrameByRarity(CardData data)
    {
        if (data == null) return;
        EnsureFrameImage();
        if (frameImage == null) return;

        Sprite rarityFrame = null;
        switch (data.rarity)
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