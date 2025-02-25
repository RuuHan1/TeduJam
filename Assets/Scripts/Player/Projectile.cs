using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float lifeTime;

    void Start()
    {
        Destroy(gameObject,lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(Tag.Player.ToString())) 
        {
            GetComponent<Animator>().SetTrigger(AnimationState.IsDeath.ToString());
        }
    }

    private void OnEndDeath()
    {
        Destroy(gameObject);
    }
}
