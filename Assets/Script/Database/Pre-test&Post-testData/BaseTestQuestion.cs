using UnityEngine;

// นี่จะเป็น "คลาสแม่" ให้คำถามทุกประเภทยืมใช้
public abstract class BaseTestQuestion : ScriptableObject
{
    [TextArea(3, 5)]
    public string questionContext; // (Optional) เผื่อใช้เป็นคำสั่งส่วนหัว
}
