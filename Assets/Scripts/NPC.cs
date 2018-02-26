/** Jonathan So, jds7523@rit.edu
 * The customizable NPC will walk and interact with the player character.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour {

	public Projectile projectile; // The projectile prefab that we'll add to our clip (and eventually shoot).
	public GameObject interactBox; // If we don't shoot, then we use close-range interactions; this is the GameObject containing the hitbox (trigger) for it.

	private bool disappearOnContact; // If checked, then the NPC will disappear after their "contact" animation.
	private bool knockbackOnContact; // If checked, then the NPC will have some knockback applied during their "contact" animation.
	private float knockback; // If the above (knockbackOnContact) is checked, then this number expresses the amount of knockback for this character.
	private bool stopToInteract; // If checked, then the NPC will stop for a brief moment before performing their "interacting" behavior.
	private bool fallOffPlatforms; // If checked, then the NPC won't prevent itself from falling off platforms.
	private bool isShooter; // If checked, then the NPC will shoot projectiles; otherwise, the NPC will use close-range interaction.
	private AudioClip interactSFX, contactSFX; // Sound effects for interaction and contact with a player's interaction.
	public int facing = 1; // The direction the NPC faces, either +1 for right or -1 for left.

	private Transform level; // Reference to the transform of the level architecture.
	private List<Projectile> clip; // The object pool which contains our projectiles. 
	private const int CLIP_SIZE = 3; // Number of projectiles to instantiate for the object pool.

	private float speed = 0.0625f; // Speed of the NPC.
	private float timer = INTERACT_TIME; // Keeps track of the time in between subsequent interactions.
	private bool interacting = false; // Whether or not the NPC is in an interacting state.
	private bool inactive = false; // Whether or not the NPC should be performing any actions.

	private const float INTERACT_TIME = 3f; // Amount of time in between subsequent interactions.
	private const float WAIT_TIME = 3/4f; // Amount of time the NPC must wait before interacting.
	private const float INTERACT_DURATION = 2/3f; // Amount of time it takes to perform the full interaction.
	private const float CONTACT_TIME = 2f; // Amount of time required to play the full "contact" animation and also land on the ground.

	private Animator anim; // Component which toggles different animations for different situations.
	private AudioSource audi; // Component which allows us to play audio such as sound effects.
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
		audi = GetComponent<AudioSource>();
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
		level = GameObject.Find("Level Architecture").transform; // Grab a reference to the level's transform.
		timer += Random.Range(-1f, 1f); // Add a bit of randomization to the interaction timer...
		interactBox = transform.Find("NPC_Interact_Hitbox").gameObject;
		interactBox.SetActive(false);
	}

	/** In the infinite case where we reuse NPCs, reset all of their behaviors if they were interacted with prior.
	 * 
	 */
	private void OnEnable() {
		Reorient(facing);
		level = GameObject.Find("Level Architecture").transform;
		inactive = false;
		timer += Random.Range(-1f, 1f); // Add a bit of randomization to the interaction timer...
		gameObject.tag = "NPC";
		this.gameObject.layer = 13; // Set this gameObject's layer to "NPC" (13) so we may interact with it.
		anim.ResetTrigger("contact");
		anim.ResetTrigger("interact");
		anim.SetBool("isActive", true);
		Invoke("LateOnEnable", Time.deltaTime);
	}

	// One frame after OnEnable, set up the sound effects for this object. This bypasses a glitch where sound won't play.
	private void LateOnEnable() {
		if (!interactSFX || !contactSFX) { // Ensure that the sound effects aren't null.
			NPC_Manager.instance.SetVariables(this);
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
				curr.transform.SetParent(level); // Set this bullet as a child of the level architecture. If we don't do this, then...
				// the bullets can be sped up or slowed down due to the way we handle scrolling.
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
			timer += Random.Range(-1f, 1f); // Add a bit of randomization to the interaction timer...
		}
		// Toggle behaviors: Walking or Interacting.
		if (!interacting) { // Walking.
			tm.Translate(Vector2.right * facing * speed  * Time.deltaTime * 60);
		} else { // Interacting.
			// Don't do anything.
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
		audi.PlayOneShot(interactSFX); // Play the interaction sound effect.
		if (isShooter) {
			ShootFromPool();
		} else {
			interactBox.SetActive(true); // Set active the hitbox.
			yield return new WaitForSeconds(INTERACT_DURATION); // Let the hitbox remain active for a bit of time.
			interactBox.SetActive(false); // Deactivate the hitbox.
		}
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

	/** Handle the first frame on which this interacts with a bound/NPC or a player's interaction hitbox.
	 * param[coll] - the 2D collision which this object is touching.
	 * PRECONDITION: The GameObject is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag.Equals("Bound") || coll.gameObject.tag.Equals("NPC")) { // If we've hit a bound or a fellow NPC...
			Reorient(facing * -1); // Face the other way.
		}
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			coll.gameObject.SetActive(false);
			StartCoroutine("Contact"); // React to it.
		}
	}

	/** Handle the first frame on which this interacts with the player's interaction hitbox or platform edge.
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			coll.gameObject.SetActive(false);
			StartCoroutine("Contact"); // React to it.
		}
		if (coll.gameObject.tag.Equals("Platform_Edge") && !fallOffPlatforms) { // If we don't want to fall off platforms...
			Reorient(-facing); // Reverse our direction of movement (turn around).
		}
	}

	/** Handle every frame where this touches a player's interaction hitbox.
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnTriggerStay2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Player_Interact")) { // If we've hit a player's interaction hitbox (such as a sword, bullet, handshake...)
			coll.gameObject.SetActive(false);
			StartCoroutine("Contact"); // React to it.
		}
	}

	/** Upon contact with the player's interaction (projectile or extended hitbox), react to it.
	 * Make this NPC uninteractible, apply knockback if desired, and add to score.
	 */
	private IEnumerator Contact() {
		ScreenManager.instance.NPCFX(); // Play any specified camera effects for the NPC.
		this.gameObject.layer = 14; // Set this gameObject's layer to "NPC Contact" (layer 14) so that it no longer comes in contact with anything else besides the floor.
		audi.PlayOneShot(contactSFX); // Play the contact sound effect.
		inactive = true;
		StopCoroutine("Interact");
		anim.SetTrigger("contact");
		anim.SetBool("isActive", false);
		if (knockbackOnContact) { // If we want knockback...
			if (!Player.instance.GetIsShooter()) { // If the player has just used melee-style interaction on us...
				if (Player.instance.transform.position.x < this.tm.position.x) { // If the player is to the left of us...
					rb.velocity = new Vector2(knockback, knockback); // Apply physics-based knockback to move right.
				} else {
					rb.velocity = new Vector2(-knockback, knockback); // Apply physics-based knockback to move left.
				} 
			} else { // The player has used projectile-style interaction.
				rb.velocity = new Vector2(-facing * knockback, knockback); // Apply physics-based knockback to move backwards.
			}
		}
		ScoreManager.instance.AddToScore(100); // Interacting with NPCs yields the player 100 points.
		yield return new WaitForSeconds(CONTACT_TIME); 
		if (disappearOnContact) { // If we choose for NPCs to disappear...
			this.gameObject.SetActive(false); // Disappear.
		} else { // NPCs will stay on-screen after contact.
			gameObject.tag = "Untagged"; // Set the nametag to something that can't trigger anything.
		}
	}



	///////////////
	/// SETTERS ///
	///////////////


	/** Set the value for disappearOnContact.
	 * param[newVal] - the new value for disappearOnContact.
	 */
	public void SetDisappearOnContact(bool newVal) {
		disappearOnContact = newVal;
	}

	/** Set the value for knockbackOnContact.
	 * param[newVal] - the new value for this boolean.
	 */
	public void SetKnockbackOnContact(bool newVal) {
		knockbackOnContact = newVal;
	}

	/** Set the value for stopToInteract.
	 * param[newVal] - the new value for this boolean.
	 */
	public void SetStopToInteract(bool newVal) {
		stopToInteract = newVal;
	}

	/** Set the value for fallOffPlatforms.
	 * param[newVal] - the new value for this boolean.
	 */
	public void SetFallOffPlatforms(bool newVal) {
		fallOffPlatforms = newVal;
	}

	/** Set the value for knockback.
	 * param[newVal] - the new value for this integer.
	 */
	public void SetKnockback(float newVal) {
		knockback = newVal;
	}

	/** Set the two sound effects, interact and contact.
	 * param[intSFX] - sound effect to play upon interaction (shooting).
	 * param[contSFX] - sound effect to play upon contact with a player's interaction.
	 */
	public void SetSFX(AudioClip intSFX, AudioClip contSFX) {
		interactSFX = intSFX;
		contactSFX = contSFX;
	}

	/** Set the value for isShooter
	 * param[newVal] - the new value for this boolean.
	 */
	public void SetIsShooter(bool newVal) {
		isShooter = newVal;
	}
}
