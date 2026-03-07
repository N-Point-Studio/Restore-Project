using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UISoundEffectTriggerButton : UISoundEffectTrigger
{
    [SerializeField] private bool isConfirm;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlaySound);
    }

    public void SetConfirm(bool isConfirm)
    {
        this.isConfirm = isConfirm;
    }

    protected override void PlaySound()
    {
        base.PlaySound();
        SoundEvents.OnPlayButtonSFX?.Invoke(isConfirm);
    }
}
