using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class BalloonGenerator : MonoBehaviour
{
    // Reference to the balloon prefab
    public GameObject []balloonPrefab;

    // Reference to the parent transform
    public Transform parentTransform;

    // Horizontal range for the random position
    public float horizontalMin;
    public float horizontalMax;

    // Vertical position for the balloon
    public float verticalPosition;

    // Duration to generate balloons
    public float duration = 10f;

    public LoadingHandler loader;
    // Time interval between balloon generation
    float interval = 0.25f;

    void Start()
    { 
        StartCoroutine(GenerateBalloons());
    }

    IEnumerator GenerateBalloons()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            GenerateBalloon();
            interval = Random.Range(.25f, .5f);
            yield return new WaitForSeconds(interval);
            elapsedTime += interval;
        }
        DOTween.KillAll(false);
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
        SceneManager.LoadScene("GamePlay");
    }

    void GenerateBalloon()
    {
        // Instantiate a new balloon instance
        int rand = Random.Range(0, balloonPrefab.Length);
        GameObject newBalloon = Instantiate(balloonPrefab[rand]);

        // Set the new balloon as a child of the parent transform
        newBalloon.transform.SetParent(parentTransform);
        newBalloon.transform.localScale = Vector3.one;
        // Generate a random horizontal position within the given range
        float randomHorizontalPosition = Random.Range(horizontalMin, horizontalMax);

        // Set the position of the new balloon
        newBalloon.transform.localPosition = new Vector3(randomHorizontalPosition, verticalPosition, 0f);

        // Optionally, you can set the local scale, rotation, etc. of the new balloon here
    }
    public void LoadNext()
    {
        Vibration.Vibrate(50);
        if(GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
        ABCManager.instance.shapeGameObject.SetActive(false);
        //if (IntitializeAdmob.instance.IsInterAvailable() || IntitializeAdmob.instance.IsStaticInterAvailable())
        //{
        //    StopAllCoroutines();
        //    PlayerPrefs.SetString("ReloadScene", "GamePlay");
        //    loader.loadNextScene = true;
        //    loader.gameObject.SetActive(true);
        //}
        //else
        {
            DOTween.KillAll(false);
            //if (RateUsHandler.Instance.rate.activeInHierarchy)
            //{
            //    RateUsHandler.Instance.Cross();
            //}
            //if (InappPanel.Instance)
            //{
            //    if (InappPanel.Instance.removeAdsPanel.activeInHierarchy)
            //    {
            //        InappPanel.Instance.RemoveInAppPanel();
            //    }
            //}
            SceneManager.LoadScene("Gameplay");
        }
        
    }
    
}
