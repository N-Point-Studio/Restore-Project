using System;
using UnityEngine;
using Unity.Cinemachine;

public class MainMenuCameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [Tooltip("Camera when in the initial Main Menu")]
    [SerializeField] private GameObject vcamMainMenu;
    
    [Tooltip("Overview camera when selecting level/artefact")]
    [SerializeField] private GameObject vcamLevelSelection;
    
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
        vcamMainMenu.SetActive(true);
        vcamLevelSelection.SetActive(false);
        vcamArtefactDetail.gameObject.SetActive(false);
    }

    private void SetCameraToLevelSelection()
    {
        vcamMainMenu.SetActive(false);
        vcamLevelSelection.SetActive(true);
        vcamArtefactDetail.gameObject.SetActive(false);
    }

    private void SetCameraToArtefact(Transform target)
    {
        if (target != null)
        {
            vcamArtefactDetail.Follow = target;
            vcamArtefactDetail.LookAt = target;
        }

        vcamMainMenu.SetActive(false);
        vcamLevelSelection.SetActive(false);
        vcamArtefactDetail.gameObject.SetActive(true);
    }
}