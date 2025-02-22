using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _input;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float jumpTimeCounter;
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private Transform pSpawnPoin; // projectile spawn point
    [SerializeField] private float projectileForce = 10.0f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxJumpForce = 10f;
    [SerializeField] private float minJumpForce = 3f;
    [SerializeField] private float maxJumpTime = 0.3f; // Uzun basýldýðýnda maksimum süresi
    [SerializeField] private float shootCooldown = 2; // bekleme süresi
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool canShoot =  true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    public void Awake()
    {
        _input = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();

    }
    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += ctx => moveInput= ctx.ReadValue<Vector2>();
        
        _input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        _input.Player.Shoot.performed += _ => TryShoot();
        _input.Player.Jump.started += ctx => StartJump();
        _input.Player.Jump.canceled += ctx => StopJump();
    }
    private void OnDisable()
    {
        _input.Disable();
    }
    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position,0.4f,groundLayer);
    }
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveSpeed*moveInput.x,rb.linearVelocity.y);
    }
    private void StartJump()
    {
        if (isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,minJumpForce);
        }
    }
    private void StopJump()
    {
        isJumping = false;
    }
    private void KeepJump()
    {
        if (isJumping && jumpTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpForce);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }
    }
    private void Shoot() 
    {
            GameObject projectile = Instantiate(lightProjectile, pSpawnPoin.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
    }
    private void FixedUpdate()
    {
       
        KeepJump();
        Move();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundCheck.position, 0.4f);
    }
    private void ResetShoot()
    {
        canShoot = true; // Cooldown bitti, tekrar ateþ edebilir
    }
    private void TryShoot()
    {
        if (canShoot)
        {
            Shoot();
            canShoot = false;
            Invoke(nameof(ResetShoot), shootCooldown); // Cooldown baþlat
        }
    }
}
