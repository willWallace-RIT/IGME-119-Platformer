/** Jonathan So, jds7523@rit.edu
 * This is a manager class for the level. Change a boolean here, and it changes a property of the level.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	public bool levelWillScroll, levelIsInfiniteAutoScroller, procedurallyGenerateLevel, proceduralLevelHasBottomPlatform, levelHasNoCoins; // Booleans corresponding to private getters/setters in PlatformSpawner.

	/** Set all of the PlatformSpawner's "open" variables according to what we want.
	 * Change the properties of the generated level using the Singleton of the PlatformSpawner.
	 */
	private void Start() {
		PlatformSpawner.instance.SetLevelWillScroll(levelWillScroll);
		PlatformSpawner.instance.SetLevelIsInfiniteAutoScroller(levelIsInfiniteAutoScroller);
		PlatformSpawner.instance.SetProcedurallyGenerateLevel(procedurallyGenerateLevel);
		PlatformSpawner.instance.SetProceduralLevelHasBottomPlatform(proceduralLevelHasBottomPlatform);
		PlatformSpawner.instance.SetLevelHasNoCoins(levelHasNoCoins);
	}
}
