using System;
using Toolbox;
using Toolbox.Game;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Disable Input Elements", menuName = "Assets/Useable Tools/Disable Input Elements")]
    public class DisableInputElements : AbstractTimerToolEffect
    {
        public bool DisableJumping;
        public bool DisableMotion;
        public bool DisableAiming;

        protected override void OnStartTimer(ITool tool)
        {
            var input = tool.Owner.FindComponentInEntity<IInputSourceComponent>(true);
            var health = tool.Owner.FindComponentInEntity<Health>(true);
            if (!health.IsDead && tool.Owner.isActiveAndEnabled)
            {
                if (DisableJumping) input.JumpEnabled = false;
                if (DisableMotion) input.MotionEnabled = false;
                if (DisableAiming) input.AimEnabled = false;
            }
        }

        protected override void OnEndTimer(ITool tool)
        {
            var input = tool.Owner.FindComponentInEntity<IInputSourceComponent>(true);

            if (DisableJumping) input.JumpEnabled = true;
            if (DisableMotion) input.MotionEnabled = true;
            if (DisableAiming) input.AimEnabled = true;
        }
    }
}
