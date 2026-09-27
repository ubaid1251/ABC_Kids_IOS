using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingCaller : MonoBehaviour
{
   public void ShowSetting()
    {
        Vibration.Vibrate(50);
       // InitializeFirebase_CB._Instance.LogFirebaseEvent("Showing_Setting_Panel");
        InitializeFi._Instance.LogFi();
        if (SoundHandler.instance.mySource.enabled == true)
            SoundHandler.instance.mySource.Play();
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.HideBanner();//remove later
        }
        SettingHandler.instance.panel.SetActive(true);
    }
}
