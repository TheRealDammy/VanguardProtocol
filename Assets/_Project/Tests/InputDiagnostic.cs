using UnityEngine;
using UnityEngine.InputSystem;

public class InputDiagnostic : MonoBehaviour
{
    private void Update()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        if (mouse != null)
            Debug.Log($"Mouse Delta: {mouse.delta.ReadValue()}");

        if (keyboard != null)
            Debug.Log($"W:{keyboard.wKey.isPressed} " +
                      $"A:{keyboard.aKey.isPressed} " +
                      $"S:{keyboard.sKey.isPressed} " +
                      $"D:{keyboard.dKey.isPressed}");

        if (gamepad != null)
            Debug.Log($"Gamepad Left Stick: {gamepad.leftStick.ReadValue()} " +
                      $"Right Stick: {gamepad.rightStick.ReadValue()}");
        else
            Debug.Log("No gamepad detected");

        foreach (var device in InputSystem.devices)
            Debug.Log($"Device: {device.name} | {device.deviceId}");
    }
}