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
	public bool disappearOnContact = true; // If checked, then the player will disappear after their "contact" animation.
	public bool knockbackOnContact = true; // If checked, then the player will have some knockback applied during their "contact" animation.

	public bool isShooter = false; // If true, then this character will shoot projectiles; if false, then this character will use close-range interaction (such as a sword slash, a handshake, etc.)

	public KeyCode up, left, right, interact; // Buttons (Keys) to press for jumping, moving left and right, and interaction, respectively.

	public bool inactive = false; // Whether or not the player should be doing anything.

	private const float GROUND_SPD = 0.125f; // Speed of the player while on the ground.
	private const float AIR_SPD = 0.0625f; // Speed of the player while in the air.
	private const float JUMP_VEL = 8f; // Y-Velocity of the player upon jumping.
	private const float KNOCK_VEL = 4f; // Knockback velocity upon contact with NPC projectile.
	private const float GCHECK_Y = -0.5f; // Y-position of the ground checker object in relation to the player; it'll be 0.5 units below the player at all times.
	private const int CLIP_SIZE = 15; // Number of projectiles to instantiate for the object pool.
	private const float CONTACT_TIME = 2f; // Amount of time required to play the full "contact" animation and also land on the ground.
	private const float INTERACT_TIME = 3/16f; // Amount of time that the close-range interaction hitbox lasts.

	private List<Projectile> clip; // The object pool which contains our projectiles. 
	private float speed = AIR_SPD; // Speed of the player, which can either be GROUND_SPD or AIR_SPD.
	private int facing = 1; // The direction that the player faces, either +1 for right or -1 for left.

	private Animator anim; // Component which toggles different animations for different situations.
	private Collider2D coll; // Collider2D component allows this player to have contact with things such as the ground, NPCs, projectiles, etc.
	private GroundCheck groundCheck; // Component which detects the ground beneath the player.
	private Rigidbody2D rb; // Access to the Rigidbody, required for jumping.
	private SpriteRenderer sr; // Component which draws the sprite to the screen.
	private Transform tm; // Shorthand reference for "transform" - I do this to reduce typing.

	/** Grab references to all of the components we'll be contacting, such as animator and transform.
	 * This function is called before the game starts; without doing this, we'll get a lot of errors and
	 * won't be able to move, animate, or get our player working.
	 * Also, set up the object pool if the player shoots projectiles.
	 */
	private void Awake() {
		if (instance == null) { // Set up the Singleton Design pattern.
			instance = this;
		}
		anim = GetComponent<Animator>();
		coll = GetComponent<Collider2D>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		tm = GetComponent<Transform>();
		groundCheck = Transform.FindObjectOfType<GroundCheck>();
		groundCheck.anim = this.anim;

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
		interactBox.SetActive(false);
	}

	/** Called every frame, this checks for user input so that the player can move and perform actions.
	 * 
	 */
	private void Update() {
		if (inactive) { // If inactive, then don't do anything.
			return;
		}
		if (Input.GetKey(up) && groundCheck.onGround) { // If the player is in contact with the ground, then...
			// Jump up.
			rb.velocity = Vector2.up * JUMP_VEL;
		}
		// Left and Right Movement
		if (Input.GetKey(left)) { // Upon pressing the left button, move left.
			Reorient(-1);
			tm.Translate(Vector2.right * facing * speed);
		} else if (Input.GetKey(right)) { // Upon pressing the right button, move right.
			Reorient(1);
			tm.Translate(Vector2.right * facing * speed);
		} else {
			ParallaxManager.instance.SetDirection(0);
			anim.SetBool("isWalking", false);
		}
		// Interact with NPCs.
		if (Input.GetKeyDown(interact)) {
			if (isShooter) { // Shoot a projectile, or...
				ShootFromPool();
			} else { // Create a hitbox.
				StartCoroutine("InteractBox");
			}
		}
		// Update the GroundChecker's position to be directly beneath the player.
		groundCheck.transform.position = new Vector2(tm.position.x, tm.position.y + GCHECK_Y);
	}
		
	private IEnumerator InteractBox() {
		anim.SetTrigger("interact");
		interactBox.SetActive(true);
		yield return new WaitForSeconds(INTERACT_TIME);
		interactBox.SetActive(false);
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
		interactBox.transform.position = new Vector2(tm.position.x + (0.75f * facing), tm.position.y);
		if (Mathf.Abs(tm.position.x) < PlatformSpawner.instance.X_SCROLL_BOUNDS) {
			ParallaxManager.instance.SetDirection(-1 * newDir); // Ensure that the parallax layers scroll OPPOSITE of our direction.
		} else {
			ParallaxManager.instance.SetDirection(0); 
		}
		anim.SetBool("isWalking", true);
	}

	/** Called automatically by Unity on the first frame that this object interacts with an object containing a trigger collider.
	 * If this object hits a coin, then collect it (make it disappear).
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, GroundCheck) is required to have a Rigidbody2D in order to use this function.
	 * PRECONDITION: The Collider2D must be a trigger.
	 */
	private void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Coin")) { // If we touch a coin...
			Destroy(coll.gameObject); // "Collect it", make it disappear.
		} else if (coll.gameObject.tag.Equals("NPC")) { // If we touch an NPC projectile...
			StartCoroutine("Contact"); // React to it.
		}
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
				break; // Break out of this loop.
			}
		}
	}

	/** Upon contact with the NPC interaction (projectile or extended hitbox), react to it.
	 * Play the "contact" animation.
	 */
	private IEnumerator Contact() {
		Debug.Log("Contact");
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
			rb.velocity = new Vector2(-facing * KNOCK_VEL, KNOCK_VEL); // Apply physics-based knockback
		}
		yield return new WaitForSeconds(CONTACT_TIME); 
		if (disappearOnContact) { // If we choose for NPCs to disappear...
			Destroy(this.gameObject); // Disappear.
		} else { // NPCs will stay on-screen after contact.
			gameObject.tag = "Untagged"; // Set the nametag to something that can't trigger anything.
			coll.isTrigger = true;
			rb.isKinematic = true;
		}
		// Load up all of the UI elements.
		MySceneManager.instance.GameLoadUI();
	}
}
