using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CardGenerator
{
    // กำหนดโฟลเดอร์ที่จะเซฟไฟล์การ์ด
    static string path = "Assets/Resources/GameContent/Cards";

    [MenuItem("Tools/Generate All Cards")]
    public static void CreateAllCards()
    {
        // สร้างโฟลเดอร์ถ้ายังไม่มี
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Debug.Log("Start Generating Cards...");

        // =================================================================================
        // 🛡️ A01: Broken Access Control
        // =================================================================================
        
        // --- 1.1 IDOR --- [cite: 347-396]
        CreateCard("M_A01_01", "Data Snoop", CardType.Monster, MainCategory.A01, SubCategory.IDOR, 2, 2, 
            "[Deploy] เมื่อการ์ดใบนี้ลงสนาม: คุณได้ดูการ์ดในมือของฝ่ายตรงข้ามทั้งหมด",
            "[A01: IDOR] (Read) มันเปลี่ยน ID ใน URL (เช่น user_id=123 เป็น 124) เพื่อ 'แอบดู' ข้อมูลคนอื่น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyHand, action = ActionType.RevealHandMultiple, value = 0 } 
            });

        CreateCard("M_A01_02", "Profile Defacer", CardType.Monster, MainCategory.A01, SubCategory.IDOR, 3, 3,
            "[Strike-Hit] เมื่อการ์ดใบนี้โจมตี HP ของฝ่ายตรงข้ามสำเร็จ: เลือกทำลาย Equip Spell 1 ใบของฝ่ายตรงข้าม",
            "[A01: IDOR] (Write) มันเปลี่ยน ID ที่ซ่อนในฟอร์มเพื่อ 'แก้ไข' หรือ 'ลบ' ข้อมูลของคนอื่น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyEquip, action = ActionType.Destroy, value = 1 } 
            });

        CreateCard("M_A01_03", "Account Hijacker", CardType.Monster, MainCategory.A01, SubCategory.IDOR, 5, 4,
            "[Strike-Hit] เมื่อการ์ดใบนี้โจมตี HP ของฝ่ายตรงข้ามสำเร็จ: คุณได้รับการควบคุม Equip Spell 1 ใบของฝ่ายตรงข้าม",
            "[A01: IDOR] (Act) มันสวมรอย ID ใน API call เพื่อ 'สั่งการ' ในนามของคนอื่น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyEquip, action = ActionType.ControlEquip } 
            });

        CreateCard("M_A01_04", "Unauthorized Creator", CardType.Monster, MainCategory.A01, SubCategory.IDOR, 3, 2,
            "[Deploy] อัญเชิญ 'Rogue Token' 1 ตัว ลงในสนามของคุณ",
            "[A01: IDOR] (Create) มันไม่ขโมย, มัน 'สร้าง'! มันแทรกแซงฟังก์ชันเพื่อ 'สร้าง User ผี'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.Self, action = ActionType.SummonToken, value = 1 } 
            });

        CreateCard("M_A01_05", "Blind Executor", CardType.Monster, MainCategory.A01, SubCategory.IDOR, 2, 2,
            "[Strike-Hit] เมื่อการ์ดใบนี้โจมตี HP ของฝ่ายตรงข้ามสำเร็จ: ฝ่ายตรงข้ามต้องทิ้งการ์ดใบบนสุดของเด็ค 1 ใบ",
            "[A01: IDOR] (Blind) มือสังหารในความมืด มันสั่งการโดย 'ไม่เห็น' ผลลัพธ์",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck, value = 1 } 
            });

        CreateCard("E_A01_01", "GUID Cloak", CardType.EquipSpell, MainCategory.A01, SubCategory.IDOR, 2, 0,
            "[Cont.] Monster ประเภท [A01: IDOR] ของฝ่ายตรงข้ามที่มี Cost 3 หรือน้อยกว่า ไม่สามารถโจมตีได้",
            "[A01: IDOR] เกราะพรางตา! มันเปลี่ยน ID ที่เดาง่ายให้เป็นสายอักขระแบบสุ่ม",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.IDOR, action = ActionType.DisableAttack, value = 3 } 
            });

        CreateCard("E_A01_02", "Session Guardian", CardType.EquipSpell, MainCategory.A01, SubCategory.IDOR, 4, 0,
            "[Cont.] ฝ่ายตรงข้ามไม่สามารถดูการ์ดในมือของคุณได้ และ Monster [A01: IDOR] ของฝ่ายตรงข้ามไม่สามารถใช้งาน Ability",
            "[A01: IDOR] ผู้พิทักษ์ที่ตรวจสอบว่า 'ID ของ User ที่ล็อกอิน ตรงกับ ID ของข้อมูลที่ขอหรือไม่?'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyPlayer, action = ActionType.DisableAbility }, // Prevent ViewHand
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.IDOR, action = ActionType.DisableAbility }
            });

        CreateCard("S_A01_01", "ID Enumerator", CardType.Spell, MainCategory.A01, SubCategory.IDOR, 2, 0,
            "เลือก Monster [A01: IDOR] ของฝ่ายตรงข้าม 1 ใบ ทำลายการ์ดใบนั้น",
            "[A01: IDOR] สคริปต์ที่วิ่งไล่หา ID ที่ถูกต้องอย่างรวดเร็ว",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.IDOR, action = ActionType.Destroy } 
            });

        CreateCard("S_A01_02", "Access Denied", CardType.Spell, MainCategory.A01, SubCategory.IDOR, 2, 0,
            "เลือก Monster [A01] ของฝ่ายตรงข้าม 1 ใบ ทำลายการ์ดใบนั้น ฟื้นฟู HP ตามพลังโจมตี",
            "[A01: Access] การปฏิเสธในวินาทีสุดท้าย! ระบบตรวจสอบพบความผิดปกติและตัดการเชื่อมต่อทันที",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A01, action = ActionType.Destroy },
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.Self, action = ActionType.HealHP }
            });

        // 🔍 การ์ดเวทย์ดูมือ
        CreateCard("S_A01_03", "Peek", CardType.Spell, MainCategory.A01, SubCategory.IDOR, 1, 0,
            "ดูการ์ด 3 ใบบนมือของฝ่ายตรงข้าม",
            "[A01: IDOR] มองลับฟ้า! มันดักจับข้อมูลที่ส่งกลับมา",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyHand, action = ActionType.RevealHandMultiple, value = 3 }
            });

        CreateCard("S_A01_04", "Clairvoyance", CardType.Spell, MainCategory.A01, SubCategory.IDOR, 2, 0,
            "ดูการ์ดทั้งหมดบนมือของฝ่ายตรงข้าม",
            "[A01: IDOR] ทะลุทะลวง! มันดึงข้อมูลส่วนตัวจากฐานข้อมูลมาดูทั้งหมด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyHand, action = ActionType.RevealHandMultiple, value = 0 }
            });

        // --- 1.2 Path Traversal --- [cite: 397-455]
        CreateCard("M_A01_06", "Dot-Dot-Slash Sneak", CardType.Monster, MainCategory.A01, SubCategory.PathTraversal, 2, 2,
            "[Deploy] เลือกทำลาย Equip Spell 1 ใบของฝ่ายตรงข้ามที่มี Cost 2 หรือน้อยกว่า",
            "[A01: Path Traversal] นักย่องเบาขั้นพื้นฐาน มันใช้ ../ เพื่อย้อนกลับ",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.Destroy, value = 2 } 
            });

        CreateCard("M_A01_07", "Encoded Infiltrator", CardType.Monster, MainCategory.A01, SubCategory.PathTraversal, 3, 3,
            "[Deploy] เลือก Equip Spell 1 ใบของฝ่ายตรงข้าม Equip Spell ใบนั้นไม่สามารถใช้ Intercept ได้ในเทิร์นนี้",
            "[A01: Path Traversal] มันเข้ารหัส (%2f) เพื่อหลบเลี่ยงฟิลเตอร์ที่ตื้นเขิน",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.DisableAbility } 
            });

        CreateCard("M_A01_08", "Null-Byte Assassin", CardType.Monster, MainCategory.A01, SubCategory.PathTraversal, 5, 3,
            "[Strike-Hit] เลือกทำลาย Monster บนสนามฝ่ายตรงข้าม 1 ใบ",
            "[A01: Path Traversal] นักฆ่าที่ใช้ NULL (%00) เพื่อ 'ตัดจบ' ชื่อไฟล์",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyMonster, action = ActionType.Destroy } 
            });

        CreateCard("M_A01_09", "Absolute Path Invoker", CardType.Monster, MainCategory.A01, SubCategory.PathTraversal, 5, 2,
            "[Strike] บังคับ Equip Spell 1 ใบของฝ่ายตรงข้ามให้ Intercept การโจมตีนี้",
            "[A01: Path Traversal] มันไม่ 'ไต่กลับ' (../) แต่จะ 'ระบุเป้าหมายโดยตรง' โดยใช้ Path แบบเต็ม",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyEquip, action = ActionType.ForceIntercept } 
            });

        CreateCard("M_A01_10", "Nested Encoder", CardType.Monster, MainCategory.A01, SubCategory.PathTraversal, 5, 3,
            "[Deploy] Equip Spell ทั้งหมดของฝ่ายตรงข้าม สูญเสีย Type ของตัวเองไปจนจบเทิร์น",
            "[A01: Path Traversal] นักแปลงกายขั้นสูง มัน 'เข้ารหัสซ้อน' 2 ชั้น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.DisableAbility } 
            });

        CreateCard("E_A01_03", "Root Jail", CardType.EquipSpell, MainCategory.A01, SubCategory.PathTraversal, 4, 0,
            "[Cont.] Monster ประเภท [A01: Path Traversal] ของฝ่ายตรงข้ามทั้งหมดในสนาม ไม่สามารถใช้งาน Ability ได้",
            "[A01: Path Traversal] คุกเสมือนที่กักขังโปรเซสไว้ในไดเรกทอรีที่กำหนด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.PathTraversal, action = ActionType.DisableAbility } 
            });

        CreateCard("E_A01_04", "Canonicalizer Shield", CardType.EquipSpell, MainCategory.A01, SubCategory.PathTraversal, 2, 0,
            "[Cont.] Monster Encoded Infiltrator ของฝ่ายตรงข้าม ไม่สามารถโจมตีและใช้งาน Ability ของตัวเองได้",
            "[A01: Path Traversal] โล่ห์ที่จะ 'ถอดรหัส' (Decode) ข้อมูลที่เข้ารหัส 1 ชั้น",
            new List<CardEffect> { 
                // ตัด stringValue ออก และใช้ DisableAttack ทั่วไปแทน (ต้องไปเขียน Logic เพิ่มเองถ้าระบุชื่อการ์ด)
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, action = ActionType.DisableAttack } 
            });

        CreateCard("E_A01_05", "Secure Path Validator", CardType.EquipSpell, MainCategory.A01, SubCategory.PathTraversal, 5, 0,
            "[Intercept] หากการ์ดใบนี้ Intercept ถูกประเภท ฟื้นฟู HP ตามพลังโจมตีของมอนสเตอร์ตัวนั้น",
            "[A01: Path Traversal] เครื่องถอดรหัสขั้นสุด! มันจะถอดรหัสวนซ้ำๆ จนกว่าข้อมูลจะ 'เกลี้ยง'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnIntercept, targetType = TargetType.Self, action = ActionType.HealHP } 
            });

        CreateCard("E_A01_06", "Recursive Normalizer", CardType.EquipSpell, MainCategory.A01, SubCategory.PathTraversal, 6, 0,
            "[Cont.] ถือว่า 'ถูกประเภท' เสมอเมื่อใช้ Intercept และ Monster [A01: Path Traversal] ไม่สามารถใช้งาน Ability ได้",
            "[A01: Path Traversal] เครื่องถอดรหัสขั้นสุด! ไม่เหลืออะไรให้ถอดรหัสอีก",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.BypassIntercept },
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.PathTraversal, action = ActionType.DisableAbility }
            });

        CreateCard("S_A01_03", "Simple Filter", CardType.Spell, MainCategory.A01, SubCategory.PathTraversal, 2, 0,
            "เลือกทำลาย Monster Path Traversal 1 ใบ",
            "[A01: Path Traversal] การป้องกันราคาถูกที่มองหาแค่ ../ แบบตรงๆ",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.PathTraversal, action = ActionType.Destroy } 
            });

        CreateCard("S_A01_04", "Decoy File Trap", CardType.Spell, MainCategory.A01, SubCategory.PathTraversal, 2, 0,
            "เลือก Monster Type A01 Set Cost เป็น 0 พลังโจมตี เป็น 0",
            "[A01: Path Traversal] ไฟล์ข้อมูลปลอมที่วางไว้เพื่อล่อแฮ็กเกอร์",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A01, action = ActionType.ModifyStat, value = 0 } 
            });

        // --- 1.3 MFLAC --- [cite: 456-485]
        CreateCard("M_A01_11", "Directory Brute-Forcer", CardType.Monster, MainCategory.A01, SubCategory.MFLAC, 3, 3,
            "[Deploy] ดูการ์ด 3 ใบบนสุดของเด็คฝ่ายตรงข้าม เลือก Equip Spell 1 ใบ ส่งลงสุสาน",
            "[A01: MFLAC] มันเดาสุ่ม URL ที่ถูกซ่อนไว้ จนกว่าจะเจอประตูที่ลืมล็อก",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyDeck, action = ActionType.Destroy } 
            });

        CreateCard("M_A01_12", "Admin Gate-Crasher", CardType.Monster, MainCategory.A01, SubCategory.MFLAC, 5, 2,
            "[Cont.] การ์ดใบนี้สามารถโจมตีได้ 2 ครั้งต่อ 1 เทิร์น",
            "[A01: MFLAC] เมื่อมันพบประตู /admin ที่ไร้การป้องกัน มันจะยึดครองสิทธิ์ทั้งหมด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.ModifyStat, value = 2 } // 2 Attacks
            });

        CreateCard("E_A01_07", "Warden of Roles", CardType.EquipSpell, MainCategory.A01, SubCategory.MFLAC, 3, 0,
            "[Cont.] Monster ของฝ่ายตรงข้ามที่มี Cost 4 หรือน้อยกว่า ไม่สามารถใช้งาน Ability ประเภท 'เมื่อลงสนาม' ได้",
            "[A01: MFLAC] ผู้คุมกฎที่จะตรวจสอบ 'บทบาท' ของผู้ใช้ในทุกฟังก์ชัน",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, action = ActionType.DisableAbility, value = 4 } 
            });

        CreateCard("E_A01_08", "Privilege Checkpoint", CardType.EquipSpell, MainCategory.A01, SubCategory.MFLAC, 6, 0,
            "[Cont.] ถือว่า 'ถูกประเภท' เสมอเมื่อใช้ Intercept และ Monster [A01] ทั้งหมด ไม่สามารถใช้งาน Ability ได้",
            "[A01: MFLAC] ด่านตรวจสอบสิทธิ์อันเข้มงวด! ใบนี้คือการ์ดป้องกัน A01 ที่ดีที่สุด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.BypassIntercept },
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A01, action = ActionType.DisableAbility } 
            });

        CreateCard("S_A01_05", "Deny by Default Policy", CardType.Spell, MainCategory.A01, SubCategory.MFLAC, 4, 0,
            "ทำลาย Monster ประเภท A01 ของฝ่ายตรงข้ามทั้งหมด",
            "[A01: MFLAC] นโยบายความปลอดภัยพื้นฐานที่สุด: 'ปฏิเสธ' (Deny) ทั้งหมด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.AllGlobal, targetMainCat = MainCategory.A01, action = ActionType.Destroy } 
            });


        // =================================================================================
        // 🪙 A02: Cryptographic Failures
        // =================================================================================

       // --- 2.1 Insecure Data in Transit --- [cite: 486-514]
        CreateCard("M_A02_01", "HTTP Sniffer", CardType.Monster, MainCategory.A02, SubCategory.InsecureTransit, 3, 1,
            "[Cont.] เมื่อใดก็ตามที่ฝ่ายตรงข้ามจั่วการ์ด คุณได้ดูการ์ดใบนั้น",
            "[A02: Insecure Transit] นักดักฟังบนเครือข่าย http:// มันขโมยข้อมูลที่ถูกส่งไป",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyHand, action = ActionType.RevealHand } 
            });

        CreateCard("M_A02_02", "SSL Stripper", CardType.Monster, MainCategory.A02, SubCategory.InsecureTransit, 3, 3,
            "[Deploy] เลือก Equip Spell 1 ใบของฝ่ายตรงข้าม การ์ดใบนั้นสูญเสีย 'ประเภท' ไปจนจบเทิร์นถัดไป",
            "[A02: Insecure Transit] อสูรเจ้าเล่ห์ มันบังคับให้เบราว์เซอร์เปลี่ยนจาก https:// ไปใช้ http://",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.DisableAbility } 
            });

        CreateCard("M_A02_03", "Weak Cipher Negotiator", CardType.Monster, MainCategory.A02, SubCategory.InsecureTransit, 5, 3,
            "[Strike] หากฝ่ายตรงข้ามใช้ Equip Intercept ส่งการ์ด 2 ใบบนสุดของเด็คฝ่ายตรงข้ามลงสุสาน",
            "[A02: Insecure Transit] มันโจมตี https:// ที่ตั้งค่าผิดพลาด โดยบังคับให้ใช้การเข้ารหัสรุ่นเก่า",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck, value = 2 } 
            });

        CreateCard("E_A02_01", "TLS Encryption Tunnel", CardType.EquipSpell, MainCategory.A02, SubCategory.InsecureTransit, 2, 0,
            "[Cont.] ฝ่ายตรงข้ามไม่สามารถดูการ์ดที่คุณจั่วได้",
            "[A02: Insecure Transit] 'ตู้นิรภัยเคลื่อนที่' ที่จะสร้างอุโมงค์ปลอดภัยห่อหุ้มข้อมูล",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyPlayer, action = ActionType.DisableAbility } 
            });

        CreateCard("E_A02_02", "HSTS Protocol", CardType.EquipSpell, MainCategory.A02, SubCategory.InsecureTransit, 4, 0,
            "[Cont.] การ์ดใบนี้ถือว่า 'ถูกประเภท' เสมอเมื่อใช้ Intercept",
            "[A02: Insecure Transit] นโยบาย 'บังคับ' ที่สั่งเบราว์เซอร์ว่า 'ห้ามติดต่อด้วย http://'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.BypassIntercept } 
            });

        // --- 2.2 Insecure Data at Rest --- [cite: 515-533]
        CreateCard("M_A02_04", "Database Peeker", CardType.Monster, MainCategory.A02, SubCategory.InsecureRest, 2, 2,
            "[Deploy] คุณได้ดูการ์ดในมือของฝ่ายตรงข้ามทั้งหมด",
            "[A02: Insecure Rest] สายลับแห่งฐานข้อมูลที่ถูกเจาะ มันจะอ่านข้อมูล Plain Text ทั้งหมด",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyHand, action = ActionType.RevealHand } 
            });

        CreateCard("E_A02_03", "Secure Hash Protocol", CardType.EquipSpell, MainCategory.A02, SubCategory.InsecureRest, 2, 0,
            "[Cont.] ฝ่ายตรงข้ามไม่สามารถดูการ์ดในมือของคุณได้",
            "[A02: Insecure Rest] 'เตาหลอมรหัส' ที่จะบดขยี้รหัสผ่านให้กลายเป็น Hash",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyPlayer, action = ActionType.DisableAbility } 
            });

        CreateCard("S_A02_01", "Database Dump", CardType.Spell, MainCategory.A02, SubCategory.InsecureRest, 2, 0,
            "ดูการ์ด 5 ใบบนสุดของเด็คฝ่ายตรงข้าม เลือก 1 ใบส่งลงสุสาน",
            "[A02: Insecure Rest] การโจมตีที่สำเร็จ! คัดลอกข้อมูลสำคัญทั้งหมดออกมา",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck } 
            });

        // --- 2.3 Weak Hash --- [cite: 534-556]
        CreateCard("M_A02_05", "MD5 Brute-Forcer", CardType.Monster, MainCategory.A02, SubCategory.WeakHash, 3, 3,
            "[Deploy] การ์ดใบนี้สามารถโจมตีได้ในเทิร์นที่ลงสนาม",
            "[A02: Weak Hash] เครื่องจักรทำลายล้างที่ออกแบบมาเพื่อ Brute-force MD5",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.Self, action = ActionType.ModifyStat } 
            });

        CreateCard("M_A02_06", "Legacy Cracker", CardType.Monster, MainCategory.A02, SubCategory.WeakHash, 3, 3,
            "[Deploy] เลือกทำลาย Equip Spell 1 ใบของฝ่ายตรงข้ามที่มี Cost 3 หรือน้อยกว่า",
            "[A02: Weak Hash] ผู้เชี่ยวชาญการทำลาย 'แม่กุญแจสนิม' มันถอดรหัส Hash ที่ตกรุ่น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.Destroy, value = 3 } 
            });

        CreateCard("M_A02_07", "SHA1 Collision Master", CardType.Monster, MainCategory.A02, SubCategory.WeakHash, 5, 3,
            "[Deploy] ทำลาย Equip Spell 1 ใบของฝ่ายตรงข้าม ที่ไม่ใช่ A02",
            "[A02: Weak Hash] เชี่ยวชาญการ 'หาจุดชน' (Collision) เพื่อปลอมแปลงข้อมูล",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyEquip, action = ActionType.Destroy } 
            });

        CreateCard("E_A02_04", "Modern Hash Standard", CardType.EquipSpell, MainCategory.A02, SubCategory.WeakHash, 5, 0,
            "[Cont.] Monster [A02] ของฝ่ายตรงข้ามไม่สามารถใช้ Ability ได้ และฟื้นฟู HP 1 หน่วยเมื่อจบเทิร์น",
            "[A02: Weak Hash] มาตรฐานสูงสุด (bcrypt, Argon2) ที่ถูกออกแบบมาให้ 'ช้า' เพื่อต้านทานการโจมตี",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A02, action = ActionType.DisableAbility } 
            });

       // --- 2.4 Missing Salt --- [cite: 557-574]
        CreateCard("M_A02_08", "Rainbow Table Fiend", CardType.Monster, MainCategory.A02, SubCategory.NoSalt, 4, 2,
            "[Cont.] ได้รับ +1 Attack ต่อการ์ดทุก 2 ใบในสุสานของฝ่ายตรงข้าม",
            "[A02: No Salt] พจนานุกรมแฮชขนาดมหึมาที่ใช้โจมตี Hash ที่ไม่ 'โรยเกลือ'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.ModifyStat } 
            });

        CreateCard("E_A02_05", "Salting Field", CardType.EquipSpell, MainCategory.A02, SubCategory.NoSalt, 2, 0,
            "[Cont.] Monster [A02: No Salt] ของฝ่ายตรงข้ามสูญเสีย Ability",
            "[A02: No Salt] การ 'โรยเกลือ' ลงบนรหัสผ่านก่อนแฮช",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.NoSalt, action = ActionType.DisableAbility } 
            });
        
        CreateCard("E_A02_08", "The Pepper Vault", CardType.EquipSpell, MainCategory.A02, SubCategory.NoSalt, 4, 0,
            "[Cont.] หากนำการ์ดใบนี้ไปรับการโจมตีของ Monster [A02: No Salt] ฟื้นฟู HP เท่ากับพลังโจมตีของมอนสเตอร์ที่โจมตี",
            "[A02: No Salt] 'Pepper' คือความลับ (Secret Key) ที่จะถูกเพิ่มเข้าไปก่อนการ Hashing และเก็บไว้ในที่ปลอดภัยแยกต่างหาก",
            new List<CardEffect> { 
                new CardEffect { 
                    trigger = EffectTrigger.OnIntercept,    // ทำงานเมื่อใช้ Intercept (รับการโจมตี)
                    targetType = TargetType.Self,           // เป้าหมายคือเรา (เพื่อฮีล)
                    targetSubCat = SubCategory.NoSalt,      // เงื่อนไข: คนตีต้องเป็น No Salt
                    action = ActionType.HealHP              // ผลลัพธ์: ฮีล HP (ระบบเกมจะคำนวณตาม Atk ผู้ตีให้เอง)
                } 
            });
        // --- 2.5 Bad Key Mgmt --- [cite: 575-613]
        CreateCard("M_A02_09", "ECB Mode Cyclops", CardType.Monster, MainCategory.A02, SubCategory.BadKey, 5, 2,
            "[Strike] บังคับ Equip Spell 1 ใบของฝ่ายตรงข้ามให้ Intercept การโจมตีนี้",
            "[A02: Bad Crypto] มันโจมตีโหมด 'ECB' ที่อ่อนแอ ทำให้มัน 'มองเห็น' รูปแบบของข้อมูล",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyEquip, action = ActionType.ForceIntercept } 
            });

        CreateCard("M_A02_10", "Hardcode Hunter", CardType.Monster, MainCategory.A02, SubCategory.BadKey, 6, 3,
            "[Deploy] ดูเด็คของฝ่ายตรงข้าม เลือกการ์ด 2 ใบ ส่งลงสุสาน",
            "[A02: Bad Key] นักล่ากุญแจที่ค้นหา 'Key' ที่โปรแกรมเมอร์เผลอฝังไว้",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck, value = 2 } 
            });

        CreateCard("M_A02_11", "Config File Raider", CardType.Monster, MainCategory.A02, SubCategory.BadKey, 6, 4,
            "[Strike-Hit] ทำลาย Equip Spell ทั้งหมดของฝ่ายตรงข้าม",
            "[A02: Bad Key] มุ่งไปที่ไฟล์ตั้งค่าบนเซิร์ฟเวอร์ที่เก็บกุญแจไว้เป็น Plain Text",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyEquip, action = ActionType.Destroy } 
            });

        CreateCard("E_A02_06", "AES-GCM Guardian", CardType.EquipSpell, MainCategory.A02, SubCategory.BadKey, 3, 0,
            "[Cont.] ป้องกันการถูกบังคับ Intercept",
            "[A02: Bad Crypto] การใช้การเข้ารหัสสมัยใหม่ (AES-GCM) ป้องกันการมองเห็นแพทเทิร์น",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyPlayer, action = ActionType.DisableAbility } 
            });

        CreateCard("E_A02_07", "Secure Key Vault", CardType.EquipSpell, MainCategory.A02, SubCategory.BadKey, 3, 0,
            "[Cont.] Equip Spell ใบอื่นของคุณ ไม่สามารถถูกทำลายโดย Ability ได้",
            "[A02: Bad Key] การป้องกันขั้นสูงสุด (KMS, Vault) แยก 'กุญแจ' ออกจาก 'ข้อมูล'",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.DisableAbility } 
            });

        CreateCard("S_A02_02", "Emergency Key Rotation", CardType.Spell, MainCategory.A02, SubCategory.BadKey, 2, 0,
            "เลือก Equip Spell 1 ใบในสุสานของคุณ นำกลับขึ้นมือ",
            "[A02: Bad Key] เมื่อกุญแจเก่าถูกบุกรุก ก็ถึงเวลา 'เปลี่ยนกุญแจ' ใหม่ทันที",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.Self, action = ActionType.ReturnEquipFromGraveyard }
            });

        CreateCard("S_A02_03", "Cryptoanalysis", CardType.Spell, MainCategory.A02, SubCategory.General, 4, 0,
            "ทำลาย Monster [A02] ทั้งหมดบนสนามของฝ่ายตรงข้าม",
            "[A02: Crypto] การวิเคราะห์การเข้ารหัสของศัตรูอย่างหนักหน่วงเพื่อค้นหาจุดอ่อน",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A02, action = ActionType.Destroy } 
            });

        
        // =================================================================================
        // 💉 A03: Injection
        // =================================================================================

        // --- 3.1 SQLi --- [cite: 615-632]
        CreateCard("M_A03_01", "Query String Manipulator", CardType.Monster, MainCategory.A03, SubCategory.SQLi, 3, 2,
            "[Strike] Equip Spell ที่ไม่ใช่ [A03] ที่มี Cost 3 หรือน้อยกว่า ไม่สามารถ Intercept ได้",
            "[A03: SQLi] นักแทรกแซงขั้นพื้นฐาน มันใช้เทคนิค ' OR '1'='1",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyEquip, action = ActionType.DisableAbility, value = 3 } 
            });

        CreateCard("M_A03_02", "Database Devourer", CardType.Monster, MainCategory.A03, SubCategory.SQLi, 7, 4,
            "[Strike-Hit] ส่งการ์ด 3 ใบบนสุดเด็คฝ่ายตรงข้ามลงสุสาน",
            "[A03: SQLi] อสูรแห่งการ UNION SELECT มันสามารถ 'ดูด' ข้อมูลทั้งฐานข้อมูลออกมาได้",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck, value = 3 } 
            });

        CreateCard("E_A03_01", "Parameterized Query Guard", CardType.EquipSpell, MainCategory.A03, SubCategory.SQLi, 2, 0,
            "[Cont.] Monster [A03: SQLi] ทั้งหมด ไม่สามารถใช้งาน Ability ได้",
            "[A03: SQLi] การป้องกันที่สมบูรณ์แบบ มันจะ 'แยก' คำสั่งออกจากข้อมูลเสมอ",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.SQLi, action = ActionType.DisableAbility } 
            });

        // --- 3.2 XSS --- [cite: 633-650]
        CreateCard("M_A03_03", "Reflected Script-Kiddie", CardType.Monster, MainCategory.A03, SubCategory.XSS, 2, 2,
            "[Deploy] ฝ่ายตรงข้ามเลือกทิ้งการ์ดบนมือ 1 ใบ",
            "[A03: XSS] การโจมตีแบบไม่ถาวร หลอกให้ผู้ใช้คลิกลิงก์ที่มีสคริปต์อันตราย",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyHand, action = ActionType.DiscardDeck, value = 1 } 
            });

        CreateCard("M_A03_04", "Stored Script Worm", CardType.Monster, MainCategory.A03, SubCategory.XSS, 4, 3,
            "[Strike-Hit] ฝ่ายตรงข้ามเลือกทิ้งการ์ดบนมือ 1 ใบ",
            "[A03: XSS] หนอน XSS แบบถาวร ฝังตัวเองในฐานข้อมูล",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrikeHit, targetType = TargetType.EnemyHand, action = ActionType.DiscardDeck, value = 1 } 
            });

        CreateCard("E_A03_02", "Content Security Policy", CardType.EquipSpell, MainCategory.A03, SubCategory.XSS, 4, 0,
            "[Cont.] เมื่อ Monster [A03: XSS] ลงสนาม ฟื้นฟู HP 1 หน่วย",
            "[A03: XSS] นโยบายป้องกันขั้นสูง Whitelist แหล่งที่มาของสคริปต์",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.Self, action = ActionType.HealHP } 
            });

        // --- 3.3 OS Command --- [cite: 651-668]
        CreateCard("M_A03_05", "Ping Abuser", CardType.Monster, MainCategory.A03, SubCategory.OSCommand, 3, 2,
            "[Strike] การโจมตีไม่สามารถถูก Intercept โดย Equip Cost 3 หรือน้อยกว่าได้",
            "[A03: OS Command] แทรกคำสั่งอันตรายต่อท้ายฟังก์ชันที่ไม่อันตราย",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyEquip, action = ActionType.BypassIntercept, value = 3 } 
            });

        CreateCard("M_A03_06", "Root Shell Dragon", CardType.Monster, MainCategory.A03, SubCategory.OSCommand, 7, 5,
            "[Strike] การโจมตีไม่สามารถถูก Intercept ได้",
            "[A03: OS Command] 'Reverse Shell' ที่สมบูรณ์แบบ ยึดครองเซิร์ฟเวอร์ได้ทั้งระบบ",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnStrike, targetType = TargetType.EnemyEquip, action = ActionType.BypassIntercept } 
            });

        CreateCard("E_A03_03", "Input Sanitizer", CardType.EquipSpell, MainCategory.A03, SubCategory.OSCommand, 3, 0,
            "[Intercept] เลือกทำลาย Monster [A03: OS Command] 1 ใบ",
            "[A03: OS Command] เกราะป้องกันที่กรองสัญลักษณ์อันตรายออกจาก Input",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnIntercept, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.OSCommand, action = ActionType.Destroy } 
            });

        // --- 3.4 XXE --- [cite: 669-681]
        CreateCard("M_A03_07", "Local File Reader", CardType.Monster, MainCategory.A03, SubCategory.XXE, 3, 3,
            "[Deploy] ดูการ์ด 3 ใบบนสุดของเด็คฝ่ายตรงข้าม เลือก 1 ใบส่งลงสุสาน",
            "[A03: XXE] มันหลอกให้ XML parser อ่านไฟล์สำคัญในระบบ",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyDeck, action = ActionType.DiscardDeck, value = 1 } 
            });

        CreateCard("E_A03_04", "XML Parser Hardening", CardType.EquipSpell, MainCategory.A03, SubCategory.XXE, 2, 0,
            "[Cont.] Monster [A03: XXE] ทั้งหมด ไม่สามารถใช้งาน Ability ได้",
            "[A03: XXE] การตั้งค่า 'ปิด' การประมวลผล DOCTYPE และ External Entities",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.Continuous, targetType = TargetType.EnemyMonster, targetSubCat = SubCategory.XXE, action = ActionType.DisableAbility } 
            });

        CreateCard("S_A03_01", "Payload Obfuscator", CardType.Spell, MainCategory.A03, SubCategory.General, 5, 0,
            "เลือก Monster 1 ตัว ในเทิร์นนี้ฝ่ายตรงข้ามไม่สามารถ Intercept การโจมตีได้",
            "[A03: Injection] การ 'อำพราง' โค้ดโจมตีเพื่อหลอก WAF",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.Self, action = ActionType.BypassIntercept } 
            });

        CreateCard("S_A03_02", "Web Application Firewall", CardType.Spell, MainCategory.A03, SubCategory.General, 2, 0,
            "เลือก Monster [A03] 1 ตัว ทำลายการ์ดใบนั้น",
            "[A03: Injection] ไฟร์วอลล์ที่คอยดักจับ 'รูปแบบ' การโจมตีที่รู้จัก",
            new List<CardEffect> { 
                new CardEffect { trigger = EffectTrigger.OnDeploy, targetType = TargetType.EnemyMonster, targetMainCat = MainCategory.A03, action = ActionType.Destroy } 
            });

        // --- Tokens ---
        CreateCard("T_001", "Rogue Token", CardType.Token, MainCategory.A01, SubCategory.IDOR, 1, 1, 
            "", "Token ที่ถูกสร้างโดย Unauthorized Creator", null);

        AssetDatabase.SaveAssets();
        Debug.Log("Finished! All cards generated in Assets/GameData/Cards");
    }

    // ฟังก์ชันสร้างการ์ดที่ปรับแก้ให้ตรงกับ Class CardData ใหม่ของคุณ
    static void CreateCard(string id, string name, CardType type, MainCategory main, SubCategory sub, int cost, int atk, string ability, string flavor, List<CardEffect> effects)
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        
        card.card_id = id;
        card.cardName = name;
        card.type = type;
        card.mainCategory = main;
        card.subCategory = sub;
        card.cost = cost;
        card.atk = atk;
        
        card.abilityText = ability;
        card.flavorText = flavor;
        if(effects != null) card.effects = effects;
        else card.effects = new List<CardEffect>();

        // -------------------------------------------------------------
        // 🔥 เพิ่มส่วนนี้: ค้นหารูปภาพตามชื่อการ์ด
        // -------------------------------------------------------------
        // สมมติว่ารูปเก็บอยู่ที่ Assets/Resources/GameContent/Art
        string imagePath = $"Assets/Resources/GameContent/cardpic/{name}.png"; 
        // หรือถ้าเป็น jpg ให้แก้เป็น .jpg
        
        Sprite foundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
        
        if (foundSprite != null) {
            card.artwork = foundSprite;
        } else {
            // ลองหาแบบ jpg เผื่อไว้
            imagePath = $"Assets/Resources/GameContent/cardpic/{name}.jpg";
            card.artwork = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            Debug.LogWarning($" หารูปไม่เจอสำหรับการ์ด: '{name}' (ลองเช็คชื่อไฟล์ในโฟลเดอร์ cardpic ดูครับ)");
        }
        
        // -------------------------------------------------------------

        string assetPath = $"{path}/{name}.asset";
        AssetDatabase.CreateAsset(card, assetPath);
    }
}