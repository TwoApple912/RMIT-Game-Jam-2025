using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    public string beginSceneName = "Level_1";
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) LoadBeginScene();
    }

    void LoadBeginScene()
    {
        SceneManager.LoadScene(beginSceneName);
    }
}
