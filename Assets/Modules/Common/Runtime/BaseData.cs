using UnityEngine;

[System.Serializable]
public class BaseData
{
    [SerializeField] protected string id;
    public string Id => id;

    [SerializeField] protected string name;
    public string Name => name;

    [SerializeField, TextArea] protected string description;
    public string Description => description;

    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] protected bool exclude;
    public bool Exclude => exclude;

    public void SetId(string id)
    {
        this.id = id;
    }
}