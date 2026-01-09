using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 100f;
    public float fadeSpeed = 3f;
    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
    }

    void Update()
    {
        // 1. ลอยขึ้นข้างบน
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. ค่อยๆ จางหายไป
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        // 3. ถ้าจางหมดแล้วให้ทำลายตัวเอง
        if (textColor.a < 0)
        {
            Destroy(gameObject);
        }
    }
}