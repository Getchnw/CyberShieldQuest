// ไฟล์นี้แสดงตัวอย่างการใช้งาน RevealHandMultiple สำหรับการ์ดเวทย์

/*
=== วิธีการใช้งานสกิล "ดูการ์ดบนมือ" ===

1. สร้างการ์ดเวทย์ใน Unity Editor (Create > Game Content > Card)

2. ตั้งค่าการ์ดดังนี้:
   - Card Type: Spell
   - Main Category: A01/A02/A03/General (ตามต้องการ)
   - Sub Category: General (หรือตามที่กำหนด)
   - Cost: PP ที่ต้องจ่าย
   - ATK: 0 (เวทย์ไม่โจมตี)

3. เพิ่ม Effect ใน Effects List:
   - Trigger: OnDeploy (เมื่อใช้เวทย์)
   - Target Type: EnemyHand (มือของฝ่ายตรงข้าม)
   - Action: RevealHandMultiple
   - Value: จำนวนการ์ดที่ต้องการดู (0 หรือไม่ระบุ = ดูทั้งหมด)
   - Target Main Cat: General
   - Target Sub Cat: General
   - Destroy Mode: SelectTarget (ไม่สำคัญสำหรับ RevealHand)

=== ตัวอย่างการ์ดเวทย์ "Peek" ===

Card Name: "Peek"
Card Type: Spell
Cost: 1
ATK: 0
Ability Text: "ดูการ์ด 3 ใบบนมือของฝ่ายตรงข้าม"

Effect:
- Trigger: OnDeploy
- Target Type: EnemyHand
- Action: RevealHandMultiple
- Value: 3
- Target Main Cat: General
- Target Sub Cat: General

=== ตัวอย่างการ์ดเวทย์ "Clairvoyance" ===

Card Name: "Clairvoyance"
Card Type: Spell
Cost: 2
ATK: 0
Ability Text: "ดูการ์ดทั้งหมดบนมือของฝ่ายตรงข้าม"

Effect:
- Trigger: OnDeploy
- Target Type: EnemyHand
- Action: RevealHandMultiple
- Value: 0 (หรือไม่ระบุ = ดูทั้งหมด)
- Target Main Cat: General
- Target Sub Cat: General

=== วิธีการตั้งค่า UI ใน Unity Inspector ===

1. เปิด Scene ของการต่อสู้ (Battle Scene)

2. เลือก BattleManager GameObject

3. ใน Inspector หา section "Hand Reveal Panel (ดูการ์ดบนมือ)"

4. ลาก GameObject ต่อไปนี้ใส่:
   - Hand Reveal Panel: Panel ที่จะแสดงการ์ด (สร้าง UI > Panel)
   - Hand Reveal List Root: Transform ที่จะวางการ์ด (สร้าง Empty GameObject ภายใน Panel)
   - Hand Reveal Title Text: TextMeshProUGUI แสดงชื่อ (เช่น "การ์ดบนมือฝ่ายตรงข้าม")
   - Hand Reveal Close Button: Button สำหรับปิด Panel

5. แนะนำให้ตั้งค่า Panel ดังนี้:
   - เพิ่ม ScrollRect component ให้ Panel (จะถูกตั้งค่าอัตโนมัติ)
   - เพิ่ม Mask component (จะถูกเพิ่มอัตโนมัติ)
   - ตั้ง Background สีดำโปร่งใส (Alpha = 0.9)
   - ตั้งขนาดให้พอเหมาะ (แนะนำ 1200x800)

=== การทดสอบ ===

1. สร้างการ์ดเวทย์ตามตัวอย่างข้างต้น
2. เพิ่มการ์ดนั้นลงในเด็คของคุณ
3. เข้าสู่การต่อสู้
4. ใช้เวทย์นั้น (คลิกหรือลากไปสนาม)
5. Panel จะเปิดขึ้นแสดงการ์ดบนมือฝ่ายตรงข้าม
6. คลิกที่การ์ดเพื่อดูรายละเอียด
7. คลิกปุ่มปิดเพื่อกลับสู่เกม

=== หมายเหตุ ===

- ถ้าฝ่ายตรงข้ามไม่มีการ์ดบนมือ จะแสดงข้อความ "Empty" ใน Log
- ถ้า value = 0 หรือไม่ระบุ จะดูการ์ดทั้งหมดบนมือ
- ถ้า value > 0 จะดูเฉพาะจำนวนที่กำหนด (นับจากซ้ายไปขวา)
- สามารถใช้กับบอทได้เช่นกัน (บอทจะใช้เวทย์ดูมือผู้เล่น)
- การ์ดที่แสดงสามารถคลิกดูรายละเอียดได้ แต่ไม่สามารถลากหรือโต้ตอบอื่นได้
*/
