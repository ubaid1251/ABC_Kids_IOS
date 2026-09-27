using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ABCManager : MonoBehaviour
{
    public Transform page;
    public SpriteRenderer bg;
    GameObject shapePrefab = null;
    public AudioSource traceStepComplete;
    public static ABCManager instance;
    [HideInInspector] public GameObject shapeGameObject = null;
    public GameObject confetti;

    string[] colorCodes = { "A8D8EA", "AAE3A8", "FFD6A5", "FFB5E8","CDB4DB", "FFF3A3", "B8F2E6"};

    //public static int firebaseCounter = 0;
    public TMP_Text title, SelectedCharacter;
    public Eraser eraser;
    public Image filler;
    public GameObject CelebrationPanel;
    public GameObject PopParticle;
    public EventSystem eventSystem;
    public AudioClip tap;
    public AudioClip[] completeLine;
    [HideInInspector] public AudioSource tapS;
    public AudioSource ColorScratch;
    public GameObject balloonPanel;
    public GameObject LoadingAd;
    public RectTransform home, banner;

    void Start()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.ShowBanner(); //remove later
        }
        else
        {
            home.DOAnchorPosY(-100, 0);
            banner.gameObject.SetActive(false);
        }

        PlayerPrefs.SetString("PrevSelectedAlph", PlayerPrefs.GetString("SelectedAlphabet"));
        instance = this;
        tapS = GetComponent<AudioSource>();
        string Path = null;
        if (PlayerPrefs.GetString("SelectedMode") == "NumbersLearning")
        {
            Path = "123Learning";
        }
        else if (PlayerPrefs.GetString("SelectedMode") == "abcLearning")
        {
            Path = "SmallABCLearning";
        }
        else if (PlayerPrefs.GetString("SelectedMode") == "ABCLearning")
        {
            Path = "ABCLearning";
        }

        bg.color = HexToColor(colorCodes[Random.Range(0, colorCodes.Length)]);
        shapePrefab = Resources.Load<GameObject>(Path + "/" + PlayerPrefs.GetString("SelectedAlphabet"));
        SelectedCharacter.text = PlayerPrefs.GetString("SelectedAlphabet");
        shapeGameObject =
            Instantiate(shapePrefab, shapePrefab.transform.position, shapePrefab.transform.rotation, page);
        //shapeGameObject.transform.SetParent(page);
        shapeGameObject.transform.localPosition = new Vector3(0,-0.2f,0);
        shapeGameObject.transform.localScale = new Vector3(1.23f, 1.23f, 1.23f);
        if (PlayerPrefs.GetString("SelectedAlphabet") == "M" || PlayerPrefs.GetString("SelectedAlphabet") == "W" 
            || PlayerPrefs.GetString("SelectedAlphabet") == "m")
        {
            //print("I am M");
            shapeGameObject.transform.localScale = new Vector3(1f, 1f,1f);
        }
        if (PlayerPrefs.GetString("SelectedMode") == "NumbersLearning")
        {
            //print("I am M");
            shapeGameObject.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        }
        if (PlayerPrefs.GetString("SelectedAlphabet") == "i")
        {
            //print("I am M");
            shapeGameObject.transform.localScale = new Vector3(1.45f, 1.45f, 1.45f);
        }
        if (PlayerPrefs.GetString("SelectedAlphabet") == "10")
        {
            //print("I am M");
            shapeGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        eraser.p = shapeGameObject.GetComponent<LetterHandler>().ErasP;
        title.text = shapeGameObject.GetComponent<LetterHandler>().MyTitle;
        title.color = shapeGameObject.GetComponent<LetterHandler>().ColorForText;
    }

    public void SetEraseEnvOn()
    {
        if (shapeGameObject != null)
        {
            if (shapeGameObject.GetComponent<LetterHandler>().currCol != null)
            {
                shapeGameObject.GetComponent<LetterHandler>().currCol.enabled = false;
                shapeGameObject.GetComponent<Collider2D>().enabled = true;
            }
        }

    }

    public void SetEraseEnvOff()
    {
        if (shapeGameObject != null)
        {
            if (shapeGameObject.GetComponent<LetterHandler>().currCol != null)
            {
                shapeGameObject.GetComponent<LetterHandler>().currCol.enabled = true;
                shapeGameObject.GetComponent<Collider2D>().enabled = false;
            }
        }

    }

    public void SetCurrentCol(Collider2D col)
    {
        shapeGameObject.GetComponent<LetterHandler>().currCol = col;
    }

    public void ShowCelebration()
    {
        float fillerAm = PlayerPrefs.GetFloat("FillAmount");
        filler.DOFillAmount(fillerAm, 0);
        if (fillerAm < .9f)
        {
            CelebrationPanel.SetActive(true);
            PlayerPrefs.SetFloat("FillAmount", PlayerPrefs.GetFloat("FillAmount") + .33f);
            fillerAm = PlayerPrefs.GetFloat("FillAmount");
            filler.DOFillAmount(fillerAm, 2).OnComplete(() =>
            {
                if (fillerAm < .9f)
                {
                    //PlayerPrefs.SetFloat("FillAmount", 0);
                    //PlayerPrefs.SetInt("TimePlayed", 0);    
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                else
                {
                    CelebrationPanel.SetActive(false);

                    ShowAnim();
                }
            });
        }

    }

    public void ShowAnim()
    {
        //InitializeFirebase_CB._Instance.LogFirebaseEvent("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_Completed");
//        Debug.Log("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_Completed");

        PlayerPrefs.SetString("SelectedAlphabet", shapeGameObject.GetComponent<LetterHandler>().nextL);
        PlayerPrefs.SetInt("TimePlayed", 0);
        PlayerPrefs.SetFloat("FillAmount", 0);

        Destroy(shapeGameObject);
        eraser.enabled = false;
        eraser.GetComponent<Image>().raycastTarget = false;
        shapePrefab = Resources.Load<GameObject>("RevealColor/" + title.text);
        shapeGameObject = Instantiate(shapePrefab, page);
        //shapeGameObject.SetActive(false);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        //if (IntitializeAdmob.instance.IsInterAvailable()||IntitializeAdmob.instance.IsStaticInterAvailable()) //remove later
        //{
        //    LoadingAd.GetComponent<LoadingHandler>().ActiveAfter = shapeGameObject;
        //    LoadingAd.SetActive(true);
        //}
        //else
        {
            shapeGameObject.SetActive(true);
            // if (RateUsHandler.Instance.CheckRateCondition())
            // {
            //     RateUsHandler.Instance.rate.SetActive(true);
            // }
        }
    }

    Color HexToColor(string hex)
    {
        // Remove the '#' from the beginning
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Parse hex color to integer
        int hexInt = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        // Extract individual color components
        float r = ((hexInt >> 16) & 0xFF) / 255.0f;
        float g = ((hexInt >> 8) & 0xFF) / 255.0f;
        float b = (hexInt & 0xFF) / 255.0f;

        // Create Color object
        Color color = new Color(r, g, b);

        return color;
    }

    public void Home()
    {
        Vibration.Vibrate(50);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        eventSystem.enabled = false;
        //InitializeFirebase_CB._Instance.LogFirebaseEvent("HomePressed_Gameplay_On_" +
        //                                                 PlayerPrefs.GetString("SelectedMode") +
        //                                                 "_Mode"); //remove later

        InitializeFi._Instance.LogFi();
        DOTween.KillAll(false);
        //if (SoundHandler.instance.mySource.enabled == true)
        //{
        //    SoundHandler.instance.PlayTap();
        //}
        if (RateUsHandler.Instance.rate.activeInHierarchy)
        {
            RateUsHandler.Instance.Cross();
        }
        if (InappPanel.Instance)
        {
            if (InappPanel.Instance.removeAdsPanel.activeInHierarchy)
            {
                InappPanel.Instance.HideRemoveAdsPanel();
            }
        }
        SceneManager.LoadScene("SubSelection");
    }

    //void LoadS()
    //{
    //    SceneManager.LoadScene("SubSelection");
    //}
    public void destroyP(GameObject g)
    {
        Destroy(g, 1);
    }
}
