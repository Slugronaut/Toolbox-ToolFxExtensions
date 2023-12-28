using System.Collections;
using Peg.AutonomousEntities;
using Peg.Graphics;
using UnityEngine;


namespace ToolFx

{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Blend Owner Outlines", menuName = "Assets/Useable Tools/Blend Owner Outlines")]
    public class BlendOwnerOutline : AbstractTimerToolEffect
    {
        [Tooltip("The starting color of the blend.")]
        [ColorUsage(true, true)]
        public Color StartColor = Color.black;
        [Tooltip("The final color to end on.")]
        [ColorUsage(true, true)]
        public Color EndColor = Color.black;
        [Tooltip("Usually we want to ensure the end state is set if the tool is disabled, but in some cases that might be bad.")]
        public bool ApplyEndStateOnDisable = true;


        string Cr;
        string CurrColor;

        protected override void OnEnable()
        {
            base.OnEnable();
            Cr = RegisterVar("Cr");
            CurrColor = RegisterVar("CurrColor");
        }

        public override void ToolDisabled(ITool tool)
        {
            var cr = tool.GetInstVar<Coroutine>(Cr);
            tool.StopToolEffectCoroutine(this, cr);
            if(ApplyEndStateOnDisable)
                SetStateEffect(tool, EndColor);
            base.ToolDisabled(tool);
        }

        void SetStateEffect(ITool tool, Color color)
        {
            var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
            for (int i = 0; i < ols.Length; i++)
                ols[i].Color = color;

            tool.SetInstVar(CurrColor, color);
        }

        public override IEnumerator Routine(ITool tool)
        {
            float acc = 0;
            SetStateEffect(tool, StartColor);
            while (true)
            {
                Color c = tool.GetInstVar<Color>(CurrColor);

                //NOTE: This is an obsurdly naive way of doing this but coroutines are pretty fucking lame when it comes to tracking time!
                //This will become highly inaccurate after just a few seconds so make sure your effect isn't very long.
                acc += Time.deltaTime;

                SetStateEffect(tool, Color.LerpUnclamped(c, EndColor, acc / WaitTime));
                yield return null;
            }

        }

        protected override void OnStartTimer(ITool tool)
        {
            var cr = tool.StartToolEffectCoroutine(this);
            tool.SetInstVar(Cr, cr);
        }

        protected override void OnEndTimer(ITool tool)
        {
            var cr = tool.GetInstVar<Coroutine>(Cr);
            tool.StopToolEffectCoroutine(this, cr);
            SetStateEffect(tool, EndColor);
        }


    }
}
