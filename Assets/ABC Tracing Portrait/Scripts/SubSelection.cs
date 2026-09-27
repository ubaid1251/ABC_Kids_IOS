using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SubSelection : MonoBehaviour
{
    public Image BG;
    public Sprite[] allbg;
    public GameObject[] modes;
    public LoadingHandler LOADING;
    public static string cameFrom = "MainSelection";
    public EventSystem eventSystem;
    RectTransform content;
    public RectTransform adult, setting,banner;
    public RectTransform[] title;
    void FindSelected()
    {
        string s = PlayerPrefs.GetString("SelectedMode");
        //if (IntitializeAdmob.instance.IsStaticInterAvailable())//remove later
        //{
        //    LOADING.gameObject.SetActive(true);
        //    LOADING.staticInter = true;
        //    if (s == "ABCLearning")
        //    {
        //        LOADING.ActiveAfter = modes[0];
        //    }
        //    else if (s == "NumbersLearning")
        //    {
        //        LOADING.ActiveAfter = modes[1];
        //    }
        //    else if (s == "abcLearning")
        //    {
        //        LOADING.ActiveAfter = modes[2];
        //    }
        //}
        //else
        //{
            if (PlayerPrefs.GetInt("RemoveAds") == 0)
            {
            Intitializeabc.instance.ShowBanner();//remove later
            }
            if (s == "ABCLearning")
            {
                modes[0].SetActive(true);
            }
            else if (s == "NumbersLearning")
            {
                modes[1].SetActive(true);
            }
            else if (s == "abcLearning")
            {
                modes[2].SetActive(true);
            }
        //}
    }
    void Start()
    {
        //PlayerPrefs.SetInt("RemoveAds", 1);
        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            adult.DOAnchorPosY(-100, 0);
            setting.DOAnchorPosY(-100, 0);
            banner.gameObject.SetActive(false);
            for (int i = 0; i < title.Length; i++)
            {
                title[i].DOAnchorPosY(-216, 0);
            }
        }
        BG.sprite = allbg[Random.Range(0, allbg.Length)];
        FindSelected();
    }
    public void Home()
    {
        Vibration.Vibrate(50);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        eventSystem.enabled = false;
        //InitializeFirebase_CB._Instance.LogFirebaseEvent("HomePressed_InSubSelection_On_" + PlayerPrefs.GetString("SelectedMode")+"_Mode");//remove later
        DOTween.KillAll(false);
        if (SoundHandler.instance.mySource.enabled == true)
        {
            SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
        }
        Invoke(nameof(LoadH), SoundHandler.instance.selectCh.length);
    }
    public void SelectCharacter(AudioClip c)
    {
        PlayerPrefs.SetFloat("FillAmount", 0);
        PlayerPrefs.SetInt("TimePlayed", 0);
        var Alph = eventSystem.currentSelectedGameObject;
        PlayerPrefs.SetString("SelectedAlphabet", Alph.name);
        PlayerPrefs.SetString("PrevSelectedAlph", Alph.name);
        PlayerPrefs.SetFloat("ContentPos", Alph.transform.parent.GetComponent<RectTransform>().anchoredPosition.x);
        DOTween.KillAll(false);
        cameFrom = "GamePlay";
        if (SoundHandler.instance.mySource.enabled == true)
        {
            SoundHandler.instance.PlaySource(c);
        }
        Invoke(nameof(LoadS), SoundHandler.instance.selectCh.length);
    
    }

    void LoadS()
    {
        //if (RateUsHandler.Instance.rate.activeInHierarchy)
        //{
        //    RateUsHandler.Instance.Cross();
        //}
        //if (InappPanel.Instance)
        //{
        //    if (InappPanel.Instance.removeAdsPanel.activeInHierarchy)
        //    {
        //        InappPanel.Instance.RemoveInAppPanel();
        //    }
        //}
        SceneManager.LoadScene("Gameplay");
    }
    void LoadH()
    {

        //if (Intitializeabc.instance.IsStaticInterAvailable())
        //{
        //    LOADING.showBannerEnd = false;
        //    LOADING.loadNextScene = true;
        //    LOADING.staticInter = true;
        //    PlayerPrefs.SetString("ReloadScene", "MainSelection");
        //    LOADING.gameObject.SetActive(true);
        //}
        //else
        {
            if (RateUsHandler.Instance.rate.activeInHierarchy)
            {
                RateUsHandler.Instance.Cross();
            }
            if (InappPanel.Instance)
            {
                if (InappPanel.Instance.removeAdsPanel.activeInHierarchy)
                {
                    InappPanel.Instance.RemoveInAppPanel();
                }
            }
            SceneManager.LoadScene("MainSelection");
        }
    }
}

