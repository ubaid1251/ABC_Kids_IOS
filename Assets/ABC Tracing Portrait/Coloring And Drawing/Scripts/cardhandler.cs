using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class cardhandler : MonoBehaviour
{
    public static cardhandler instance;
    public RectTransform home, setting;
    public GameObject loading,ad;
    public RectTransform []contents;
    public GameObject []Mode;
    public RectTransform[] title;
    public RectTransform content2;
    private void Awake()
    {
        ToolGrid.SelectedTool = 0;
        if (ResCheck.ResolutionType == ResType.tab)
        {
            print("i am tab");
            content2.localScale=new Vector3(0.9f, 0.9f, 0.9f);
        }
    }
    private void OnEnable()
    {
        instance = this;
        if (PlayerPrefs.GetInt("RemoveAds") != 0)//remove after
        {
            ad.SetActive(false);
            home.DOAnchorPosY(-100, 0);
            setting.DOAnchorPosY(-100, 0);
            for (int i = 0; i < title.Length; i++)
            {
                title[i].DOAnchorPosY(-216, 0);
            }
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i].DOAnchorPosY(50, 0);
            }

            //remove after
        }
        int index = PlayerPrefs.GetInt("SelectedMode");
        //if (IntitializeAdmob.instance.IsStaticInterAvailable())
        //{
        //    loading.GetComponent<LoadingHandler>().showBannerEnd=true;
        //    loading.GetComponent<LoadingHandler>().staticInter=true;
        //    loading.GetComponent<LoadingHandler>().ActiveAfter= Mode[index];
        //    loading.SetActive(true);
        //}
        //else
        //{
        //    IntitializeAdmob.instance.ShowBanner();
            Mode[index].SetActive(true);
        //}
    }
    public void Home()
    {
        Intitializeabc.instance.HideBanner();//remove after   
        //SoundManager.instance.PlayButtonSound(0);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        SoundHandler.instance.PlayTap();
        //if (Intitializeabc.instance.IsStaticInterAvailable())//remove after
        //{
        //    PlayerPrefs.SetString("ReloadScene","MainSelection");
        //    loading.GetComponent<LoadingHandler>().loadNextScene = true;
        //    loading.GetComponent<LoadingHandler>().staticInter = true;
        //    loading.SetActive(true);
        //}
        //else
        {
            SceneManager.LoadScene("MainSelection");
        }
    }
}
