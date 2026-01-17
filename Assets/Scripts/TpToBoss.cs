using UnityEngine;
using UnityEngine.SceneManagement;

public class TpToBoss : MonoBehaviour
{
    public string targetSceneName;
    public Vector3 spawnPosition;
    public string playerTag = "Player";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (RunContextSystem.Instance != null)
            {
                RunContextSystem.Instance.SaveRunContext();
            }
            TeleportToScene();
        }
    }

    private void TeleportToScene()
    {
        SceneManager.LoadScene("BossRoom");
    }
}
