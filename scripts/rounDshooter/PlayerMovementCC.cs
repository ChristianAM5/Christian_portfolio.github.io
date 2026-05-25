using UnityEngine;
using UnityEngine.InputSystem; // Importante: Usa el nuevo sistema de Input de Unity

public class PlayerMovementCC : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACIÓN ---
    [Header("Ajustes de Movimiento")]
    public float speed = 5f;
    public float crouchSpeed = 2.5f;
    public float mouseSensitivity = 0.2f;
 
    [Header("Ajustes de Agachado")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float timeToCrouch = 10f;
 
    [Header("Física Manual")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
 
    [Header("Sprint & Zoom")]
    public float sprintMultiplier = 2f;
 
    // ── MODIFICADO: El zoom ahora es dinámico según el arma activa ─────────
    // Este valor se usa como FALLBACK si WeaponManager no está asignado
    // o el arma activa no tiene un FOV personalizado en su WeaponData.
    [Tooltip("FOV al apuntar por defecto. Se sobreescribe automáticamente si el " +
             "arma activa tiene adsZoomFOV configurado en su WeaponData.")]
    public float zoomFOV = 40f;
    public float normalFOV = 60f;
    public float zoomSpeed = 10f;
 
    // ── NUEVO: Referencia al WeaponManager para leer el FOV del arma activa ─
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el GameObject que tiene el componente WeaponManager. " +
             "Normalmente es el propio jugador o un hijo suyo.")]
    [SerializeField] private WeaponManager weaponManager;
 
    [Header("Head Bobbing")]
    public float bobFrequency = 5f;
    public float bobAmount = 0.1f;
    private float _bobTimer;
 
    [Header("Interacción Física")]
    public float pushPower = 2.0f;
 
    // --- VARIABLES PRIVADAS ---
    private bool _isSprinting;
    private bool _isZooming;
    private bool _isGrounded;
    private bool _isCrouching;
 
    private CharacterController _controller;
    private Transform _cameraTransform;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private float _xRotation = 0f;
 
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
 
        // Intentamos encontrar el WeaponManager automáticamente si no se asignó en el Inspector
        if (weaponManager == null)
            weaponManager = GetComponentInChildren<WeaponManager>();
    }
 
    // --- MÉTODOS DE INPUT ---
    void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    void OnLook(InputValue value) => ProcessRotation(value.Get<Vector2>());
    void OnJump(InputValue value)
    {
        if (_isGrounded && !_isCrouching)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    void OnCrouch(InputValue value) => _isCrouching = value.isPressed;
    void OnSprint(InputValue value) => _isSprinting = value.isPressed;
    void OnZoom(InputValue value) => _isZooming = value.isPressed;
 
    void Update()
    {
        if (_controller == null || !_controller.enabled) return;
        HandleHeight();
 
        // ── MODIFICADO: FOV dinámico según el arma activa ──────────────────
        // Preguntamos al WeaponManager qué FOV debe usarse para el arma actual.
        // Si no hay WeaponManager o el arma no tiene FOV personalizado, usamos
        // el valor por defecto "zoomFOV" de esta clase.
        float targetZoomFOV = (weaponManager != null)
            ? weaponManager.GetActiveZoomFOV()
            : zoomFOV;
 
        float targetFOV = _isZooming ? targetZoomFOV : normalFOV;
        Camera cam = _cameraTransform.GetComponent<Camera>();
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        // ──────────────────────────────────────────────────────────────────
 
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;
 
        float currentSpeed = _isCrouching ? crouchSpeed : speed;
        if (_isSprinting && !_isCrouching && _moveInput.y > 0) currentSpeed *= sprintMultiplier;
 
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * currentSpeed * Time.deltaTime);
 
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
 
        HandleHeadBob();
    }
 
    void HandleHeight()
    {
        float targetHeight = _isCrouching ? crouchHeight : standingHeight;
        _controller.height = Mathf.Lerp(_controller.height, targetHeight, timeToCrouch * Time.deltaTime);
        _controller.center = new Vector3(0, _controller.height / 2f, 0);
    }
 
    void HandleHeadBob()
    {
        float inputMagnitude = _moveInput.magnitude;
        float baseHeight = _isCrouching ? (crouchHeight * 0.8f) : (standingHeight * 0.8f);
 
        if (inputMagnitude > 0.1f && _isGrounded)
        {
            _bobTimer += Time.deltaTime * (speed * bobFrequency);
            float offset = Mathf.Sin(_bobTimer) * bobAmount;
            _cameraTransform.localPosition = new Vector3(0, baseHeight + offset, 0);
        }
        else
        {
            _bobTimer = 0;
            float smoothY = Mathf.Lerp(_cameraTransform.localPosition.y, baseHeight, Time.deltaTime * 10f);
            _cameraTransform.localPosition = new Vector3(0, smoothY, 0);
        }
    }
 
    void ProcessRotation(Vector2 look)
    {
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);
        _xRotation -= look.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }
 
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;
 
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
    }
}