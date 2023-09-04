using DamageSystem;
using System;
using Peg;
using Peg.Lazarus;
using UnityEngine;
using Peg.Lib;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Beam Emitter", menuName = "Assets/Projectile Tools/Beam Emitter")]
    public class BeamEmitter : AbstractToolEffect
    {
        public ProjectileBeam BeamPrefab;
        public float Speed;
        [Tooltip("If set, projectiles will spawn in aim-space even if the tool operates in bilateral space.")]
        public bool OverrideAimSpaceSpawning = false;
        [Tooltip("An additional aim-offset applied to this effect in relation to the tool's own aim-offset.")]
        public Vector3 LocalAimOffset;
        //[Tooltip("An optional transform that will be scaled, positioning, and oriented along with the laser beam.")]
        //public Transform OptionalScalable;
        string ProjBeam;


        protected override void OnEnable()
        {
            base.OnEnable();
            ProjBeam = RegisterVar("ProjBeam");
        }
        
        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }


        public override void ToolDestroyed(ITool tool)
        {
        }

        public override void ToolDisabled(ITool tool)
        {
            KillBeam(tool);
        }

        public override void Use(ITool tool)
        {
            //Since this object can store instance-state
            //and this method is only called once, we need
            //to rely on the ProjectileBeam object to handle
            //updating it's own position and whatnot.
            var toolTrans = tool.gameObject.transform;

            //calculate spawn position and orientation
            var forward = toolTrans.forward;
            Vector3 localOffset = tool.MirrorLocalOffset ? new Vector3(-LocalAimOffset.x, LocalAimOffset.y, LocalAimOffset.z) : LocalAimOffset;

            Vector3 spawnPos = toolTrans.position;
            if (tool.AimMode == AimOffsetModes.AimSpace || OverrideAimSpaceSpawning)
                spawnPos = MathUtils.ForwardSpaceOffset(toolTrans.position, forward, LocalAimOffset + tool.AimOffset);
            else if (tool.AimMode == AimOffsetModes.Bilateral)
                spawnPos = toolTrans.position + localOffset + tool.AimOffset;
            forward.Normalize();

            //spawn the projectile and apply speed, direction, and starting position
            var go = Lazarus.Instance.Summon(BeamPrefab.gameObject, spawnPos, false);
            var projTrans = go.transform;
            
            /*
            if (AllowLayerChange)
            {
                int l = Layer;
                go.layer = l;
                for (int i = 0; i < projTrans.childCount; i++)
                    projTrans.GetChild(i).gameObject.layer = l;
            }
            */

            //set projectile stats
            var proj = go.GetComponent<ProjectileBeam>();
            projTrans.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            proj.Speed = (Speed + Global.DifficultySpeed);
            proj.Source = tool;
            proj.gameObject.SetActive(true);
            proj.ResetToDefault();
            proj.Fired();
            //proj.AddDespawnCallback(ProjDespawnCallback);
            SetActiveBeam(tool, proj);
            //TODO: we can obtain references to stateful projectile mods here (for things like fire patters and such)
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {
            KillBeam(tool);
        }

        void KillBeam(ITool tool)
        {
            var activeBeam = GetActiveBeam(tool);
            if (activeBeam != null)
                activeBeam.KillBeam();
        }

        /// <summary>
        /// Helper for getting a blackboard attached to the tool that stores our active projectiles.
        /// We need this because this object is a shared scriptable object so state like this must be
        /// stored on the actual GameObject itself.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        ProjectileBeam GetActiveBeam(ITool tool)
        {
            //weapon might have gone away since this was fired 
            if (TypeHelper.IsReferenceNull(tool)) return null;
            return tool.GetInstVar<ProjectileBeam>(ProjBeam);
        }

        /// <summary>
        /// Helper for getting a blackboard attached to the tool that stores our active projectiles.
        /// We need this because this object is a shared scriptable object so state like this must be
        /// stored on the actual GameObject itself.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        void SetActiveBeam(ITool tool, ProjectileBeam beam)
        {
            //weapon might have gone away since this was fired 
            if (!TypeHelper.IsReferenceNull(tool))
                tool.SetInstVar(ProjBeam, beam);
        }
    }
}
