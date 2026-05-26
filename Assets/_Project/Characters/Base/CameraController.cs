using UnityEngine;

namespace VanguardProtocol.Characters
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _pivot;

        [Header("Sensitivity")]
        [SerializeField] private float _xSensitivity = 0.15f;
        [SerializeField] private float _ySensitivity = 0.15f;

        [Header("Clamping")]
        [SerializeField] private float _minY = -20f;
        [SerializeField] private float _maxY = 10f;

        private PlayerInputHandler _inputHandler;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            _inputHandler = GetComponentInParent<PlayerInputHandler>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _yaw = transform.eulerAngles.y;
        }

        private void LateUpdate()
        {
            _yaw += _inputHandler.LookInput.x * _xSensitivity;
            _pitch -= _inputHandler.LookInput.y * _ySensitivity;
            _pitch = Mathf.Clamp(_pitch, _minY, _maxY);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (_pivot != null)
            {
                _pivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
