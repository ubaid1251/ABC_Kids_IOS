using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MatchingManager : MonoBehaviour
{
    [HideInInspector] public List<int> picked;
    [HideInInspector] public List<Sprite> randomSp;

    public List<GameObject> bg;
    public RectTransform[] matchedTo, matchedWith;
    public Sprite[] matchedToSp, matchedWithSp;
    private int _st = 0, _des = 2, _showIndex = 0;
    public float showDuration = .5f;
    public static MatchingManager Instance;
     public int size = 0, matched = 0;
    public IndicationHandler handler;
    public AudioClip click, wrong, correct, matchSame;
    public AudioClip[] voiceOver, alphabets, objects, allbg;
    [HideInInspector] public AudioSource myS, mainCam;
    public GameObject completion;
    public GameObject loading,banner;
    public RectTransform home;
    private void Start()
    {
        mainCam = Camera.main.GetComponent<AudioSource>();
        mainCam.clip = allbg[Random.Range(0, allbg.Length)];
        if (mainCam.enabled)
            mainCam.Play();
        myS = GetComponent<AudioSource>();
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
            Intitializeabc.instance.ShowBanner();
        else
        {
            home.DOAnchorPosY(-100, 0);
            banner.SetActive(false);
        }
        bg[Random.Range(0, bg.Count)].SetActive(true);
        _st = PlayerPrefs.GetInt("StartMatch");
        _des = PlayerPrefs.GetInt("DestMatch");
        size = (_des - _st) + 1;
        for (int i = _st; i <= _des; i++)
        {
            picked.Add(i);
        }
        SetRandomSprites();
        Instance = this;
    }

    void SetRandomSprites()
    {
        if (picked.Count < size)
        {
            // Debug.LogError("Not enough elements in picked list.");
            return;
        }

        var pickedCopy = new List<int>(picked); // Create a copy of the list
        var tempRandomSp = new List<Sprite>(); // Temporary list for random sprites

        for (int i = 0, j = _st; i < size; i++, j++)
        {
            var randomIndex = Random.Range(0, pickedCopy.Count); // Get a random index in the copy list
            var temp = pickedCopy[randomIndex]; // Get the element at that index
            matchedTo[i].GetComponent<AudioSource>().clip = objects[temp];
            matchedTo[i].GetComponent<DragAlpha>().moveTo = matchedWith[(temp % 3)];
            tempRandomSp.Add(matchedToSp[temp]); // Add the corresponding sprite to the temp list
            pickedCopy.RemoveAt(randomIndex); // Remove the element from the copy list
            matchedWith[i].GetComponent<Image>().sprite = matchedWithSp[j];
            matchedWith[i].GetComponent<AudioSource>().clip = alphabets[j];
        }

        if (matchedTo.Length < size)
        {
            // Debug.LogError("Not enough elements in matchedTo array.");
            return;
        }

        for (int i = 0; i < size; i++)
        {
            if (i < matchedTo.Length) // Ensure index is within bounds
            {
                matchedTo[i].GetComponent<Image>().sprite = tempRandomSp[i];
                matchedTo[i].GetComponent<Image>().SetNativeSize();
            }
            else
            {
                // Debug.LogError("Index out of bounds for matchedTo array.");
                break;
            }
        }

        ShowDownItems();
    }

    void ShowDownItems()
    {
        matchedWith[_showIndex].gameObject.SetActive(true);
        if (myS.enabled)
            myS.PlayOneShot(click);
        matchedWith[_showIndex].DOScale(.4f, showDuration).OnComplete(() =>
        {
            _showIndex++;
            if (_showIndex < size)
            {
                ShowDownItems();
            }
            else
            {
                _showIndex = 0;
                ShowUpItems();
            }
        });
    }
    void ShowUpItems()
    {
        matchedTo[_showIndex].gameObject.SetActive(true);
        if (myS.enabled)
            myS.PlayOneShot(click);
        matchedTo[_showIndex].DOScale(.4f, showDuration).OnComplete(() =>
        {
            matchedTo[_showIndex].GetComponent<Image>().raycastTarget = true;
            _showIndex++;
            if (_showIndex < size)
            {
                ShowUpItems();
            }
            else
            {
                if (myS.enabled)
                    myS.PlayOneShot(matchSame);
                handler.enabled = true;
                matchedTo[0].transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
            }

        });
    }

    public void Home()
    {
        Vibration.Vibrate(50);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        myS.PlayOneShot(click);
       // InitializeFirebase_CB._Instance.LogFirebaseEvent(SceneManager.GetActiveScene().name+"_Switched_ByHome");
        InitializeFi._Instance.LogFi();
        DOTween.KillAll(false);
        //if (Intitializeabc.instance.IsStaticInterAvailable())
        //{
        //    PlayerPrefs.SetString("ReloadScene", "MainSelection");
        //    loading.GetComponent<LoadingHandler>().showBannerEnd = false;
        //    loading.GetComponent<LoadingHandler>().loadNextScene = true;
        //    loading.GetComponent<LoadingHandler>().staticInter = true;
        //    loading.SetActive(true);
        //}
        //else
        {
            SceneManager.LoadScene("MainSelection");
        }
    }

    public void EndAnim()
    {
        var n = _des + 1;
        PlayerPrefs.SetInt("StartMatch", n);
        if (n + 2 != 26)
            PlayerPrefs.SetInt("DestMatch", (n + 2));
        else
            PlayerPrefs.SetInt("DestMatch", (n + 1));

        if (myS.enabled)
            myS.PlayOneShot(click);
        matchedWith[2].DOScale(0, .5f).OnComplete(() =>
        {
            if (myS.enabled)
                myS.PlayOneShot(click);
            matchedWith[1].DOScale(0, .5f).OnComplete(() =>
            {
                if (myS.enabled)
                    myS.PlayOneShot(click);
                matchedWith[0].DOScale(0, .5f).OnComplete(() =>
                {
                    PlayerPrefs.SetInt("Turn", PlayerPrefs.GetInt("Turn") + 1);
                    PlayerPrefs.SetString("ReloadScene", n > 25 ? "MainSelection" : SceneManager.GetActiveScene().name);
                    int t = PlayerPrefs.GetInt("Turn");
                    if (t % 3 == 0)
                    {
                        print("Completed on end anim");
                        completion.SetActive(true);
                    }
                    else
                    {
                         DOTween.KillAll(false);
                        print("scene loaded end anim");
                        SceneManager.LoadScene(PlayerPrefs.GetString("ReloadScene"));
                    }
                });
            });
        });
    }
}
