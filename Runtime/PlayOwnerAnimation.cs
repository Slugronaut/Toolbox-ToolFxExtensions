using Peg;
using Peg.AutonomousEntities;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Play Owner Animation", menuName = "Assets/Useable Tools/Play Owner Animation")]
    public class PlayOwnerAnimation : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [Tooltip("The animation state to play.")]
        public HashedString State;
        [Tooltip("The layer to play the animation on.")]
        public int Layer;

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
            //needed because of the fact that ScriptableObject lifetimes are fucky
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
            var fsms = tool.Owner.FindComponentsInEntity<Animator>(true);
            for (int i = 0; i < fsms.Length; i++)
                fsms[i].Play(State.Hash, Layer);

        }
    }
}
