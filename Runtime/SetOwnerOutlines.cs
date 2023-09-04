using Peg.Graphics;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Set Owner Outlines", menuName = "Assets/Useable Tools/Set Owner Outlines")]
    public class SetOwnerOutlines : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [Tooltip("Should the outline be enabled?")]
        public bool OutlineEnabled;
        [Tooltip("The color of the outline to use.")]
        [ColorUsage(true, true)]
        public Color OutlineColor = Color.black;


        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
                SetOutline(tool);
        }

        public override void Use(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
                SetOutline(tool);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }

        void SetOutline(ITool tool)
        {
            var ols = tool.Owner.FindComponentsInEntity<ISpriteOutline>(true);
            for (int i = 0; i < ols.Length; i++)
            {
                ols[i].Color = OutlineColor;
                ols[i].enabled = OutlineEnabled;
            }

        }
    }
}
