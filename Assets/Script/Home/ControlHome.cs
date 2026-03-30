using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlHome : MonoBehaviour
{
    public Button stageButton;
    // AudioManger AudioManger;
    // // Start is called before the first frame update
    void Update()
    {
        if (stageButton != null)
            if (GameManager.Instance.CurrentGameData.statusPostTest != null)
            {
                stageButton.interactable = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A01;
            }

    }
    public void LoadSence(string Namescene)
    {
        SceneManager.LoadScene(Namescene);
    }

    // private void Awake()
    // {
    //     AudioManger = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    // }

    public async void Use_SFX_Click_Button()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        await Task.Delay(300);
    }
}
