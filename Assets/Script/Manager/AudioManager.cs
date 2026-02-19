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
    public AudioClip sfxCardHover; // เสียงรูดการ์ด (ฟึ่บ)
    public AudioClip sfxCardSelect; // เสียงคลิกการ์ด (ปิ๊ง)
    public AudioClip sfxAttack; // เสียงเลเซอร์/ระเบิด (ตู้ม)
    public AudioClip sfxDamage; // เสียงโดนตี (อึก!)

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