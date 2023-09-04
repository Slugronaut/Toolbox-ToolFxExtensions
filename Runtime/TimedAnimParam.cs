using Peg;
using Peg.Behaviours;
using UnityEngine;
using Sirenix.OdinInspector;
using Peg.Graphics;

namespace ToolFx
{
    /// <summary>
    /// Sets animator parameters for a time period, then sets them to a new value when the timer is up.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Timed Owner Animator Param", menuName = "Assets/Useable Tools/Timed Animator Param")]
    public class TimedAnimParam : AbstractTimerToolEffect
    {
        [FoldoutGroup("Start Params")]
        public SetAnimParamOnMessage.BoolParam[] Bools;
        [FoldoutGroup("Start Params")]
        public SetAnimParamOnMessage.FloatParam[] Floats;
        [FoldoutGroup("Start Params")]
        public SetAnimParamOnMessage.IntParam[] Ints;
        [FoldoutGroup("Start Params")]
        public HashedString[] Triggers;
        [FoldoutGroup("Start Params")]
        public HashedString[] QueuedTriggers;

        [FoldoutGroup("End Params")]
        public SetAnimParamOnMessage.BoolParam[] EndBools;
        [FoldoutGroup("End Params")]
        public SetAnimParamOnMessage.FloatParam[] EndFloats;
        [FoldoutGroup("End Params")]
        public SetAnimParamOnMessage.IntParam[] EndInts;

        [Space(10)]
        public SetAnimParamOnMessage.AnimationTimeParam AnimationTimeParam;


        protected override void OnEndTimer(ITool tool)
        {
            HandleEnd(tool);
        }

        protected override void OnStartTimer(ITool tool)
        {
            HandleStart(tool);
        }

        void HandleStart(ITool tool)
        {
            var anims = tool.Owner.FindComponentsInEntity<Animator>(true);

            foreach (var anim in anims)
            {
                for (int i = 0; i < Bools.Length; i++)
                    anim.SetBool(Bools[i].Name.Hash, Bools[i].State);

                for (int i = 0; i < Floats.Length; i++)
                    anim.SetFloat(Floats[i].Name.Hash, Floats[i].State);

                for (int i = 0; i < Ints.Length; i++)
                    anim.SetInteger(Ints[i].Name.Hash, Ints[i].State);

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

        void HandleEnd(ITool tool)
        {
            var anims = tool.Owner.FindComponentsInEntity<Animator>(true);

            foreach (var anim in anims)
            {
                for (int i = 0; i < EndBools.Length; i++)
                    anim.SetBool(EndBools[i].Name.Hash, EndBools[i].State);

                for (int i = 0; i < EndFloats.Length; i++)
                    anim.SetFloat(EndFloats[i].Name.Hash, EndFloats[i].State);

                for (int i = 0; i < EndInts.Length; i++)
                    anim.SetInteger(EndInts[i].Name.Hash, EndInts[i].State);
            }
        }


    }
}
