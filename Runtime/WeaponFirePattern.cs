using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// Tool plugin that provides bullet-hell fireing patterns to weapon tools.
    /// </summary>
    public abstract class AbstractWeaponFirePatternEffect : AbstractProjectileEmitter
    {
        string PatternStartTime;
        string PatternTimerStarted;

        
        protected override void OnEnable()
        {
            base.OnEnable();
            PatternStartTime = RegisterVar("PatternStartTime");
            PatternTimerStarted = RegisterVar("PatternTimerStarted");
        }
        
        /// <summary>
        /// Returns the time that this tool started being used. It is reset
        /// on the first call to base.Use() after base.EndUse() has been called.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected float GetStartTime(ITool tool)
        {
            return tool.GetInstVar<float>(PatternStartTime);
        }
        
        /// <summary>
        /// You must call this before anything else in derived classes to ensure proper timer settings.
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            if (!tool.GetInstVar<bool>(PatternTimerStarted))
            {
                tool.SetInstVar<float>(PatternStartTime, Time.time);
                tool.SetInstVar(PatternTimerStarted, true);
            }
            
            var trans = tool.gameObject.transform;
            var oldForward = trans.forward;

            ApplyPattern(tool, trans, GetStartTime(tool));
            Fire(tool, trans.position, trans.forward);

            trans.forward = oldForward;
        }
        
        public override void EndUse(ITool tool)
        {
            tool.SetInstVar(PatternTimerStarted, false);
        }

        protected abstract void ApplyPattern(ITool tool, Transform toolTrans, float startTime);
    }

}
