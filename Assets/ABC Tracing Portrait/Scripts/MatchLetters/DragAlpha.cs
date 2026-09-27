using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DragAlpha : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform _rect;
    private Vector2 _pos;
    private GameObject _child;
    private Image _my;
    private Canvas _myCan;

    [Header("Correct Drop Target")]
    public RectTransform moveTo;

    [Header("Drag Settings")]
    public bool useDragOffset = true;

    private Vector2 _dragOffset;

    private bool _completed = false;
    private bool _up = false;

    private Camera _dragCamera;

    private void Awake()
    {
        _my = GetComponent<Image>();
        _myCan = GetComponent<Canvas>();
        _rect = GetComponent<RectTransform>();

        if (transform.childCount > 0)
            _child = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        // Make sure the object starts in the correct state
        if (_child != null)
            _child.SetActive(false);

        _my.raycastTarget = true;
    }

    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_completed)
            return;

        _up = false;

        // Kill any previous movement animation
        _rect.DOKill();

        // Save original position
        _pos = _rect.anchoredPosition;

        // Camera used for converting screen coordinates
        _dragCamera = eventData.pressEventCamera;

        if (_dragCamera == null &&
            _myCan.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            _dragCamera = _myCan.worldCamera;
        }

        // -----------------------------------------------------
        // Calculate the exact point where the user touched
        // -----------------------------------------------------

        if (useDragOffset)
        {
            Vector2 localPointerPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect.parent as RectTransform,
                    eventData.position,
                    _dragCamera,
                    out localPointerPosition))
            {
                _dragOffset =
                    _rect.anchoredPosition -
                    localPointerPosition;
            }
        }

        // Bring object to front
        if (_myCan != null)
            _myCan.sortingOrder = 10;

        // Show child
        if (_child != null)
            _child.SetActive(true);
    }

    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (_completed)
            return;

        RectTransform parentRect =
            _rect.parent as RectTransform;

        if (parentRect == null)
            return;

        Vector2 localPointerPosition;

        // Convert pointer screen position into parent's local space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                _dragCamera,
                out localPointerPosition))
        {
            if (useDragOffset)
            {
                // Maintain exact point where pointer touched object
                _rect.anchoredPosition =
                    localPointerPosition + _dragOffset;
            }
            else
            {
                // Directly follow pointer
                _rect.anchoredPosition =
                    localPointerPosition;
            }
        }
    }

    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_completed)
            return;

        _up = true;

        bool correctDrop = IsDraggedObjectOverTarget();

        Debug.Log(
            gameObject.name +
            " | Correct Drop = " +
            correctDrop
        );

        // =====================================================
        // CORRECT DROP
        // =====================================================

        if (correctDrop)
        {
            CompleteDrop();
        }

        // =====================================================
        // WRONG DROP
        // =====================================================

        else
        {
            WrongDrop();
        }
    }

    // =========================================================
    // CORRECT DROP
    // =========================================================

    private void CompleteDrop()
    {
        if (_completed)
            return;

        Vibration.Vibrate(50);

        if (MatchingManager.Instance.myS.enabled)
        {
            MatchingManager.Instance.myS.PlayOneShot(
                MatchingManager.Instance.correct
            );
        }

        if (MatchingManager.Instance.myS.enabled &&
            MatchingManager.Instance.voiceOver != null &&
            MatchingManager.Instance.voiceOver.Length > 0)
        {
            MatchingManager.Instance.myS.PlayOneShot(
                MatchingManager.Instance.voiceOver[
                    Random.Range(
                        0,
                        MatchingManager.Instance.voiceOver.Length
                    )
                ]
            );
        }

        _completed = true;

        // Stop receiving raycasts
        _my.raycastTarget = false;

        // Move exactly to target
        _rect.DOMove(
            moveTo.position,
            0.5f
        ).SetEase(Ease.OutQuad);

        IndicationHandler.Instance.timeSinceLastInput = 0;

        // Remove indication safely
        if (transform.childCount > 1)
        {
            RectTransform indication =
                transform.GetChild(1)
                    .GetComponent<RectTransform>();

            if (indication != null)
            {
                IndicationHandler.Instance.allIndi.Remove(
                    indication
                );
            }
        }

        // Scale down
        _rect.DOScale(
            Vector3.zero,
            0.5f
        )
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            MatchingManager.Instance.matched++;

            gameObject.SetActive(false);

            if (MatchingManager.Instance.size ==
                MatchingManager.Instance.matched)
            {
                MatchingManager.Instance.EndAnim();
            }
        });
    }

    // =========================================================
    // CHECK CORRECT TARGET
    // =========================================================

    private bool IsDraggedObjectOverTarget()
    {
        if (moveTo == null)
        {
            Debug.LogError(
                gameObject.name +
                " : moveTo is NOT assigned!"
            );

            return false;
        }

        Camera cam = null;

        if (_myCan.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = _myCan.worldCamera;
        }

        // -----------------------------------------------------
        // Dragged object center
        // -----------------------------------------------------

        Vector2 draggedCenter =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                _rect.position
            );

        // -----------------------------------------------------
        // Target corners
        // -----------------------------------------------------

        Vector3[] corners = new Vector3[4];

        moveTo.GetWorldCorners(corners);

        Vector2 bottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[0]
            );

        Vector2 topLeft =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[1]
            );

        Vector2 topRight =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[2]
            );

        Vector2 bottomRight =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[3]
            );

        float minX = Mathf.Min(
            bottomLeft.x,
            topLeft.x,
            topRight.x,
            bottomRight.x
        );

        float maxX = Mathf.Max(
            bottomLeft.x,
            topLeft.x,
            topRight.x,
            bottomRight.x
        );

        float minY = Mathf.Min(
            bottomLeft.y,
            topLeft.y,
            topRight.y,
            bottomRight.y
        );

        float maxY = Mathf.Max(
            bottomLeft.y,
            topLeft.y,
            topRight.y,
            bottomRight.y
        );

        bool inside =
            draggedCenter.x >= minX &&
            draggedCenter.x <= maxX &&
            draggedCenter.y >= minY &&
            draggedCenter.y <= maxY;

        Debug.Log(
            "Dragged Center: " +
            draggedCenter +
            " | Target Rect: " +
            minX + "," +
            minY +
            " -> " +
            maxX + "," +
            maxY +
            " | Inside: " +
            inside
        );

        return inside;
    }

    // =========================================================
    // WRONG DROP
    // =========================================================

    private void WrongDrop()
    {
        if (_child != null)
            _child.SetActive(false);

        _my.raycastTarget = false;

        Vibration.Vibrate(50);

        if (MatchingManager.Instance.myS.enabled)
        {
            MatchingManager.Instance.myS.PlayOneShot(
                MatchingManager.Instance.wrong
            );
        }

        // Reset rotation first
        _rect.DOKill();

        _rect.DORotate(
            new Vector3(0, 0, -10),
            0.1f
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            _rect.DORotate(
                new Vector3(0, 0, 10),
                0.1f
            )
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                _rect.DORotate(
                    Vector3.zero,
                    0.1f
                )
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Return to original position
                    _rect.DOAnchorPos(
                        _pos,
                        0.35f
                    )
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        if (_myCan != null)
                            _myCan.sortingOrder = 5;

                        _my.raycastTarget = true;
                    });
                });
            });
        });
    }
}