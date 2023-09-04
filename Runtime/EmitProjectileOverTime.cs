using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DamageSystem;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Projectile Emitter - Overtime", menuName = "Assets/Projectile Tools/Projectile Emitter - Overtime")]
    public class EmitProjectileOverTime : AbstractProjectileEmitter
    {
        [Tooltip("The number of projectiles emitted. The are emitted evenly spaced in a circle starting at the facing location of the tool.")]
        public float Count = 4;
        public bool UseFixedForward;
        [ShowIf("UseFixedForward")]
        public Vector3 FixedForward;
        [ShowIf("UseFixedForward")]
        public float RotateSpeed = 0;
        [Tooltip("Is the timer allowed to restart if this tool effect is used again while still counting down?")]
        public bool AllowRestart;
        public float EmitTime = 1;
        [Tooltip("Delay between emittance during the emission period.")]
        public float Cooldown = 0.25f;


        List<Projectile> Temp;
        string LastForward;
        string Started;

        protected override void OnEnable()
        {
            base.OnEnable();
            Temp = new List<Projectile>(1);
            LastForward = RegisterVar("LastForward");
            Started = RegisterVar("Started");
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void Use(ITool tool)
        {
            if (!tool.GetInstVar<bool>(Started) || AllowRestart)
            {
                tool.SetInstVar(Started, true);
                tool.DelayedInvoke(EndTime, EmitTime);
                tool.StartToolEffectCoroutine(this);
                //OnStartTimer(tool);
            }

            
        }

        public override void EndUse(ITool tool)
        {

        }

        /// <summary>
        /// This must be duplicated exactly every time because Unity can be fucking stupid sometimes.
        /// Especially when ScriptableObject are involved.
        /// </summary>
        protected virtual void EndTime(ITool tool)
        {
            tool.SetInstVar(Started, false);
            //this.OnEndTimer(tool);
            tool.InvokeEffectCallback(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public override IEnumerator Routine(ITool tool)
        {
            var wait = Peg.CoroutineWaitFactory.RequestWait(Cooldown);
            float startTime = Time.time;
            var trans = tool.gameObject.transform;
            while(Time.time - startTime < EmitTime)
            {
                EmitRadial(tool, trans.position, trans.forward);
                yield return wait;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public List<Projectile> EmitRadial(ITool tool, Vector3 pos, Vector3 forward)
        {
            Temp.Clear();
            if (Count < 1) return Temp;
            
            forward = UseFixedForward ? FixedForward : forward;
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
                Temp.Add(Fire(tool, pos, forward));
                forward = Quaternion.AngleAxis(diff, Vector3.up) * forward;
            }

            return Temp;
        }

    }
}
