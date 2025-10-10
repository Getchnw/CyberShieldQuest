using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    AudioManger AudioManger;
    private void Awake()
    {
       AudioManger =  GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    }

    public async void Use_SFX_Click_Button() {
        AudioManger.PlaySfx(AudioManger.buttonClick);
        await Task.Delay(300);
    }
    public void NewGame ()
    {
        Use_SFX_Click_Button();
        SceneManager.LoadScene("Home");
    }
    public void ContinuosGame()
    {
        Use_SFX_Click_Button();
        SceneManager.LoadScene("Home");
    }
    public void QuitGame ()
    {
        Use_SFX_Click_Button();
        Debug.Log("Quit GAme");
        Application.Quit();
    }
}
