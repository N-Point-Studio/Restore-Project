using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArtefactSensor : MonoBehaviour
{
    [SerializeField] private Outline outline; 
    public bool isBoxObject; 

    private Artefact3DItem brain;

    private void Awake()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void Initialize(Artefact3DItem boss)
    {
        brain = boss;
    }

    private void OnMouseEnter()
    {
        if (brain != null && brain.CanInteract(isBoxObject))
        {
            if (outline != null) outline.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        if (outline != null) outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (brain != null && brain.CanInteract(isBoxObject))
        {
            if (outline != null) outline.enabled = false;
            brain.OnSensorClicked(isBoxObject); 
        }
    }
}