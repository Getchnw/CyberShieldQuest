using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Recheck_PopUp : MonoBehaviour
{
    public GameObject PopUp;
    public GameObject Panel_detailPopup;
    public StageDetailPopup detailPopup;
    public Button Yes;
    public Button No;
    public Button close;


    void Start()
    {
        if (Yes != null)
        {
            // Yes.onClick.RemoveAllListeners();
            Yes.onClick.AddListener(OpenStoryDetail);
        }
        if (No != null)
        {
            No.onClick.RemoveAllListeners();
            No.onClick.AddListener(Close);
        }
        if (close != null)
        {
            close.onClick.RemoveAllListeners();
            close.onClick.AddListener(Close);
        }
    }

    public void Open()
    {
        // AudioManager.Instance.PlaySFX("ButtonClick");
        PopUp.gameObject.SetActive(true);
    }
    public void Close()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        PopUp.gameObject.SetActive(false);
    }

    public void OpenStoryDetail()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        Close();
        // Panel_detailPopup.gameObject.SetActive(true);
        // detailPopup.gameObject.SetActive(true);
    }
}