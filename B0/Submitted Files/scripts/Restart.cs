using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{

    // Update is called once per frame
    public void RestartGame()
    {

        SceneManager.LoadScene("Roll_Ball");

    }
}
