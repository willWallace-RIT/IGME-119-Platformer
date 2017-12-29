/** Jonathan So, jds7523@rit.edu
 * This handles parallax movement for an individual layer.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParallaxLayer : MonoBehaviour {

	public float SPEED; // Speed at which to scroll.

	private int dir; // Determines which way to scroll; -1 is left, +1 is right, and 0 means no movement.
	private const float X_TELEPORT = 8f - 0.125f; // X-position to teleport to during scrolling process.

	private SpriteRenderer sr; // Reference to the SpriteRenderer.
	private Transform tm; // Shorthand reference to the Transform, used for movement.

	// Grabs a reference to the Transform and sets this layer not to scroll yet.
	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
		tm = GetComponent<Transform>();
		dir = 0;
	}

	// Scrolls this parallax layer in the desired direction; if facing = 0, then the layer won't move.
	private void Update() {
		tm.Translate(Vector2.right * dir * SPEED);
		if (Mathf.Abs(tm.position.x) > (X_TELEPORT + (sr.bounds.size.x / 2))) { // We must be out of bounds.
			if (dir == -1) { // This layer was moving left, so place it at the right-hand side.
				tm.position = new Vector2(X_TELEPORT + (sr.bounds.size.x / 2), tm.position.y);
			} else { // This layer then must've been travelling to the right.
				tm.position = new Vector2(-1 * (X_TELEPORT + (sr.bounds.size.x / 2)), tm.position.y);
			}
		}
	}

	/** Set the new direction of this parallax layer.
	 * 
	 * param[newDir] - the new direction of this parallax layer. Must be either -1, 0, or +1.
	 */
	public void SetDirection(int newDir) {
		if (Mathf.Abs(newDir) != 1 && newDir != 0) { // Eliminate any invalid inputs.
			Debug.Log("ParallaxLayer.SetDirection() -- Invalid. Parameter newDir = " + newDir);
			return;
		}
		dir = newDir;
	}

}
