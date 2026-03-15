using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Artefact3DItem : MonoBehaviour
{
    [Header("Artefact Configuration")]
    [SerializeField] private string artefactId; 
    
    [Header("3D Visuals")]
    [SerializeField] private Transform artefactTransform; 
    [SerializeField] private Transform boxTransform;      

    [Header("Sticky Notes Visual")]
    [SerializeField] private StickyNote3DItem[] stickyNoteObjects; 
    
    [Header("Highlight System")]
    [SerializeField] private Outline boxOutline;      // Outline for box
    [SerializeField] private Outline artefactOutline; // Outline for artefact
    
    private Outline activeOutline; // Which outline is currently used (box/artefact)

    private ArtefactData artefactData;
    private bool isInteractable = false;
    
    // State to differentiate clicks in Level Selection vs Detail
    private bool isInDetailMode = false;
    private bool hasUnpeeledNotes = false;

    public string ArtefactId => artefactId;

    private void Awake()
    {
        // Make sure both outlines are off when the game starts
        if (boxOutline != null) boxOutline.enabled = false;
        if (artefactOutline != null) artefactOutline.enabled = false;
    }

    public void Initialize(ActiveArtefactData activeData)
    {
        artefactData = activeData.GetArtefactDatabase().GetItem(artefactId);
        if (artefactData == null) return;

        bool isUnlocked = activeData.IsArtefactUnlocked(artefactId);
        bool isCompleted = activeData.IsArtefactCompleted(artefactId);
        bool isStoryRead = activeData.IsStoryRead(artefactId);

        if (isCompleted)
        {
            artefactTransform.gameObject.SetActive(true);
            boxTransform.gameObject.SetActive(false);
            isInteractable = true;
            
            activeOutline = artefactOutline; // Use artefact outline
        }
        else if (isUnlocked)
        {
            artefactTransform.gameObject.SetActive(false);
            boxTransform.gameObject.SetActive(true);
            isInteractable = true;
            
            activeOutline = boxOutline; // Use box outline
        }
        else
        {
            artefactTransform.gameObject.SetActive(false);
            boxTransform.gameObject.SetActive(false);
            isInteractable = false;
            
            activeOutline = null; // Nothing can be hovered
        }

        hasUnpeeledNotes = !isCompleted && !isStoryRead;

        if (stickyNoteObjects != null)
        {
            foreach (var note in stickyNoteObjects)
            {
                if (note != null) note.gameObject.SetActive(hasUnpeeledNotes);
            }
        }
    }

    // Called by ArtefactDetailController when detail menu is opened/closed
    public void SetDetailMode(bool isDetail, bool notesRemaining)
    {
        isInDetailMode = isDetail;
        hasUnpeeledNotes = notesRemaining;
        
        if (activeOutline != null) activeOutline.enabled = false; // Turn off outline during transition
    }

    private void OnMouseEnter()
    {
        if (!isInteractable || artefactData == null) return;
        
        // BLOCK HIGHLIGHT: If in Detail Mode, box MUST NOT be hovered while notes still exist
        if (isInDetailMode && hasUnpeeledNotes) return;

        if (activeOutline != null) activeOutline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (activeOutline != null) activeOutline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (!isInteractable || artefactData == null) return;
        
        // BLOCK CLICK: If in Detail Mode and notes still exist, ignore click on box
        if (isInDetailMode && hasUnpeeledNotes) return;

        if (activeOutline != null) activeOutline.enabled = false;

        if (isInDetailMode)
        {
            // If clicked in Detail Mode (and notes are gone), play directly!
            MainMenuEvents.TriggerArtefactPlay(artefactData);
        }
        else
        {
            // If clicked in Level Selection, open Detail
            MainMenuEvents.TriggerOpenArtefactDetail(artefactData);
            
            // Focus camera to the currently active transform (can be box or artefact)
            Transform targetTransform = activeOutline == artefactOutline ? artefactTransform : boxTransform;
            MainMenuEvents.TriggerCameraFocusToArtefact(targetTransform);
        }
    }
}