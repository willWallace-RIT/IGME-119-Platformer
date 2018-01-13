/** Jonathan So, jds7523@rit.edu
 * This is a manager class for the player's properties. Change a property here, and upon runtime, it changes properties of the player. Communicates with the Player script.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Manager : MonoBehaviour {

	public bool disappearOnContact, knockbackOnContact, isShooter; // Booleans corresponding to similarly-named properties in the Player. Please set them right here in the Player Manager.
	public KeyCode up, left, right, interact; // Keycodes so that you may define the game's control scheme.

	/** Set all of the player's "open" properties (the ones above).
	 * Grab a reference to the player and change its properties to the desired values as specified in this Player Manager.
	 */
	private void Start() {
		Player player = GameObject.FindObjectOfType<Player>(); // Grab reference to Player.
		// Set our desired values.
		player.SetDisappearOnContact(disappearOnContact);
		player.SetKnockbackOnContact(knockbackOnContact);
		player.SetIsShooter(isShooter);
		player.SetControls(up, left, right, interact);
	}
}
