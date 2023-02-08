using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter - Radial", menuName = "Assets/Projectile Tools/Projectile Emitter - Radial")]
    public class RadialProjectileEmitter : AbstractProjectileEmitter
    {
        [Tooltip("The number of projectiles emitted. The are emitted evenly spaced in a circle starting at the facing location of the tool.")]
        public float Count = 4;
        public bool UseFixedForward;
        [ShowIf("UseFixedForward")]
        [Indent]
        public Vector3 FixedForward;
        [ShowIf("UseFixedForward")]
        [Indent]
        public float RotateSpeed = 0;
        
        string LastForward;

        protected override void OnEnable()
        {
            base.OnEnable();
            LastForward = RegisterVar("LastForward");
        }

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
            Vector3 forward = UseFixedForward ? FixedForward : trans.forward;
            var last = tool.GetInstVar<Vector3>(LastForward);
            if (last == Vector3.zero)
                last = FixedForward;

            if (UseFixedForward && RotateSpeed > 0)
            {
                forward = Quaternion.AngleAxis(RotateSpeed, Vector3.up) * last;
                tool.SetInstVar(LastForward, forward);
            }

            float diff = 360.0f / (float)Count;
            for (int i = 0; i < Count; i++)
            {
                Fire(tool, pos, forward);
                forward = Quaternion.AngleAxis(diff, Vector3.up) * forward;
            }
        }

        public override void EndUse(ITool tool)
        {

        }

    }
}
