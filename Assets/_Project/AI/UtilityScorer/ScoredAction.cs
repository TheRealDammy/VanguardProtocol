using System;
using VanguardProtocol.AI.BehaviourTrees;

namespace VanguardProtocol.AI.Utility
{
    public class ScoredAction
    {
        public UtilityActionType ActionType { get; }
        public string Label { get; }
        public float LastScore { get; private set; }

        private readonly Func<BTContext, float> _scorer;

        public ScoredAction(UtilityActionType type, string label,
            Func<BTContext, float> scorer)
        {
            ActionType = type;
            Label = label;
            _scorer = scorer;
        }

        public float Evaluate(BTContext ctx)
        {
            LastScore = _scorer(ctx);
            return LastScore;
        }
    }
}