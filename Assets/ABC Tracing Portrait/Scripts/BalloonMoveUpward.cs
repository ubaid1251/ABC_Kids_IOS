using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class BalloonMoveUpward : MonoBehaviour, IPointerDownHandler
{
    // Speed at which the balloon will move upward
    float moveSpeed = 100f;

    // Y position at which the balloon will destroy itself
    public float destroyYPosition = 500f;

    // Reference to the RectTransform component
    private RectTransform rectTransform;
    public AudioClip[] popS;
    void Start()
    {
        TMP_Text t= transform.GetChild(0).GetComponent<TMP_Text>();
        t.text = null;
        t.text = PlayerPrefs.GetString("PrevSelectedAlph");
        //print(PlayerPrefs.GetString("PrevSelectedAlph"));
        moveSpeed = Random.Range(400, 600);
        // Get the RectTransform component
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Move the balloon upward
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // Check if the balloon has reached the destroy position
        if (rectTransform.anchoredPosition.y >= destroyYPosition)
        {
            Destroy(gameObject);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Vibration.Vibrate(50);
        Destroy(GetComponent<Image>());
        Destroy(transform.GetChild(0).gameObject);
        Destroy(transform.GetChild(1).gameObject);
        var v = ABCManager.instance.PopParticle;
        var t=Instantiate(v,transform.position,v.transform.rotation,transform);
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            t.GetComponent<AudioSource>().clip = popS[Random.Range(0, popS.Length)];
            t.GetComponent<AudioSource>().Play();
        }
    }
}
