using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ArtefactData), menuName = "App/Data/Artefact Data")]
public class ArtefactData : ScriptableObject
{
    [SerializeField] private BaseData baseData;
    public BaseData BaseData => baseData;

    [Header("Story / Journal Data")]
    [TextArea(2, 4)]
    [SerializeField] private string[] stickyNoteTexts;
    public string[] StickyNoteTexts => stickyNoteTexts;

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