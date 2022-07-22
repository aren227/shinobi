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
    public Button backToTitleButton;

    void Start() {
        if (newGameButton) {
            newGameButton.onClick.AddListener(
                () => SceneManager.LoadScene("SampleScene")
            );
        }
        if (creditsButton) {
            creditsButton.onClick.AddListener(
                () => SceneManager.LoadScene("Credits")
            );
        }
        if (quitButton) {
            quitButton.onClick.AddListener(
                () => Application.Quit()
            );
        }
        if (backToTitleButton) {
            backToTitleButton.onClick.AddListener(
                () => SceneManager.LoadScene("Title")
            );
        }
    }
}
