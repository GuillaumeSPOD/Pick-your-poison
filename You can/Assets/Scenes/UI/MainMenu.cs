using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button StartButton;

    private float Timer = 2f;

    void Start()
    {
        StartButton.interactable = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
    }

    void FixedUpdate()
    {
        if (Timer !=0)
        {
            Timer -= Time.deltaTime;
            if (Timer < 0)
            {  
                Timer = 0;
                StartButton.interactable = true;
            }
        }

    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Main Game");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
