using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float enemySpeed;
    [SerializeField] private float damage;
    [SerializeField] private float followRange = 5f;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag(Tag.Player.ToString()).transform;
    }


    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= followRange)
        {
            MoveTowardsPlayer();
        }
    }
    private void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // X ekseninde hareket, Y sabit
        Vector2 targetPosition = new Vector2(playerTransform.position.x, transform.position.y);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, enemySpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, playerTransform.rotation.y == 0 ? 180 : 0, -90);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.Player.ToString()))
        {
            //gameObject.GetComponent<Player>().TakeDamage(damage);
        }
        else if (collision.gameObject.CompareTag(Tag.Projectile.ToString()))
        {
            Destroy(gameObject);
        }
    }

}
