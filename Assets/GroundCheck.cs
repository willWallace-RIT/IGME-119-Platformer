/** Jonathan So, jds7523@rit.edu
* This object, child of the Player, detects whether or not the player is in contact with the ground.
*/
using UnityEngine;
using System.Collections;

public class GroundCheck : MonoBehaviour {

	public bool onGround = false; // Whether or not the player is on the ground.
	public Animator anim; // The Animator component of the parent object (Player), used to set the onGround variable.

	/** Called automatically by Unity whenever this object interacts with an object containing a collider.
	 * If this object is touching a platform, then change the onGround variable to "true".
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, GroundCheck) is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnCollisionStay2D(Collision2D coll) {
		if (coll.gameObject.tag.Equals("Platform") && !Player.instance.inactive) { // If we're currently touching a platform...
			onGround = true; // We are "on the ground."
			anim.SetBool("onGround", true);
			Player.instance.SetSpeed(onGround);
		}
	}

	/** Called automatically by Unity when this object leaves contact from an object containing a collider.
	 * If this object leaves contact with a platform, then change the onGround variable to "false".
	 * 
	 * param[coll] - the 2D collider which this object is touching.
	 * PRECONDITION: The GameObject (in this case, GroundCheck) is required to have a Rigidbody2D in order to use this function.
	 */
	private void OnCollisionExit2D(Collision2D coll) {
		if (coll.gameObject.tag.Equals("Platform") && !Player.instance.inactive) { // If we're currently touching a platform...
			onGround = false; // We are off the ground now.
			anim.SetBool("onGround", false);
			Player.instance.SetSpeed(onGround);
		}
	}
}
