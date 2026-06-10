using System.Collections.Generic;

namespace VanguardProtocol.AI.BehaviourTrees
{
    // Sequence: Executes child nodes in order until one fails. If all succeed, the sequence succeeds.
    public class BTSequence : BTNode
    {
        private readonly List<BTNode> _children;
        private int _currentIndex;

        public BTSequence(string name, List<BTNode> children) : base(name)
        {
            _children = children;
        }

        public override void OnEnter()
        {
            _currentIndex = 0;
            _children[0].OnEnter();
        }

        public override BTNodeStatus Tick(BTContext context)
        {
            while (_currentIndex < _children.Count)
            {
                var status = _children[_currentIndex].Tick(context);

                if (status == BTNodeStatus.Running)
                    return BTNodeStatus.Running;

                if (status == BTNodeStatus.Failure)
                {
                    _children[_currentIndex].OnExit(status);
                    return BTNodeStatus.Failure;
                }

                // success, move to next child
                _children[_currentIndex].OnExit(status);
                _currentIndex++;

                if (_currentIndex < _children.Count)
                    _children[_currentIndex].OnEnter();
            }

            return BTNodeStatus.Success;
        }

        public override void OnExit(BTNodeStatus status)
        {
            if (_currentIndex < _children.Count)
                _children[_currentIndex].OnExit(status);

            _currentIndex = 0;
        }
    }

    // Selector: Executes child nodes in order until one succeeds. If all fail, the selector fails.
    public class BTSelector : BTNode
    {
        private readonly List<BTNode> _children;
        private int _currentIndex;
        public BTSelector(string name, List<BTNode> children) : base(name)
        {
            _children = children;
        }
        public override void OnEnter()
        {
            _currentIndex = 0;
            _children[0].OnEnter();
        }
        public override BTNodeStatus Tick(BTContext context)
        {
            while (_currentIndex < _children.Count)
            {
                var status = _children[_currentIndex].Tick(context);

                if (status == BTNodeStatus.Running)
                    return BTNodeStatus.Running;

                if (status == BTNodeStatus.Success)
                {
                    _children[_currentIndex].OnExit(status);
                    return BTNodeStatus.Success;
                }

                // failure, move to next child
                _children[_currentIndex].OnExit(status);
                _currentIndex++;
                if (_currentIndex < _children.Count)
                    _children[_currentIndex].OnEnter();
            }
            return BTNodeStatus.Failure;
        }
        public override void OnExit(BTNodeStatus status)
        {
            if (_currentIndex < _children.Count)
                _children[_currentIndex].OnExit(status);
            _currentIndex = 0;
        }
    }
}