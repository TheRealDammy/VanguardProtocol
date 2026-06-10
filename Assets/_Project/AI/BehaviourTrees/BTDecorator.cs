using System;

namespace VanguardProtocol.AI.BehaviourTrees
{
    // Condition - A decorator that checks a condition before executing its child node. If the condition fails, the decorator returns failure without executing the child.
    public class BTCondition : BTNode
    {
        private readonly Func<BTContext, bool> _condition;
        private readonly BTNode _child;

        public BTCondition(string name, Func<BTContext, bool> condition, BTNode child) : base(name)
        {
            _condition = condition;
            _child = child;
        }
        public override void OnEnter() => _child.OnEnter(); // Enter the child node when this decorator is entered

        public override BTNodeStatus Tick(BTContext context)
        {
            if (!_condition(context))
                return BTNodeStatus.Failure; // Condition failed, return failure without ticking child

            return _child.Tick(context); // Condition passed, tick the child node
        }
        public override void OnExit(BTNodeStatus status) => _child.OnExit(status); // Exit the child node when this decorator is exited
    }

    // Inverter - A decorator that inverts the result of its child node. Success becomes failure, and failure becomes success.
    public class BTInverter : BTNode
    {
        private readonly BTNode _child;
        public BTInverter(string name, BTNode child) : base(name)
        {
            _child = child;
        }
        public override void OnEnter() => _child.OnEnter(); // Enter the child node when this decorator is entered
        public override BTNodeStatus Tick(BTContext context)
        {
            var status = _child.Tick(context);

            return status switch
            {
                BTNodeStatus.Success => BTNodeStatus.Failure,
                BTNodeStatus.Failure => BTNodeStatus.Success,
                _ => BTNodeStatus.Running // Running remains unchanged
            };
        }
        public override void OnExit(BTNodeStatus status) => _child.OnExit(status); // Exit the child node when this decorator is exited
    }

    // Repeater - A decorator that repeats its child node n number of times or indefinitely until the child fails.
    public class BTRepeater : BTNode
    {
        private readonly BTNode _child;
        private readonly int _repeatCount; // -1 for infinite
        private int _currentCount;

        public BTRepeater(string name, BTNode child, int repeatCount = -1) : base(name)
        {
            _child = child;
            _repeatCount = repeatCount;
        }
        public override void OnEnter()
        {
            _currentCount = 0;
            _child.OnEnter(); // Enter the child node when this decorator is entered
        }
        public override BTNodeStatus Tick(BTContext context)
        {
            if (_repeatCount != -1 && _currentCount >= _repeatCount)
                return BTNodeStatus.Success; // Reached the repeat count, return success

            var status = _child.Tick(context);

            if (status == BTNodeStatus.Running)
                return BTNodeStatus.Running; // If child is still running, keep running

            _child.OnExit(status); // Exit the child node after it finishes (success or failure)


            if (status == BTNodeStatus.Failure)
                return BTNodeStatus.Failure; // If child fails, stop repeating and return failure

            if (status == BTNodeStatus.Success)
            {
                _currentCount++;
                if (_repeatCount != -1 && _currentCount >= _repeatCount)
                    return BTNodeStatus.Success; // Reached the repeat count, return success
                _child.OnEnter(); // Re-enter the child node for the next iteration
            }

            _child.OnEnter(); // Re-enter the child node for the next iteration
            return BTNodeStatus.Running; // Still running
        }
        public override void OnExit(BTNodeStatus status) => _child.OnExit(status); // Exit the child node when this decorator is exited
    }
}
