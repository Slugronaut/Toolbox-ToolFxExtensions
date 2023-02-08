using NodeCanvas.Framework;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// Applies a sequencial spread pattern to the transform of this tool by rotating its transform.
    /// </summary>
    [CreateAssetMenu(fileName = "Projectile Emitter - Oscillate", menuName = "Assets/Projectile Tools/Projectile Emitter - Oscillate")]
    public class SequenceSpreadPatternEffect : AbstractWeaponFirePatternEffect
    {
        [Tooltip("The max spread angle in degrees, offcenter.")]
        public float OffCenterAngle = 45;
        [Tooltip("Change in degrees per second.")]
        public float OccilationPeriod = 1;
        [Tooltip("Should the vertical aim component be persisted after applying the occilation pattern?")]
        public bool UseVerticalAim;

        /*
        public static float Oscillate(float startRange, float endRange, float oscillateRange, float oscillateOffset, float time)
        {
            oscillateRange = (endRange - startRange) * 0.5f;
            oscillateOffset = oscillateRange + startRange;
            return oscillateOffset + Mathf.Sin(time) * oscillateRange;
        }
        */

        float Offset(float time, float startTime, float period)
        {
            //we apply start-time to ensure we get the same result every use
            return Mathf.PingPong((time - startTime) / period, 1);
        }

        float OscillateAngle(float t, float offCenterAngle, float centerAngle)
        {
            return Mathf.Lerp(-offCenterAngle, offCenterAngle, t) + centerAngle;
        }

        void SetAngle(Transform transform, float startTime)
        {
            var ang = transform.eulerAngles;
            transform.eulerAngles = new Vector3(
                UseVerticalAim ? ang.x : 0,
                OscillateAngle(
                    Offset(Time.time, startTime, OccilationPeriod),
                    OffCenterAngle,
                    transform.eulerAngles.y),
                UseVerticalAim ? ang.y : 0);
        }

        protected override void ApplyPattern(ITool tool, Transform toolTrans, float startTime)
        {
            SetAngle(toolTrans, startTime);
        }
                
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void ToolDestroyed(ITool tool)
        {
            //throw new System.NotImplementedException();
        }

        public override void ToolDisabled(ITool tool)
        {
            //throw new System.NotImplementedException();
        }

    }
}
