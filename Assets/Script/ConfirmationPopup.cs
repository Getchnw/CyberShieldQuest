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
    }

    void OnConfirmClicked()
    {
        // เรียกคำสั่งที่เก็บไว้
        onConfirmAction?.Invoke();
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}