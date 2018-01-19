/** Jonathan So, jds7523@rit.edu
 * This handles the speed of the animations in the Animation Test scene.
 */
using UnityEngine;
using UnityEngine.UI; // Include this to manipulate UI elements.
using System.Collections;
using System.Collections.Generic;

public class SpeedController : MonoBehaviour {

	public Slider speedSlider; // The slider which controls speed.
	public Text speedDisplay; // The text element which shows the speed of the scene.

	/** Called by the speedSlider, this changes the value of the Time Scale based on
	 * the slider's current value. It also displays to the user the current value of the
	 * speedSlider.
	 */
	public void ChangeSpeed() {
		Time.timeScale = speedSlider.value;
		speedDisplay.text = speedSlider.value.ToString();
	}
}
