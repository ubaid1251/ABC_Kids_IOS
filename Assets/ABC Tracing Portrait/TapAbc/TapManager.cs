using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class TapManager : MonoBehaviour, IPointerDownHandler
{
    public AudioSource mySource;
    public AudioClip[] AllClips;
    private int index = 0;
    public Sprite[] allSprites;
    public GameObject objectToInstantiate; // Reference to the GameObject to be instantiated
    public GameObject hand;
    public float handActiveDuration = 3f; // Duration for which the hand should be active
    public float handInactiveDuration = 3f; // Duration for which the hand should be inactive if no tap occurs
    public GameObject musicOn, musicOff;
    private Coroutine handCoroutine;
    public RectTransform home, music,BANNER;
    public GameObject loading;
    private void Start()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.ShowBanner();
        }
        else
        {
            BANNER.gameObject.SetActive(false);
            home.DOAnchorPosY(-110, 0);
            music.DOAnchorPosY(-110, 0);
        }

        if (PlayerPrefs.GetInt("bgm") == 1)
        {
            Camera.main.GetComponent<AudioSource>().volume = 0;
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
        handCoroutine = StartCoroutine(ManageHandActivation());
    }

    public void Home()
    {
        Vibration.Vibrate(50);
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
      //  InitializeFirebase_CB._Instance.LogFirebaseEvent("TapABC_Switched_ByHome");
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
           SceneManager.LoadScene("MainSelection");
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Vibration.Vibrate(30);
        if (handCoroutine != null)
        {
            StopCoroutine(handCoroutine);
        }
        
        hand.SetActive(false);
        // Play audio clip
        if (mySource.enabled)
            mySource.PlayOneShot(AllClips[index]);
        // Convert mouse position to world position
        Vector2 mousePosition = eventData.position;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0; // Ensure the object is placed at the correct z position (2D games typically use z = 0)

        // Instantiate the object at the mouse position
        if (objectToInstantiate != null)
        {
            var v = Instantiate(objectToInstantiate, worldPosition, Quaternion.identity, transform);
            v.GetComponent<Image>().sprite = allSprites[index];
            v.GetComponent<Image>().SetNativeSize();
            v.GetComponent<AlphabetFade>().enabled = true;
        }
        else
        {
            Debug.LogWarning("No object assigned to instantiate!");
        }
        index++;
        if (index >= AllClips.Length)
        {
            //if (Intitializeabc.instance.IsInterAvailable() || Intitializeabc.instance.IsStaticInterAvailable())
            //{
            //    loading.SetActive(true);
            //}
            index = 0;
        }

        // Start the coroutine to manage hand activation and deactivation
        handCoroutine = StartCoroutine(ManageHandActivation());
    }

    private IEnumerator ManageHandActivation()
    {
        int p = UnityEngine.Random.Range(-600, 600);
        hand.GetComponent<RectTransform>().DOAnchorPosY(p, 0);
        // Wait for the hand active duration before deactivating the hand
        yield return new WaitForSeconds(handActiveDuration);
        hand.SetActive(true);
        // Wait for the hand inactive duration before checking again
        yield return new WaitForSeconds(handInactiveDuration);
        hand.SetActive(false);
        handCoroutine = StartCoroutine(ManageHandActivation());
    }
}
