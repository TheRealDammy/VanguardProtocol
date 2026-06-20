using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.AI.BehaviourTrees;
using VanguardProtocol.AI.Utility;

namespace VanguardProtocol.AI.BehaviourTree
{
    public class BTNode_UtilityDispatch : BTNode
    {
        private readonly Dictionary<UtilityActionType, BTNode> _subtrees;

        private BTNode _activeSubtree;
        private UtilityActionType _activeAction = UtilityActionType.None;

        public BTNode_UtilityDispatch(Dictionary<UtilityActionType, BTNode> subtrees)
            : base("UtilityDispatch")
        {
            _subtrees = subtrees;
        }

        public override void OnEnter()
        {
            _activeSubtree = null;
            _activeAction = UtilityActionType.None;
        }

        public override BTNodeStatus Tick(BTContext ctx)
        {
            var selected = ctx.TryGetData<UtilityActionType>(BTContext.KEY_SELECTED_ACTION);

            if (selected == UtilityActionType.None)
                return BTNodeStatus.Running;

            if (!_subtrees.TryGetValue(selected, out var subtree))
                return BTNodeStatus.Running;

            // Action changed — cleanly transition
            if (subtree != _activeSubtree)
            {
                _activeSubtree?.OnExit(BTNodeStatus.Failure);
                _activeSubtree = subtree;
                _activeAction = selected;
                _activeSubtree.OnEnter();
            }

            var status = _activeSubtree.Tick(ctx);

            // Don't propagate Success/Failure — dispatch node runs continuously
            // until the BT's higher-priority branches interrupt it
            if (status != BTNodeStatus.Running)
            {
                _activeSubtree.OnExit(status);
                _activeSubtree = null;
                _activeAction = UtilityActionType.None;
            }

            return BTNodeStatus.Running;
        }

        public override void OnExit(BTNodeStatus status)
        {
            _activeSubtree?.OnExit(status);
            _activeSubtree = null;
        }
    }
}