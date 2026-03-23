using System;
using UnityEngine;
using Unity.Cinemachine;

public class MainMenuCameraManager : MonoBehaviour
{
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
    }

    private void SetCameraToLevelSelection()
    {
        vcamMainMenu.Priority = 0;
        vcamLevelSelection.Priority = 10;
        vcamArtefactDetail.Priority = 0;
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
    }
}