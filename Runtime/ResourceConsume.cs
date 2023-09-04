using Sirenix.OdinInspector;
using System;
using Peg;
using Peg.Game;
using UnityEngine;
using static Peg.Game.GlobalResourceResponder;
using static ToolFx.ResourceCheck;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Resource Consume", menuName = "Assets/Useable Tools/Resource Consume")]
    public class ResourceConsume : AbstractCommonToolEffect
    {
        public enum ConsumeModes
        {
            Amount,
            Percent,
        }

        public HashedString ResourceName = new HashedString("Health");
        [Tooltip("Where should the resource be queried from?")]
        public Scopes Scope;
        [ShowIf("IsGlobalScope")]
        [Indent]
        [Tooltip("An identifier used in global message handlers to determine if the handler is the correct one.")]
        public HashedString GlobalId;
        public ConsumeModes ConsumeMode;
        public bool ConsumePercentage;
        [Tooltip("The amount to reduce the resource by.")]
        public float Consume = 1.0f;

        bool IsGlobalScope => Scope == Scopes.Global;



        public override void Process(ITool tool)
        {
            IEntityResource res = null;
            switch (Scope)
            {
                case Scopes.Owner:
                    {
                        res = tool.Owner.gameObject.FindEntityResourceInterface(ResourceName.Hash);
                        break;
                    }
                case Scopes.Tool:
                    {
                        res = tool.gameObject.FindEntityResourceInterface(ResourceName.Hash);
                        break;
                    }
                case Scopes.Global:
                    {
                        GlobalMessagePump.Instance.PostMessage(DemandEntityResource.PrepareDemand(GlobalId.Hash));
                        res = DemandEntityResource.Shared.Desired;
                        break;
                    }
            }


            if (res != null)
            {
                if (ConsumeMode == ConsumeModes.Amount)
                    res.Current -= Consume;
                else res.CurrentPercent -= Consume;
            }
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
