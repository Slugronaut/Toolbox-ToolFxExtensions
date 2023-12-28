using Sirenix.OdinInspector;
using Peg;
using Peg.Behaviours;
using Peg.Graphics;
using UnityEngine;
using Peg.AutonomousEntities;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Set Owner Animator Param", menuName = "Assets/Useable Tools/Animator Param")]
    public class SetOwnerAnimatorParam : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [FoldoutGroup("Params")]
        public SetAnimParamOnMessage.BoolParam[] Bools;
        [FoldoutGroup("Params")]
        public SetAnimParamOnMessage.FloatParam[] Floats;
        [FoldoutGroup("Params")]
        public HashedString[] Triggers;
        [FoldoutGroup("Params")]
        public HashedString[] QueuedTriggers;
        public SetAnimParamOnMessage.AnimationTimeParam AnimationTimeParam;


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
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }

        void PlayAnims(ITool tool)
        {
            var anims = tool.Owner.FindComponentsInEntity<Animator>(true);

            foreach (var anim in anims)
            {
                for (int i = 0; i < Bools.Length; i++)
                    anim.SetBool(Bools[i].Name.Hash, Bools[i].State);

                for (int i = 0; i < Floats.Length; i++)
                    anim.SetFloat(Floats[i].Name.Hash, Floats[i].State);

                //WANRING: This cast might break if we have different kinds of ITools in the future!!
                for (int i = 0; i < Triggers.Length; i++)
                    anim.SetTriggerOneFrame(tool as Tool, Triggers[i].Hash);

                for (int i = 0; i < QueuedTriggers.Length; i++)
                    anim.SetTrigger(QueuedTriggers[i].Hash);
            }

            if (AnimationTimeParam.Name.Hash != 0)
            {
                var animsEx = tool.Owner.FindComponentsInEntity<AnimatorEx>(true);
                foreach (var animEx in animsEx)
                {
                    animEx.Pump();
                    animEx.SetAnimationScaledParam(AnimationTimeParam.Value, AnimationTimeParam.Name.Hash, 0);
                }
            }

        }
    }
}
