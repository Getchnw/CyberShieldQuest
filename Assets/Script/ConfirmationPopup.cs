using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // จำเป็นสำหรับการส่งคำสั่ง (Action)

public class ConfirmationPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI messageText; // ข้อความคำถาม
    public Button confirmButton;        // ปุ่มตกลง
    public Button cancelButton;         // ปุ่มยกเลิก

    private Action onConfirmAction;     // เก็บคำสั่งที่จะทำ
    private bool isProcessing = false;  // 🔥 ป้องกันการกดซ้ำ

    void Start()
    {
        // ผูกปุ่มไว้เลย
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(Close);

        gameObject.SetActive(false); // เริ่มต้นซ่อนไว้
    }

    // ฟังก์ชันเปิดหน้าต่าง (รับข้อความ และ คำสั่งที่จะให้ทำ)
    public void Open(string message, Action confirmAction)
    {
        gameObject.SetActive(true);
        messageText.text = message;
        onConfirmAction = confirmAction; // เก็บคำสั่งไว้ก่อน (เช่น CraftCard)

        // 🔥 รีเซ็ตสถานะและเปิด raycast ของปุ่ม
        isProcessing = false;
        confirmButton.interactable = true;
        cancelButton.interactable = true;
    }

    void OnConfirmClicked()
    {
        // 🔥 ถ้ากำลังประมวลผลอยู่ ห้ามกดซ้ำ
        if (isProcessing) return;

        isProcessing = true;
        confirmButton.interactable = false;
        cancelButton.interactable = false;
        Debug.Log($"[ConfirmationPopup] Confirmed: {messageText.text}");

        // เรียกคำสั่งที่เก็บไว้
        onConfirmAction?.Invoke();
        gameObject.SetActive(false);
    }

    public void Close()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        gameObject.SetActive(false);
    }
}