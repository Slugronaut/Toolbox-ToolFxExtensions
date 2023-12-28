using Peg;
using Peg.Graphics;
using UnityEngine;
using Sirenix.OdinInspector;
using Peg.AutonomousEntities;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Play Owner AnimationEx", menuName = "Assets/Useable Tools/Play Owner AnimationEx")]
    public class PlayOwnerAnimationEx : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [Tooltip("The animation state to play.")]
        public HashedString State;
        [Tooltip("The layer to play the animation on.")]
        public int Layer;
        [Tooltip("Should the animations speed be contolled by an animator param?")]
        public bool UseAnimSpeedParam;
        [ShowIf("UseAnimSpeedParam")]
        [Indent]
        public HashedString ParamName;
        [Tooltip("Should time be controlled by this SO or read from the instanced tool's blackboard?")]
        public bool BlackboardTime;
        [HideIf("BlackboardTime", true)]
        [Indent(1)]
        [Tooltip("How long the animation should play. Animations will be scaled to fit this time.")]
        public float Time;
        [ShowIf("BlackboardTime", true)]
        [Indent(1)]
        [Tooltip("A blackboard variable that stores how long the animation should play. Animations will be scaled to fit this time.")]
        public string TimeVariable = "AnimTime";
        [Tooltip("Does 'Time' represent a time in seconds or a scaling factor?")]
        public bool FixedTime = true;
        [Tooltip("How does this animation queue up with previously executed animations?")]
        public AnimatorEx.PlayMode Mode;


        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnEndUse)
                PlayAnims(tool);
        }

        public override void Use(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
                PlayAnims(tool);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //needed because of the fact that ScriptableObject lifetimes are fucky when in the editor
            if(State != null)
                State.Value = State.Value;
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }

        void PlayAnims(ITool tool)
        {
            float time = BlackboardTime ? tool.GetInstVar<float>(TimeVariable) : Time;

            var fsms = tool.Owner.FindComponentsInEntity<AnimatorEx>(true);
            for (int i = 0; i < fsms.Length; i++)
            {
                fsms[i].ExecuteAnimation(State.Hash, Layer, time, FixedTime, Mode);
                if (UseAnimSpeedParam)
                {
                    fsms[i].Pump();
                    fsms[i].SetAnimationScaledParam(time, ParamName.Hash, 0);
                }
            }

        }
    }
}
