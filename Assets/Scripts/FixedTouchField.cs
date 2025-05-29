using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FixedTouchField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public Vector2 TouchDist;
    [HideInInspector] public Vector2 PointerOld;
    [HideInInspector] protected int PointerId;
    [HideInInspector] public bool Pressed;

    public float touchScale = 1f; // Balanced touch scale for Android

    void Update()
    {
        if (Pressed && PointerId >= 0 && PointerId < Input.touchCount)
        {
            Vector2 currentPos = Input.touches[PointerId].position;
            Vector2 delta = currentPos - PointerOld;
            TouchDist = delta * touchScale;
            PointerOld = currentPos;
        }
        else
        {
            TouchDist = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        PointerId = eventData.pointerId;
        PointerOld = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
    }
}
