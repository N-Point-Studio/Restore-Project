using DG.Tweening;
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

    [Header("Animation Settings")]
    [SerializeField] private float slideDownDistance = 3f;

    private ArtefactData artefactData;
    private bool isInteractable = false;
    private bool isInDetailMode = false;
    private bool hasUnpeeledNotes = false;
    private bool isSelectionModeActive = false;
    private bool isCompleted = false;
    private Vector3 originalPosition;
    private bool isPosInitialized = false;

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
        isCompleted = activeData.IsArtefactCompleted(artefactId);
        bool isStoryRead = activeData.IsStoryRead(artefactId);

        bool hasPendingUnlock = activeData.GetPendingUnlockAnimations().Exists(x => x.artefactId == artefactId);
        bool hasPendingCompletion = activeData.GetPendingCompletionAnimations().Exists(x => x.artefactId == artefactId);

        if (hasPendingCompletion) isCompleted = false;

        if (boxSensor != null) boxSensor.Initialize(this);
        if (artefactSensor != null) artefactSensor.Initialize(this);

        if (hasPendingUnlock)
        {
            if (artefactTransform != null) artefactTransform.gameObject.SetActive(false);
            if (boxTransform != null) boxTransform.gameObject.SetActive(false);
        }
        else
        {
            if (artefactTransform != null) artefactTransform.gameObject.SetActive(isCompleted);
            if (boxTransform != null) boxTransform.gameObject.SetActive(!isCompleted && isUnlocked);
        }

        // artefactTransform.gameObject.SetActive(isCompleted);
        // boxTransform.gameObject.SetActive(!isCompleted && isUnlocked);

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

    private void InitPositionIfNeeded()
    {
        if (!isPosInitialized)
        {
            originalPosition = transform.localPosition;
            isPosInitialized = true;
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

        if (isInDetailMode && isCompleted) return false;

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
        if (boxTransform != null) 
        {
            boxTransform.localScale = Vector3.zero;
            boxTransform.gameObject.SetActive(true); 
        }

        if (unlockFeedback != null) unlockFeedback.PlayFeedbacks();
    }

    public void PlayCompletionAnimation()
    {
        if (artefactTransform != null) 
        {
            artefactTransform.localScale = Vector3.zero; 
            artefactTransform.gameObject.SetActive(true);
        }
        if (boxTransform != null) boxTransform.gameObject.SetActive(false);
        
        if (completionFeedback != null) completionFeedback.PlayFeedbacks();
    }

    public void AnimateItem(bool show, float duration = 0.5f)
    {
        InitPositionIfNeeded();

        if (show) gameObject.SetActive(true);

        if (assignedPedestal != null)
        {
            PedestalAnimator animator = assignedPedestal.GetComponent<PedestalAnimator>();
            if (animator != null) 
            {
                animator.AnimatePedestal(show, duration);
            }
        }

        float targetAlpha = show ? 1f : 0f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.material == null) continue;
            
            r.material.DOKill();
            if (r.material.HasProperty("_BaseColor")) r.material.DOFade(targetAlpha, "_BaseColor", duration);
            else if (r.material.HasProperty("_Color")) r.material.DOFade(targetAlpha, "_Color", duration);
        }

        transform.DOKill();
        Vector3 targetPos = show ? originalPosition : originalPosition + (Vector3.down * slideDownDistance);
        transform.DOLocalMove(targetPos, duration).SetEase(show ? Ease.OutBack : Ease.InBack)
            .OnComplete(() => 
            {
                if (!show) gameObject.SetActive(false);
            });
    }
}