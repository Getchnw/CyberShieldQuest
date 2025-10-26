using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SettingMenu : MonoBehaviour
{
    [Header("Audio Sources")]
    AudioManger AudioManger;

    [Header("Slider UI")]
    public Slider volume_Music_Slider;    // Slider UI musix
    public Slider volume_SFX_Slider;    // Slider UI SFX

    [Header("Audio Settings")]
    public AudioMixer audioMixer;  // อ้างถึง AudioMixer ใน Inspector

    private void Awake()
    {
       AudioManger =  GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    }

    private void Start()
    {
        // โหลดค่าที่เคยเซฟไว้ (PlayerPrefs)
        float savedVolume_Music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float savedVolume_SFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // ตั้งค่า Slider ให้ตรงกับค่าที่โหลดมา
        volume_Music_Slider.value = savedVolume_Music;
        volume_SFX_Slider.value = savedVolume_SFX;

        // ตั้งค่า AudioMixer ให้ตรงกับค่าที่โหลดมา
        SetMusicVolume(savedVolume_Music);
        SetSFXVolume(savedVolume_SFX);
    }


    public void SetMusicVolume(float volume)
    {
        Debug.Log(volume);
        // แปลงเป็น dB เพราะ AudioMixer ใช้หน่วย dB
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);

        // เซฟค่าไว้สำหรับเปิดเกมครั้งต่อไป
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume(float volume)
    {
        volume_SFX_Slider.value = volume;
        // แปลงเป็น dB เพราะ AudioMixer ใช้หน่วย dB
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);

        // เซฟค่าไว้สำหรับเปิดเกมครั้งต่อไป
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void IncreaseMusicVolume()
    {
        float newValue = Mathf.Clamp(volume_Music_Slider.value + 0.05f, 0.0001f, 1f);
        volume_Music_Slider.value = newValue;
        SetMusicVolume(newValue);
    }
    public void DecreaseMusicVolume()
    {
        float newValue = Mathf.Clamp(volume_Music_Slider.value - 0.05f, 0.0001f, 1f);
        volume_Music_Slider.value = newValue;
        SetMusicVolume(newValue);
    }

    public void IncreaseSFXVolume()
    {
        float newValue = Mathf.Clamp(volume_SFX_Slider.value + 0.05f, 0.0001f, 1f);
        volume_SFX_Slider.value = newValue;
        SetSFXVolume(newValue);
    }
    public void DecreaseSFXVolume()
    {
        float newValue = Mathf.Clamp(volume_SFX_Slider.value - 0.05f, 0.0001f, 1f);
        volume_SFX_Slider.value = newValue;
        SetSFXVolume(newValue);
    }
    
    public async void Use_SFX_Click_Button() {
        AudioManger.PlaySfx(AudioManger.buttonClick);
        await Task.Delay(300);
    }

    public void QuitGame() {
        Use_SFX_Click_Button();
        SceneManager.LoadScene("MainMenu");
    }
}
