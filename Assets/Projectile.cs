/** Jonathan So, jds7523@rit.edu
 * The projecile class, shared by both player and NPC. These are designed to be shot and reused in an object pool, not instantiated/destroyed.
 */
using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float speed = 0.25f; // Speed of the projectile.

	private int facing = 1; // +1 for right, -1 for left.
	private const float ACT_TIME = 3f; // Time that the projectile lasts.

	private SpriteRenderer sr; // Sprite Renderer; the component which draws this projectile to the screen.
	private Transform tm; // Shorthand reference for "transform" - I do this to reduce typing.

	// Grab a reference to the sprite renderer and the transform.
	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
		tm = GetComponent<Transform>();
	}

	/** Every time this bullet is shot from its object pool, make sure that 
	 * we "destroy" (Deactivate) it when its time ends.
	 */
	private void OnEnable() {
		Invoke("Destruct", ACT_TIME);
	}

	// Update this projectile's position.
	private void Update() {
		tm.Translate(transform.right * speed * facing);
	}

	// Sets this gameObject to be inactive.
	private void Destruct() {
		this.gameObject.SetActive(false);
	}
	/** Orients this projectile to face and move in the correct direction.
	 * 
	 * param[newDir] - an int, either -1 or +1, specifying the direction which to face.
	 */
	public void SetFacing(int newDir) {
		if (Mathf.Abs(newDir) != 1) { // Reject any illegitimate values of newDir. The newDir must be either +1 or -1.
			return;
		}
		facing = newDir; // Set the new facing direction.
		if ((facing == -1 && !sr.flipX) || (facing == 1 && sr.flipX)) { // If the player sprite is facing the wrong way...
			sr.flipX = !sr.flipX; // Reverse it to make it right.
		}
	}

	private void OnDisable() {
		CancelInvoke();
	}
}
