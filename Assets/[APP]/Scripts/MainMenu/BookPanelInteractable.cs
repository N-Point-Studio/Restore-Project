using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BookPanelInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Action OnBookHoverEnter;
    public Action OnBookHoverExit;
    public Action OnBookClicked;

    public void OnPointerEnter(PointerEventData eventData) => OnBookHoverEnter?.Invoke();
    
    public void OnPointerExit(PointerEventData eventData) => OnBookHoverExit?.Invoke();
    
    public void OnPointerClick(PointerEventData eventData) => OnBookClicked?.Invoke();
}