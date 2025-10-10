using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    // คลิเสียงเพลงและเสียงเอฟเฟค
    [Header ("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    // คลิเสียง
    [Header ("Audio Clips")]
    public AudioClip background;
    public AudioClip buttonClick;
    

    public static AudioManger instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

   


    // เอาไว้ใช้ในตัวที่ต้องการจะเรียกเสียง
    //private void Awake()
    //{
    //    GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
