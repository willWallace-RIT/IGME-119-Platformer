/** Jonathan So, jds7523@rit.edu
 * This script implements camera-based player feedback effects, such as screen-shake and screen-flashing.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFX : MonoBehaviour {

	public GameObject flash; // The gameObject to draw to the screen during a screen flash.
	public static CameraFX instance; // Singleton design pattern.

	private const float E = 2.7183f; // The mathematical constant, "e", used for the dampened sine wave movement of the ScreenShake() coroutine.

	private float shakeMagnitude; // The magnitude of screen-shaking.
	private float shakeTime; // The amount of time spent screen-shaking, in seconds.
	private int flashFrames; // Number of frames to flash upon a hit.

	// Set up the singleton design pattern.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	// Calls the ScreenShake Coroutine after stopping it (if it currently is in progress.)
	public void CallShake() {
		StopCoroutine("ScreenShake");
		StartCoroutine("ScreenShake");
	}

	// Calls the ScreenFlash Coroutine.
	public void CallFlash() {
		StartCoroutine("ScreenFlash");
	}

	/** Use a modified damped sine wave movement to make the screen shake for an amount of time.
	 */
	private IEnumerator ScreenShake() {
		float timer = 0;
		float xPos = shakeTime;
		while (timer < shakeTime) {
			xPos = shakeMagnitude * (Mathf.Pow(E, -timer) * Mathf.Cos(2 * Mathf.PI * timer));
			transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
			timer += Time.deltaTime * 4; // I believe that the effect looks better if we multiply the timer by 4.
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return new WaitForSeconds(Time.fixedDeltaTime);
		transform.position = new Vector3(0, 0, transform.position.z);
	}

	/** Make the screen flash for the specified amount of frames.
	 */
	private IEnumerator ScreenFlash() {
		flash.SetActive(true);
		yield return new WaitForSeconds(flashFrames / 60f);
		flash.SetActive(false);
	}

	/** Public setter which sets the three "public" variables of this.
	 * param[newShake] - the amount of time to shake the screen.
	 * param[newFlash] - the amount of frames to flash the screen.
	 * param[newMag] - the magnitude of the screen shake variable.
	 */
	public void SetVariables(float newShake, int newFlash, float newMag) {
		shakeTime = newShake;
		flashFrames = newFlash;
		shakeMagnitude = newMag;
	}
}
