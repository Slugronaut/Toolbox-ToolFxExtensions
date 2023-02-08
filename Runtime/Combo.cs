using UnityEngine;
using System;


namespace ToolFx
{
    /// <summary>
    /// A simple combo system. If it is used within a cirtain time-frame it will
    /// chain to the next hit of the combo.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Simple Combo", menuName = "Assets/Useable Tools/Simple Combo")]
    public class Combo : ToolOverride
    {
        [Serializable]
        public class ComboEffect
        {
            public float MaxDelay;
            public ToolOverride ComboEffects;
        }

        [Tooltip("How long before a new attack can start after a full combo was completed?")]
        public float DelayAfterCombo = 1;
        [Tooltip("A list of ToolOverrides and the max allowed delay since the previous combo before they will be triggered.")]
        public ComboEffect[] Combos;

        string ComboIndex;
        string LastCompleteTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            ComboIndex = RegisterVar("ComboIndex");
            LastCompleteTime = RegisterVar("LastCompleteTime");
        }

        bool CanCombo(ITool tool)
        {
            int index = tool.GetInstVar<int>(ComboIndex);
            float lastTime = tool.GetInstVar<float>(LastCompleteTime);

            if (index >= Combos.Length)
            {
                if (Time.time - lastTime < DelayAfterCombo)
                    return false;
                else tool.SetInstVar(ComboIndex, 0);
            }

            return true;
        }

        ToolOverride GetComboEffect(ITool tool, float lastTime)
        {
            int index = tool.GetInstVar<int>(ComboIndex);
            //get next combo element
            if (index == 0 || Time.time - lastTime < Combos[index].MaxDelay)
                return Combos[index].ComboEffects;
            else
            {
                //reset the combo
                tool.SetInstVar(ComboIndex, 0);
                return Combos[0].ComboEffects;
            }
        }

        public override void Use(ITool tool)
        {
            if (CanCombo(tool))
            {
                float lastTime = tool.GetInstVar<float>(LastCompleteTime);

                GetComboEffect(tool, lastTime).Use(tool);
                int index = tool.GetInstVar<int>(ComboIndex);
                tool.SetInstVar(ComboIndex, index+1);
                tool.SetInstVar(LastCompleteTime, Time.time);
                base.Use(tool);
            }
        }


    }
}
