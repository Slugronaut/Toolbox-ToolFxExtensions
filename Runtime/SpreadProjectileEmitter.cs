using System;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter - Spread", menuName = "Assets/Projectile Tools/Projectile Emitter - Spread")]
    public class SpreadProjectileEmitter : AbstractProjectileEmitter
    {
        [Tooltip("The number of projectiles emitted. The are emitted evenly spaced in a circle starting at the facing location of the tool.")]
        public float Count = 4;
        [Tooltip("The angle to increment the spread of projectiles off-center.")]
        public float AngleSpread = 15;
        
        

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void Use(ITool tool)
        {
            if (Count < 1) return;

            var trans = tool.gameObject.transform;
            Vector3 pos = trans.position;
            Vector3 forward = trans.forward;

            //fire our first shot in the direction of forward
            Fire(tool, pos, forward);

            //now shoot two bullets equal rotations off-center
            float angle = AngleSpread;
            for (int i = 0; i < Count; i++)
            {
                Fire(tool, pos, Quaternion.AngleAxis(angle, Vector3.up) * forward);
                Fire(tool, pos, Quaternion.AngleAxis(-angle, Vector3.up) * forward);
                angle += AngleSpread; //kinda inaccurate but who cares?
            }
        }

        public override void EndUse(ITool tool)
        {

        }

    }
}
