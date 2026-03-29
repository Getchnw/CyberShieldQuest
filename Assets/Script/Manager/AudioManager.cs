using UnityEngine;
using UnityEngine.SceneManagement; // ต้องเพิ่มอันนี้เพื่อจัดการ Scene
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource bgmSource2;
    public AudioSource sfxSource;

    private AudioSource activeSource;

    [Header("Scene BGM Settings")]
    // สร้าง List เพื่อจับคู่ชื่อฉากกับเพลงที่จะเล่น
    public List<SceneBGM> sceneBgmList;

    [System.Serializable]
    public struct SceneBGM
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    [Header("Sound Clips Library")]
    // public AudioClip bgmBattle; // เพลงมันส์ๆ
    public AudioClip sfxButtonClick; // เสียงคลิกปุ่ม
    public AudioClip sfxCardHover; // เสียงรูดการ์ด 
    public AudioClip sfxCardSelect; // เสียงคลิกการ์ด 
    public AudioClip sfxAttack; // เสียงเลเซอร์/ระเบิด 
    public AudioClip sfxDefend; // เสียงป้องกัน 
    public AudioClip sfxDropCard;//เสียงลงการ์ด
    public AudioClip sfxDrawCard;//เสียงจั่วการ์ด
    public AudioClip sfxHeal;//เสียงฟื้นพลัง
    public AudioClip sfxLow_Hp;
    public AudioClip sfxUseSpell;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        activeSource = bgmSource;
    }

    // void Start()
    // {
    //     PlayBGM(bgmBattle);
    // }

    void OnEnable()
    {
        // ลงทะเบียน Event: เมื่อโหลดฉากเสร็จ ให้เรียกฟังก์ชัน OnSceneLoaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // ยกเลิกการลงทะเบียนเพื่อป้องกัน Memory Leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติทุกครั้งที่เปลี่ยน Scene
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ค้นหาเพลงที่ตรงกับชื่อ Scene ปัจจุบัน
        foreach (var item in sceneBgmList)
        {
            if (item.sceneName == scene.name)
            {
                ChangeMusic(item.bgmClip, 2.0f); // สั่งเปลี่ยนเพลงแบบ Fade 2 วินาที
                break;
            }
        }
    }

    public void ChangeMusic(AudioClip newClip, float fadeDuration = 1.5f)
    {
        if (newClip == null || (activeSource.clip == newClip && activeSource.isPlaying)) return;

        AudioSource newSource = (activeSource == bgmSource) ? bgmSource2 : bgmSource;
        StopAllCoroutines();
        StartCoroutine(CrossfadeMusicRoutine(newSource, newClip, fadeDuration));
    }

    IEnumerator CrossfadeMusicRoutine(AudioSource newSource, AudioClip newClip, float duration)
    {
        newSource.clip = newClip;
        newSource.Play();
        newSource.volume = 0;

        float timer = 0;
        float startVol = activeSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            activeSource.volume = Mathf.Lerp(startVol, 0, t);
            newSource.volume = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        activeSource.Stop();
        activeSource = newSource;
    }

    // public void PlayBGM(AudioClip clip)
    // {
    //     bgmSource.clip = clip;
    //     bgmSource.Play();
    // }



    /// <summary>
    /// How to use
    /// Example:
    ///     AudioManager.Instance.PlaySFX("ButtonClick");
    ///     AudioManager.Instance.PlaySFX("CardHover");
    ///     AudioManager.Instance.PlaySFX("Attack");
    /// </summary>

    public void PlaySFX(string soundName)
    {
        switch (soundName)
        {
            case "ButtonClick": sfxSource.PlayOneShot(sfxButtonClick); break;
            case "CardHover": sfxSource.PlayOneShot(sfxCardHover); break;
            case "CardSelect": sfxSource.PlayOneShot(sfxCardSelect); break;
            case "Attack": sfxSource.PlayOneShot(sfxAttack); break;
            case "Defend": sfxSource.PlayOneShot(sfxDefend); break;
            case "DropCard": sfxSource.PlayOneShot(sfxDropCard); break;
            case "DrawCard": sfxSource.PlayOneShot(sfxDrawCard); break;
            case "Heal": sfxSource.PlayOneShot(sfxHeal); break;
            case "Low_Hp_loop":
                PlayLoopSFX(sfxLow_Hp);
                break;
            case "Stop_Loop":
                StopSFX();
                break;
            case "UseSpell": sfxSource.PlayOneShot(sfxUseSpell); break;
        }
    }

    public void PlayLoopSFX(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.loop = true; // เปิดโหมดวนลูป
        sfxSource.Play();
    }

    // ฟังก์ชันสำหรับหยุดเสียง Loop
    public void StopSFX()
    {
        sfxSource.Stop();
        sfxSource.loop = false; // ปิดโหมดวนลูปเพื่อไม่ให้กระทบเสียงอื่น
    }
}