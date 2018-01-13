/** Jonathan So, jds7523@rit.edu
 * This is a manager class for all of the NPCs. Change a property here, and upon runtime, it changes properties of all NPCs in the scene. Communicates with the "NPC" script.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Manager : MonoBehaviour {

	public static NPC_Manager instance; // Singleton design pattern.

	public bool disappearOnContact, knockbackOnContact, stopToInteract, fallOffPlatforms; // Boolean properties for each NPC. 
	[Range(1.0f, 40.0f)] // The knockback can be anywhere from 1 to 40.
	public float knockback; // Amount of knockback to apply on NPCs upon contact, if knockbackOnContact is true.
	public AudioClip interactSFX, contactSFX; // Sound effects to play on an NPC when they perform interaction and contact behaviors.

	// Set up the Singleton design pattern.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	// Call the LateStart function one frame after the game starts.
	private void Start() {
		Invoke("LateStart", Time.deltaTime);
	}

	// For each NPC, set the variables (such as disappearOnContact and knockback) based on our desired values.
	private void LateStart() {
		List<NPC> npcs = new List<NPC>(GameObject.FindObjectsOfType<NPC>());
		foreach (NPC currentNPC in npcs) {
			SetVariables(currentNPC);
		}
	}

	/** Set all variables of a chosen NPC to our desired values.
	 * param[npc] - the NPC whose variables we want to set.
	 */
	public void SetVariables(NPC npc) {
		npc.SetDisappearOnContact(disappearOnContact);
		npc.SetKnockbackOnContact(knockbackOnContact);
		npc.SetStopToInteract(stopToInteract);
		npc.SetFallOffPlatforms(fallOffPlatforms);
		npc.SetKnockback(knockback);
		npc.SetSFX(interactSFX, contactSFX);
	}
}
