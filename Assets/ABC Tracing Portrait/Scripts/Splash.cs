using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    public GameObject privacyP;
    //public UmpManager1 UMP;
    private void Start()
    {
        PlayerPrefs.SetInt("fromGP", 0);
        PlayerPrefs.SetInt("RateDone", 1);
        PlayerPrefs.SetInt("Character_Index", 0);
        PlayerPrefs.SetInt("Welcome", 1);
        PlayerPrefs.SetInt("SelectedMode", 0);
        PlayerPrefs.SetInt("Completed", 0);
        PlayerPrefs.SetInt("ModeIndex",0);
        PlayerPrefs.SetInt("RemoveAds", 1);
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Input.multiTouchEnabled = false;
        if (PlayerPrefs.GetInt("Privacy") == 1)
        {
            Invoke(nameof(Accept), 1);
        }
        else
        {
            privacyP.SetActive(true);
        }
    }

    public void visitPrivacy()
    {
        Application.OpenURL("https://sites.google.com/view/amasconsultant-privacy-policy/home");
    }
    public void Accept()
    {
        PlayerPrefs.SetInt("Privacy", 1);
        try
        {
            if (PlayerPrefs.GetInt("ConsentCall") == 0)
            {
                PlayerPrefs.SetInt("ConsentCall", 1);
                //UMP.ConsentCall();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        PlayerPrefs.SetInt("ConsentCall", 1);
        SceneManager.LoadScene("MainSelection");
    }
}
