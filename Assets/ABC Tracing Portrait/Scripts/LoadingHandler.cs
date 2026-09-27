using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoadingHandler : MonoBehaviour
{
    public GameObject ActiveAfter;
    public bool showBannerEnd = false;
    public bool loadNextScene=false;
    public bool staticInter = false;
    private void OnEnable()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            if (ActiveAfter != null)
            {
                ActiveAfter.SetActive(true);
                if (SceneManager.GetActiveScene().name == "Gameplay")
                {
                    if (RateUsHandler.Instance.CheckRateCondition())
                    {
                        RateUsHandler.Instance.rate.SetActive(true);
                    }
                }
            }
            gameObject.SetActive(false);
        }
    }
    
    public void ShowInter()
    {
        if (staticInter)
        {
            //staticInter = false;
            //if (Intitializeabc.instance.IsStaticInterAvailable())
            //{
            //    if (SceneManager.GetActiveScene().name == "ColorGamePlay")
            //    {
            //        showBannerEnd = true;
            //    }
            //    Intitializeabc.instance.ShowStaticInterstitial();//remove later
            //}
        }
        else
        {
            //if (Intitializeabc.instance.IsInterAvailable() || Intitializeabc.instance.IsStaticInterAvailable())
            //{
            //    if (SceneManager.GetActiveScene().name == "ColorGamePlay")
            //    {
            //        showBannerEnd = true;
            //    }
            //    Intitializeabc.instance.ShowInterstitialAd();//remove later
            //}
        }
    }
    public void EndAnim()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0&&showBannerEnd)
        {
            if (SceneManager.GetActiveScene().name == "ColorGamePlay")
            {
                showBannerEnd = false;
                Intitializeabc.instance.ShowBanner();//remove later
            }
            else
            {
                Intitializeabc.instance.ShowBanner();//remove later
            }
        }

        if (ActiveAfter != null)
        {
            ActiveAfter.SetActive(true);
            // if (SceneManager.GetActiveScene().name == "Gameplay")
            // {
            //     if (RateUsHandler.Instance.CheckRateCondition())
            //     {
            //         RateUsHandler.Instance.rate.SetActive(true);
            //     }
            // }
        }
        
        if (loadNextScene)
        {
            DOTween.KillAll(false);
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
            SceneManager.LoadScene(PlayerPrefs.GetString("ReloadScene"));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
