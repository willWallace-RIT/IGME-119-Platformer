/** Jonathan So, jds7523@rit.edu
 * This class will handle swapping between/amongst the different game screens.
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class MySceneManager : MonoBehaviour {

	public static MySceneManager instance; // Singleton design pattern
	public Button playAgain, back; // References to the Play Again and Back buttons during any "Game" scene.

	// Set up the singleton design pattern. Also, if we're in the "Fixed Game" or "Scroll Game" scenes, hide the buttons.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
		if (SceneManager.GetActiveScene().name == "Fixed Game" || SceneManager.GetActiveScene().name == "Scroll Game") { // If we're in a "Game" scene...
			// Disable the two buttons.
			playAgain.gameObject.SetActive(false);
			back.gameObject.SetActive(false);
		}
	}

	/** Loads a scene specified as a string.
	 * When the button is pressed, load a specific scene.
	 * 
	 * param[sceneName] - the name of the scene we wish to load.
	 * PRECONDITION: The scene must be present in Build Settings' "Scenes In Build."
	 */
	public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

	/** During the "Game" scene, load the UI elements for "Play Again" and "Back".
	 * 
	 */
	public void GameLoadUI() {
		playAgain.gameObject.SetActive(true);
		back.gameObject.SetActive(true);
	}

	/** Quit the game to desktop.
	 * When the "Exit" button is pressed, the Unity application will close.
	 */
	public void ExitGame() {
		Application.Quit();
	}
}
