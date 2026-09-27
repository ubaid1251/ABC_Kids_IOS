using UnityEngine;
using DG.Tweening;
public class MaskCoverageChecker : MonoBehaviour
{
    public GameObject maskPrefab, parentForPrefab;
    public SpriteRenderer maskedObj, coloredObj;
    public int totalMasksToComplete = 450; // Total number of masks required to complete
    private int currentMaskCount = 0;
    private GameObject currentMask;
    private bool completed = false;

    void OnMouseDrag()
    {
        if (!completed)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0; // Ensure the mask is placed on the same z-axis
            currentMask = Instantiate(maskPrefab, worldPosition, Quaternion.identity);
            currentMask.transform.SetParent(parentForPrefab.transform);

            // Increment the mask count
            currentMaskCount++;

            // Check if the required number of masks have been instantiated
            
        }
    }
    private void OnMouseUp()
    {
        if (currentMaskCount >= totalMasksToComplete)
        {
            completed = true;
            DisplayCompletionMessage();
        }
    }
    void DisplayCompletionMessage()
    {
        maskedObj.DOFade(1, 1f);
        //for (int i = 0; i < parentForPrefab.transform.childCount; i++)
        //{
        //    Destroy(parentForPrefab.transform.GetChild(i).gameObject);
        //}
        Debug.Log("Image fully revealed!");
    }
}
