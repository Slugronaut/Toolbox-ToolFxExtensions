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
    [CreateAssetMenu(fileName = "Disable Input Source Tool", menuName = "Assets/Useable Tools/Disable Input Source")]
    public class DisableInputSystemTool : AbstractTimerToolEffect
    {
        protected override void OnStartTimer(ITool tool)
        {
            var input = tool.Owner.FindComponentInEntity<IInputSourceComponent>(true);
            var health = tool.Owner.FindComponentInEntity<Health>(true);
            if (!health.IsDead && tool.Owner.isActiveAndEnabled)
                input.enabled = false;
        }

        protected override void OnEndTimer(ITool tool)
        {
            var input = tool.Owner.FindComponentInEntity<IInputSourceComponent>(true);
            var health = tool.Owner.FindComponentInEntity<Health>(true);
            if (!health.IsDead)
                input.enabled = true;
        }

        
    }
}
