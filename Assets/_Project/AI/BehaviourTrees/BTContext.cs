using System.Collections.Generic;
using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AI.BehaviourTrees
{
    public class BTContext
    {
        // -- Owner --
        public AICharacterBase Owner { get; }

        // -- Blackboard --
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        // -- Shared keys (avoiding magic strings) --
        public const string KEY_TARGET = "CurrentTarget";
        public const string KEY_ALLY_NEED = "AllyInNeed";
        public const string KEY_COVER_POINT = "CoverPoint";
        public const string KEY_FLANK_POINT = "FlankPoint";
        public const string KEY_ABILITY_READY = "AbilityReady";
        public const string KEY_SELECTED_ACTION = "SelectedAction";

        public BTContext(AICharacterBase owner)
        {
            Owner = owner;
        }

        // -- Blackboard methods --
        public void SetData<T>(string key, T value) => _data[key] = value;
        public T TryGetData<T>(string key)
        {
            if (_data.TryGetValue(key, out object val) && val is T typed)
            {
                return typed;
            }
            return default;
        }

        public bool HasData(string key) => _data.ContainsKey(key);
        public void ClearData(string key) => _data.Remove(key);

        // -- Convinience methods for common keys --
        public CharacterBase GetTarget() => TryGetData<CharacterBase>(KEY_TARGET);

        public void SetTarget(CharacterBase target)
        {
            SetData(KEY_TARGET, target);
            Owner.SetTarget(target);
        }

        public Vector3? GetCoverPoint() => HasData(KEY_COVER_POINT) ? TryGetData<Vector3>(KEY_COVER_POINT) : null;
    }
}
