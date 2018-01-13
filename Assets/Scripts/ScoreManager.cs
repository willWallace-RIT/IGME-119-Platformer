/** Jonathan So, jds7523@rit.edu
 * This handles the scoring system as well as display of the score.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public static ScoreManager instance; // Singleton design pattern - there can only be one score manager at any given time.

	private Text scoreDisplay; // The Canvas-Text component which displays the score as a text string.
	private int score = 0; // The score of the player.

	// Set up the singleton and grab a reference to the UI scoreDisplay object.
	private void Awake() {
		if (instance == null) { // Singleton set up.
			instance = this;
		}
		scoreDisplay = GetComponent<Text>();
		UpdateDisplay();
	}

	/** Add a specified amount of points to the score, then update the score display.
	 * param[points] - the number of points to add to the player's score.
	 */
	public void AddToScore(int points) {
		score += points;
		UpdateDisplay();
	}

	// Update the scoreDisplay string to accurately reflect the player's score.
	private void UpdateDisplay() {
		scoreDisplay.text = "SCORE " + score;
	}
}
