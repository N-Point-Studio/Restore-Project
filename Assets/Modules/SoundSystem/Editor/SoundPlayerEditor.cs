using UnityEditor;
using UnityEngine;

namespace Modules.SoundSystems
{
    [CustomEditor(typeof(SoundPlayer))]
    public class SoundPlayerEditor : BaseEditor
    {
        private SoundPlayer script;

        private void OnEnable()
        {
            script = (SoundPlayer)target;
        }

        protected override void OnDrawCustomInspector()
        {
            script.UseDatabase = DrawToggle("Use Database", script.UseDatabase);
            if (script.UseDatabase)
            {
                DrawPropertyField("audioKey");
            }
            else
            {
                DrawPropertyField("audioClip");
                DrawPropertyField("audioType");
            }

            DrawPropertyField("autoplay");
            DrawPropertyField("is3DAudio");
            DrawPropertyField("isLooped");
        }
    }
}