using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainSelection : MonoBehaviour
{
    // Start is called before the first frame update
    public EventSystem eventSystem;
    public ScrollRect scroll;
    //public RectTransform content;
    public float[] allpos;
    public RectTransform adult, setting, noAds,banner,mainCards, scrollObj;
    public GameObject obj,obj2;
    void Start()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            adult.DOAnchorPosY(-100, 0);
            mainCards.DOAnchorPosY(450, 0);
            setting.DOAnchorPosY(-100, 0);
            scrollObj.DOAnchorPosY(-120, 0);
            noAds.gameObject.SetActive(false);
            banner.gameObject.SetActive(false);
            obj2.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 220f);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 700f);
        }



        //if (PlayerPrefs.GetInt("RemoveAds") == 0&& ResCheck.ResolutionType == ResType.tab)
        //{
        //    content.DOAnchorPosY(500, 0);
        //}
        //else if(PlayerPrefs.GetInt("RemoveAds") == 1 && ResCheck.ResolutionType == ResType.tab)
        //{
        //    content.DOAnchorPosY(600, 0);
        //}
        //SongManager.counter = 0;
        PlayerPrefs.SetInt("Turn", 0);
        PlayerPrefs.SetInt("StartMatch",0);
        PlayerPrefs.SetInt("DestMatch",2);
        //eventSystem.enabled = false;
        int i=PlayerPrefs.GetInt("ModeIndex");
        //if (i <3)
        //{
        //    content.DOAnchorPosX(allpos[allpos.Length-1], .5f).SetEase(Ease.Linear).OnComplete(() =>
        //    {
        //        content.DOAnchorPosX(allpos[i], .5f).SetEase(Ease.Linear).OnComplete(() =>
        //        {
        //            if (RateUsHandler.Instance.CheckRateCondition())
        //            {
        //                RateUsHandler.Instance.rate.SetActive(true);
        //            }
        //            else
        //            {
        //                IntitializeAdmob.instance.ShowBanner();//remove later
        //            }
        //            scroll.enabled = true;
        //            eventSystem.enabled = true;
                    
        //        });
        //    });
        //}
        //else
        //{
        //    content.DOAnchorPosX(allpos[0], .5f).SetEase(Ease.Linear).OnComplete(() =>
        //    {
        //        content.DOAnchorPosX(allpos[i], .5f).SetEase(Ease.Linear).OnComplete(() =>
        //        {
        //            if (RateUsHandler.Instance.CheckRateCondition())
        //            {
        //                RateUsHandler.Instance.rate.SetActive(true);
        //            }
        //            else
        //            {
        //                IntitializeAdmob.instance.ShowBanner();//remove later
        //            }
        //            scroll.enabled = true;
        //            eventSystem.enabled = true;
                    
        //        });
        //    });
        //}
    }

    public void SetModeIndex(int t)
    {
        PlayerPrefs.SetInt("ModeIndex",t);
    }
    public void SetModeIndexForColoring(int t)
    {
            PlayerPrefs.SetInt("SelectedMode",t);
    }
    public void LoadSubSelection(string mode)
    {
        eventSystem.enabled = false;
        PlayerPrefs.SetString("SelectedMode", mode);
        SubSelection.cameFrom = "MainSelection";
        
       //InitializeFirebase_CB._Instance.LogFirebaseEvent(mode + "_SelectedMode");
        DOTween.KillAll(false);
        if (SoundHandler.instance.mySource.enabled)
        {
            SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
        }
        Invoke(nameof(LoadS), SoundHandler.instance.selectCh.length);
        
    }

    public void LoadOtherLevel(string name)
    {
        eventSystem.enabled = false;
        SubSelection.cameFrom = "MainSelection";
        //InitializeFirebase_CB._Instance.LogFirebaseEvent(name + "_SelectedMode");
        InitializeFi._Instance.LogFi();
        DOTween.KillAll(false);
        if (SoundHandler.instance.mySource.enabled)
        {
            SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
        }
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
        SceneManager.LoadScene(name);
    }
    void LoadS()
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
        SceneManager.LoadScene("SubSelection");
    }

    public void LogEvent(string n)
    {
        //InitializeFirebase_CB._Instance.LogFirebaseEvent("User_Trying_to_Open_"+n);
        InitializeFi._Instance.LogFi();
    }
    public void showAdult(GameObject p)
    {
        Vibration.Vibrate(50);
        if (SoundHandler.instance.mySource.enabled)
            SoundHandler.instance.mySource.Play();
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.HideBanner();//remove later
        }
        p.SetActive(true);
    }
}
