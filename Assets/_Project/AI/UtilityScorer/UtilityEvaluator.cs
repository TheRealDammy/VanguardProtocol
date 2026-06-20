using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.AI.BehaviourTrees;

namespace VanguardProtocol.AI.Utility
{
    public class UtilityEvaluator : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _evaluationInterval = 0.25f;

        [Header("Debug")]
        [SerializeField] private bool _logScores = false;

        private readonly List<ScoredAction> _actions = new List<ScoredAction>();

        private BehaviourTrees.BehaviourTree _bt;
        private float _timer;

        // Exposed for telemetry and debug UI
        public IReadOnlyList<ScoredAction> Actions => _actions;
        public UtilityActionType SelectedAction { get; private set; }

        private void Awake()
        {
            _bt = GetComponent<BehaviourTrees.BehaviourTree>();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = _evaluationInterval;
            Evaluate();
        }

        // Called by role-specific AI to register their scoring functions
        public void RegisterAction(ScoredAction action)
        {
            _actions.Add(action);
        }

        private void Evaluate()
        {
            if (_bt?.Context == null || _actions.Count == 0) return;

            ScoredAction best = null;
            float bestScore = float.MinValue;

            foreach (var action in _actions)
            {
                float score = action.Evaluate(_bt.Context);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            if (best == null) return;

            SelectedAction = best.ActionType;
            _bt.Context.SetData(BTContext.KEY_SELECTED_ACTION, SelectedAction);

            if (_logScores) LogScores(best);
        }

        private void LogScores(ScoredAction winner)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Utility] {gameObject.name} — Selected: {winner.Label} ({winner.LastScore:F1})");

            foreach (var a in _actions)
                sb.AppendLine($"  {a.Label}: {a.LastScore:F1}{(a == winner ? " ✓" : "")}");

            Debug.Log(sb.ToString());
        }
    }
}