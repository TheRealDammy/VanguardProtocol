using UnityEngine;
using VanguardProtocol.AbilitySystem;

public class TagSystemTest : MonoBehaviour
{
    void Start()
    {
        var container = new GameplayTagContainer();

        container.OnTagAdded += tag => Debug.Log($"[TagSystem] Tag added: {tag}");
        container.OnTagRemoved += tag => Debug.Log($"[TagSystem] Tag removed: {tag}");

        container.AddTag(GameplayTags.State_InCombat);
        container.AddTag(GameplayTags.AIState_Engaging);

        Debug.Log($"Has State_InCombat: {container.HasTag(GameplayTags.State_InCombat)}");
        Debug.Log($"Has AIState_Idle: {container.HasTag(GameplayTags.AIState_Idle)}");

        container.RemoveTag(GameplayTags.State_InCombat);
        Debug.Log($"Has State_InCombat after removal: {container.HasTag(GameplayTags.State_InCombat)}");
    }
}
