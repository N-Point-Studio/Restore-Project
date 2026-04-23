using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BookPanelInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Action OnBookHoverEnter;
    public Action OnBookHoverExit;
    public Action OnBookClicked;

    private void OnDisable()
    {
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        OnBookHoverEnter?.Invoke();
        CursorController.instance?.SetCursorState(CursorState.Hover);
    }
    
    public void OnPointerExit(PointerEventData eventData) 
    {
        OnBookHoverExit?.Invoke();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }
    
    public void OnPointerClick(PointerEventData eventData) 
    {
        OnBookClicked?.Invoke();
        // Optional: Reset on click if opening the book changes the UI context
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded); 
    }
}