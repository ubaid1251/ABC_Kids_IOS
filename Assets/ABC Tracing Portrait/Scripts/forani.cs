using UnityEngine;

public class forani : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void offAnim()
    {
        GetComponent<Animator>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
