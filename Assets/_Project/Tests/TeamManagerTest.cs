using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Systems;

public class TeamManagerTest : MonoBehaviour
{
    [SerializeField] private VanguardProtocol.Characters.CharacterBase _player;
    [SerializeField] private VanguardProtocol.Characters.CharacterBase _enemy;

    private void Start()
    {
        var tm = TeamManager.Instance;

        tm.OnCharacterEliminated += (character, team) =>
        {
            Debug.Log($"Character {character.name} from team {team} was eliminated.");
        };

        tm.OnMatchOver += team =>
        {
            Debug.Log($"Match over! Winning team: {team}");
        };

        tm.RegisterCharacter(_player, Team.Blue);
        tm.RegisterCharacter(_enemy, Team.Red);

        Debug.Log($"Blue team members: {tm.GetTeamMembers(Team.Blue).Count}");
        Debug.Log($"Red team members: {tm.GetTeamMembers(Team.Red).Count}");
        Debug.Log($"Alive Blue team members: {tm.GetAliveTeamMembers(Team.Blue).Count}");
        Debug.Log($"Alive Red team members: {tm.GetAliveTeamMembers(Team.Red).Count}");
        Debug.Log($"Lowest HP ally on Blue team: {tm.GetLowestHPAlly(Team.Blue)?.name}");
        Debug.Log($"Lowest HP ally on Red team: {tm.GetLowestHPAlly(Team.Red)?.name}");
        Debug.Log($"Nearest enemy to player: {tm.GetNearestEnemy(_player.transform.position, Team.Blue)?.name}");
        Debug.Log($"Team Health State: Blue - {tm.GetTeamHealthState(Team.Blue)}, Red - {tm.GetTeamHealthState(Team.Red)}");

        _enemy.TakeDamage(100); // Simulate enemy being eliminated
    }
}
