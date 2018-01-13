/** Jonathan So, jds7523@rit.edu
 * This class will handle swapping between/amongst the different game screens.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class MySceneManager : MonoBehaviour {

	public static MySceneManager instance; // Singleton design pattern
	public Button playAgain, back; // References to the Play Again and Back buttons during any "Game" scene.

	private VideoPlayer vid; // Reference to the video player that'll only be populated and used in the "Cutscene" scene.

	// Set up the singleton design pattern.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	/** Checks to see if we're playing the cutscene.
	 * If we are in the cutscene scene, then automatically load the "Game" scene after the video is done playing.
	 */
	private void Start() {
		if (SceneManager.GetActiveScene().name.Equals("Cutscene")) {
			vid = GameObject.FindObjectOfType<VideoPlayer>();
			Invoke("LoadGame", (float) vid.clip.length + 1f);
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

	// Load the "Game" scene.
	public void LoadGame() {
		SceneManager.LoadScene("Game");
	}

	// During the "Game" scene, load the UI elements for "Play Again" and "Back".
	public void GameLoadUI() {
		// Activate the two UI buttons, "Play Again" and "Back".
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
