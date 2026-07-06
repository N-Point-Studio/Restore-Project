using System.Collections.Generic;
using UnityEngine;
using Modules.SoundSystems;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = nameof(ArtefactData), menuName = "App/Data/Artefact Data")]
public class ArtefactData : ScriptableObject
{
    [SerializeField] private BaseData baseData;
    public BaseData BaseData => baseData;

    [Header("Localization Data")]
    [SerializeField] private LocalizedString localizedItemName;
    public LocalizedString LocalizedItemName => localizedItemName;

    [SerializeField] private LocalizedString localizedItemDescription;
    public LocalizedString LocalizedItemDescription => localizedItemDescription;

    [SerializeField] private LocalizedString[] localizedStickyNoteTexts;
    public LocalizedString[] LocalizedStickyNoteTexts => localizedStickyNoteTexts;

    [Header("Story / Journal Data")]
    [TextArea(2, 4)]
    [SerializeField] private string[] stickyNoteTexts;
    public string[] StickyNoteTexts => stickyNoteTexts;

    [Header("Audio Settings")]
    [SerializeField] private AudioKey customGameplayBGM;
    public AudioKey CustomGameplayBGM => customGameplayBGM;

    [Header("Journal Content (Prefabs)")]
    [SerializeField] private JournalPageAnimator preRestorationPagePrefab;
    public JournalPageAnimator PreRestorationPagePrefab => preRestorationPagePrefab;
    [SerializeField] private JournalPageAnimator postRestorationPagePrefab;
    public JournalPageAnimator PostRestorationPagePrefab => postRestorationPagePrefab;

    [SerializeField] private bool unlockedByDefault;
    public bool UnlockedByDefault => unlockedByDefault;

    [SerializeField] private ArtefactData[] unlockRequirements;
    public ArtefactData[] UnlockRequirements => unlockRequirements;

    [SerializeField] private ToolType allowedToolTypes;
    public ToolType AllowedToolTypes => allowedToolTypes;

    [SerializeField] private List<ArtefactFragmentData> artefactFragmentDatas;
    public List<ArtefactFragmentData> ArtefactFragmentDatas => artefactFragmentDatas;

    [SerializeField] private Vector3 finalRotation;
    public Vector3 FinalRotation => finalRotation;

    [SerializeField] private float finalScaleMultiplier;
    public float FinalScaleMultiplier => finalScaleMultiplier;

    [SerializeField] private float clueTreshold;
    public float ClueTreshold => clueTreshold;

    [SerializeField] private float zoomValue;
    public float ZoomValue => zoomValue;

    private string artefactGroupId;
    public string ArtefactGroupId => artefactGroupId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        baseData.SetId(this.name);
    }
#endif
}

[System.Serializable]
public struct ArtefactFragmentData
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [SerializeField] private CustomTransform spawnTransform;
    public CustomTransform SpawnTransform => spawnTransform;
}

[System.Serializable]
public struct CustomTransform
{
    [SerializeField] private Vector3 rotation;
    public Vector3 Rotation => rotation;

    [SerializeField] private Vector3 position;
    public Vector3 Position => position;
}