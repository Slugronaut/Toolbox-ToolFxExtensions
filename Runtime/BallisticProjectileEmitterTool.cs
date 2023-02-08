using System;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter - Ballistic", menuName = "Assets/Projectile Tools/Projectile Emitter - Ballistic")]
    public class BallisticProjectileEmitterTool : AbstractProjectileEmitter
    {
        [Tooltip("Offset applied to the final target position where needed.")]
        public Vector3 TargetOffset;
        public float Angle;
       
        
        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void Use(ITool tool)
        {
            var trans = tool.gameObject.transform;
            FireBallistic(tool, trans.position, trans.forward, TargetOffset, Angle);
        }

        public override void EndUse(ITool tool)
        {

        }

    }
}
