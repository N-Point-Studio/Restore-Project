using UnityEngine;

public class Artefact3DItem : MonoBehaviour
{
    [Header("Artefact Configuration")]
    [SerializeField] private string artefactId; 
    
    [Header("3D Visuals")]
    [SerializeField] private Transform artefactTransform; 
    [SerializeField] private Transform boxTransform;      

    [Header("Sensors")]
    [SerializeField] private ArtefactSensor boxSensor;
    [SerializeField] private ArtefactSensor artefactSensor;

    [Header("Sticky Notes")]
    [SerializeField] private StickyNote3DItem[] stickyNoteObjects; 

    [Header("Environment")]
    [SerializeField] private GameObject assignedPedestal;
    
    private ArtefactData artefactData;
    private bool isInteractable = false;
    private bool isInDetailMode = false;
    private bool hasUnpeeledNotes = false;

    public string ArtefactId => artefactId;

    public void Initialize(ActiveArtefactData activeData)
    {
        artefactData = activeData.GetArtefactDatabase().GetItem(artefactId);
        if (artefactData == null) return;

        bool isUnlocked = activeData.IsArtefactUnlocked(artefactId);
        bool isCompleted = activeData.IsArtefactCompleted(artefactId);
        bool isStoryRead = activeData.IsStoryRead(artefactId);

        if (boxSensor != null) boxSensor.Initialize(this);
        if (artefactSensor != null) artefactSensor.Initialize(this);

        artefactTransform.gameObject.SetActive(isCompleted);
        boxTransform.gameObject.SetActive(!isCompleted && isUnlocked);
        
        isInteractable = isUnlocked || isCompleted;
        hasUnpeeledNotes = !isCompleted && !isStoryRead;

        if (stickyNoteObjects != null)
        {
            for (int i = 0; i < stickyNoteObjects.Length; i++)
            {
                StickyNote3DItem note = stickyNoteObjects[i];
                if (note != null)
                {
                    if (hasUnpeeledNotes)
                    {
                        string labelText = "";
                        if (artefactData.StickyNoteTexts != null && i < artefactData.StickyNoteTexts.Length)
                        {
                            labelText = artefactData.StickyNoteTexts[i];
                        }
                        
                        note.Initialize(labelText);
                    }
                    else
                    {
                        note.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void SetDetailMode(bool isDetail, bool notesRemaining)
    {
        isInDetailMode = isDetail;
        hasUnpeeledNotes = notesRemaining;
    }

    public bool CanInteract(bool isSensorBox)
    {
        if (!isInteractable || artefactData == null) return false;
        if (isSensorBox && isInDetailMode && hasUnpeeledNotes) return false;
        return true;
    }

    public void OnSensorClicked(bool isSensorBox)
    {
        if (isInDetailMode)
        {
            MainMenuEvents.TriggerArtefactPlay(artefactData);
        }
        else
        {
            MainMenuEvents.TriggerOpenArtefactDetail(artefactData);
            Transform targetTransform = isSensorBox ? boxTransform : artefactTransform;
            MainMenuEvents.TriggerCameraFocusToArtefact(targetTransform);
        }
    }

    public void SetVisibility(bool isVisible)
    {
        if (assignedPedestal != null)
        {
            assignedPedestal.SetActive(isVisible);
        }

        if (artefactTransform != null)
        {
            artefactTransform.gameObject.SetActive(isVisible);
        }

        if (boxTransform != null)
        {
            boxTransform.gameObject.SetActive(isVisible);
        }
    }
}