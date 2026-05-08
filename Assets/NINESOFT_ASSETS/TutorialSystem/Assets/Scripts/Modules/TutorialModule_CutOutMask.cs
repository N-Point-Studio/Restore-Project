using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace NINESOFT.TUTORIAL_SYSTEM
{
    [ExecuteAlways]
    public class TutorialModule_CutOutMask : TutorialModule
    {
        [SerializeField] public Image MaskObject;
        [SerializeField] private Image MaskFill;

        [SerializeField][Range(1, 10)] private float HoleScale = 1;
        [SerializeField] private Color MaskColor;

        [SerializeField] private RectTransform TargetUI;

        private ParentConstraint parentConstraint;

        public override IEnumerator ActiveTheModuleEnum()
        {
            if (!Application.isPlaying) yield break;

            //StartCoroutine(StartFocusAnimation());

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator StartFocusAnimation()
        {
            float maskScale = HoleScale;
            HoleScale = 250f;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 0.5f;
                HoleScale = Mathf.Lerp(HoleScale, maskScale, t);
                yield return new WaitForEndOfFrame();
            }

        }

        private void Update()
        {
            UpdateData();
        }

        private void UpdateData()
        {
            if (parentConstraint == null)
            {
                parentConstraint = GetComponentInChildren<ParentConstraint>();
            }

            if (TargetUI == null)
                return;

            if (parentConstraint.sourceCount == 0)
            {
                parentConstraint.AddSource(new ConstraintSource() { sourceTransform = TargetUI, weight = 1 });
            }
            else if (parentConstraint.GetSource(0).sourceTransform != TargetUI)
            {
                parentConstraint.SetSource(0, new ConstraintSource() { sourceTransform = TargetUI, weight = 1 });
            }

            parentConstraint.constraintActive = true;
            MaskObject.transform.localScale = Vector3.one * HoleScale;
            MaskObject.rectTransform.anchorMax = TargetUI.anchorMax;
            MaskObject.rectTransform.anchorMin = TargetUI.anchorMin;
            MaskObject.rectTransform.sizeDelta = TargetUI.sizeDelta;
            MaskObject.rectTransform.pivot = TargetUI.pivot;
        }

        public void SetTarget(RectTransform target)
        {
            TargetUI = target;
        }
    }
}