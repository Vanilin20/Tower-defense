using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    // Назва сцени, куди потрібно перейти
    public string sceneName;

    // Метод, який викликається кнопкою
    public void LoadSceneByName()
    {
        SceneManager.LoadScene(sceneName);
    }
}
