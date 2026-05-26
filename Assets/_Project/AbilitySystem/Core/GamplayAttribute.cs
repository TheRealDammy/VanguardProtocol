using System;

namespace VanguardProtocol.AbilitySystem
{
    [Serializable]
    public class GameplayAttribute
    {
        public string name { get; set; }

        private float _baseValue;
        private float _currentValue;

        public float BaseValue
        {
            get => _baseValue;
            private set => _baseValue = value;
        }

        public float CurrentValue
        {
            get => _currentValue;
            private set => _currentValue = value;
        }

        // Old value is used for change tracking and event triggering, New value is the actual value after modification
        public event Action<float, float> OnValueChanged;

        public GameplayAttribute(string name, float baseValue)
        {
            this.name = name;
            _baseValue = baseValue;
            _currentValue = baseValue;
        }

        // Called by modifiers to change the current value, will trigger events if value changes
        public void SetCurrentValue(float newValue)
        {
            float clamped = Math.Max(0, newValue); // Prevent negative values, can be changed to allow if needed
            if (Math.Abs(_currentValue - clamped) < float.Epsilon) return; // No change

            float oldValue = _currentValue;
            _currentValue = clamped;
            OnValueChanged?.Invoke(oldValue, _currentValue);
        }

        // Called when a permanent change to the attribute is needed (e.g. leveling up, equipping item), will adjust current value proportionally
        public void SetBaseValue(float newBaseValue)
        {
            if (Math.Abs(_baseValue - newBaseValue) < float.Epsilon) return; // No change
            float oldBase = _baseValue;
            _baseValue = Math.Max(0, newBaseValue); // Prevent negative values, can be changed to allow if needed

            // Adjust current value by the same delta to maintain the same percentage of the base value
            float deltaBase = _baseValue - oldBase;
            SetCurrentValue(_currentValue + deltaBase);
        }

        public void ResetToBaseValue() => SetCurrentValue(_baseValue);

        public override string ToString() => $"{name}: {_currentValue:F1}/{_baseValue:F1}";
    }
}
