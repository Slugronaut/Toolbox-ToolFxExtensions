using DamageSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Toolbox.Lazarus;
using Toolbox.Math;
using UnityEngine;
using Random = UnityEngine.Random;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter - Charged", menuName = "Assets/Projectile Tools/Projectile Emitter - Charged")]
    public class ChargedProjectileEmitter : AbstractChargedToolEffect
    {
        [AssetsOnly]
        [AssetList(CustomFilterMethod = "IsProjectileAsset")]
        [Tooltip("A collection of projectiles, where each one represents a different charge level.")]
        public GameObject[] ProjPrefab;
        public bool ParentToSource;
        public float Speed;
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


        int Layer;
        string ProjList;

        protected override void OnEnable()
        {
            base.OnEnable();
            ProjList = RegisterVar("ProjList");
            Layer = LayerMask.NameToLayer(ProjectileLayer);
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }
        
        public override void ToolDisabled(ITool tool)
        {
            var list = ActiveProjectileList(tool);
            foreach (var proj in list)
                proj.LifeTimeout();
            list.Clear();
        }

        public override void ToolDestroyed(ITool tool)
        {
        }

        public override void Use(ITool tool)
        {
            base.Use(tool);
            if (Trigger == Tool.TriggerPoint.OnUse)
                Process(tool, CurrentLevel(tool));
        }

        public override void EndUse(ITool tool)
        {
            if(CanEndUse(tool))
            {
                if (Trigger == Tool.TriggerPoint.OnEndUse)
                    Process(tool, CurrentLevel(tool));
            }
            base.EndUse(tool);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="level"></param>
        public void Process(ITool tool, int level)
        {
            var toolTrans = tool.gameObject.transform;

            //calculate spawn position and orientation
            var forward = toolTrans.forward;
            Vector3 localOffset = tool.MirrorLocalOffset ? new Vector3(-LocalAimOffset.x, LocalAimOffset.y, LocalAimOffset.z) : LocalAimOffset;

            Vector3 spawnPos = toolTrans.position;
            if (tool.AimMode == AimOffsetModes.AimSpace || OverrideAimSpaceSpawning)
            {
                spawnPos = MathUtils.ForwardSpaceOffset(toolTrans.position, forward, LocalAimOffset + tool.AimOffset);
            }
            else if (tool.AimMode == AimOffsetModes.Bilateral)
            {
                spawnPos = toolTrans.position + localOffset + tool.AimOffset;
            }
            float y = forward.y;
            forward += Random.insideUnitSphere * Mathf.Clamp01(Spread);
            forward.y = y;
            forward.Normalize();
            forward += AdditionalForceVector;

            //spawn the projectile and apply speed, direction, and starting position
            var go = Lazarus.Instance.Summon(ProjPrefab[level], spawnPos, false);
            var projTrans = go.transform;
            if (ParentToSource)
                projTrans.SetParent(toolTrans, true);


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
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                if (body.isKinematic)
                    body.velocity = forward * Speed;
                else body.AddForce(forward * Speed, ForceMode.VelocityChange);
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

            if (OverrideLifetime)
                proj.Lifetime = Lifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="tool"></param>
        /// <param name="target"></param>
        void ProjDespawnCallback(Projectile projectile, ITool tool, Collider target)
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
        HashSet<Projectile> ActiveProjectileList(ITool tool)
        {
            //weapon might have gone away sicne this was fired 
            if (Toolbox.TypeHelper.IsReferenceNull(tool)) return new HashSet<Projectile>();
            var v = tool.GetBlackboardVar(ProjList, new HashSet<Projectile>());
            return v.value;
        }

    }


    


}
