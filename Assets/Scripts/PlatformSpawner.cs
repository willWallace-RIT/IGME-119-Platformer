/** Jonathan So, jds7523@rit.edu
 * Manages the layout of the level and, if the user desires, procedurally generates a level for the player to roam around.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour {

	public static PlatformSpawner instance; // Singleton design pattern.

	private bool levelWillScroll; // If true, then the level scrolls. 
	private bool levelIsInfiniteAutoScroller; // If true, then the level infinitely moves to the right, spawning new platforms.
	private bool procedurallyGenerateLevel; // If true, instantiate platforms and procedurally generate the level.
	private bool proceduralLevelHasBottomPlatform; // If true and the level is procedurally generated, then the level will have a platform at the bottom.
	private bool levelHasNoCoins; // If true, then the level will NOT contain collectibles.

	public float X_SCROLL_BOUNDS = 25f; // Bounds for the boundary objects in a scrolling level.

	public GameObject[] prefabs; // Platforming prefabs for randomly generating a level.
	public GameObject[] bounds; // The left and right screen bounds. We will set their X-positions based on whether or not the level will scroll.
	public GameObject level; // Empty GameObject whose children, combined, form the entire level architecture.
	public GameObject bottom; // Bottom platform prefab.
	public GameObject platBelowPlayer; // Platform below the player in the case that the level is procedurally generated and doesn't have a platform covering the bottom.
	public GameObject infinitePrefab; // Specialized platforming prefab for generating an auto-scrolling, infinite level.

	// In ClearLevel(), Used to clear the level architecture.
	private List<GameObject> platforms; // List of platform gameObjects.
	private List<GameObject> npcs; // List of NPC gameObjects.
	private List<GameObject> coins; // List of Coin gameObjects.

	private const float X_BOUNDS = 5.5f; // Magnitude of the x-positions in which to spawn platform prefabs; spawn prefabs from -5.5 to +5.5.
	private const float Y_BOUNDS = 4.5f; // Magnitude of the y-positions in which to spawn platform prefabs; spawn prefabs from -4.5 to +4.5.
	private const float SCROLL_BOUND_POS = 27f; // Position of the bound GameObjects in a scrolling, procedurally-generated level.
	private const float BOUND_POS = 6.5f; // Position of the bound GameObjects in a non-scrolling, procedurally-generated level.
	private const int MIN_CHILDREN = 32; // Minimum amount of children the level object may have for a non-scrolling, procedurally-generated level.
	private const int SCROLL_MIN_CHILDREN = 120; // Minimum amount of children the level object may have for a scrolling, procedurally-generated level.

	// Set up the Singleton.
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	// Call the LateStart function one frame after our game begins.
	private void Start() {
		Invoke("LateStart", Time.deltaTime);
	}

	/** Based on the parameters set by the user, generate a level (or don't, if the player has created their own layout.)
	 * Also, if the level is an infinite auto scroller, then lock the settings to ensure that the level is procedurally generated
	 * and doesn't run into any errors.
	 */
	private void LateStart() {
		if (levelIsInfiniteAutoScroller) {
			levelWillScroll = false;
			procedurallyGenerateLevel = true;
			proceduralLevelHasBottomPlatform = false;
		}
		GenerateLevel();
	}

	/** Moves the level architecture around the Player to simulate movement within a scrolling level.
	 * NOTE: This function will only be called by the Player, and only in the case that the level will scroll.
	 * 
	 * param[moveVec] - a Vector2 which is the OPPOSITE direction that the player is movement, or the vector that moves the level around the player.
	 */
	public void MoveLevel(Vector2 moveVec) {
		level.transform.Translate(moveVec);
	}

	/** Find all objects of the level architecture and destroy them all.
	 * Populate three separate lists with platforms, NPCs, and coins, and get rid of their objects.
	 */
	private void ClearLevel() {
		platforms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Platform"));
		npcs = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPC"));
		coins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Coin"));
		foreach (GameObject plat in platforms) {
			Destroy(plat);
		}
		foreach (GameObject npc in npcs) {
			Destroy(npc);
		}
		foreach (GameObject coin in coins) {			
			Destroy(coin);
		}
	}

	/** Create a prefab at random and parent it to the transform for the level (so that it is a child of the level object).
	 * param[xPos] - the X position of the object we're placing down.
	 * param[yPos] - the Y position of the object we're placing down.
	 */
	private void CreateRandomObject(float xPos, float yPos) {
		GameObject obj = (GameObject) Instantiate(prefabs[Random.Range(0, prefabs.Length)], new Vector2(xPos, yPos), Quaternion.identity);
		obj.transform.SetParent(level.transform);
	}

	/** Procedurally generate a level based on user settings.
	 * Based on whether or not the user wants an infinite autoscroller, a fixed-camera level, or a scrolling one, 
	 * set up the bounds of the level and randomly generate objects throughout.
	 */
	private void GenerateLevel() {		
		if (levelIsInfiniteAutoScroller) { // We have a special function to create autoscrolling levels.
			GenerateAutoScrollingLevel();
			return;
		} // Otherwise, create a level of finite length.
		float xBound = 0;
		float maxBound = 0;
		int minChildren = 0;
		if (levelWillScroll) { // Set up the bounds for a scrolling (but fixed-length) level.
			xBound = -X_SCROLL_BOUNDS;
			maxBound = X_SCROLL_BOUNDS;
			bounds[0].transform.position = new Vector2(-SCROLL_BOUND_POS, 0);
			bounds[1].transform.position = new Vector2(SCROLL_BOUND_POS, 0);
			minChildren = SCROLL_MIN_CHILDREN;
		} else { // Set up the bounds for a single-screen (but fixed-length) level.
			xBound = -X_BOUNDS;
			maxBound = X_BOUNDS;
			bounds[0].transform.position = new Vector2(-BOUND_POS, 0);
			bounds[1].transform.position = new Vector2(BOUND_POS, 0);
			minChildren = MIN_CHILDREN;
		}
		if (!procedurallyGenerateLevel) {
			DestroyCoins(); // If the user wants to destroy all coins, then do it.
			return;
		}
		ClearLevel(); // Clear the level to make way for the level we're generating now.
		if (proceduralLevelHasBottomPlatform) {
			GameObject obj = (GameObject) Instantiate(bottom, transform.position, Quaternion.identity);
			obj.transform.SetParent(level.transform);
		} else {
			GameObject obj = (GameObject) Instantiate(platBelowPlayer, new Vector2(transform.position.x, -3.5f), Quaternion.identity);
			obj.transform.SetParent(level.transform);
		}
		int rand = 0;
		// Spawn platforms and set up their x and y positions.
		for (float xPos = xBound; xPos < maxBound; xPos++) {
			for (float yPos = -4.5f; yPos <= 4.5f; yPos += 2) {
				rand = Random.Range(0, 8); // 7 in 8 chance of spawning a platform.
				if (rand == 0 && (xPos > 2 || xPos < -2)) { // Make sure not to spawn the platforms in a way where the player is stuck.
					CreateRandomObject(xPos, yPos);
				}
			}
		}
		if (level.transform.childCount < minChildren) { // If the level is too barren and sparse...
			GenerateLevel(); // Re-do it until the level is populated enough.
		}
		DestroyCoins(); // If the user wants to destroy all coins, then do it.
	}

	/** Checks to see if the user wants to destroy all coins; if so, then destroy all coins.
	 */
	private void DestroyCoins() {
		if (levelHasNoCoins) { // Get rid of ALL coins, if so desired.
			GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
			foreach (GameObject coin in coins) {
				Destroy(coin.gameObject);
			}
		}
	}

	/** Set up an infinitely auto-scrolling level (similar to Capcom's "Son Son") with three rows of platforms.
	 * All of the "infinitePrefab" objects will scroll automatically and recycle themselves. This function only sets up
	 * the positions of the infinitePrefabs.
	 */
	private void GenerateAutoScrollingLevel() {
		float xBound = -11;
		float maxBound = 11;
		foreach (GameObject bound in bounds) { // The infinite level has no formal bounds.
			bound.SetActive(false);
		}
		if (!procedurallyGenerateLevel) {
			return;
		}
		ClearLevel(); // Clear the level architecture.
		int rand = 0;
		// Generate and set positions of all of the infinitePrefab objects.
		for (float xPos = xBound; xPos < maxBound; xPos += 5f) {
			for (float yPos = -4.5f; yPos <= 3.5f; yPos += 3) {
				rand = Random.Range(0, 10); // There is a 9 in 10 chance of spawning the prefab in order to create gaps the player must jump over.
				if (rand != 0) {
					GameObject obj = (GameObject) Instantiate(infinitePrefab, new Vector2(xPos, yPos), Quaternion.identity);
					obj.transform.SetParent(level.transform);
				}
			}
		}
		DestroyCoins(); // If the user wants to destroy all coins, then do it.
	}


	///////////////
	/// GETTERS ///
	///////////////


	/** Getter for the field "levelWillScroll".
	 * return - the value of levelWillScroll, a boolean.
	 */
	public bool GetLevelWillScroll() {
		return levelWillScroll;
	}

	/** Getter for the field "levelIsInfiniteAutoScroller".
	 * return - the value of levelIsInfiniteAutoScroller, a boolean.
	 */
	public bool GetLevelIsInfiniteAutoScroller() {
		return levelIsInfiniteAutoScroller;
	}


	///////////////
	/// SETTERS ///
	///////////////

	/** Setter for the field "levelWillScroll".
	* param[newVar] - the new value of the boolean we're setting.
	*/
	public void SetLevelWillScroll(bool newVar) {
		levelWillScroll = newVar;
	}

	/** Setter for the field "levelIsInfiniteAutoScroller".
	* param[newVar] - the new value of the boolean we're setting.
	*/
	public void SetLevelIsInfiniteAutoScroller(bool newVar) {
		levelIsInfiniteAutoScroller = newVar;
	}

	/** Setter for the field "procedurallyGenerateLevel".
	* param[newVar] - the new value of the boolean we're setting.
	*/
	public void SetProcedurallyGenerateLevel(bool newVar) {
		procedurallyGenerateLevel = newVar;
	}

	/** Setter for the field "proceduralLevelHasBottomPlatform".
	* param[newVar] - the new value of the boolean we're setting.
	*/
	public void SetProceduralLevelHasBottomPlatform(bool newVar) {
		proceduralLevelHasBottomPlatform = newVar;
	}

	/** Setter for the field "levelHasNoCoins".
	* param[newVar] - the new value of the boolean we're setting.
	*/
	public void SetLevelHasNoCoins(bool newVar) {
		levelHasNoCoins = newVar;
	}
}
