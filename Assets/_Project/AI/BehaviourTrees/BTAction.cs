using System;
using VanguardProtocol.AI.BehaviourTrees;

namespace VanguardProtocol.AI.BehaviourTrees
{
    public class BTAction : BTNode
    {
        private readonly Func<BTContext, BTNodeStatus> _action;

        public BTAction(string name, Func<BTContext, BTNodeStatus> action)
            : base(name)
        {
            _action = action;
        }

        public override BTNodeStatus Tick(BTContext context) => _action(context);
    }
}