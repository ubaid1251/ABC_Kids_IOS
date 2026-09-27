using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
//using Firebase.Analytics;

public class Star_Controller : MonoBehaviour
{
    public GameObject[] Star;
    public AudioSource PlaySound;
    public RectTransform cross;
    //public GameObject hand;
    private void Start()
    {
        //if (ResCheck.instance.resType == ResType.tab)
        //{
        //    transform.GetChild(0).GetComponent<RectTransform>().DOScale(.25f, 0);
        //    transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosX(-12f, 0);
        //}
    }
    public IEnumerator PlayMyAudio()
    {
        AudioSource s = GetComponent<AudioSource>();
        yield return new WaitForSeconds(2);
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            s.enabled = true;
            s.Play();
        }
        //hand.SetActive(true);
        float l = s.clip.length;
        yield return new WaitForSeconds(l);
        DisableHand();
        yield return new WaitForSeconds(1);
        StartCoroutine(PlayMyAudio());
    }

    void DisableHand()
    {
        //hand.SetActive(false);
    }
    private void OnEnable()
    {
        cross.DOScale(0f, 0.6f).OnComplete(() =>
        {
            cross.DOScale(1.1f, 1);
        });
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.HideBanner();
        }
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            StartCoroutine(PlayMyAudio());
        }
        else
        {
            GetComponent<AudioSource>().enabled = false;
        }
    }

    public void RateStar(int selectedStar)
    {
        StopAllCoroutines();
        //hand.SetActive(false);
        StartCoroutine(StarActive(selectedStar));
    }

    public IEnumerator StarActive(int selectedStars = 1)
    {
        //yield return new WaitForSeconds(0.6f);
        for (int i = 0; i < selectedStars; i++)
        {
            Star[i].SetActive(true);
            Star[i].transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(0.02f);
            if (PlayerPrefs.GetInt("sfx") == 0)
                PlaySound.PlayOneShot(PlaySound.clip);
            yield return new WaitForSeconds(0.15f);
        }
     //   InitializeFirebase_CB._Instance.LogFirebaseEvent("Rate_Game_Rated_With_" + selectedStars.ToString() + "_Stars");
        InitializeFi._Instance.LogFi();

        if (selectedStars < 4)
        {
            RateUsHandler.Instance.Cross();
        }
        else
        {
            RateUsHandler.Instance.Rate();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        cross.DOScale(0, 0);
        for (int i = 0; i < Star.Length; i++)
        {
            Star[i].SetActive(false);
            Star[i].transform.DOScale(2f, 0f);
        }
    }
    public void Deactive()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.ShowBanner();
        }
        gameObject.SetActive(false);
    }
}
