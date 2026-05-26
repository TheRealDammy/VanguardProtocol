using System;
using Unity.VisualScripting;

namespace VanguardProtocol.AbilitySystem
{
    [Serializable]
    public struct GameplayTag : IEquatable<GameplayTag>
    {
        public static readonly GameplayTag none = new GameplayTag(string.Empty);

        public readonly string name;
        public readonly string description;
        public readonly int hash;

        public GameplayTag(string name, string description = "")
        {
            this.name = name;
            this.description = description;
            hash = string.IsNullOrEmpty(name) ? 0 : name.GetHashCode();
        }

        public bool IsValid() => hash != 0 && hash != none.hash;

        // Hierachy check - "State.LowHealth" should match "State"
        public bool MatchesTag(GameplayTag other)
        {
            if (!other.IsValid())
                return false;
            if (hash == other.hash)
                return true;
            // Check if this tag is a parent of the other tag
            return other.name.StartsWith(name + ".", StringComparison.Ordinal);
        }

        public bool Equals(GameplayTag other) => hash == other.hash;
        public override bool Equals(object obj) => obj is GameplayTag other && Equals(other);
        public override int GetHashCode() => hash;
        public override string ToString() => name;

        public static bool operator ==(GameplayTag left, GameplayTag right) => left.Equals(right);
        public static bool operator !=(GameplayTag left, GameplayTag right) => !left.Equals(right);
    }
}
