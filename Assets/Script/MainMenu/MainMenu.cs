using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // AudioManger AudioManger;
    [SerializeField] private GameObject recheckPanel;
    [SerializeField] private Button recheckNewgameButton;

    [SerializeField] private Button continuousButton; // ประกาศตัวแปรนี้แล้วลากปุ่มจาก Inspector มาใส่

    // private void Awake()
    // {
    //     // แนะนำให้หา Audio ผ่าน Tag ใน Awake เพื่อความเร็ว
    //     GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
    //     if (audioObj != null)
    //     {
    //         AudioManger = audioObj.GetComponent<AudioManger>();
    //     }
    // }

    private void Start()
    {
        RefreshMenuButtons();
    }

    // แยก Method ออกมาเพื่อให้เรียกใช้ซ้ำได้ (เช่น หลังจากลบเซฟ)
    public void RefreshMenuButtons()
    {
        bool hasSave = SaveSystem.SaveFileExists();

        if (continuousButton != null)
        {
            continuousButton.interactable = hasSave;
        }
        else
        {
            // ถ้าไม่ได้ลากใส่ Inspector ให้ลองหาด้วยชื่อ (ไม่แนะนำแต่ใส่ไว้กันพลาด)
            GameObject btnObj = GameObject.Find("ContinuousButton");
            if (btnObj != null)
            {
                btnObj.GetComponent<Button>().interactable = hasSave;
            }
        }
    }

    public async Task Use_SFX_Click_Button()
    {
        if (AudioManager.Instance != null)
        {
            // AudioManger.PlaySfx(AudioManger.buttonClick);
            AudioManager.Instance.PlaySFX("ButtonClick");
        }
        await Task.Delay(300); // รอ 0.3 วินาทีเพื่อให้เสียงเล่นจบ
    }

    public async void NewGame()
    {
        await Use_SFX_Click_Button();

        if (SaveSystem.SaveFileExists())
        {
            recheckPanel.SetActive(true);
            recheckNewgameButton.onClick.RemoveAllListeners();
            recheckNewgameButton.onClick.AddListener(() => ConfirmNewGame());
        }
        else
        {
            ConfirmNewGame();
        }
    }

    private async void ConfirmNewGame()
    {
        await Use_SFX_Click_Button();

        // ลบข้อมูลเก่าก่อนเริ่มเกมใหม่ (สำคัญมาก)
        SaveSystem.DeleteSaveData();

        GameManager.Instance.CreateNewGame();
        SceneManager.LoadScene("Home");
    }

    public async void ContinuousGame()
    {
        await Use_SFX_Click_Button();

        if (GameManager.Instance.LoadGame())
        {
            SceneManager.LoadScene("Home");
        }
        else
        {
            Debug.LogError("Failed to load save data!");
            RefreshMenuButtons(); // รีเฟรชปุ่มอีกรอบเผื่อไฟล์เสีย
        }
    }

    public async void QuitGame()
    {
        await Use_SFX_Click_Button();
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
