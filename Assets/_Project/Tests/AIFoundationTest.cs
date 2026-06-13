using UnityEngine;
using VanguardProtocol.AI.BehaviourTrees;
using VanguardProtocol.Characters;

public class AIFoundationTest : MonoBehaviour
{
    private AICharacterBase _ai;
    private BehaviourTree _bt;
    private AIStateMachine _sm;

    private void Start()
    {
        _ai = GetComponent<AICharacterBase>();
        _bt = GetComponent<BehaviourTree>();
        _sm = GetComponent<AIStateMachine>();

        // Build a minimal test BT — move to a point then succeed
        var root = new BTSequence("Root", new System.Collections.Generic.List<BTNode>
        {
            new BTCondition("IsAlive",
                ctx => ctx.Owner.IsAlive,
                new BTAction("LogAlive", ctx =>
                {
                    Debug.Log($"[BT] {ctx.Owner.name} is alive " +
                              $"| State: {_sm.CurrentState}");
                    return BTNodeStatus.Success;
                })
            )
        });

        _bt.Initialize(_ai, root);

        // Test state machine transitions
        _sm.OnStateChanged += (old, next) =>
            Debug.Log($"[StateMachine] {old} → {next}");

        _sm.TransitionTo(AIStateType.Engaging);
        _sm.TransitionTo(AIStateType.Retreating);
        _sm.TransitionTo(AIStateType.Idle);

        // Test NavMesh movement — move to a point 5 units ahead
        Vector3 target = transform.position + transform.forward * 5f;
        _ai.MoveTo(target);
        Debug.Log($"[Nav] Moving to {target}");
    }

    private void Update()
    {
        if (_ai.HasReachedDestination())
            Debug.Log("[Nav] Reached destination");
    }
}