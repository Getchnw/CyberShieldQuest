// using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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

    public void Use_SFX_Click_Button()
    {
        // สั่งให้ไปเรียกตัวลูกน้องที่ชื่อต่อท้ายด้วย Routine
        StartCoroutine(Use_SFX_Click_Button_Routine());
    }

    private IEnumerator Use_SFX_Click_Button_Routine()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // ใช้ yield return new WaitForSeconds แทน Task.Delay
        yield return new WaitForSeconds(0.3f);
    }
    // public async void NewGame()
    // {
    //     await Use_SFX_Click_Button();
    //     Debug.Log("New Game");
    //     bool hasSave = SaveSystem.SaveFileExists();
    //     Debug.Log($"Save file exists: {hasSave}");
    //     bool dkd = false;
    //     if (dkd)
    //     {
    //         recheckPanel.SetActive(true);
    //         recheckNewgameButton.onClick.RemoveAllListeners();
    //         recheckNewgameButton.onClick.AddListener(() => ConfirmNewGame());
    //     }
    //     else
    //     {
    //         Debug.Log("No save file found. After SFX Delay");
    //         ConfirmNewGame();
    //     }
    // }

    public void NewGame() // <-- ให้ปุ่ม UI เรียกตัวนี้
    {
        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        // รอให้เสียงเล่นจบ

        yield return StartCoroutine(Use_SFX_Click_Button_Routine());

        Debug.Log("New Game Button Clicked");

        bool hasSave = SaveSystem.SaveFileExists();
        Debug.Log($"Save file exists: {hasSave}");

        if (hasSave)
        {
            recheckPanel.SetActive(true);
            recheckNewgameButton.onClick.RemoveAllListeners();
            // ให้ปุ่มยืนยัน ไปเรียกฟังก์ชัน ConfirmNewGame แบบ void 
            recheckNewgameButton.onClick.AddListener(() => ConfirmNewGame());
        }
        else
        {
            Debug.Log("No save file found. Starting game directly.");
            ConfirmNewGame();
        }
    }

    // private async void ConfirmNewGame()
    // {
    //     await Use_SFX_Click_Button();

    //     // ลบข้อมูลเก่าก่อนเริ่มเกมใหม่ (สำคัญมาก)
    //     // SaveSystem.DeleteSaveData();

    //     GameManager.Instance.CreateNewGame();
    //     SceneManager.LoadScene("Home");
    // }

    // public async void ContinuousGame()
    // {
    //     await Use_SFX_Click_Button();

    //     if (GameManager.Instance.LoadGame())
    //     {
    //         SceneManager.LoadScene("Home");
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to load save data!");
    //         RefreshMenuButtons(); // รีเฟรชปุ่มอีกรอบเผื่อไฟล์เสีย
    //     }
    // }

    // private async void ConfirmNewGame()
    // {
    //     // ปิด Panel ทันทีเพื่อป้องกันการกดซ้ำ
    //     if (recheckPanel != null) recheckPanel.SetActive(false);

    //     await Use_SFX_Click_Button();

    //     // ไม่ต้องสั่ง SaveSystem.DeleteSaveData() เพราะ CreateNewGame จะเขียนทับไฟล์เดิมเอง
    //     // การลบไฟล์ก่อนอาจทำให้เกิด Error ในบาง Platform ถ้า System ยังจัดการไฟล์ไม่เสร็จ
    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.CreateNewGame();

    //         // รอสักนิดเพื่อให้แน่ใจว่าไฟล์ถูกเขียนลง Disk (สำคัญมากสำหรับ WebGL)
    //         await Task.Delay(500);

    //         SceneManager.LoadScene("Home");
    //     }
    // }

    private void ConfirmNewGame() // <-- ให้สคริปต์ในโค้ดเรียกตัวนี้
    {
        StartCoroutine(ConfirmNewGameRoutine());
    }
    private IEnumerator ConfirmNewGameRoutine()
    {
        if (recheckPanel != null) recheckPanel.SetActive(false);

        // yield return StartCoroutine(Use_SFX_Click_Button());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CreateNewGame();

            // รอ 0.5 วินาทีเพื่อให้ WebGL เขียนไฟล์ลง IndexedDB จนเสร็จ
            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene("Home");
        }
    }

    // public async void ContinuousGame()
    // {
    //     await Use_SFX_Click_Button();

    //     // โหลดข้อมูลเข้า GameManager ก่อนเปลี่ยนฉาก
    //     if (GameManager.Instance.LoadGame())
    //     {
    //         SceneManager.LoadScene("Home");
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to load save data!");
    //         // ถ้าโหลดไม่ได้ ให้ปิดปุ่มเล่นต่อทันที
    //         RefreshMenuButtons();
    //     }
    // }

    public void ContinuousGame() // <-- ให้ปุ่ม UI เรียกตัวนี้
    {
        StartCoroutine(ContinuousGameRoutine());
    }

    private IEnumerator ContinuousGameRoutine()
    {
        yield return StartCoroutine(Use_SFX_Click_Button_Routine());

        if (GameManager.Instance.LoadGame())
        {
            SceneManager.LoadScene("Home");
        }
        else
        {
            Debug.LogError("Failed to load save data!");
            RefreshMenuButtons();
        }
    }

    // public async void QuitGame()
    // {
    //     await Use_SFX_Click_Button();
    //     Debug.Log("Quit Game");
    //     Application.Quit();
    // }

    public void QuitGame() // <-- ให้ปุ่ม UI เรียกตัวนี้
    {
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        yield return StartCoroutine(Use_SFX_Click_Button_Routine());
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
