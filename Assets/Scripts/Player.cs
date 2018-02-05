/** Jonathan So, jds7523@rit.edu
 * The player may walk, jump, and interact with non-player characters.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public static Player instance; // There should only be one player at any given time, so this is the singleton design pattern.
	public Projectile projectile; // The projectile prefab that we'll add to our clip (and eventually shoot).
	public GameObject interactBox; // If we don't shoot, then we use close-range interactions; this is the GameObject containing the hitbox (trigger) for it.

	private bool disappearOnContact; // If checked, then the player will disappear after their "contact" animation.
	private bool knockbackOnContact; // If checked, then the player will have some knockback applied during their "contact" animation.
	private bool isShooter; // If true, then this character will shoot projectiles; if false, then this character will use close-range interaction (such as a sword slash, a handshake, etc.)

	private KeyCode up, left, right, interact; // Buttons (Keys) to press for jumping, moving left and right, and interaction, respectively.

	public bool inactive = false; // Whether or not the player should be doing anything.

	public AudioClip jumpSFX, interactSFX, collectSFX, contactSFX; // Sound effects for jumping, interaction, collecting objects, and contact.

	private const float GROUND_SPD = 0.125f; // Speed of the player while on the ground.
	private const float AIR_SPD = 0.0625f; // Speed of the player while in the air.
	private const float JUMP_VEL = 8f; // Y-Velocity of the player upon jumping.
	private const float KNOCK_VEL = 4f; // Knockback velocity upon contact with NPC projectile.
	private const float GCHECK_Y = -0.5f; // Y-position of the ground checker object in relation to the player; it'll be 0.5 units below the player at all times.
	private const int CLIP_SIZE = 20; // Number of projectiles to instantiate for the object pool.
	private const float CONTACT_TIME = 2f; // Amount of time required to play the full "contact" animation and also land on the ground.
	private const float INTERACT_TIME = 3/16f; // Amount of time that the close-range interaction hitbox lasts.
	private const float ADJUST_FORCE = 2f; // Force to add to the Player's Rigidbody2D when the player is about to go off-camera in a scrolling level.

	private List<Projectile> clip; // The object pool which contains our projectiles. 
	private float speed = AIR_SPD; // Speed of the player, which can either be GROUND_SPD or AIR_SPD.
	private int facing = 1; // The direction that the player faces, either +1 for right or -1 for left.
	private bool levelScrolls; // A bool indicating whether or not the level will scroll. We set this by grabbing the "levelWillScroll" boolean from the PlatformSpawner.
	private bool isTouchingBound = false; // Boolean indicating if we're touching one of the boundary objects.
	private bool adjusting = false; // Whether or not this player is adjusting the camera so that the player stays centered.

	private Animator anim; // Component which toggles different animations for different situations.
	private AudioSource audi; // Component which allows for this object to play audio such as sound effects.
	private GroundCheck groundCheck; // Component which detects the ground beneath the player.
	private Rigidbody2D rb; // Access to the Rigidbody, required for jumping.
	private SpriteRenderer sr; // Component which draws the sprite to the screen.
	private Transform tm; // Shorthand reference for "transform" - I do this to reduce typing.

	/** Grab references to all of the components we'll be contacting, such as animator and transform.
	 * This function is called before the game starts; without doing this, we'll get a lot of errors and
	 * won't be able to move, animate, or get our player working.
	 */
	private void Awake() {
		if (instance == null) { // Set up the Singleton Design pattern.
			instance = this;
		}
		anim = GetComponent<Animator>();
		audi = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		tm = GetComponent<Transform>();
		groundCheck = Transform.FindObjectOfType<GroundCheck>();
		groundCheck.anim = this.anim;

		interactBox.SetActive(false); // Deactivate the interaction hitbox.
	}

	/** Grab the value of "levelWillScroll" and put it into our value for "levelScrolls."
	 * This is called after Awake (and after Start) becacuse in Awake of the PlatformSpawner, we set up the singleton, so after Awake 
	 * and in Start, we ensure that the singleton object is ready to go.
	 */
	private void Start() {
		Invoke("LateStart", Time.deltaTime);
	}
		
	// Determine whether or not the level scrolls and set up the object pool.
	private void LateStart() {
		levelScrolls = PlatformSpawner.instance.GetLevelWillScroll();
		if (isShooter) { // If the player shoots projectiles...
			// Set up the object pool.
			clip = new List<Projectile>();
			Projectile obj; 
			for (int i = 0; i < CLIP_SIZE; i++) {
				obj = (Projectile) Instantiate(projectile, tm.position, Quaternion.identity); // Make a new projectile at our position.
				obj.gameObject.SetActive(false); // Deactivate the projectile.
				clip.Add(obj); // Add the projectile to our clip.
			}
		}
	}

	/** Called every frame, this checks for user input so that the player can move and perform actions.
	 * 
	 */
	private void Update() {
		if (inactive) { // If inactive, then don't do anything.
			return;
		}
		if (Input.GetKey(up) && groundCheck.onGround) { // If the player is in contact with the ground, and hits the 'up' button, then...
			// Jump up.
			rb.velocity = Vector2.up * JUMP_VEL;
			if (!audi.isPlaying) { // Play the respective sound effect for jumping.
				audi.PlayOneShot(jumpSFX);
			}
		}
		// Left and Right Movement
		if (Input.GetKey(left)) { // Upon pressing the left button, move left.
			Reorient(-1);
			Move(Vector2.right * facing * speed);
		} else if (Input.GetKey(right)) { // Upon pressing the right button, move right.
			Reorient(1);
			Move(Vector2.right * facing * speed);
		} else {
			ParallaxManager.instance.SetDirection(0);
			anim.SetBool("isWalking", false);
			StartCoroutine("Adjust"); // Adjust the camera if the player has stopped moving.
		}

		// Interact with NPCs.
		if (Input.GetKeyDown(interact)) {
			if (isShooter) { // Shoot a projectile, or...
				ShootFromPool();
			} else { // Create an interaction hitbox.
				StartCoroutine("InteractBox");
			}
		}

		// Update the GroundChecker's position to be directly beneath the player.
		groundCheck.transform.position = new Vector2(tm.position.x, tm.position.y + GCHECK_Y);
		// Check to see if the player has fallen off-screen.
		if (tm.position.y <= -6.5f && !inactive) {
			StartCoroutine("Contact"); // If the player has fallen, then make the player inactive through the coroutine.
		}
	}

	/** (Scrolling, non-infinite level only) If the player is off-center, then realign the camera so that the player is centered.
	 * Wait a second, then move the level and player in a way that centers the player to the center of the scene.
	 */
	private IEnumerator Adjust() {
		if (adjusting || !levelScrolls) {
			yield break;
		}
		adjusting = true;
		yield return new WaitForSeconds(1f);
		// Adjustment - if the player is going off-screen, keep their X-position around 0.
		// Make sure the player is centered on-screen.
		while (Mathf.Abs(tm.position.x) > 1f) {
			if (tm.position.x > 0) { // Player is to the right
				PlatformSpawner.instance.MoveLevel(Vector2.right * -speed * 1/2f); // Move the level.
				tm.Translate(Vector2.right * -speed);
			} else { // Player is to the left
				PlatformSpawner.instance.MoveLevel(Vector2.right * speed * 1/2f); // Move the level.
				tm.Translate(Vector2.right * speed);
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		adjusting = false;
	}

	/** Handles different modes of movement for the wide variety of games that can be created with this project package.
	 * If the level scrolls, then move the level architecture instead of the player themselves.
	 * Otherwise, the player may move in a fixed, single-screen environment.
	 * 
	 * param[moveVec] - The vector2 that the player is moving by. Set in Update(), above.
	 */
	private void Move(Vector2 moveVec) {
		StopCoroutine("Adjust");
		adjusting = false;
		if (levelScrolls && !isTouchingBound) { // If the level scrolls...
			// Don't actually move the player; instead, move the level around them. 
			// I do this to maintain the parallax illusion, since it breaks when the camera moves around too much.
			if (Mathf.Abs(tm.position.x) < 4.5f) { // If the player is within screen bounds...
				PlatformSpawner.instance.MoveLevel(-moveVec); // Move the level.
			} // Otherwise, wait for the Adjust() function above to reorient the camera.
		} else { // With single-screen levels, the player may move conventionally.
			tm.Translate(moveVec * Time.deltaTime * 60);
		}
	}

	/** If the Player uses melee-style (close-range) interaction, then this triggers the interaction animation and hitbox.
	 * Plays the "Interact" animation of the player and sets the interaction hitbox active for a brief amount of time before
	 * disabling it.
	 */
	private IEnumerator InteractBox() {
		anim.SetTrigger("interact"); // Trigger the animation of interaction.
		interactBox.SetActive(true); // Set active the hitbox.
		audi.PlayOneShot(interactSFX); // Play the sound effect for interaction.
		yield return new WaitForSeconds(INTERACT_TIME); // Let the hitbox remain active for a bit of time.
		interactBox.SetActive(false); // Deactivate the hitbox.
	}

	/** Reorients both the player movement and the sprite orientation to a new direction.
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
		interactBox.transform.position = new Vector2(tm.position.x + (0.75f * facing), tm.position.y); // Ensure that the interaction hitbox (for melee-style interaction) is in the right spot.
		if (Mathf.Abs(tm.position.x) < PlatformSpawner.instance.X_SCROLL_BOUNDS) { // If we are in bounds...
			ParallaxManager.instance.SetDirection(-1 * newDir); // Ensure that the parallax layers scroll OPPOSITE of our direction to give the desired effect.
		} else { // We are out of bounds.
			ParallaxManager.instance.SetDirection(0); // Do not move the parallax layers.
		}
		anim.SetBool("isWalking", true); // Play the "walking" animation.
	}

	/** Function called by GroundCheck to update the speed of the player.
	 * If the player is on the ground, set their current speed to the ground speed, and
	 * if in air, set the speed to air speed.
	 * 
	 * param[onGround] - a bool stating whether or not the player is on the ground.
	 */
	public void SetSpeed(bool onGround) {
		if (onGround) {
			speed = GROUND_SPD;
		} else {
			speed = AIR_SPD;
		}
	}

	/** Fire one pooled object from our object pool.
	 * Finds the first inactive object in our clip and activates it.
	 */
	private void ShootFromPool() {
		anim.SetTrigger("interact");
		Projectile curr; // The current object to check and potentially fire.
		for (int i = 0; i < CLIP_SIZE; i++) {
			curr = clip[i]; 
			if (!curr.gameObject.activeInHierarchy) { // Find the first projectile that's inactive.
				curr.gameObject.SetActive(true); // Activate it.
				curr.transform.position = tm.position; // Set its position to ours.
				curr.SetFacing(facing); // Make it shoot in the direction we face.
				audi.PlayOneShot(interactSFX); // Play the interaction sound effect.
				break; // Break out of this loop.
			}
		}
	}

	/** Upon contact with the NPC interaction (projectile or extended hitbox), react to it.
	 * Play the "contact" animation.
	 */
	private IEnumerator Contact() {
		ScreenManager.instance.PlayerFX(); // Play any specified camera effects for the Player.
		audi.PlayOneShot(contactSFX);
		this.gameObject.layer = 14; // Set this gameObject's layer to "NPC Contact" (layer 14) so that it no longer comes in contact with anything else besides the floor.
		// Stop the parallax scrolling. 
		ParallaxManager.instance.SetDirection(0);
		// Undo all of the animations in order to play the "contact" one.
		anim.SetBool("onGround", false);
		anim.SetBool("isWalking", false);
		inactive = true;
		StopCoroutine("Interact");
		anim.SetTrigger("contact");
		anim.SetBool("KOd", true);
		if (knockbackOnContact) { // If we want knockback...
			rb.velocity = new Vector2(-facing * KNOCK_VEL, KNOCK_VEL); // Apply physics-based knockback.
		}
		yield return new WaitForSeconds(CONTACT_TIME); 
		if (disappearOnContact) { // If we choose for Player to disappear...
			Destroy(this.gameObject); // Disappear.
		} else { // Player will stay on-screen after contact.
			gameObject.tag = "Untagged"; // Set the nametag to something that can't trigger anything.
		}
		// Load up all of the UI elements.
		MySceneManager.instance.GameLoadUI();
		// The user won't see the player anyway, so its gameobject should be safe to destroy. This fixes a bug where the level architecture continues to scroll even after Contact.
		if (tm.position.y <= 6.5f) {
			this.gameObject.SetActive(false);
		}
	}

	/** Called automatically by Unity on the first frame that this object interacts with an object containing a trigger collider.
	 * If this object hits a coin, then collect it (make it disappear).
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, Player) is required to have a Rigidbody2D in order to use this function.
	 * PRECONDITION: The Collider2D must be a trigger.
	 */
	private void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Coin")) { // If we touch a coin...
			ScoreManager.instance.AddToScore(50); // Collecting a coin yields the player 50 points.
			audi.PlayOneShot(collectSFX);
			Destroy(coll.gameObject); // "Collect it"; make it disappear.
		} else if (coll.gameObject.tag.Equals("NPC")) { // If we touch an NPC projectile...
			StartCoroutine("Contact"); // React to it.
		}
	}

	/** Called automatically by Unity on every frame that this object interacts with an object containing a collider.
	 * If the player hits a level bound, then prevent them from moving towards it.
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, Player) is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnCollisionStay2D (Collision2D coll) {
		if (coll.gameObject.tag.Equals("Bound")) { // If we touch a bound...
			isTouchingBound = true;
		}
	}

	/** Called automatically by Unity on the final frame that this object interacts with an object containing a collider.
	 * If the player exits a level bound, then they can once again move.
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, Player) is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnCollisionExit2D (Collision2D coll) {
		if (coll.gameObject.tag.Equals("Bound")) { // When we stop colliding with a bound...
			isTouchingBound = false;
		}
	}


	///////////////
	/// GETTERS ///
	///////////////


	/** Getter for the field "isShooter".
	 * return - the value of isShooter, a boolean.
	 */
	public bool GetIsShooter() {
		return isShooter;
	}

	///////////////
	/// SETTERS ///
	///////////////

	/** Setter for the field "disappearOnContact".
	 * param[newVar] - the new value of the boolean we're setting.
	 */
	public void SetDisappearOnContact(bool newVar) {
		disappearOnContact = newVar;
	}

	/** Setter for the field "knockbackOnContact".
	 * param[newVar] - the new value of the boolean we're setting.
	 */
	public void SetKnockbackOnContact(bool newVar) {
		knockbackOnContact = newVar;
	}

	/** Setter for the field "isShooter".
	 * param[newVar] - the new value of the boolean we're setting.
	 */
	public void SetIsShooter(bool newVar) {
		isShooter = newVar;
	}

	/** Setter for all of the controls.
	 * param[newUp] - the new key to press for up (jumping).
	 * param[newLeft] - the new key to press for walking left.
	 * param[newRight] - the new key to press for walking right.
	 * param[newInteract] - the new key to press for interaction.
	 */
	public void SetControls(KeyCode newUp, KeyCode newLeft, KeyCode newRight, KeyCode newInteract) {
		up = newUp;
		left = newLeft;
		right = newRight;
		interact = newInteract;
	}
}
