using UnityEngine;

public class Gate : MonoBehaviour
{
    

    private void Start()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("girdi");
        if (collision.gameObject.CompareTag("Player"))
        {
            
            //�arj olma sahnesi
            SceneMan.LoadNextScene();
        }
    }
}
