/** Jonathan So, jds7523@rit.edu
 * Platform prefabs in an infinite, auto-scrolling level are reused; rewrapped around the screen and randomly changed.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinitePlatformPrefab : MonoBehaviour {

	public GameObject[] prefabs; // Array of possible prefabs that this object may be.
	private List<GameObject> objects; // List of instances of prefabs that this object will be.
	private const float X_BOUND = 11f; // The magnitude of the X-position at which we rewrap this object.
	private const float SPEED = 3/64f; // Speed at which this platform scrolls, since this level is an autoscroller.

	// Create the list of objects and create objects from prefabs.
	private void Awake() {
		objects = new List<GameObject>();
		CreateObjectsFromPrefabs();
	}

	// After creating all of the objects, set all but one to be inactive.
	private void Start() {
		SetObjectsInactive();
		objects[Random.Range(0, objects.Count)].SetActive(true);
	}

	/** Create all possible prefabs that this object can be; for example, it can be a normal platform, a platform with coins, one with NPCs, etc.
	 * Set each object's parent to this object so that it follows our transform.
	 */
	private void CreateObjectsFromPrefabs() {
		foreach (GameObject pre in prefabs) {
			GameObject obj = (GameObject) Instantiate(pre, transform.position, Quaternion.identity);
			obj.transform.parent = transform;
			objects.Add(obj);
		}
	}

	// Set all objects in our objects array to be inactive.
	private void SetObjectsInactive() {
		foreach(GameObject obj in objects) {
			obj.SetActive(false);
		}
	}

	// Keep this platform scrolling and in-bounds; if it goes out of bounds, then wrap it around the screen and change to a different prefab.
	private void Update() {
		// Screen-wrap
		if (transform.position.x <= -X_BOUND) {
			transform.position = new Vector2(X_BOUND, transform.position.y);
			SetObjectsInactive();
			if (Random.Range(0, objects.Count + 1) != 0) { // Randomly choose to make this a different prefab or even nothing (to create jumpable gaps).
				GameObject obj = objects[Random.Range(0, objects.Count)];
				obj.SetActive(true);
				foreach (Transform child in obj.GetComponentsInChildren<Transform>()) { // Make sure to reactivate all NPCs and coins.
					child.gameObject.SetActive(true);

				}
			}
		}
		transform.Translate(Vector2.right * -SPEED); // Move the platform to the left.
	}
}
