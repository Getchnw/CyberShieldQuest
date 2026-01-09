using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Sound Clips Library")]
    public AudioClip bgmBattle; // เพลงมันส์ๆ
    public AudioClip sfxCardHover; // เสียงรูดการ์ด (ฟึ่บ)
    public AudioClip sfxCardSelect; // เสียงคลิกการ์ด (ปิ๊ง)
    public AudioClip sfxAttack; // เสียงเลเซอร์/ระเบิด (ตู้ม)
    public AudioClip sfxDamage; // เสียงโดนตี (อึก!)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayBGM(bgmBattle);
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(string soundName)
    {
        switch (soundName)
        {
            case "CardHover": sfxSource.PlayOneShot(sfxCardHover); break;
            case "CardSelect": sfxSource.PlayOneShot(sfxCardSelect); break;
            case "Attack": sfxSource.PlayOneShot(sfxAttack); break;
            case "Damage": sfxSource.PlayOneShot(sfxDamage); break;
        }
    }
}