using Sirenix.OdinInspector;
using System;
using Peg;
using Peg.Game;
using UnityEngine;
using static Peg.Game.GlobalResourceResponder;

namespace ToolFx
{
    /// <summary>
    /// Checks the current level of a resource. If not sufficient, a signal will be
    /// sent to the tool to stop all subsequent effects.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Resource Check", menuName = "Assets/Useable Tools/Resource Check")]
    public class ResourceCheck : AbstractCommonToolEffect
    {
        public enum CheckModes
        {
            LessThan,
            GreaterThan,
        }

        public enum Scopes
        {
            Owner,
            Tool,
            Global,
        }

        public HashedString ResourceName = new HashedString("Health");
        public float Required;
        public CheckModes FailureIf;
        [Tooltip("Where should the resource be queried from?")]
        public Scopes Scope;
        [ShowIf("IsGlobalScope")]
        [Indent]
        [Tooltip("An identifier used in global message handlers to determine if the handler is the correct one.")]
        public HashedString GlobalId;

        bool IsGlobalScope => Scope == Scopes.Global;

        [Tooltip("A list of effects that will have their UseFailed() actions executed. Note: The tool itself will also run all fail effects.")]
        public AbstractToolEffect FailureEffects;

        

        public override void Process(ITool tool)
        {
            IEntityResource res = null;
            switch(Scope)
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

            //if no interface found, fail
            if (res == null)
            {
                SendFail(tool);
                if (FailureEffects != null)
                    FailureEffects.UseFailed(tool);
                return;
            }
            

            switch(FailureIf)
            {
                case CheckModes.GreaterThan:
                    {
                        if (res.Current > Required)
                        {
                            SendFail(tool);
                            if (FailureEffects != null)
                                FailureEffects.UseFailed(tool);
                        }
                        break;
                    }
                case CheckModes.LessThan:
                    {
                        if (res.Current < Required)
                        {
                            SendFail(tool);
                            if(FailureEffects != null)
                                FailureEffects.UseFailed(tool);
                        }
                        break;
                    }
            }
        }

        void SendFail(ITool tool)
        {
            tool.CancelToolEffects();
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
