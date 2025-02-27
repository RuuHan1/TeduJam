using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _input;
    private Rigidbody2D _rb;
    private Transform _transform;

    private Vector2 _moveInput;
    private bool _isMoveable = true;
    private bool _isJumping = false;
    private float _jumpTimeCounter;

    private bool _isGrounded = false;
    private bool _canShoot = true;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileForce = 10f;
    [SerializeField] private float shootCooldown = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpForce = 10f;
    [SerializeField] private float minJumpForce = 3f;
    [SerializeField] private float maxJumpTime = 0.3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        _input = new PlayerInput();
        _rb = GetComponent<Rigidbody2D>();
        _transform = transform;
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += OnMovePerformed;
        _input.Player.Move.canceled += OnMoveCanceled;

        _input.Player.Shoot.performed += OnShootPerformed;

        _input.Player.Jump.started += OnJumpStarted;
        _input.Player.Jump.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        _input.Disable();

        _input.Player.Move.performed -= OnMovePerformed;
        
        _input.Player.Move.canceled -= OnMoveCanceled;
        _input.Player.Shoot.performed -= OnShootPerformed;
        _input.Player.Jump.started -= OnJumpStarted;
        _input.Player.Jump.canceled -= OnJumpCanceled;
    }

    private void Update()
    {

        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.4f, groundLayer);

        if (_isGrounded && animator.GetInteger(AnimationState.JumpState.ToString()) == 3)
        {
            animator.SetInteger(AnimationState.JumpState.ToString(), 0);
        }
        else if (!_isGrounded && animator.GetInteger(AnimationState.JumpState.ToString()) > 1)
        {
            _isMoveable = true;
        }

        if (!_isGrounded && _rb.linearVelocity.y < 0)
        {
            animator.SetInteger(AnimationState.JumpState.ToString(), 3);
        }
  
    }

    private void FixedUpdate()
    {
        ProcessMovement();
        ProcessJump();
    }

    private void ProcessMovement()
    {
        if (_isMoveable)
        {
            
            _rb.linearVelocity = new Vector2(moveSpeed * _moveInput.x, _rb.linearVelocity.y);
        }
    }

    private void ProcessJump()
    {
        if (_isJumping && _jumpTimeCounter > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, maxJumpForce);
            _jumpTimeCounter -= Time.fixedDeltaTime;
        }
    }

    private void StartJump()
    {
        _isJumping = true;
        _jumpTimeCounter = maxJumpTime;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, minJumpForce);
        
    }

    private void StopJump()
    {
        _isJumping = false;
        SetJumpState(3);
    }

    private void SetJumpState(int state)
    {
        animator.SetInteger(AnimationState.JumpState.ToString(), state);
    }
    private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();

        if (_moveInput.x != 0)
        {
            //SoundManager.PlaySound(SoundType.Run); // Footstep sound
        }
        if (_moveInput.x > 0)
        {
            _transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (_moveInput.x < 0)
        {
            _transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        animator.SetFloat(AnimationState.Velocity.ToString(), 1);
        
    }

    private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _moveInput = Vector2.zero;
        animator.SetFloat(AnimationState.Velocity.ToString(), 0);
    }

    private void OnShootPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {

        if (_canShoot)
        {
            //SoundManager.PlaySound(SoundType.Charge);
           
            _rb.linearVelocity = Vector2.zero;
            _isMoveable = false;
            animator.SetTrigger(AnimationState.IsAttack.ToString());
        }
    }

    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (_isGrounded && animator.GetInteger(AnimationState.JumpState.ToString()) == 0)
        {
            // SoundManager.PlaySound(SoundType.Jump);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
            _isMoveable = false;
            SetJumpState(1);
            StartJump();
        }
    }

    private void OnJumpCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        StopJump();
    }

    private void Shoot()
    {
        
        
        GameObject projectile = Instantiate(lightProjectile, projectileSpawnPoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        Vector2 shootDirection = (_transform.rotation.y == 0) ? Vector2.right : Vector2.left;
        projectile.transform.rotation = _transform.rotation;
        projectileRb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
    }

    public void TryShoot()
    {
        
        Shoot();
        _canShoot = false;
        Invoke(nameof(ResetShoot), shootCooldown);
    }

    private void ResetShoot()
    {
        _canShoot = true;
    }

    public void OnAttackEnd()
    {
        _isMoveable = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.Border.ToString()))
        {
            SingleSceneManager.LoadActiveLevel();
        }
        else if (collision.gameObject.CompareTag(Tag.Gate.ToString()))
        {
            VideoManager.Instance.LoadVideo(1);
        }
    }
}
