using System;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Screen Shake Tool", menuName = "Assets/Useable Tools/Screen Shake Tool")]
    public class ScreenShakeLeveledTool : AbstractChargedToolEffect
    {
        [Tooltip("The charge level required for this to trigger.")]
        public int ChargeLevel;
        public float Magnitude;
        public float Roughness;
        public float FadeInTime;
        public float FadeOutTime;
        public Vector3 PosInfluence = Vector3.one;
        public Vector3 RotInfluence = Vector3.zero;



        public override void Use(ITool tool)
        {
            base.Use(tool);
            if (Trigger == Tool.TriggerPoint.OnUse && ChargeLevel == CurrentLevel(tool))
                EZCameraShake.CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

        }

        public override void EndUse(ITool tool)
        {
            if(Trigger == Tool.TriggerPoint.OnEndUse && ChargeLevel == CurrentLevel(tool))
                EZCameraShake.CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
            base.EndUse(tool);
        }

        public override void ToolDestroyed(ITool tool)
        {
        }

        public override void ToolDisabled(ITool tool)
        {
        }
        
        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
