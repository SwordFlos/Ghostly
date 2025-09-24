using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform target;
    private float _distanceToPlayer;
    private Vector2 _mouseInput;
    private Vector2 _joystickInput;

    [Header("Input Settings")]
    [SerializeField] private MouseSensitivity mouseSensitivity;
    [SerializeField] private JoystickSensitivity joystickSensitivity;
    [SerializeField] private CameraAngle cameraAngle;

    private CameraRotation _cameraRotation;

    #endregion

    private void Awake()
    {
        _distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // Lock and hide the cursor when the game starts
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Mouse input handler
    public void LookMouse(InputAction.CallbackContext context)
    {
        _mouseInput = context.ReadValue<Vector2>();
    }

    // Joystick input handler (NEW)
    public void LookJoystick(InputAction.CallbackContext context)
    {
        _joystickInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        HandleMouseInput();
        HandleJoystickInput();

        // Apply clamping
        _cameraRotation.Pitch = Mathf.Clamp(_cameraRotation.Pitch, cameraAngle.min, cameraAngle.max);
    }

    private void HandleMouseInput()
    {
        _cameraRotation.Yaw += _mouseInput.x * mouseSensitivity.horizontal * BoolToInt(mouseSensitivity.invertHorizontal) * Time.deltaTime;
        _cameraRotation.Pitch += _mouseInput.y * mouseSensitivity.vertical * BoolToInt(mouseSensitivity.invertVertical) * Time.deltaTime;
    }

    private void HandleJoystickInput()
    {
        // Joystick input is already frame-rate independent, so we don't need Time.deltaTime
        // The deadzone is handled by the Input System's stick deadzone settings
        _cameraRotation.Yaw += _joystickInput.x * joystickSensitivity.horizontal * BoolToInt(joystickSensitivity.invertHorizontal);
        _cameraRotation.Pitch += _joystickInput.y * joystickSensitivity.vertical * BoolToInt(joystickSensitivity.invertVertical);
    }

    private void LateUpdate()
    {
        transform.eulerAngles = new Vector3(_cameraRotation.Pitch, _cameraRotation.Yaw, 0.0f);
        transform.position = target.position - transform.forward * _distanceToPlayer;
    }

    private static int BoolToInt(bool b) => b ? 1 : -1;
}

[Serializable]
public struct MouseSensitivity
{
    public float horizontal;
    public float vertical;
    public bool invertHorizontal;
    public bool invertVertical;
}

// NEW: Separate sensitivity settings for joystick
[Serializable]
public struct JoystickSensitivity
{
    public float horizontal;
    public float vertical;
    public bool invertHorizontal;
    public bool invertVertical;
}

public struct CameraRotation
{
    public float Pitch;
    public float Yaw;
}

[Serializable]
public struct CameraAngle
{
    public float min;
    public float max;
}