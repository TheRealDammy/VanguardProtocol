using System.Collections;
using UnityEngine;
using VanguardProtocol.Systems;
using VanguardProtocol.AbilitySystem;

public class MatchStateTest : MonoBehaviour
{
    [SerializeField] private VanguardProtocol.Characters.CharacterBase _enemy;

    private IEnumerator Start()
    {
        var msm = MatchStateManager.Instance;
        var tm = TeamManager.Instance;

        msm.OnStateChanged += (old, next) =>
            Debug.Log($"[Test] State: {old} → {next}");

        msm.OnCombatStarted += () =>
            Debug.Log($"[Test] Combat started — timer running");

        msm.OnMatchResolved += winner =>
            Debug.Log($"[Test] Resolved — Winner: {winner} " +
                      $"| Match time: {msm.GetMatchTime()}");

        // Register characters
        var player = FindFirstObjectByType<VanguardProtocol.Characters.PlayerCharacter>();
        tm.RegisterCharacter(player, Team.Blue);
        tm.RegisterCharacter(_enemy, Team.Red);

        // Start match
        msm.StartMatch();

        // Wait for combat phase then simulate kill
        yield return new WaitForSeconds(4f);
        Debug.Log($"[Test] In combat: {msm.IsInCombat()}");

        _enemy.TakeDamage(999f);
    }
}