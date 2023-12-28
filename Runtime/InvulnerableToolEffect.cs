using Peg;
using Peg.AutonomousEntities;
using Peg.Game.ConsumableResource;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Invulnerable", menuName = "Assets/Useable Tools/Invulnerable Tool")]
    public class InvulnerableToolEffect : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;

        [Tooltip("How much invincibility time should be applied to the entity?")]
        public float Time;

        [Tooltip("Should the invulnerable state be removed if this tool is disabled?")]
        public bool CancelOnDisable;
        

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void ToolDisabled(ITool tool)
        {
            if (CancelOnDisable && !TypeHelper.IsReferenceNull(tool) && !TypeHelper.IsReferenceNull(tool.Owner))
            {
                base.ToolDisabled(tool);
                var hp = tool.Owner.FindComponentInEntity<Health>(true);
                if(!TypeHelper.IsReferenceNull(hp))
                    hp.CancelInvincibility();
            }
        }

        public override void Use(ITool tool)
        {
            if(Trigger == Tool.TriggerPoint.OnUse)
            {
                var hp = tool.Owner.FindComponentInEntity<Health>(true);
                if (!hp.IsDead)
                    hp.UnionInvincibility(Time);
            }
        }

        public override void EndUse(ITool tool)
        {
            if(Trigger == Tool.TriggerPoint.OnEndUse)
            {
                var hp = tool.Owner.FindComponentInEntity<Health>(true);
                if (!hp.IsDead)
                    hp.UnionInvincibility(Time);
            }
        }
    }
}
