using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public Button newGameButton;
    public Button creditsButton;
    public Button quitButton;

    void Start() {
        newGameButton.onClick.AddListener(
            () => SceneManager.LoadScene("SampleScene")
        );
        creditsButton.onClick.AddListener(
            () => {
                // @Todo
            }
        );
        quitButton.onClick.AddListener(
            () => Application.Quit()
        );
    }
}
