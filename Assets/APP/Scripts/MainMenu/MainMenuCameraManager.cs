using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class MainMenuCameraManager : MonoBehaviour
{
    [Header("Brain")]
    [SerializeField] private CinemachineBrain brain;

    [Header("Cinemachine Cameras")]
    [Tooltip("Camera when in the initial Main Menu")]
    [SerializeField] private CinemachineCamera vcamMainMenu;
    
    [Tooltip("Overview camera when selecting level/artefact")]
    [SerializeField] private CinemachineCamera vcamLevelSelection;
    
    [Tooltip("Detail camera that will zoom to a specific artefact")]
    [SerializeField] private CinemachineCamera vcamArtefactDetail;

    private void Awake()
    {
        MainMenuEvents.OnCameraToMainMenu += SetCameraToMainMenu;
        MainMenuEvents.OnCameraToLevelSelection += SetCameraToLevelSelection;
        MainMenuEvents.OnCameraFocusToArtefact += SetCameraToArtefact;
    }

    private void Start()
    {
        SetCameraToMainMenu();
    }

    private void OnDestroy()
    {
        MainMenuEvents.OnCameraToMainMenu -= SetCameraToMainMenu;
        MainMenuEvents.OnCameraToLevelSelection -= SetCameraToLevelSelection;
        MainMenuEvents.OnCameraFocusToArtefact -= SetCameraToArtefact;
    }

    private void SetCameraToMainMenu()
    {
        vcamMainMenu.Priority = 10;
        vcamLevelSelection.Priority = 0;
        vcamArtefactDetail.Priority = 0;

        MoveToTargetCamera();
    }

    private void SetCameraToLevelSelection()
    {
        vcamMainMenu.Priority = 0;
        vcamLevelSelection.Priority = 10;
        vcamArtefactDetail.Priority = 0;

        MoveToTargetCamera();
    }

    private void SetCameraToArtefact(Transform target)
    {
        if (target != null)
        {
            vcamArtefactDetail.Follow = target;
            vcamArtefactDetail.LookAt = target;
        }

        vcamMainMenu.Priority = 0;
        vcamLevelSelection.Priority = 0;
        vcamArtefactDetail.Priority = 10;

        MoveToTargetCamera();
    }

    private void MoveToTargetCamera()
    {
        StartCoroutine(WaitUntilBlendFinished());
    }

    private IEnumerator WaitUntilBlendFinished()
    {
        yield return new WaitForSeconds(0.1f);

        while (brain.IsBlending)
        {
            yield return null;
        }

        MainMenuEvents.TriggerCameraMoveFinished();
    }
}