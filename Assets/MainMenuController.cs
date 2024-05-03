using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playSoloButton;
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button exitGameButton;

    // Start is called before the first frame update
    void Start()
    {
        playSoloButton.onClick.AddListener(() => {
            SceneManager.LoadScene("AIFlatLevel");
        });
        playMultiplayerButton.onClick.AddListener(() => {
            // Debug.Log("Transition does not work right now");
            SceneManager.LoadScene("AIFlatLevelForNetwork");
        });
        exitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}
