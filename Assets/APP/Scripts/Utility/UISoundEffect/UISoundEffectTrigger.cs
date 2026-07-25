using Modules.SoundSystems;
using UnityEngine;

public class UISoundEffectTrigger : MonoBehaviour
{
    [SerializeField] protected AudioKey customKey = AudioKey.None;
    
    protected virtual void PlaySound()
    {
    }
}