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
            
            //Þarj olma sahnesi
            SceneMan.LoadNextScene();
        }
    }
}
