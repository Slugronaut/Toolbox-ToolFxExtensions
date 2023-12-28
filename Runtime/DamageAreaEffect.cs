using DamageSystem;
using System;
using Peg;
using Peg.Collections;
using UnityEngine;
using Peg.Lib;
using Peg.AutonomousEntities;

namespace ToolFx
{
    /// <summary>
    /// Performs a SphereCheck and applies damage to anything it collides with.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Damage AoE", menuName = "Assets/Projectile Tools/Damage AoE")]
    public class DamageAreaEffect : AbstractCommonToolEffect
    {
        [Tooltip("The half extents of the trigger check.")]
        public Vector3 HalfExtents = Vector3.one / 2;
        [Tooltip("The layers to check against.")]
        public LayerMask Mask;
        public QueryTriggerInteraction TriggerInteraction;
        [Tooltip("If set, projectiles will spawn in aim-space even if the tool operations in bilateral space.")]
        public bool OverrideAimSpaceSpawning = false;
        [Tooltip("An additional aim-offset applied to this effect in relation to the tool's own aim-offset.")]
        public Vector3 LocalAimOffset;
        public float MinDamage;
        public float MaxDamage;
        public bool HonorInvincibility = true;
        public ushort MaxTarget = 2;
        public HashedString[] DamageTypes;
        [Tooltip("If a target is set for the tool, is it passed to the damage calculator?")]
        bool PassTarget;

        [Tooltip("Is this checking in 2D or 3D physics?")]
        public bool Use2D;


        public override void Process(ITool tool)
        {
            if (Use2D)
                throw new UnityException("2D AoE check not yet implemented.");
            else
            {
                var trans = tool.gameObject.transform;
                var position = trans.position;
                var forward = trans.forward;

                //calculate spawn position and orientation
                Vector3 localOffset = tool.MirrorLocalOffset ? new Vector3(-LocalAimOffset.x, LocalAimOffset.y, LocalAimOffset.z) : LocalAimOffset;

                Vector3 spawnPos = position;
                if (tool.AimMode == AimOffsetModes.AimSpace || OverrideAimSpaceSpawning)
                {
                    spawnPos = MathUtils.ForwardSpaceOffset(position, forward, LocalAimOffset + tool.AimOffset);
                }
                else if (tool.AimMode == AimOffsetModes.Bilateral)
                {
                    spawnPos = position + localOffset + tool.AimOffset;
                }

                EntityRoot hitEnt = null;
                var cols = SharedArrayFactory.RequestTempArray<Collider>(MaxTarget);
                int hitCount = Physics.OverlapBoxNonAlloc(spawnPos, HalfExtents, cols, Quaternion.identity, Mask, TriggerInteraction);
                for (int i = 0; i < hitCount; i++)
                {
                    //TODO: use message to query for this
                    hitEnt = cols[i].gameObject.GetEntityRoot();
                    CombatCalculator.ProcessDirectDamage(tool.Owner, hitEnt, MinDamage, MaxDamage, DamageTypes, 1, HonorInvincibility);
                }
            }

        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}