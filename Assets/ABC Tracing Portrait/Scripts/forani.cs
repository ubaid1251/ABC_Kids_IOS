using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class forani : MonoBehaviour
{
    public ScrollRect scroll;
   public void offAnim()
    {
        scroll.enabled = true;
        GetComponent<Animator>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
