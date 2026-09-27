using Coffee.UIExtensions;
using UnityEngine;

public class BackgroundColorChanger : MonoBehaviour
{
    //public Camera camera; // Reference to the Camera
    public float changeInterval = 3.0f; // Interval to change color
    public float changeInterval1= 3.0f; // Interval to change color
    public float lerpSpeed = 1.0f; // Speed of color transition
    public float lerpSpeed1 = 1.0f; // Speed of color transition

    private Color targetColor,targetColor1;
    private float timer,timer1;
    public UIGradient gradient;
    void Start()
    {
        SetRandomTargetColor();
    }

    void Update()
    {
        timer += Time.deltaTime;
        timer1 += Time.deltaTime;

        if (timer >= changeInterval)
        {
            SetRandomTargetColor();
            timer = 0;
        }
        if (timer1 >= changeInterval1)
        {
            SetRandomTargetColor1();
            timer1 = 0;
        }
        // Smoothly interpolate between the current color and the target color
        gradient.color1 = Color.Lerp(gradient.color1, targetColor, lerpSpeed * Time.deltaTime);
        gradient.color2 = Color.Lerp(gradient.color2, targetColor1, lerpSpeed1* Time.deltaTime);
    }

    void SetRandomTargetColor()
    {
        targetColor = new Color(Random.value, Random.value, Random.value);
    }
    void SetRandomTargetColor1()
    {
        targetColor1 = new Color(Random.value, Random.value, Random.value);
    }
}
