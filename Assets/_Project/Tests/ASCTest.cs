using System.Collections;
using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Characters;

public class ASCTest : MonoBehaviour
{
    private IEnumerator Start()
    {
        var character = GetComponent<CharacterBase>();
        var asc = GetComponent<AbilitySystemComponent>();

        // Test duration effect — simulate Domino's Slowed status
        var slowEffect = ScriptableObject.CreateInstance<GameplayEffect>();
        slowEffect.effectName = "Slowed";
        slowEffect.durationType = EffectDurationType.Duration;
        slowEffect.duration = 2f;
        slowEffect.grantedTags.Add("Status.Slowed");
        slowEffect.attributeModifiers.Add(new AttributeModifierConfig
        {
            attributeName = "MoveSpeed",
            magnitude = -2f,
            modType = ModificationType.Additive
        });

        Debug.Log($"MoveSpeed before: {character.attributes.moveSpeed.CurrentValue}");
        Debug.Log($"Has Slowed before: {character.tags.HasTag(GameplayTags.Status_Slowed)}");

        asc.ApplyEffect(slowEffect, character);

        Debug.Log($"MoveSpeed after apply: {character.attributes.moveSpeed.CurrentValue}");
        Debug.Log($"Has Slowed after apply: {character.tags.HasTag(GameplayTags.Status_Slowed)}");

        yield return new WaitForSeconds(2.5f);

        Debug.Log($"MoveSpeed after expiry: {character.attributes.moveSpeed.CurrentValue}");
        Debug.Log($"Has Slowed after expiry: {character.tags.HasTag(GameplayTags.Status_Slowed)}");
    }
}