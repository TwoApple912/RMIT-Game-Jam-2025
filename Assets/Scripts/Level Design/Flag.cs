using UnityEngine;
using UnityEngine.SceneManagement;

public class Flag : MonoBehaviour
{
    void 
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        Debug.Log("Level Completed!");
        SceneManager.LoadScene();
    }
}
