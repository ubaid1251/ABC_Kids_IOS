using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AboutPanelHandler : MonoBehaviour
{
    public GameObject[] panels;
    // Start is called before the first frame update
    public GameObject[] selectedIcons;
    public GameObject panel;
    public string PrivacyLink;

    private void OnEnable()
    {
        Intitializeabc.instance.HideBanner();//remove later
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        for (int i = 0; i < selectedIcons.Length; i++)
        {
            selectedIcons[i].SetActive(false);
        }
        panels[0].SetActive(true);
        selectedIcons[0].SetActive(true);
    }
    public void OpenPrivacy()
    {
        Vibration.Vibrate(50);
        if (SoundHandler.instance.mySource.enabled == true)
        {
            SoundHandler.instance.PlayClick();
        }
        Application.OpenURL(PrivacyLink);
    }
    public void ShowPanel(int index)
    {
        Vibration.Vibrate(50);
        if (SoundHandler.instance.mySource.enabled == true)
        {
            SoundHandler.instance.PlayClick();
        }
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        for (int i = 0; i < selectedIcons.Length; i++)
        {
            selectedIcons[i].SetActive(false);
        }
        selectedIcons[index].SetActive(true);
        panels[index].SetActive(true);
    }

    public void CloseAbout()
    {
        Vibration.Vibrate(50);
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        for (int i = 0; i < selectedIcons.Length; i++)
        {
            selectedIcons[i].SetActive(false);
        }
        panels[0].SetActive(true);
        selectedIcons[0].SetActive(true);
        gameObject.SetActive(false);
        if (SoundHandler.instance.mySource.enabled == true)
        {
            SoundHandler.instance.PlayTap();
        }
        if (PlayerPrefs.GetInt("RemoveAds") == 0)//remove later
            Intitializeabc.instance.ShowBanner();//remove later
    }
}
