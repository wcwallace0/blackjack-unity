using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Fades out the buttons and blur effect, waits a little, then loads the Blackjack scene
    public void StartGame() {
        SceneManager.LoadScene("Blackjack");
    }
}
