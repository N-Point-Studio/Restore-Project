using MoreMountains.Feedbacks;
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

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player unlockFeedback;
    [SerializeField] private MMF_Player completionFeedback;

    private ArtefactData artefactData;
    private bool isInteractable = false;
    private bool isInDetailMode = false;
    private bool hasUnpeeledNotes = false;
    private bool isSelectionModeActive = false;

    public string ArtefactId => artefactId;
    public bool IsInDetailMode => isInDetailMode;
    public bool CheckUnpeeledNotes()
    {
        if (stickyNoteObjects == null) return false;

        for (int i = 0; i < stickyNoteObjects.Length; i++)
        {
            if (stickyNoteObjects[i] != null && stickyNoteObjects[i].gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

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
        hasUnpeeledNotes = isUnlocked && !isCompleted && !isStoryRead;

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

                        note.Initialize(labelText, this);
                        note.ToggleCollider(false);
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

        if (stickyNoteObjects != null)
        {
            for (int i = 0; i < stickyNoteObjects.Length; i++)
            {
                StickyNote3DItem note = stickyNoteObjects[i];
                if (note != null) note.ToggleCollider(isDetail);
            }
        }
    }

    public bool CanInteract(bool isSensorBox)
    {
        if (!isSelectionModeActive) return false;

        if (!isInteractable || artefactData == null) return false;
        if (isSensorBox && isInDetailMode && hasUnpeeledNotes) return false;
        return true;
    }

    public void SetSelectionModeActive(bool isActive)
    {
        isSelectionModeActive = isActive;
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

    /// <summary>
    /// Visibility of Pedestal
    /// </summary>
    /// <param name="isVisible"></param>
    public void ShowPedestal(bool isVisible)
    {
        if (assignedPedestal != null)
        {
            assignedPedestal.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Visibility of Artefact and Box
    /// </summary>
    /// <param name="isVisible"></param>
    public void SetVisibility(bool isVisible)
    {
        if (artefactTransform != null)
        {
            artefactTransform.gameObject.SetActive(isVisible);
        }

        if (boxTransform != null)
        {
            boxTransform.gameObject.SetActive(isVisible);
        }
    }

    public void PlayUnlockAnimation()
    {
        if (boxTransform != null) boxTransform.localScale = Vector3.zero;

        if (unlockFeedback != null) unlockFeedback.PlayFeedbacks();
    }

    public void PlayCompletionAnimation()
    {
        if (artefactTransform != null) 
        {
            artefactTransform.gameObject.SetActive(true);
            artefactTransform.localScale = Vector3.zero; 
        }
        if (boxTransform != null) boxTransform.gameObject.SetActive(false);
        
        if (completionFeedback != null) completionFeedback.PlayFeedbacks();
    }
}