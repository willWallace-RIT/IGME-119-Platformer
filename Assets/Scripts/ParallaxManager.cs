/** Jonathan So, jds7523@rit.edu
 * Manages all parallax layers in the scene, including their movement and instantiation.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour {

	public static ParallaxManager instance; // Singleton design pattern.

	public ParallaxLayer bg, back, front; // Prefabs for the background, back and front parallax layers which we'll instantiate at runtime.

	private List<ParallaxLayer> layers; // List of all parallax layers.
	private const int NUM_LAYERS = 2; // Number of layers to instantiate.

	/** Set up the ParallaxManager's functionality.
	 * Set the Singleton design pattern, make the layers list, and instantiate all of the 
	 * parallax layers.
	 */
	private void Awake() {
		if (instance == null) { // Set up the Singleton design pattern.
			instance = this;
		}
		layers = new List<ParallaxLayer>();
		Vector3 layerPos = Vector3.zero;
		for (int i = 0; i < NUM_LAYERS; i++) {
			ParallaxLayer pl = Instantiate(bg, layerPos, Quaternion.identity);
			ParallaxLayer plA = Instantiate(back, layerPos, Quaternion.identity);
			ParallaxLayer plB = Instantiate(front, layerPos, Quaternion.identity);
			layers.Add(pl);
			layers.Add(plA);
			layers.Add(plB);
			layerPos = new Vector3(layerPos.x + back.GetComponent<SpriteRenderer>().bounds.size.x, 0, 0);
		}
	}

	/** Uses the SetDirection function present in each ParallaxLayer to set all parallax layer directions.
	 * 
	 * param[newDir] - the new direction for each of these layers. Must be -1, 0, or +1.
	 */
	public void SetDirection(int newDir) {
		if (PlatformSpawner.instance.GetLevelIsInfiniteAutoScroller()) { // If we're in an autoscrolling level...
			newDir = -1; // Make sure that the parallax layers always scroll to the left.
		}
		foreach (ParallaxLayer layer in layers) {
			layer.SetDirection(newDir); // Set the movement direction of every parallax layer.
		}
	}
}
