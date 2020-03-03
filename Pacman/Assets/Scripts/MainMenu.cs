using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text highScoreText;

    private void Start()
    {
        if(PlayerPrefs.HasKey("highScore"))
            highScoreText.text = "High  score:   " + PlayerPrefs.GetInt("highScore").ToString();
        else
            highScoreText.text = "High  score:   " + 0.ToString();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("ClassicPacman");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
