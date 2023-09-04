using Sirenix.OdinInspector;
using Peg;
using Peg.Behaviours;
using Peg.Graphics;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Reset Animator Trigger", menuName = "Assets/Useable Tools/Reset Animator Trigger")]
    public class ResetAnimatorTrigger : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        public HashedString[] Triggers;


        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
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
                //WANRING: This cast might break if we have different kinds of ITools in the future!!
                for (int i = 0; i < Triggers.Length; i++)
                    anim.ResetTrigger(Triggers[i].Hash);
            }

        }
    }
}
