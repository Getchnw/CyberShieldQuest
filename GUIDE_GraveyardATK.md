# 🔥 วิธีตั้งค่าสกิล GraveyardATK

## สกิล GraveyardATK คืออะไร?
Monster ที่มีสกิลนี้จะได้รับพลังโจมตีเพิ่มขึ้นตามจำนวนการ์ดในสุสาน (Graveyard) ของฝั่งตรงข้าม

**ตัวอย่าง:** 
- Monster มี ATK 3 และสกิล GraveyardATK (value=1)
- ถ้าสุสานศัตรูมี 5 ใบ → ATK จะกลายเป็น 3 + (5 × 1) = **8**

---

## ⚙️ วิธีตั้งค่าใน Unity Inspector

### 1. เปิดการ์ด Monster ที่ต้องการเพิ่มสกิล
ใน Project → Assets → Script → Database → Card → เลือกการ์ด Monster

### 2. ใน Inspector ไปที่ส่วน **Effects**
กด **+** เพื่อเพิ่ม Effect ใหม่

### 3. ตั้งค่า Effect ดังนี้:

```
Trigger:              OnStrike  ⚠️ สำคัญมาก!
Target Type:          Self (หรืออะไรก็ได้)
Action:               GraveyardATK  ⚠️ สำคัญมาก!
Target Main Cat:      General (ไม่สำคัญ)
Target Sub Cat:       General (ไม่สำคัญ)
Value:                1  ⬅️ +1 ATK ต่อ 1 การ์ดในสุสาน
Destroy Mode:         SelectTarget (ไม่สำคัญ)
Token Card Id:        (ว่างไว้)
Bypass Allowed Main:  General (ไม่สำคัญ)
Bypass Allowed Sub:   General (ไม่สำคัญ)
```

---

## ⚠️ สิ่งที่ต้องตรวจสอบ

### ✅ Trigger ต้องเป็น **OnStrike**
- ❌ ถ้าเป็น `Continuous` → ไม่ทำงาน
- ❌ ถ้าเป็น `OnDeploy` → ไม่ทำงาน
- ✅ ถ้าเป็น `OnStrike` → ทำงานถูก

### ✅ Action ต้องเป็น **GraveyardATK**
- เลือกจาก dropdown ใน Inspector

### ✅ Value = จำนวน ATK ที่เพิ่มต่อ 1 การ์ด
- Value = 1 → +1 ATK ต่อ 1 การ์ด
- Value = 2 → +2 ATK ต่อ 1 การ์ด

---

## 🧪 วิธีทดสอบ

1. สร้างการ์ด Monster ที่มี GraveyardATK
2. เข้าเกม Battle
3. วางการ์ดบนสนาม
4. ทำให้การ์ดฝั่งตรงข้ามเข้าสุสาน (ทำลายการ์ดศัตรู)
5. ดูที่มุมซ้ายล่างของการ์ด → **ตัวเลข ATK ควรเพิ่มขึ้นเป็นสีเขียว**

---

## 🐛 Debug: ถ้ายังไม่ทำงาน

### 1. เปิด Console (Ctrl+Shift+C ใน Unity Editor)

### 2. เพิ่มโค้ด Debug ใน GetModifiedATK():

```csharp
public int GetModifiedATK(bool isPlayerAttack = true)
{
    if (_cardData == null) return 0;
    int baseATK = _cardData.atk;

    var graveyardEffect = _cardData.effects.FirstOrDefault(e => e.trigger == EffectTrigger.OnStrike && e.action == ActionType.GraveyardATK);
    
    // 🔥 เพิ่ม Debug Log
    Debug.Log($"[{_cardData.cardName}] Checking GraveyardATK:");
    Debug.Log($"  - Has effect? {graveyardEffect.action == ActionType.GraveyardATK}");
    Debug.Log($"  - Trigger: {graveyardEffect.trigger}");
    Debug.Log($"  - Action: {graveyardEffect.action}");
    Debug.Log($"  - Value: {graveyardEffect.value}");
    
    if (graveyardEffect.action == ActionType.GraveyardATK)
    {
        int graveCount = 0;
        
        if (BattleManager.Instance != null)
        {
            if (isPlayerAttack)
            {
                graveCount = BattleManager.Instance.GetEnemyGraveyardCount();
            }
            else
            {
                graveCount = BattleManager.Instance.GetPlayerGraveyardCount();
            }
        }
        
        Debug.Log($"  - Graveyard Count: {graveCount}");
        int extraATK = graveCount * graveyardEffect.value;
        Debug.Log($"  - Extra ATK: {extraATK}");
        Debug.Log($"  - Total ATK: {baseATK + extraATK}");
        
        return baseATK + extraATK;
    }

    return baseATK;
}
```

### 3. ดู Console log ตอนโจมตี
- ถ้า "Has effect?" = false → ตรวจสอบว่าตั้งค่า Effect ถูกต้องหรือไม่
- ถ้า "Graveyard Count" = 0 → ยังไม่มีการ์ดในสุสาน
- ถ้า "Extra ATK" = 0 → ตรวจสอบ Value

---

## 📝 สรุป

**สำหรับการ์ด Monster ที่ต้องการสกิล GraveyardATK:**

```
✅ Trigger:  OnStrike
✅ Action:   GraveyardATK  
✅ Value:    1 (หรือค่าที่ต้องการ)
```

**ตัวอย่าง Monster:**
- **Necromancer** (ATK 2, GraveyardATK value=1)
  - สุสานศัตรู 0 ใบ → ATK = 2
  - สุสานศัตรู 3 ใบ → ATK = 5
  - สุสานศัตรู 10 ใบ → ATK = 12

**UI จะแสดง:**
- มุมซ้ายล่าง = ATK ปัจจุบัน (สีเขียวถ้าเพิ่ม)
- มุมขวาบน = Cost
