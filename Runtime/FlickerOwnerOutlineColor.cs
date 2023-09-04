using System.Collections;
using Peg;
using Peg.Graphics;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Flicker Owner Outline Color", menuName = "Assets/Useable Tools/Flicker Owner Outline Color")]
    public class FlickerOwnerOutlineColor : AbstractTimerToolEffect
    {
        //NOTE: For now we'll use just two colors due to limitations in Unity's inspector
        //If we use FullInspector, we can apply per-array element inspectors that allow us
        //to make each color of the array and HDR color
        [Tooltip("The first target color.")]
        [ColorUsage(true, true)]
        public Color Color1 = Color.black;
        [Tooltip("The second target color.")]
        [ColorUsage(true, true)]
        public Color Color2 = Color.black;
        [Tooltip("The final color to end on.")]
        [ColorUsage(true, true)]
        public Color EndColor = Color.black;
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
        
        void StopEffect(ITool tool)
        {
            var cr = tool.GetInstVar<Coroutine>(Cr);
            tool.StopToolEffectCoroutine(this, cr);

            var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
            for (int i = 0; i < ols.Length; i++)
                ols[i].Color = EndColor;
        }

        public override IEnumerator Routine(ITool tool)
        {
            var wait = CoroutineWaitFactory.RequestWait(TickFreq);
            bool flip = false;

            while (true)
            {
                var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
                for (int i = 0; i < ols.Length; i++)
                    ols[i].Color = flip ? Color1 : Color2;

                flip = !flip;
                yield return wait;
            }

        }

        protected override void OnStartTimer(ITool tool)
        {
            var cr = tool.GetInstVar<Coroutine>(Cr);
            if (cr != null)
                tool.StopToolEffectCoroutine(this, cr);
            cr = tool.StartToolEffectCoroutine(this);
            tool.SetInstVar(Cr, cr);
        }

        protected override void OnEndTimer(ITool tool)
        {
            StopEffect(tool);
        }


    }
}
