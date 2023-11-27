using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string scenename;

    public void PlayScene()
    {
        SceneManager.LoadScene(scenename);
    }
}


