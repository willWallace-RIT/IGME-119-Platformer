/** Jonathan So, jds7523@rit.edu
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour {

	public static PlatformSpawner instance; // Singleton design pattern.

	public bool levelWillScroll; // If true, then the level scrolls. 
	public bool procedurallyGenerateLevel; // If true, instantiate platforms and procedurally generate the level.
	public float X_SCROLL_BOUNDS = 10f; // 

	public GameObject plat_3, plat_4, plat_5; // Platform prefabs that span 3, 4, and 5 tiles, respectively.

	public GameObject level; // Empty GameObject whose children, combined, form the entire level architecture.

	private List<GameObject> platforms; //
	private List<GameObject> npcs; //
	private List<GameObject> coins; //

	private const float X_BOUNDS = 5.5f; //
	private const float Y_BOUNDS = 4.5f; //

	/** If the level will be procedurally generated, then make it according to specifications.
	 *
	 */
	private void Awake() {
		// Set up the Singleton.
		if (instance == null) {
			instance = this;
		}
		if (procedurallyGenerateLevel) {
			ClearLevel();
			GenerateLevel();
		}
	}

	/** Moves the level architecture around the Player to simulate movement within a scrolling level.
	 * NOTE: This function will only be called by the Player, and only in the case that the level will scroll.
	 * 
	 * param[moveVec] - a Vector2 which is the OPPOSITE direction that the player is movement, or the vector that moves the level around the player.
	 */
	public void MoveLevel(Vector2 moveVec) {
		level.transform.Translate(moveVec);
	}

	/**
	 * 
	 */
	private void ClearLevel() {
		platforms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Platform"));
		npcs = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPC"));
		coins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Coin"));
		foreach (GameObject plat in platforms) {
			platforms.Remove(plat);
			Destroy(plat);
		}
		foreach (GameObject npc in npcs) {
			platforms.Remove(npc);
			Destroy(npc);
		}
		foreach (GameObject coin in coins) {
			platforms.Remove(coin);
			Destroy(coin);
		}
	}

	/**
	 * 
	 */
	private void GenerateLevel() {
		float xBound = 0;
		if (levelWillScroll) {
			xBound = -X_SCROLL_BOUNDS;
		} else {
			xBound = -X_BOUNDS;
		}

	}
}
