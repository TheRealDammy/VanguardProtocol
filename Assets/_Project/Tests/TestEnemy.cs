using UnityEngine;
using VanguardProtocol.Characters;
using VanguardProtocol.AbilitySystem;

public class EnemySetup : MonoBehaviour
{
    private void Start()
    {
        var character = GetComponent<CharacterBase>();
        character.SetTeam(GameplayTags.Team_Red);

        character.OnDamageTaken += (source, amount) =>
            Debug.Log($"[TestEnemy] Took {amount} damage. " +
                      $"HP: {character.attributes.health.CurrentValue:F1}");

        character.OnDeath += _ =>
            Debug.Log("[TestEnemy] Eliminated");
    }
}