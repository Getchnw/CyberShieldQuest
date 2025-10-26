using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    AudioManger AudioManger;

    private void Start()
    {
        //
        bool hasSaveFile = SaveSystem.SaveFileExists();
        GameObject.Find("ContinuousButton").GetComponent<Button>().interactable = hasSaveFile;
    }

    private void Awake()
    {
       AudioManger =  GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManger>();
    }

    public async void Use_SFX_Click_Button() {
        AudioManger.PlaySfx(AudioManger.buttonClick);
        await Task.Delay(300);
    }

    public void NewGame()
    {
        Use_SFX_Click_Button();

        // สั่งให้ GameManager สร้างข้อมูลเกมใหม่
        GameManager.Instance.CreateNewGame();

        SceneManager.LoadScene("Home");
    }

    // แก้ชื่อให้ถูกต้องเป็น ContinuousGame จะดีกว่าครับ
    public void ContinuousGame() 
    {
        Use_SFX_Click_Button();

        // สั่งให้ GameManager โหลดข้อมูลจากไฟล์
        // เมธอด LoadGame() จะคืนค่า true ถ้าโหลดสำเร็จ
        if (GameManager.Instance.LoadGame())
        {
            // ถ้าโหลดสำเร็จ ก็ไปฉาก Home ได้เลย
            SceneManager.LoadScene("Home");
        }
        else
        {
            // ถ้าโหลดไม่สำเร็จ (อาจจะไฟล์เสีย) ก็แสดงข้อความเตือน
            Debug.LogError("Failed to load save data!");
            // อาจจะทำ UI แจ้งเตือนผู้เล่นตรงนี้ก็ได้
        }
    }
    public void QuitGame ()
    {
        Use_SFX_Click_Button();
        Debug.Log("Quit GAme");
        Application.Quit();
    }
}
