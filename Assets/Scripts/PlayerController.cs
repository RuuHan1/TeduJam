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
    private bool canShoot = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;
    private bool isMoveable = true;

    public void Awake()
    {
        _input = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            if (moveInput.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (moveInput.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            animator.SetFloat("Velocity", 1);
        };

        _input.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
            animator.SetFloat("Velocity", 0);
        };
        _input.Player.Shoot.performed += _ =>
        {
            if (canShoot)
            {
                rb.linearVelocity = Vector2.zero;
                isMoveable = false;
                animator.SetTrigger("IsAttack");
            }
        };
        _input.Player.Jump.started += ctx =>
        {
            if (isGrounded && animator.GetInteger("JumpState") == 0)
            {
                rb.linearVelocity = Vector2.zero;
                isMoveable = false;
                JumpStateChange(1);
            }
        };
        _input.Player.Jump.canceled += ctx => StopJump();
    }
    private void OnDisable()
    {
        _input.Disable();
    }
    private void Update()
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.4f, groundLayer);
        if (isGrounded && animator.GetInteger("JumpState") == 3)
        {
            animator.SetInteger("JumpState", 0);
        }

        if (!isGrounded && animator.GetInteger("JumpState") > 1)
        {
            isMoveable = true;
        }

        if (!isGrounded && rb.linearVelocityY < 0)
        {
            animator.SetInteger("JumpState", 3);
        }
    }
    private void Move()
    {
        if (isMoveable)
        {
            rb.linearVelocity = new Vector2(moveSpeed * moveInput.x, rb.linearVelocity.y);
        }
    }
    public void StartJump()
    {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, minJumpForce);
    }

    private void StopJump()
    {
        isJumping = false;
        JumpStateChange(3);
    }

    private void KeepJump()
    {
        if (isJumping && jumpTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpForce);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }
    }

    public void JumpStateChange(int state)
    {
        animator.SetInteger("JumpState", state);
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(lightProjectile, pSpawnPoin.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = transform.rotation.y == 0 ? Vector2.right : Vector2.left;
        projectile.transform.rotation = transform.rotation;
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
        Gizmos.DrawWireSphere(groundCheck.position, 0.4f);
    }

    private void ResetShoot()
    {
        canShoot = true; // Cooldown bitti, tekrar ateþ edebilir
    }

    public void TryShoot()
    {
        Shoot();
        canShoot = false;
        Invoke(nameof(ResetShoot), shootCooldown); // Cooldown baþlat
        //Canýmýz azalsýn
    }

    private void AttackEnd()
    {
        isMoveable = true;
    }
}
