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
        if (!collision.gameObject.CompareTag("Player")) 
        {
            GetComponent<Animator>().SetTrigger("IsBoom");
        }
    }

    private void BoomEnd()
    {
        Destroy(gameObject);
    }
}
