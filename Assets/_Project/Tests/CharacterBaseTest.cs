using UnityEngine;
using VanguardProtocol.Characters;
using VanguardProtocol.AbilitySystem;

public class CharacterBaseTest : MonoBehaviour
{
    private void Start()
    {
        var character = GetComponent<CharacterBase>();
        character.SetRole(GameplayTags.Role_Assault);
        character.SetTeam(GameplayTags.Team_Red);

        character.OnHealthChanged += (old, current) =>
        {
            Debug.Log($"Health changed: {old:F1}/{current:F1}");
        };

        character.OnDeath += (c) =>
        {
            Debug.Log($"{c.gameObject.name} has died.");
        };

        // Simulate taking damage
        character.TakeDamage(30f); // 100 -> 70
        character.TakeDamage(50f); // 70 -> 20 (should trigger low health tag)
        character.Heal(15f); // 20 -> 35 (should remove low health tag)
        character.TakeDamage(40f); // 35 -> 0 (should trigger death)

        Debug.Log($"Has Role_Assault tag: {character.Tags.HasTag(GameplayTags.Role_Assault)}");
        Debug.Log($"Has Team_Red tag: {character.Tags.HasTag(GameplayTags.Team_Red)}");
        Debug.Log($"Is Alive: {character.IsAlive}");
    }
}
