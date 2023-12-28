using Peg;
using Peg.AutonomousEntities;
using Peg.GCCI;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Disable Gravity", menuName = "Assets/Useable Tools/Disable Gravity")]
    public class DisableGravityTool: AbstractTimerToolEffect
    {

        protected override void OnStartTimer(ITool tool)
        {
            tool.Owner.FindComponentInEntity<IGravity>(true).GravityEnabled = false;
        }

        protected override void OnEndTimer(ITool tool)
        {
            tool.Owner.FindComponentInEntity<IGravity>(true).GravityEnabled = true;
        }


    }
}
