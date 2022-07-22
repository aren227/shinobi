using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public GlobalData globalData;
    public Button newGameButton;
    public Button creditsButton;
    public Button quitButton;
    public Button backToTitleButton;

    public Text resultText;

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
        if (resultText) {
            if (globalData.isComplete) {
                resultText.text = "MISSION COMPLETE";
            }
            else {
                resultText.text = "MISSION FAILED";
            }
        }
    }
}
