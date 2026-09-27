using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LetterHandler : MonoBehaviour
{
    public GameObject[] allIndi,movingArrow;
    public int CurrentIndi = 0;
    public GameObject[] allTexture;
    [HideInInspector]
    public GameObject complete;
    public Transform ErasP;
    public string nextL;
    [HideInInspector]
    public Collider2D currCol;
    public string MyTitle;
    public Color ColorForText;
    AudioSource mySource;
    public AudioClip Letter, traceThe;
    public static bool canShowIndiNow = true;
    private void OnEnable()
    {
        canShowIndiNow = true;
        mySource = GetComponent<AudioSource>();
        StartCoroutine(TraceTheLetter());

    }
    IEnumerator TraceTheLetter()
    {
        //mySource.clip = Letter;
        //mySource.Play();
        //yield return new WaitForSeconds(Letter.length+.2f);
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            mySource.clip = traceThe;
            mySource.Play();
        }

        yield return new WaitForSeconds(traceThe.length + .2f);
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            mySource.clip = Letter;
            mySource.Play();
        }

        yield return new WaitForSeconds(Letter.length);
    }
    public void FComplete()
    {
        complete.gameObject.SetActive(true);
        print("completed");
        GetComponent<SpriteRenderer>().enabled = false;
        Invoke(nameof(LoadNextWord), 4);
    }
    void LoadNextWord()
    {
        PlayerPrefs.SetInt("TimePlayed", PlayerPrefs.GetInt("TimePlayed") +1);
        if (PlayerPrefs.GetInt("TimePlayed") >=3)
        {
            //ABCManager.firebaseCounter = 0;
            //InitializeFirebase_CB._Instance.LogFirebaseEvent("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_Completed");//remove later
            Debug.Log("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_Completed");
            PlayerPrefs.SetString("SelectedAlphabet",nextL);
            ABCManager.instance.ShowCelebration();
        }
        /*else if(ABCManager.instance.RateUsEnable && PlayerPrefs.GetInt("TimePlayed") >= 3)
        {
            ABCManager.instance.RateUsPanel.SetActive(true);
        }*/
        else
        {
            //InitializeFirebase_CB._Instance.LogFirebaseEvent("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_nextTexture");//remove later
            Debug.Log("Letter_" + PlayerPrefs.GetString("SelectedAlphabet") + "_nextTexture");
            ABCManager.instance.ShowCelebration();
        }
    }
    void loadNext()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void Start()
    {
        int t = findUnique();
        allTexture[t].SetActive(true);
    }
    int findUnique()
    {
        int t, tt;
        t = PlayerPrefs.GetInt(name);
        do
        {
            tt = Random.Range(0, allTexture.Length);
        }
        while (t == tt);
        PlayerPrefs.SetInt(name,tt);
        return tt;
    }

    public void StartIndi()
    {
        // if (canShowIndiNow)
        {
            if (CurrentIndi < allIndi.Length)
            {
                allIndi[CurrentIndi].SetActive(true);
            }
        }
    }

    public void SetCurrentOff()
    {
        if (CurrentIndi < movingArrow.Length)
            movingArrow[CurrentIndi].SetActive(false);
    }

    public void SetCurrentOn()
    {
        // if (canShowIndiNow)
        {
            if (CurrentIndi < movingArrow.Length)
                movingArrow[CurrentIndi].SetActive(true);
        }
    }
}
