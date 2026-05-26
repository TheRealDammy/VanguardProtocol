using UnityEngine;
using UnityEngine.InputSystem;
using VanguardProtocol.Input;

namespace VanguardProtocol.Characters
{
    public class PlayerInputHandler : MonoBehaviour
    {
        // -- Raw Input Values --
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool ADSHeld { get; private set; }

        // -- Consumable Input Events --
        public bool RollPressed { get; private set; }
        public bool FirePressed { get; private set; }
        public bool Ability1Pressed { get; private set; }
        public bool Ability2Pressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool PingPressed { get; private set; }


        private VG_InputActions _inputActions;

        private void Awake()
        {
            _inputActions = new VG_InputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            var player = _inputActions.Player;

            // -- Update Raw Input Values --
            MoveInput = player.Move.ReadValue<Vector2>();
            LookInput = player.Look.ReadValue<Vector2>();
            SprintHeld = player.Sprint.IsPressed();
            ADSHeld = player.ADS.IsPressed();

            // -- Update Consumable Input Events --
            RollPressed = player.Roll.WasPressedThisFrame();
            FirePressed = player.Fire.WasPressedThisFrame();
            Ability1Pressed = player.Ability1.WasPressedThisFrame();
            Ability2Pressed = player.Ability2.WasPressedThisFrame();
            InteractPressed = player.Interact.WasPressedThisFrame();
            PingPressed = player.Ping.WasPressedThisFrame();
        }
    }
}
