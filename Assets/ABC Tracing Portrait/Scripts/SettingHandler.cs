using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingHandler : MonoBehaviour
{
    public static SettingHandler instance;
    [HideInInspector]
    public GameObject panel;

    private void Start()
    {
        panel = transform.GetChild(0).gameObject;
        instance = this;
    }
    
    public void Cross()
    {
        SoundHandler.instance.PlaySource(SoundHandler.instance.mySource.clip);
        // InitializeFirebase_CB._Instance.LogFirebaseEvent("Disabling_Setting_Panel");
        InitializeFi._Instance.LogFi();
        transform.GetChild(0).GetComponent<Animator>().Play("PanelOut");
        Invoke(nameof(HidePanel), 0.9f);
    }
    void HidePanel()
    {
        Intitializeabc.instance.ShowBanner();
        panel.gameObject.SetActive(value: false);
    }
}
