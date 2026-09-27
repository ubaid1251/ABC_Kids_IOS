using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class CheckReso : MonoBehaviour
{
    public GameObject obj1, obj2;
    void Start()
    {
        if (ResCheck.ResolutionType == ResType.iphonex)
        {
            obj1.SetActive(true);
        }
        else
        {
            obj2.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
