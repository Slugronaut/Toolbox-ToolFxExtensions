using DamageSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Peg;
using Peg.Lazarus;
using UnityEngine;
using Random = UnityEngine.Random;
using Peg.Lib;
using Peg.AutonomousEntities;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter", menuName = "Assets/Projectile Tools/Projectile Emitter")]
    public class ProjectileEmitterTool : AbstractProjectileEmitter
    {
        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            var trans = tool.gameObject.transform;
            Fire(tool, trans.position, trans.forward);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {

        }
        
    }



    public interface IEmitterTool
    {
        Vector3 EffectiveOffset { get; }
        bool OverridesAimSpace { get; }
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractProjectileEmitter : AbstractToolEffect, IEmitterTool
    {
        [AssetsOnly]
        [AssetList(CustomFilterMethod = "IsProjectileAsset")]
        public GameObject ProjPrefab;
        public bool ParentToSource;
        public float Speed;
        public bool IgnoreDifficultySpeed;
        public float Spread;
        [Tooltip("Does the projectile inherit the velocity of the parent?")]
        public bool InheritVelocity;
        [ShowIf("InheritVelocity")]
        [Indent]
        public Vector3 ParentVelInfluence = new Vector3(1, 0, 1);
        [Tooltip("Should projectile face the direction they are emitted?")]
        public bool SetProjectileHeading = true;
        [Tooltip("If set, projectiles will spawn in aim-space even if the tool operations in bilateral space.")]
        public bool OverrideAimSpaceSpawning = false;
        [Tooltip("An additional aim-offset applied to this effect in relation to the tool's own aim-offset.")]
        public Vector3 LocalAimOffset;
        [Tooltip("An additional force that is applied to the projectile when spawned. Useful for lobbing forces and such.")]
        public Vector3 AdditionalForceVector;
        public bool AllowLayerChange;
        [ShowIf("AllowLayerChange")]
        [Indent]
        [Tooltip("The layer the projectiles will be one when emitted.")]
        public string ProjectileLayer;
        [Tooltip("Does this emitter set a new damage output for the projectiles emitted?")]
        public bool OverrideDamage;
        [ShowIf("OverrideDamage")]
        [Indent]
        [Tooltip("The damage to apply to projectiles emitted. Only used if 'OverrideDamage' is set to true.")]
        public float Damage;
        [Tooltip("Does thie emitter set a new liftime for the projectiles emitted?")]
        public bool OverrideLifetime;
        [ShowIf("OverrideLifetime")]
        [Indent]
        [Tooltip("The lifetime to apply to projectiles emitted. Only used if 'OverrideLifetime' is set to true.")]
        public float Lifetime;


        /// <summary>
        /// Inspector-use only.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool IsProjectileAsset(GameObject obj) => obj.GetComponent<Projectile>() != null;


        public Vector3 EffectiveOffset
        {
            get { return LocalAimOffset; }
        }

        public bool OverridesAimSpace
        {
            get { return OverrideAimSpaceSpawning; }
        }

        protected int Layer { get; private set; }
        string ProjList;

        /// <summary>
        /// Base classes must call this before anything else.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            ProjList = RegisterVar("ProjList");
            Layer = LayerMask.NameToLayer(ProjectileLayer);
        }

        public override void ToolDisabled(ITool tool)
        {
            ClearProjectileList(tool);
        }

        public override void ToolDestroyed(ITool tool)
        {
            //ClearProjectileList(tool);
        }

        protected void ClearProjectileList(ITool tool)
        {
            var list = ActiveProjectileList(tool);
            foreach (var proj in list)
                proj.LifeTimeout();
            list.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        protected Projectile Fire(ITool tool, Vector3 position, Vector3 forward)
        {
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
            float y = forward.y;
            forward += Random.insideUnitSphere * Mathf.Clamp01(Spread);
            forward.y = y;
            forward.Normalize();
            forward += AdditionalForceVector;

            //spawn the projectile and apply speed, direction, and starting position
            var go = Lazarus.Instance.Summon(ProjPrefab, spawnPos, false);
            var projTrans = go.transform;
            if (ParentToSource)
                projTrans.SetParent(tool.gameObject.transform, true);


            if (AllowLayerChange)
            {
                int l = Layer;
                go.layer = l;
                for (int i = 0; i < projTrans.childCount; i++)
                    projTrans.GetChild(i).gameObject.layer = l;
            }

            //set projectile stats
            var proj = go.GetComponent<Projectile>();
            if (SetProjectileHeading && proj.AdjustableHeading)
                projTrans.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            proj.gameObject.SetActive(true);
            proj.ResetToDefault();
            proj.Source = tool;
            var body = proj.Body;
            if (body != null)
            {
                Vector3 parentVel = Vector3.zero;
                if(InheritVelocity)
                {
                    var cc = tool.Owner.FindComponentInEntity<CharacterController>(true);
                    if (cc != null)
                        parentVel = Vector3.Scale(cc.velocity, ParentVelInfluence);
                }
                body.velocity = Vector3.zero;

                if (body.isKinematic)
                    body.velocity = parentVel + (forward * (Speed + (IgnoreDifficultySpeed ? 0 : Global.DifficultySpeed)));
                else body.AddForce(parentVel + (forward * (Speed + (IgnoreDifficultySpeed ? 0 : Global.DifficultySpeed))), ForceMode.VelocityChange);
            }

            proj.Fired();
            proj.AddDespawnCallback(ProjDespawnCallback);

            ActiveProjectileList(tool).Add(proj);

            //TODO: we can obtain references to stateful projectile mods here (for things like fire patters and such)
            if (OverrideDamage)
            {
                //NOTE: This is pretty ineffient with the null ref checks here!
                DamageOnTrigger dam = proj.DamageTrigger;
                dam.MinDamage = Damage;
                dam.MaxDamage = Damage;
            }

            if(OverrideLifetime)
                proj.Lifetime = Lifetime;

            return proj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        protected Projectile FireBallistic(ITool tool, Vector3 position, Vector3 forward, Vector3 targetOffset, float angle)
        {
            //var toolTrans = tool.gameObject.transform;
            //var toolPos = toolTrans.position;
            var targetPos = tool.CurrentTarget.transform.position + targetOffset;

            //calculate spawn position and orientation
            //var forward = toolTrans.forward;
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
            float y = forward.y;
            forward += Random.insideUnitSphere * Mathf.Clamp01(Spread);
            forward.y = y;
            forward.Normalize();
            forward += AdditionalForceVector;

            //spawn the projectile and apply speed, direction, and starting position
            var go = Lazarus.Instance.Summon(ProjPrefab, spawnPos, false);
            var projTrans = go.transform;
            if (ParentToSource)
                projTrans.SetParent(tool.gameObject.transform, true);


            if (AllowLayerChange)
            {
                int l = Layer;
                go.layer = l;
                for (int i = 0; i < projTrans.childCount; i++)
                    projTrans.GetChild(i).gameObject.layer = l;
            }

            //set projectile stats
            var proj = go.GetComponent<Projectile>();
            if (SetProjectileHeading && proj.AdjustableHeading)
                projTrans.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);


            proj.Source = tool;
            proj.gameObject.SetActive(true);
            proj.ResetToDefault();
            var body = proj.Body;
            Vector3 ballForward = MathUtils.BallisticVelocityVector(position, targetPos, angle);
            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                //if our ballistic calculation gave us bullshit, just make it a straight line I guess?
                if (MathUtils.Approximately(ballForward, Vector3.zero, 0.001f))
                    ballForward = targetPos - position;

                if (body.isKinematic)
                    body.velocity = ballForward;
                else body.AddForce(ballForward, ForceMode.VelocityChange);
            }

            proj.Fired();
            proj.AddDespawnCallback(ProjDespawnCallback);

            ActiveProjectileList(tool).Add(proj);

            //TODO: we can obtain references to stateful projectile mods here (for things like fire patters and such)
            return proj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="tool"></param>
        /// <param name="target"></param>
        protected void ProjDespawnCallback(Projectile projectile, ITool tool, Collider target)
        {
            ActiveProjectileList(tool).Remove(projectile);
        }

        /// <summary>
        /// Helper for getting a blackboard attached to the tool that stores our active projectiles.
        /// We need this because this object is a shared scriptable object so state like this must be
        /// stored on the actual GameObject itself.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected HashSet<Projectile> ActiveProjectileList(ITool tool)
        {
            //weapon might have gone away since this was fired 
            if (Peg.TypeHelper.IsReferenceNull(tool)) return new HashSet<Projectile>();
            var v = tool.GetBlackboardVar(ProjList, new HashSet<Projectile>());
            return v.value;
        }
    }
}
