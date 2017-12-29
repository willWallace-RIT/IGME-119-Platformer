/** Jonathan So, jds7523@rit.edu
 * The customizable NPC will walk and interact with the player character.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour {

	public Projectile projectile; // The projectile prefab that we'll add to our clip (and eventually shoot).

	public bool disappearOnContact = true; // If checked, then the NPC will disappear after their "contact" animation.
	public bool knockbackOnContact = true; // If checked, then the NPC will have some knockback applied during their "contact" animation.
	public bool stopToInteract = true; // If checked, then the NPC will stop for a brief moment before performing their "interacting" behavior.
	public int facing = 1; // The direction the NPC faces, either +1 for right or -1 for left.

	private List<Projectile> clip; // The object pool which contains our projectiles. 
	private const int CLIP_SIZE = 4; // Number of projectiles to instantiate for the object pool.

	private float speed = 0.0625f; // Speed of the NPC.
	private float timer = INTERACT_TIME; // Keeps track of the time in between subsequent interactions.
	private bool interacting = false; // Whether or not the NPC is in an interacting state.
	private bool inactive = false; // Whether or not the NPC should be performing any actions.

	private const float INTERACT_TIME = 3f; // Amount of time in between subsequent interactions.
	private const float WAIT_TIME = 3/4f; // Amount of time the NPC must wait before interacting.
	private const float INTERACT_DURATION = 2/3f; // Amount of time it takes to perform the full interaction.
	private const float CONTACT_TIME = 2f; // Amount of time required to play the full "contact" animation and also land on the ground.
	private const float KNOCK_VEL = 4f; // Knockback velocity applied to the rigidbody.

	private Animator anim; // Component which toggles different animations for different situations.
	private Collider2D coll; // Collider2D component allows this NPC to have contact with things such as the ground, players, projectiles, etc.
	private Rigidbody2D rb; // Access to the Rigidbody, required for Unity's built-in collision functions.
	private SpriteRenderer sr; // Component which draws the sprite to the screen.
	private Transform tm; // Shorthand reference for "transform" - I do this to reduce typing.

	/** Grab references to all of the components we'll be contacting, such as animator and transform.
	 * This function is called before the game starts; without doing this, we'll get a lot of errors and
	 * won't be able to move, animate, or get the NPC to do much.
	 * Also, create an object pool of projectiles so that we may shoot them.
	 */
	private void Awake() {
		anim = GetComponent<Animator>();
		coll = GetComponent<Collider2D>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		tm = GetComponent<Transform>();
		Reorient(facing);
		// Set up the object pool.
		clip = new List<Projectile>();
		Projectile obj; 
		for (int i = 0; i < CLIP_SIZE; i++) {
			obj = (Projectile) Instantiate(projectile, tm.position, Quaternion.identity); // Make a new projectile at our position.
			obj.gameObject.SetActive(false); // Deactivate the projectile.
			clip.Add(obj); // Add the projectile to our clip.
		}
	}

	/** Fire one pooled object from our object pool.
	 * Finds the first inactive object in our clip and activates it.
	 */
	private void ShootFromPool() {
		Projectile curr; // The current object to check and potentially fire.
		for (int i = 0; i < CLIP_SIZE; i++) {
			curr = clip[i]; 
			if (!curr.gameObject.activeInHierarchy) { // Find the first projectile that's inactive.
				curr.gameObject.SetActive(true); // Activate it.
				curr.transform.position = tm.position; // Set its position to ours.
				curr.SetFacing(facing); // Make it shoot in the direction we face.
				break; // Break out of this loop.
			}
		}
	}

	// Called every frame by Unity, this moves the NPC and exhibits its behaviors.
	private void Update() {
		if (inactive) {
			return;
		}
		if (timer <= 0) { // Check the timer; if below zero, then it's time to interact.
			StartCoroutine("Interact");
			timer = INTERACT_TIME; // Reset the timer.
		}
		// Toggle behaviors: Walking or Interacting.
		if (!interacting) { // Walking.
			tm.Translate(Vector2.right * facing * speed);
		} else { // Interacting.

		}
		timer -= Time.deltaTime; // Keep track of time.
	}

	/** The NPC will pause to interact (if that variable is checked) before performing their 
	 * "interacting" behavior.
	 */
	private IEnumerator Interact() {
		interacting = true;
		anim.SetTrigger("interact");
		if (stopToInteract) { // Wait for a moment if stopToInteract is true.
			yield return new WaitForSeconds(WAIT_TIME);
		} else { // Wait for one frame, not a significant amount of time.
			yield return new WaitForSeconds(Time.deltaTime); 
		}
		// Perform the interaction.
		ShootFromPool();
		yield return new WaitForSeconds(INTERACT_DURATION); 
		anim.ResetTrigger("interact");
		interacting = false;
	}

	/** Reorients both the NPC movement and the sprite orientation to a new direction.
	 * 
	 * param[newDir] - an int, -1 or +1, which will become the new facing direction.
	 */
	private void Reorient(int newDir) {
		if (Mathf.Abs(newDir) != 1) { // Reject any illegitimate values of newDir. The newDir must be either +1 or -1.
			return;
		}
		facing = newDir; // Set the new facing direction.
		if ((facing == -1 && !sr.flipX) || (facing == 1 && sr.flipX)) { // If the player sprite is facing the wrong way...
			sr.flipX = !sr.flipX; // Reverse it to make it right.
		}
	}

	private void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag.Equals("Bound") || coll.gameObject.tag.Equals("NPC")) { // If we've hit a bound or a fellow NPC...
			Reorient(facing * -1); // Face the other way.
		}
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			StartCoroutine("Contact"); // React to it.
		}
	}

	private void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			StartCoroutine("Contact"); // React to it.
		}
	}

	private void OnTriggerStay2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			StartCoroutine("Contact"); // React to it.
		}
	}

	/** Upon contact with the player's interaction (projectile or extended hitbox), react to it.
	 * 
	 */
	private IEnumerator Contact() {
		this.gameObject.layer = 14; // Set this gameObject's layer to "NPC Contact" (layer 14) so that it no longer comes in contact with anything else besides the floor.
		inactive = true;
		StopCoroutine("Interact");
		anim.SetTrigger("contact");
		if (knockbackOnContact) { // If we want knockback...
			if (!Player.instance.isShooter) { // If the player has just used melee-style interaction on us...
				if (Player.instance.transform.position.x < this.tm.position.x) { // If the player is to the left of us...
					rb.velocity = new Vector2(KNOCK_VEL, KNOCK_VEL); // Apply physics-based knockback to move right.
				} else {
					rb.velocity = new Vector2(-KNOCK_VEL, KNOCK_VEL); // Apply physics-based knockback to move left.
				} 
			} else { // The player has used projectile-style interaction.
				rb.velocity = new Vector2(-facing * KNOCK_VEL, KNOCK_VEL); // Apply physics-based knockback to move backwards.
			}
		}
		yield return new WaitForSeconds(CONTACT_TIME); 
		if (disappearOnContact) { // If we choose for NPCs to disappear...
			Destroy(this.gameObject); // Disappear.
		} else { // NPCs will stay on-screen after contact.
			gameObject.tag = "Untagged"; // Set the nametag to something that can't trigger anything.
			coll.isTrigger = true;
			rb.isKinematic = true;
		}
	}
}
