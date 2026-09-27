using System;
using System.Collections.Generic;
using UnityEngine;


public class Intitializeabc : MonoBehaviour
{

    public static Intitializeabc instance;


    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
      //  Debug.unityLogger.logEnabled = showLog;
    }

    public void ShowInterstitial()
    {
       
    }

    public void ShowStaticInterstitial()
    {

    }
 
    public void ShowInterstitialAd()
    {
        ShowInterstitial();
    }



    public void ShowBanner()
    {
      
    }

    public void HideBanner()
    {

    }

    public void DestroyBannerView()
    {

    }


  
}

