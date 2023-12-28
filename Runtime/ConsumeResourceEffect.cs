using System;
using UnityEngine;
using Peg.Game.ConsumableResource;
using Peg.AutonomousEntities;

namespace ToolFx
{
    /// <summary>
    /// Changes the current level of a resource on the tool owner.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Resource Consumption", menuName = "Assets/Useable Tools/Resource Consumption")]
    public class ConsumeResourceEffect : AbstractCommonToolEffect
    {
        public enum Targets
        {
            Owner,
            Tool,
        }
        public string ResourceName = "Health";
        public float Consumption = 1;
        public bool ConsumePercentage;
        [Tooltip("Who will be the target of the resource consumption? The owner of the tool or the tool itself?")]
        public Targets Target;


        public override void Process(ITool tool)
        {
            var target = (Target == Targets.Owner) ? tool.Owner : tool.gameObject.GetEntityRoot();
            var res = target.FindEntityResourceInterface(ResourceName, false);
            //var res = tool.Owner.gameObject.FindEntityResourceInterface(ResourceName);
            if (ConsumePercentage)
                res.CurrentPercent -= Consumption;
            else res.Current -= Consumption;
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
