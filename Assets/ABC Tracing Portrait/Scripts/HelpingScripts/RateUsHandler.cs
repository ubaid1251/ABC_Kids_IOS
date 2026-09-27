//using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RateUsHandler : MonoBehaviour
{
    public static RateUsHandler Instance;
    public GameObject rate;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
    public void Deactive()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.ShowBanner();//remove later
        }
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    public void Rate()
    {
        Intitializeabc.instance.ShowBanner();//remove later
        PlayerPrefs.SetInt("RateDone", 1);
        rate.SetActive(false);
        InAppReview.Instance.ShowInGameRating();
    }

    public void Cross()
    {
        SoundHandler.instance.PlaySource(SoundHandler.instance.mySource.clip);
        rate.GetComponent<Animator>().Play("PanelOut");
       // InitializeFi._Instance.LogFirebaseEvent("Rate_Game_CrossBtn_Pressed"); //lock
        InitializeFi._Instance.LogFi();
        Invoke(nameof(HidePanel), 0.9f);
    }
    void HidePanel()
    {
        Intitializeabc.instance.ShowBanner();
        rate.gameObject.SetActive(value: false);
    }
    public bool CheckRateCondition()
    {
        
        if (PlayerPrefs.GetInt("RateDone") == 0)
        {
            Debug.Log("rateUs Issue");
            if ((PlayerPrefs.GetInt("RateCounter") % 5 == 0) && PlayerPrefs.GetInt("Completed") == 1 /*&&
                        PlayerPrefs.GetInt("RateUsAppear") < 3*/)
            {
                PlayerPrefs.SetInt("RateUsAppear", PlayerPrefs.GetInt("RateUsAppear") + 1);
                Debug.Log("5 Time RateUs " + PlayerPrefs.GetInt("RateUsAppear"));
                PlayerPrefs.SetInt("Completed", 0);
                return true;
            }
            if (PlayerPrefs.GetInt("RateCounter") == 3 && PlayerPrefs.GetInt("FirstTime") == 0 &&
                     PlayerPrefs.GetInt("Completed") == 1 /*&& PlayerPrefs.GetInt("RateUsAppear") < 3*/)
            {
                PlayerPrefs.SetInt("RateCounter", 0);
                PlayerPrefs.SetInt("RateUsAppear", PlayerPrefs.GetInt("RateUsAppear") + 1);
                Debug.Log("3 Time RateUs " + PlayerPrefs.GetInt("RateUsAppear"));
                PlayerPrefs.SetInt("FirstTime", 1);
                PlayerPrefs.SetInt("Completed", 0);
                return true;
            }
            if (rate.activeInHierarchy) return false;
        }

        return false;
    }
}
