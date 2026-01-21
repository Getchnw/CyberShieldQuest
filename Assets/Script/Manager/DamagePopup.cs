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

    public void Setup(string damageText)
    {
        textMesh.text = damageText;
    }

    void Update()
    {
        transform.position += new Vector3(0f, moveSpeed * Time.deltaTime, 0f);

        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a < 0f)
        {
            Destroy(gameObject);
        }
    }
}