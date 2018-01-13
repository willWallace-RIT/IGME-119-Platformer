/** Jonathan So, jds7523@rit.edu
 * Camera follows the player's X position if the screen is meant to scroll.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private bool scroll; // If true, then follow the player's X-position and move the camera.
	private float MAX_X; // Maximum X-position; the negative of this value is the minimum X-position.

	private Transform tm; // Shorthand reference to the Transform.

	// Grab a reference to the Transform so that we may reference it by "tm" instead of typing out "transform".
	private void Awake() {
		tm = GetComponent<Transform>();
	}

	// Find out whether or not we're going to be scrolling by contacting the PlatformSpawner singleton.
	private void Start() {
		scroll = PlatformSpawner.instance.GetLevelWillScroll();
		MAX_X = PlatformSpawner.instance.X_SCROLL_BOUNDS;
	}

	// Set our position to the player's X-position.
	private void Update() {
		if (scroll) { 
			if (Mathf.Abs(Player.instance.transform.position.x) < MAX_X) {
				tm.position = new Vector3(Player.instance.transform.position.x, tm.position.y, tm.position.z);
			}
		}
	}
}
