using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ArtefactSensor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Outline outline; 
    public bool isBoxObject; 

    private Artefact3DItem brain;

    private void Awake()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    private void OnDisable()
    {
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    public void Initialize(Artefact3DItem boss)
    {
        brain = boss;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (brain != null && brain.CanInteract(isBoxObject))
        {
            if (outline != null) outline.enabled = true;
            CursorController.instance?.SetCursorState(CursorState.Hover); // Show Hover Cursor
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null) outline.enabled = false;
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded); // Reset Cursor
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (brain != null && brain.CanInteract(isBoxObject))
        {
            if (outline != null) outline.enabled = false;
            CursorController.instance?.SetCursorState(CursorState.DefaultRounded); // Reset Cursor on click
            brain.OnSensorClicked(isBoxObject); 
        }
    }
}