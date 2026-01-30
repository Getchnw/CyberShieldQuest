// ไฟล์ test เพื่อให้ CardData ทำงานอย่างถูกต้อง

// ปัญหา: cardData.atk และ cardData.hp = 0
// สาเหตุ: GetData() ส่งคืน CardData ที่เป็น empty/default

// วิธีแก้:
// 1. ให้ Setup() force override ทั้งหมด
// 2. ให้ GetData() ตรวจสอบว่า _cardData ถูกต้อง
// 3. ให้เพิ่ม debug log ทั้งหมด

// สัญชาต: ปัญหาคือ _cardData reference กำลังชี้ไปยัง default CardData หรือ CardData ที่เป็น copy empty
// ต้องตรวจสอบ cardPrefab ว่ามี prefab instance ของ CardData ไว้ใน Inspector ไหม
