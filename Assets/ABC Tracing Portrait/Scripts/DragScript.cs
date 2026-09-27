    using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragScript : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool isDragging = false;
    private bool isComplete = false;
    public GameObject next;
    public GameObject objectToMove; // Reference to the object you want to move

    [SerializeField]
    private float pointSpacing = 0.2f;
    [SerializeField]
    private float targetDistanceThreshold = 1.5f;
    [SerializeField]
    private int layer = 50;
    public Transform targetPosition, AnotherTarget;
    public ParticleSystem dragP;
    public float moveSpeed = 5f; // Adjust this value to control the speed of the object
    public float rotationSpeed = 360f; // Adjust this value to control the rotation speed
    public Transform startP, AnotherStartP;
    private int currentPositionIndex = 0;
    Collider2D boundsCollider; // Collider to define the dragging bounds
    public bool ChildAnim;
    public GameObject[] ForAnim;
    public LetterHandler myP;
    public bool switcher = false;
    public string ShaderName;
    public Shader toReplace;
    public Renderer[] Renderer;
    Shader my;
    public Animator[] allAnims;
    public GameObject particle;
    public AudioSource moveSound;
    EventSystem eve;
    void OnEnable()
    {
        if (myP)
        {
            Invoke(nameof(startIndi), 1);
        }
        eve=EventSystem.current;
        lineRenderer = GetComponent<LineRenderer>();
        boundsCollider = GetComponent<Collider2D>();
        ABCManager.instance.SetCurrentCol(boundsCollider);
        if (next != null)
        {
            //print("helootrace"+ GetComponent<LineRenderer>().startWidth);
            //print("helootrace end" + GetComponent<LineRenderer>().endWidth);
            //print("helootrace widthMultiplier " + GetComponent<LineRenderer>().widthMultiplier);

            if (PlayerPrefs.GetString("SelectedMode") == "NumbersLearning")
            {
                print("num");
                next.transform.GetChild(0).GetComponent<LineRenderer>().startWidth = 1.5f;
                next.transform.GetChild(0).GetComponent<LineRenderer>().endWidth = 1.5f;
                next.transform.GetChild(0).GetComponent<LineRenderer>().widthMultiplier = 1.5f;
            }
            else
            {
                print("alpha");
                next.transform.GetChild(0).GetComponent<LineRenderer>().startWidth = GetComponent<LineRenderer>().startWidth;
                next.transform.GetChild(0).GetComponent<LineRenderer>().endWidth = GetComponent<LineRenderer>().endWidth;
                next.transform.GetChild(0).GetComponent<LineRenderer>().widthMultiplier = GetComponent<LineRenderer>().widthMultiplier;
            }

            if (PlayerPrefs.GetString("SelectedAlphabet") == "m")
            {
                print("in small m");
                next.transform.GetChild(0).GetComponent<LineRenderer>().startWidth = 1.2f;
                next.transform.GetChild(0).GetComponent<LineRenderer>().endWidth = 1.2f;
                next.transform.GetChild(0).GetComponent<LineRenderer>().widthMultiplier = 1.2f;
            }
            
            // next.transform.GetChild(0).GetComponent<LineRenderer>().endWidth = 1.6f;
        }
    }
    void startIndi()
    {
        myP.StartIndi();
    }
    void Update()
    {
        if (Input.GetMouseButton(0) || isDragging)
        {
            

            if (!isComplete)
            {
                HandleInput();
                CheckMousePosition();
            }
            //else

        }
        if (isComplete)
        {
            if (islast)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    all[i].MoveObjectAlongLine();
                }
            }
        }
    }
    public DragScript[] all;
    public bool islast = false;
    void HandleInput()
    {
        if (isDragging)
        {
            ContinueDrag();
        }
        else
        {
            StartDrag();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (myP)
            {
                LetterHandler.canShowIndiNow=false;
            }
            if (!ABCManager.instance.tapS.isPlaying)
            {
                ABCManager.instance.tapS.clip = ABCManager.instance.tap;
                if (ABCManager.instance.tapS.enabled)
                    ABCManager.instance.tapS.Play();
            }
            StopAllCoroutines();
            if (myP)
            {
                myP.SetCurrentOff();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (myP)
            {
                LetterHandler.canShowIndiNow=true;
            }
            if (isDragging)
            {
                AddCurrentPositionToLineRenderer();
            }
            if (ChildAnim)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
            }
            if (!isComplete)
            {
                ResetLine();
                StopAllCoroutines();
                Invoke(nameof(OnIndi), 1);
            }
            StopDrag();
        }
    }
    void OnIndi()
    {
        if (!isDragging)
            myP.SetCurrentOn();
    }
    void StartDrag()
    {
        isDragging = true;
        UpdateLineRendererPosition();
    }

    void ContinueDrag()
    {
        UpdateLineRendererPosition();
    }

    void StopDrag()
    {
        dragP.gameObject.SetActive(false);
        isDragging = false;
        if (isComplete && myP)
        {
            myP.allIndi[myP.CurrentIndi].SetActive(false);
            myP.SetCurrentOff();
            myP.CurrentIndi++;

            if (myP.CurrentIndi >= myP.allIndi.Length)
            {
               ABCManager.instance.eraser.enabled = false;
                myP.complete=particle;
                myP.FComplete();
            }
        }
    }

    bool IsMouseOverUI()
    {
        // Check if the left mouse button is clicked
        if (eve.enabled)
        {
            // Create a PointerEventData to hold the cursor position
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            // Create a list of RaycastResults to store all the elements hit
            System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();

            // Raycast using the EventSystem and the PointerEventData
            eve.RaycastAll(eventData, results);

            // Return true if there are any results
            return results.Count > 0;
        }

        // Return false if the mouse button is not clicked
        return false;
    }

    void UpdateLineRendererPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        if (!IsMouseOverUI())
            dragP.transform.position = mousePosition;


        if (Vector3.Distance(startP.position, mousePosition) <= 2.5f || lineRenderer.positionCount > 0)
        {
            if (lineRenderer.positionCount == 0 || Vector3.Distance(lineRenderer.GetPosition(lineRenderer.positionCount - 1), mousePosition) >= pointSpacing)
            {
                if (IsInBounds(mousePosition))
                {
                    dragP.gameObject.SetActive(true);
                    if (ChildAnim && lineRenderer.positionCount == 0)
                    {
                        transform.GetChild(0).transform.position = startP.position;
                        transform.GetChild(0).gameObject.SetActive(true);
                    }
                    lineRenderer.positionCount++;
                    if (lineRenderer.positionCount == 1 && AnotherStartP == null)
                    {
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, startP.position);
                    }
                    else if (lineRenderer.positionCount == 1 && AnotherStartP != null)
                    {
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, AnotherStartP.position);
                    }
                    else
                    {
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);
                    }

                }
                else
                {
                    // Reset the line if dragged outside bounds
                    ResetLine();
                }
            }
        }
    }

    void AddCurrentPositionToLineRenderer()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Check if the distance between the last point and the current mouse position is greater than the specified spacing
        if (lineRenderer.positionCount == 0 || Vector3.Distance(lineRenderer.GetPosition(lineRenderer.positionCount - 1), mousePosition) >= pointSpacing)
        {
            if (IsInBounds(mousePosition))
            {
                lineRenderer.positionCount++;
                if (lineRenderer.positionCount == 1 && AnotherStartP == null)
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, startP.position);
                }
                else if (lineRenderer.positionCount == 1 && AnotherStartP != null)
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, AnotherStartP.position);
                }
                else
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);
                }
            }
            else
            {
                // Reset the line if dragged outside bounds
                ResetLine();
            }
        }
    }

    bool IsInBounds(Vector3 position)
    {
        // Create a ray from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        // Perform the raycast and check if it hits the collider
        return hit.collider != null && hit.collider == boundsCollider;
    }

    void ResetLine()
    {
        lineRenderer.positionCount = 0;
    }

    void CheckMousePosition()
    {
        if (lineRenderer.positionCount > 0)
        {
            if (isDragging && Vector3.Distance(lineRenderer.GetPosition(lineRenderer.positionCount - 1), targetPosition.position) < /*targetDistanceThreshold*/ 1.5f)
            {
                lineRenderer.sortingOrder = layer;
                if (AnotherTarget == null)
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, targetPosition.position);
                }
                else
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, AnotherTarget.position);
                }
                isComplete = true;
                Debug.Log("Mouse reached the target position!");
                StopDrag();
                if (ABCManager.instance.traceStepComplete.enabled)
                    ABCManager.instance.traceStepComplete.Play();

                ABCManager.instance.tapS.clip = ABCManager.instance.completeLine[Random.Range(0, ABCManager.instance.completeLine.Length)];
                if (ABCManager.instance.tapS.enabled)
                    ABCManager.instance.tapS.Play();

                Destroy(boundsCollider);
                if (next != null)
                {
                    next.SetActive(true);
                }
                for (int i = 0; i < ForAnim.Length; i++)
                {
                    ForAnim[i].SetActive(true);
                }
                if (ChildAnim)
                {
                    transform.GetChild(1).transform.position = targetPosition.position;
                    transform.GetChild(1).gameObject.SetActive(true);
                }

                // Snap the object to the starting position of the line
                if (objectToMove)
                    objectToMove.transform.position = lineRenderer.GetPosition(0);
                for (int i = 0; i < Renderer.Length; i++)
                {
                    my = toReplace;
                    Renderer[i].material.shader = my;
                }
                if (switcher)
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        all[i].GetComponent<TextureSwitcher>().enabled = true;
                    }
                }
                for (int i = 0; i < allAnims.Length; i++)
                {
                    allAnims[i].enabled = true;
                }
            }
        }
    }

    // Public method to start the movement from a specific point
    public void StartMoveObjectAlongLine()
    {
        currentPositionIndex = 0;
        MoveObjectAlongLine();
    }

    //public void MoveObjectAlongLine()
    //{
    //    objectToMove.SetActive(true);
    //    if (currentPositionIndex < lineRenderer.positionCount - 1)
    //    {
    //        float step = moveSpeed * Time.deltaTime;
    //        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, lineRenderer.GetPosition(currentPositionIndex + 1), step);

    //        // Calculate rotation towards the next point
    //        Vector3 direction = (lineRenderer.GetPosition(currentPositionIndex + 1) - objectToMove.transform.position).normalized;
    //        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

    //        // Smoothly rotate towards the target rotation
    //        objectToMove.transform.rotation = Quaternion.RotateTowards(objectToMove.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    //        if (Vector3.Distance(objectToMove.transform.position, lineRenderer.GetPosition(currentPositionIndex + 1)) < 0.001f)
    //        {
    //            currentPositionIndex++;
    //        }
    //    }
    //    else
    //    {
    //        objectToMove.SetActive(false);
    //        objectToMove.transform.position = lineRenderer.GetPosition(0);

    //        Vector3 direction = (lineRenderer.GetPosition(0) - objectToMove.transform.position).normalized;
    //        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //        objectToMove.transform.rotation = targetRotation;
    //        Invoke(nameof(StartMoveObjectAlongLine), 0.01f);
    //    }
    //}
    public void MoveObjectAlongLine()
    {
        objectToMove.SetActive(true);

        if (movingForward)
        {
            if (currentPositionIndex < lineRenderer.positionCount - 1)
            {
                MoveAndRotateTowards(currentPositionIndex + 1);
                if (Vector3.Distance(objectToMove.transform.position, lineRenderer.GetPosition(currentPositionIndex + 1)) < 0.001f)
                {
                    currentPositionIndex++;
                }
            }
            else
            {
                if (moveSound&& PlayerPrefs.GetInt("sfx")==0)
                    moveSound.Play();
                movingForward = false;
            }
        }
        else
        {
            if (currentPositionIndex > 0)
            {
               
                MoveAndRotateTowards(currentPositionIndex - 1);
                if (Vector3.Distance(objectToMove.transform.position, lineRenderer.GetPosition(currentPositionIndex - 1)) < 0.001f)
                {
                    currentPositionIndex--;
                }
            }
            else
            {
                if (moveSound&& PlayerPrefs.GetInt("sfx")==0)
                    moveSound.Play();

                movingForward = true;
            }
        }
    }

    private void MoveAndRotateTowards(int targetIndex)
    {
        float step = moveSpeed * Time.deltaTime;
        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, lineRenderer.GetPosition(targetIndex), step);

        // Calculate rotation towards the next point
        Vector3 direction = (lineRenderer.GetPosition(targetIndex) - objectToMove.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Smoothly rotate towards the target rotation
        objectToMove.transform.rotation = Quaternion.RotateTowards(objectToMove.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Initialize necessary variables
    //private int currentPositionIndex = 0;
    private bool movingForward = true;

}
