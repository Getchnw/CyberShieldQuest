using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

//public class LocalizationScanner : EditorWindow
//{
//    [MenuItem("Tools/Localization/Scan Game Content")]
//    public static void ScanContent()
//    {
//        // 1. ดึง String Table Collection (ชื่อต้องตรงกับที่คุณตั้งใน Localization Tables window)
//        var collection = LocalizationEditorSettings.GetStringTableCollection("MyGameTable");
//        if (collection == null)
//        {
//            Debug.LogError("ไม่พบ Table ชื่อ MyGameTable กรุณาสร้างก่อนครับ!");
//            return;
//        }

//        var thTable = collection.GetTable("th") as StringTable;

//        // 2. สแกน DialogueLinesData (บทพูด)
//        string[] dialogueGuids = AssetDatabase.FindAssets("t:DialogueLinesData");
//        foreach (var guid in dialogueGuids)
//        {
//            var path = AssetDatabase.GUIDToAssetPath(guid);
//            var data = AssetDatabase.LoadAssetAtPath<DialogueLinesData>(path);
//            foreach (var text in data.Dialog_Text)
//            {
//                AddKeyToTable(collection, thTable, text);
//            }
//        }

//        // 3. สแกน QuestionData (คำถาม Quiz)
//        string[] questionGuids = AssetDatabase.FindAssets("t:QuestionData");
//        foreach (var guid in questionGuids)
//        {
//            var path = AssetDatabase.GUIDToAssetPath(guid);
//            var data = AssetDatabase.LoadAssetAtPath<QuestionData>(path);
//            AddKeyToTable(collection, thTable, data.questionText);
//            foreach (var ans in data.answerOptions)
//            {
//                AddKeyToTable(collection, thTable, ans);
//            }
//        }

//        // 4. สแกน FillInTheBlankData (เติมคำในช่องว่าง)


//        AssetDatabase.SaveAssets();
//        Debug.Log("สแกนเสร็จเรียบร้อย! ลองเปิด Localization Tables ดูครับ");
//    }

//    private static void AddKeyToTable(StringTableCollection collection, StringTable thTable, string text)
//    {
//        if (string.IsNullOrEmpty(text)) return;

//        // ใช้ข้อความไทยเป็น Key เลย เพื่อให้ LanguageBridge.Get(text) ทำงานได้
//        if (!collection.SharedData.Contains(text))
//        {
//            collection.SharedData.AddKey(text);
//            thTable.AddEntry(text, text); // ใส่ค่าภาษาไทยเริ่มต้นไว้ให้ด้วย
//        }
//    }
//}



public class LocalizationScanner : EditorWindow
{
    [MenuItem("Tools/Localization/Scan All Content (Full)")]
    public static void ScanAllContent()
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection("MyGameTable");
        if (collection == null) return;
        var thTable = collection.GetTable("th") as StringTable;

        // --- 1. สแกน Dialogue & Quiz (ของเดิม) ---
        ScanSO<DialogueLinesData>(collection, thTable, (data) => {
            foreach (var line in data.Dialog_Text) AddKey(collection, thTable, line);
        });

        ScanSO<QuestionData>(collection, thTable, (data) => {
            // สแกนคำถาม
            AddKey(collection, thTable, data.questionText);
            // สแกนตัวเลือกคำตอบ
            foreach (var ans in data.answerOptions) AddKey(collection, thTable, ans);
        });

        // --- 2. สแกน True/False ---
        // โจทย์
        ScanSO<TrueFalseQuestion>(collection, thTable, (data) => {
            // สมมติว่าใน TrueFalseDescription มีตัวแปรชื่อ question หรือ description
            // ให้ใส่ชื่อตัวแปรที่เก็บข้อความภาษาไทยจริงๆ ลงไปตรงนี้ครับ
            AddKey(collection, thTable, data.statement);
        });
        // คำอธิบาย
        ScanSO<TrueFalseDescription>(collection, thTable, (data) => {
            // คำตอบของคำอธิบาย
            AddKey(collection, thTable, data.UserAnswertext);
            // คำอธิบาย
            AddKey(collection, thTable, data.DescriptionContext);
        });

        // --- 3. สแกน Matching ---
        // โจทย์
        ScanSO<MatchingQuestion>(collection, thTable, (data) => {
            // สแกนโจทย์ที่เป็นรายการคำถาม (prompts)
            foreach (var question in data.prompts)
                AddKey(collection, thTable, question);
            // สแกนตัวเลือกคำตอบ (options)
            foreach (var option in data.options)
                AddKey(collection, thTable, option);

        });
        // คำอธิบาย
        ScanSO<MatchingDescription>(collection, thTable, (data) => {
            // คำตอบของคำอธิบาย
            AddKey(collection, thTable, data.UserAnswertext);
            // สมมติว่า Matching มีข้อความด้านซ้ายและขวา
            AddKey(collection, thTable, data.DescriptionContext);
        });

        // --- 4. สแกน Fill in the Blank ---
        // โจทย์
        ScanSO<FillInBlankQuestion>(collection, thTable, (data) => {
            // สแกนโจทย์ที่
            foreach (var text in data.sentences)
            {
                AddKey(collection, thTable, text.sentencePart1);
                AddKey(collection, thTable, text.sentencePart2);
            }
            // สแกนคำตอบที่เป็นช่องว่าง
            foreach (var answer in data.wordBank)
                AddKey(collection, thTable, answer);
        });
        // คำอธิบาย
        ScanSO<FillInBlankDescription>(collection, thTable, (data) => {
            // คำตอบของคำอธิบาย
            AddKey(collection, thTable, data.UserAnswertext);
            // สแกนคำอธิบาย
            AddKey(collection, thTable, data.DescriptionContext);
        });

        //--- 5. Daily Quest ---
        ScanSO<DailyQuestsData>(collection, thTable, (data) => {
            //รายการโจทย์ประจำวัน
            AddKey(collection, thTable, data.description);
        });

        //--- 6. Achievement ---
        ScanSO<AchievementData>(collection, thTable, (data) => {
            // ชื่อความสำเร็จ
            //AddKey(collection, thTable, data.achievementName);
            // รายละเอียดความสำเร็จ
            AddKey(collection, thTable, data.description);
        });

        //--- 7. Card ---
        ScanSO<CardData>(collection, thTable, (data) => {
            // ข้อความสกิลการ์ด
            AddKey(collection, thTable, data.abilityText);
            AddKey(collection, thTable, data.flavorText);
        });

        AssetDatabase.SaveAssets();
        Debug.Log("สแกนข้อมูลโจทย์ทุกประเภทเรียบร้อย!");
    }



    // ฟังก์ชันช่วยสแกน Generic เพื่อให้โค้ดสะอาดขึ้น
    private static void ScanSO<T>(StringTableCollection col, StringTable th, System.Action<T> action) where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (var guid in guids)
        {
            T data = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null) action(data);
        }
    }

    private static void AddKey(StringTableCollection collection, StringTable thTable, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (!collection.SharedData.Contains(text))
        {
            collection.SharedData.AddKey(text);
            thTable.AddEntry(text, text);
        }
    }
}