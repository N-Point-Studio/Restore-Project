using Modules.SoundSystems;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundEffectTrigger : MonoBehaviour
{
    [SerializeField] protected AudioKey customKey = AudioKey.None;
    protected virtual void PlaySound()
    {

    }
}
