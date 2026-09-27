using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
public class InappPanel : MonoBehaviour
{
    public static InappPanel Instance;
    public GameObject removeAdsPanel,hand;
    public RectTransform cross;
    //public InAppCalling_CB buy;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void ShowRemoveAdsPanel()
    {
        SoundHandler.instance.PlayTap();
        removeAdsPanel.SetActive(true);
        removeAdsPanel.transform.DOScale(1, 1).OnComplete(() =>
        {
            hand.SetActive(true);
        });
        Invoke(nameof(showCross),2);
    }

    void showCross()
    {
        cross.DOScale(1.2f, 1);
    }
    public void HideRemoveAdsPanel()
    {
        hand.SetActive(false);
        CancelInvoke(nameof(showCross));
        SoundHandler.instance.PlaySource(SoundHandler.instance.mySource.clip);
        removeAdsPanel.GetComponent<Animator>().Play("PanelOut");
      //  InitializeFirebase_CB._Instance.LogFirebaseEvent("Inapp_CrossBtn_Pressed"); //lock
        InitializeFi._Instance.LogFi();
        Invoke(nameof(HidePanel), 0.9f);
    }
    void HidePanel()
    {
        Intitializeabc.instance.ShowBanner();
        cross.DOScale(0, 0);
        removeAdsPanel.SetActive(false);
    }
    public void RemoveInAppPanel()
    {
        hand.SetActive(false);
        CancelInvoke(nameof(showCross));
        removeAdsPanel.SetActive(false);
    }
    
    public void BuyInapp()
    {
        //if (buy)
        //{
        //    hand.SetActive(false);
        //    SoundHandler.instance.PlayTap();
        //    buy.BuyInApp();
        //}
    }
    
}
