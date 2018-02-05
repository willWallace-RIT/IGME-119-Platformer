/** Jonathan So, jds7523@rit.edu
 * This is a manager class for the camera-based effects. Communicates with the "CameraFX" script.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {

	public bool playerHasScreenShake, playerHasScreenFlash, NPCHasScreenShake, NPCHasScreenFlash; // Booleans which describe whether or not to apply specific camera effects to different characters in the game.
	[Range(0f, 10f)]
	public float shakeTime; // The amount of time spent screen-shaking, in seconds.
	[Range(0, 2)]
	public float shakeMagnitude; // The magnitude of screen-shaking.
	[Range(0, 60)]
	public int flashFrames; // The number of frames that a screen flash will be active.
	public static ScreenManager instance; // Singleton Design Pattern.

	// Setup the Singleton design pattern.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	// Set the variables of the camera's feedback.
	private void Start() {
		CameraFX.instance.SetVariables(shakeTime, flashFrames, shakeMagnitude);
	}

	/** Plays any specified camera effects for the player.
	 * If playerHasScreenShake is toggled, then shake the screen; do the same for playerHasScreenFlash.
	 */
	public void PlayerFX() {
		if (playerHasScreenShake) {
			CameraFX.instance.CallShake();
		}
		if (playerHasScreenFlash) {
			CameraFX.instance.CallFlash();
		}
	}

	/** Plays any specified camera effects for the NPC.
	 * If NPCHasScreenShake is toggled, then shake the screen; do the same for NPCHasScreenFlash.
	 */
	public void NPCFX() {
		if (NPCHasScreenShake) {
			CameraFX.instance.CallShake();
		}
		if (NPCHasScreenFlash) {
			CameraFX.instance.CallFlash();
		}
	}

}
