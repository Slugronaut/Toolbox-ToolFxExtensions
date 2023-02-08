using System.Collections;
using Toolbox;
using Toolbox.Graphics;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Flicker Owner Outlines", menuName = "Assets/Useable Tools/Flicker Owner Outlines")]
    public class FlickerOwnerOutline : AbstractTimerToolEffect
    {
        [Tooltip("The final state to end on.")]
        public bool EndingState;
        public float TickFreq = 0.1f;
        [Tooltip("Usually we want to ensure the end state is set if the tool is disabled, but in some cases that might be bad.")]
        public bool ApplyEndStateOnDisable = true;

        string Cr;

        protected override void OnEnable()
        {
            base.OnEnable();
            Cr = RegisterVar("Cr");
        }

        public override void ToolDisabled(ITool tool)
        {
            if(ApplyEndStateOnDisable)
                StopEffect(tool);
            base.ToolDisabled(tool);
        }

        void AlternateOutline(ITool tool)
        {
            //TODO: this shoudld really be cached somehow!
            var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
            for (int i = 0; i < ols.Length; i++)
                ols[i].enabled = !ols[i].enabled;

        }

        void StopEffect(ITool tool)
        {
            var cr = tool.GetInstVar<Coroutine>(Cr);
            tool.StopToolEffectCoroutine(this, cr);

            var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
            for (int i = 0; i < ols.Length; i++)
                ols[i].enabled = EndingState;
        }

        public override IEnumerator Routine(ITool tool)
        {
            var wait = CoroutineWaitFactory.RequestWait(TickFreq);

            while(true)
            {
                AlternateOutline(tool);
                yield return wait;
            }
            
        }

        protected override void OnStartTimer(ITool tool)
        {
            var cr = tool.StartToolEffectCoroutine(this);
            tool.SetInstVar(Cr, cr);
        }

        protected override void OnEndTimer(ITool tool)
        {
            StopEffect(tool);
        }


    }
}
