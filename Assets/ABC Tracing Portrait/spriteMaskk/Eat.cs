using UnityEngine;

public class Eat : MonoBehaviour
{
    // Public reference to the prefab to instantiate
    public GameObject itemPrefab;

    // This method is called when the mouse is clicked on the GameObject this script is attached to
    void OnMouseDown()
    {
        // Get the mouse position in world coordinates
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0; // Set z to 0 if you're in 2D

        // Instantiate the item prefab at the mouse position
        Instantiate(itemPrefab, mouseWorldPosition, Quaternion.identity);
    }
}