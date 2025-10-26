using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlHome : MonoBehaviour
{
    AudioManger AudioManger;
    // Start is called before the first frame update
    public void LoadSence (string Namescene) {
        Use_SFX_Click_Button();
        SceneManager.LoadScene(Namescene);
    }

    private void Awake()
    {
        AudioManger = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    }

    public async void Use_SFX_Click_Button()
    {
       AudioManger.PlaySfx(AudioManger.buttonClick);
       await Task.Delay(300);
    }
}
