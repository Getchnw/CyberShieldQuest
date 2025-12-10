using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ExpandableItem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Button triggerButton; // ปุ่มที่จะกด (ตัว box)
    [SerializeField] private List<GameObject> contentPanel; // ส่วนที่จะซ่อน/แสดง (ตัว detail box)
    [SerializeField] private RectTransform arrowIcon; // ไอคอนลูกศร (ถ้ามีให้หมุน)

    private RectTransform myRectTransform;
    private bool isExpanded = false;

    void Start()
    {
        myRectTransform = GetComponent<RectTransform>(); // ดึงค่าของตัวเองไว้
        // เริ่มต้นให้ปิด detail box ไว้ก่อน
        if (contentPanel != null)
        {
            foreach (GameObject go in contentPanel)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        // สั่งให้ปุ่มทำงานเมื่อกด
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(ToggleContent);
        }
    }

    void ToggleContent()
    {
        isExpanded = !isExpanded;

        // 1. เปิด/ปิด เนื้อหา
        if (contentPanel != null)
        {
            foreach (GameObject go in contentPanel)
            {
                if (go != null)
                {
                    go.SetActive(isExpanded);
                }
            }
        }

        // 2. หมุนลูกศร (ถ้ามี)
        if (arrowIcon != null)
        {
            // หมุน 180 องศาเมื่อเปิด, กลับเป็น 0 เมื่อปิด
            float zRotation = isExpanded ? 180f : 0f;
            arrowIcon.rotation = Quaternion.Euler(0, 0, zRotation);
            // *หมายเหตุ: ถ้าอยากให้หมุนสวยๆ อาจต้องใช้ DG.Tweening หรือ Coroutine เพิ่มเติม
        }
    }
}