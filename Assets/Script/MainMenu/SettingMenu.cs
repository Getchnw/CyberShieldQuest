using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SettingMenu : MonoBehaviour
{
    // เปลี่ยนจาก AudioManager AudioManager เป็นชื่ออื่นเพื่อไม่ให้สับสนกับ Class
    private AudioManager audioManager;

    [Header("Slider UI")]
    public Slider volume_Music_Slider;
    public Slider volume_SFX_Slider;

    [Header("Audio Settings")]
    public AudioMixer audioMixer;

    private void Start()
    {
        // ใช้ Instance จาก AudioManager โดยตรงจะปลอดภัยกว่าการหา Tag
        audioManager = AudioManager.Instance;

        // โหลดค่าที่เคยเซฟไว้ (PlayerPrefs)

        float savedVolume_Music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float savedVolume_SFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // ตั้งค่า Slider ให้ตรงกับค่าที่โหลดมา
        if (volume_Music_Slider != null) volume_Music_Slider.value = savedVolume_Music;
        if (volume_SFX_Slider != null) volume_SFX_Slider.value = savedVolume_SFX;

        // ตั้งค่า AudioMixer (ใช้เวลาเล็กน้อยเพื่อให้ Mixer พร้อมทำงาน)
        SetMusicVolume(savedVolume_Music);
        SetSFXVolume(savedVolume_SFX);
    }

    public void SetMusicVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
        audioMixer.SetFloat("MusicVolume", dB); // ต้องเป็น "MusicVolume" ตามที่ตั้งใน Mixer
        PlayerPrefs.SetFloat("MusicVolume", volume);
        Debug.Log($"Music Volume set to: {volume} (dB: {dB})");
    }

    public void SetSFXVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
        audioMixer.SetFloat("SFXVolume", dB); // ต้องเป็น "SFXVolume" ตามที่ตั้งใน Mixer
        PlayerPrefs.SetFloat("SFXVolume", volume);
        Debug.Log($"SFX Volume set to: {volume} (dB: {dB})");
    }

    // ฟังก์ชันเพิ่ม-ลดเสียง ปรับให้ใช้ร่วมกับฟังก์ชันหลัก
    public void IncreaseMusicVolume() => AdjustMusic(0.05f);
    public void DecreaseMusicVolume() => AdjustMusic(-0.05f);
    public void IncreaseSFXVolume() => AdjustSFX(0.05f);
    public void DecreaseSFXVolume() => AdjustSFX(-0.05f);

    private void AdjustMusic(float amount)
    {
        volume_Music_Slider.value = Mathf.Clamp(volume_Music_Slider.value + amount, 0.0001f, 1f);
        SetMusicVolume(volume_Music_Slider.value);
    }

    private void AdjustSFX(float amount)
    {
        volume_SFX_Slider.value = Mathf.Clamp(volume_SFX_Slider.value + amount, 0.0001f, 1f);
        SetSFXVolume(volume_SFX_Slider.value);
    }

    public async void Use_SFX_Click_Button()
    {
        // แก้ไขการเรียกชื่อ AudioManager และชื่อฟังก์ชันให้ตรงกับใน Script
        if (AudioManager.Instance != null)
        {
            // ใน AudioManager คุณใช้ switch-case ชื่อ "CardSelect" แทนเสียงคลิกทั่วไปได้
            AudioManager.Instance.PlaySFX("CardSelect");
        }
        await Task.Delay(300);
    }

    public void QuitGame()
    {
        Use_SFX_Click_Button();
        SceneManager.LoadScene("MainMenu");
    }
}


