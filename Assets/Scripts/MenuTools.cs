using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuTools : MonoBehaviour
{
    public void PlayScene(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnClickSFX(AudioSource ButtonClickSFX)
    {
        ButtonClickSFX.Play();
    }
}


