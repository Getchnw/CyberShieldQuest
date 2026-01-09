using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StageAnimationController : MonoBehaviour
{
    [Header("1. Nodes Configuration")]
    public List<Transform> stageNodes; // ลากปุ่มด่านมาใส่เรียงตามลำดับ (1 -> 2 -> 3)
    public float popUpSpeed = 0.5f;    // ความเร็วในการเด้ง
    public float delayBetweenNodes = 0.2f; // เวลารอระหว่างปุ่ม

    [Header("2. Lines Configuration")]
    public List<Image> connectionLines; // ลากเส้นมาใส่ (ต้องตั้ง Image Type เป็น Filled)
    public float drawLineSpeed = 1.0f;  // ความเร็วในการวาดเส้น

    [Header("3. Idle Floating")]
    public float floatAmplitude = 5f;  // ลอยขึ้นลงสูงแค่ไหน
    public float floatFrequency = 2f;  // ลอยเร็วแค่ไหน
    
    // เก็บตำแหน่งเดิมไว้คำนวณการลอย
    private List<Vector3> originalPositions = new List<Vector3>();
    private bool isEntranceFinished = false;

    void Start()
    {
        // 1. เก็บตำแหน่งเริ่มต้น และซ่อนปุ่มให้เหลือขนาด 0
        foreach (var node in stageNodes)
        {
            originalPositions.Add(node.localPosition);
            node.localScale = Vector3.zero; // หดเหลือ 0
        }

        // 2. ซ่อนเส้น (Fill Amount = 0)
        foreach (var line in connectionLines)
        {
            line.fillAmount = 0;
        }

        // 3. เริ่มเล่นอนิเมชั่น
        StartCoroutine(PlayEntranceSequence());
    }

    void Update()
    {
        // ถ้าเปิดตัวเสร็จแล้ว ให้ทำท่าลอยขึ้นลง (Floating)
        if (isEntranceFinished)
        {
            for (int i = 0; i < stageNodes.Count; i++)
            {
                // สูตร Sine Wave: ให้ลอยไม่พร้อมกันโดยใช้ index มาบวกเพิ่ม
                float newY = originalPositions[i].y + Mathf.Sin(Time.time * floatFrequency + i) * floatAmplitude;
                
                Vector3 newPos = stageNodes[i].localPosition;
                newPos.y = newY;
                stageNodes[i].localPosition = newPos;
            }
        }
    }

    // Coroutine สำหรับลำดับการเปิดตัว
    IEnumerator PlayEntranceSequence()
    {
        // รอแป๊บนึงตอนโหลดฉาก
        yield return new WaitForSeconds(0.3f);

        // วนลูปเล่นทีละคู่ (ปุ่ม -> เส้น -> ปุ่มต่อไป)
        for (int i = 0; i < stageNodes.Count; i++)
        {
            // 1. ปุ่มเด้งขึ้นมา (Scale 0 -> 1)
            yield return StartCoroutine(AnimatePopUp(stageNodes[i]));

            // 2. วาดเส้นไปหาปุ่มถัดไป (ถ้ามีเส้นที่ตรงกับลำดับนี้)
            if (i < connectionLines.Count)
            {
                yield return StartCoroutine(AnimateLine(connectionLines[i]));
            }

            // รอแป๊บก่อนไปอันต่อไป
            yield return new WaitForSeconds(delayBetweenNodes);
        }

        isEntranceFinished = true; // จบการเปิดตัว เริ่มลอยได้
    }

    IEnumerator AnimatePopUp(Transform target)
    {
        float timer = 0;
        // ใช้ BackEaseOut เพื่อให้เด้งดึ๋ง (เกิน 1 นิดนึงแล้วหดกลับ)
        while (timer < 1f)
        {
            timer += Time.deltaTime / popUpSpeed;
            float progress = Mathf.Clamp01(timer);
            
            // สูตร Easing ให้เด้งๆ
            float scale = 1 + 2.70158f * Mathf.Pow(progress - 1, 3) + 1.70158f * Mathf.Pow(progress - 1, 2);
            
            target.localScale = Vector3.one * scale;
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    IEnumerator AnimateLine(Image line)
    {
        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime / (drawLineSpeed * 0.5f); // วาดเร็วๆ หน่อย
            line.fillAmount = Mathf.Lerp(0, 1, timer);
            yield return null;
        }
        line.fillAmount = 1;
    }
}