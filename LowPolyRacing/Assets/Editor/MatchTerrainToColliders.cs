//
// Script originally from user @Zer0cool at:
//
// https://forum.unity.com/threads/terrain-leveling.926483/
//
// Revamped by @kurtdekker as follows:
//
//	- put this on the object (or object hierarchy) with colliders
//	- drag the terrain reference into it
//	- use the editor button to "Stamp"
//	- support for a ramped perimeter, curved as specified
//
// Also posted at / about:
// https://twitter.com/kurtdekker/status/1281619776587001856?s=20
// https://www.youtube.com/watch?v=FykNmJ3NIpI
// https://pastebin.com/DYFAYgnE
// 
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MatchTerrainToColliders : MonoBehaviour {
	[Tooltip(
		"Assign Terrain here if you like, otherwise we search for one.")]
	public Terrain terrain;

	[Tooltip(
		"Default is to cast from below. This will cast from above and bring the terrain to match the TOP of our collider.")]
	public bool CastFromAbove;

	[Header("Related to smoothing around the edges.")]

	[Tooltip(
		"Size of gaussian filter applied to change array. Set to zero for none")]
	public int PerimeterRampDistance;

	[Tooltip(
		"Use Perimeter Ramp Curve in lieu of direct gaussian smooth.")]
	public bool ApplyPerimeterRampCurve;

	[Tooltip(
		"Optional shaped ramp around perimeter.")]
	public AnimationCurve PerimeterRampCurve;

	[Header("Misc/Editor")]

	[Tooltip(
		"Enable this if you want undo. It is SUPER-dog slow though, so I would leave it OFF.")]
	public bool EnableEditorUndo;

	// This extends the binary on/off blend stencil out by one pixel,
	// making one sheet at a time, then stacks (adds) them all together and
	// renormalizes them back to 0.0-1.0.
	//
	// it simultaneously takes the average of the "hitting" perimeter neighboring
	// heightmap cells and extends it outwards as it expands.
	//
	void GeneratePerimeterHeightRampAndFlange(float[,] heightMap, float[,] blendStencil, int distance) {
		int w = blendStencil.GetLength(0);
		int h = blendStencil.GetLength(1);

		// each stencil, expanded by one more pixel, before we restack them
		float[][,] stencilPile = new float[distance + 1][,];

		// where we will build the horizontal heightmap flange out
		float[,] extendedHeightmap = new float[w, h];

		// directonal table: 4-way and 8-way available
		int[] neighborXYPairs = new int[] {
			// compass directions first
			0, 1,
			1, 0,
			0, -1,
			-1, 0,
			// diagonals next
			1,1,
			-1,1,
			1,-1,
			-1,-1,
		};

		int neighborCount = 4;                  // 4 and 8 are supported from the table above

		float[,] source = blendStencil;         // this is NOT a copy! This is a reference!
		for(int n = 0; n <= distance; n++) {
			// add it to the pile BEFORE we expand it;
			// that way the first one is the original
			// input blendStencil.
			stencilPile[n] = source;

			// Debug: WritePNG( source, "pile-" + n.ToString());

			// this is gonna be an actual true deep copy of the stencil
			// as it stands now, and it will steadily grow outwards, but
			// each time it is always 0.0 or 1.0 cells, nothing in between.
			float[,] expanded = new float[w, h];
			for(int j = 0; j < h; j++) {
				for(int i = 0; i < w; i++) {
					expanded[i, j] = source[i, j];
				}
			}

			// we have to quit so we don't further expand the flange heightmap
			if(n == distance) {
				break;
			}

			// Add one solid pixel around perimeter of the stencil.
			// Also ledge-extend the perimeter heightmap value for those
			// non-zero cells, not reducing them at all (they are like
			// flat flange going outwards that we need in order to later blend).
			//
			for(int j = 0; j < h; j++) {
				for(int i = 0; i < w; i++) {
					if(source[i, j] == 0) {
						// serves as "hit" or not too
						int count = 0;

						// for average of neighboring heights
						float height = 0.0f;

						for(int neighbor = 0; neighbor < neighborCount; neighbor++) {
							int x = i + neighborXYPairs[neighbor * 2 + 0];
							int y = j + neighborXYPairs[neighbor * 2 + 1];
							if((x >= 0) && (x < w) && (y >= 0) && (y < h)) {
								// found a neighbor: we will:
								//	- areally expand the stencil by this one pixel
								//	- sample the neighbor height for the flange extension
								if(source[x, y] != 0) {
									height += heightMap[x, y];
									count++;
								}
							}
						}

						// extend the height of this cell by the average height
						// of the neighbors that contained source stencil true
						if(count > 0) {
							expanded[i, j] = 1.0f;

							extendedHeightmap[i, j] = height / count;
						}
					}
				}
			}

			// Copy the new ledge back to the original heightmap.
			// WARNING: this is an "output" operation because it is
			// modifying the supplied input heightmap data, areally
			// adding around the edge by the pixels encountered.
			for(int j = 0; j < h; j++) {
				for(int i = 0; i < w; i++) {
					var height = extendedHeightmap[i, j];

					// only lift... this still allows us to lower terrain,
					// since it is lifting from absolute zero to the altitude
					// that we actually sensed at this hit neighbor pixels,
					// and we need this unattenuated height for later blending.
					if(height > 0) {
						heightMap[i, j] = height;
					}

					// zero it too, for next layer (might not be necessary??)
					extendedHeightmap[i, j] = 0;
				}
			}

			// assign the source to this fresh copy
			source = expanded;          // shallow copy (reference)
		}

		// now tally the pile, summarizing each stack of 0/1 solid pixels,
		// copying it to to the stencil array passed in, which will change
		// its contents directly, and renormalize it back down to 0.0 to 1.0
		//
		// WARNING: this is also an output operation, as it modifies the
		// blendStencil inbound dataset
		//
		for(int j = 0; j < h; j++) {
			for(int i = 0; i < w; i++) {
				float total = 0;
				for(int n = 0; n <= distance; n++) {
					total += stencilPile[n][i, j];
				}

				total /= (distance + 1);

				blendStencil[i, j] = total;
			}
		}

		// Debug: WritePNG( blendStencil, "blend");
	}

	void BringTerrainToUndersideOfCollider() {
		var Colliders = GetComponentsInChildren<Collider>();

		if(Colliders == null || Colliders.Length == 0) {
			Debug.LogError("We must have at least one collider on ourselves or below us in the hierarchy. " +
				"We will cast to it and match terrain to that contour.");
			return;
		}

		// if you don't provide a terrain, it searches and warns
		if(!terrain) {
			terrain = FindObjectOfType<Terrain>();
			if(!terrain) {
				Debug.LogError("couldn't find a terrain");
				return;
			}
			Debug.LogWarning(
				"Terrain not supplied; finding it myself. I found and assigned " + terrain.name +
				", but I didn't do anything yet... click again to actually DO the modification.");
			return;
		}

		TerrainData terData = terrain.terrainData;
		int Tw = terData.heightmapResolution;
		int Th = terData.heightmapResolution;
		var heightMapOriginal = terData.GetHeights(0, 0, Tw, Th);

		// where we do our work when we generate the new terrain heights
		var heightMapCreated = new float[heightMapOriginal.GetLength(0), heightMapOriginal.GetLength(1)];

		// for blending heightMapCreated with the heightMapOriginal to form
		var heightAlpha = new float[heightMapOriginal.GetLength(0), heightMapOriginal.GetLength(1)];

#if UNITY_EDITOR
		if(EnableEditorUndo) {
			Undo.RecordObject(terData, "ModifyTerrain");
		}
#endif

		for(int Tz = 0; Tz < Th; Tz++) {
			for(int Tx = 0; Tx < Tw; Tx++) {
				// start under the terrain and cast up?
				var pos = terrain.transform.position +
					new Vector3((Tx * terData.size.x) / (Tw - 1),
					-10,
					(Tz * terData.size.z) / (Th - 1));

				Ray ray = new Ray(pos, Vector3.up);

				// nope, start from above and cast down
				if(CastFromAbove) {
					pos.y = transform.position.y + terData.size.y + 10;
					ray = new Ray(pos, Vector3.down);
				}

				bool didHit = false;
				float yHit = 0;

				// scan all the colliders and take the "firstest" distance we hit at
				foreach(var ourCollider in Colliders) {
					RaycastHit hit;
					if(ourCollider.Raycast(ray, out hit, 1000)) {
						if(!didHit) {
							yHit = hit.point.y;
						}

						didHit = true;

						// take lowest or highest, as appropriate
						if(CastFromAbove) {
							if(hit.point.y > yHit) {
								yHit = hit.point.y;
							}
						} else {
							if(hit.point.y < yHit) {
								yHit = hit.point.y;
							}
						}

					}

					if(didHit) {
						var height = (yHit - terrain.transform.position.y) / terData.size.y;

						heightMapCreated[Tz, Tx] = height;
						heightAlpha[Tz, Tx] = 1.0f;             // opaque
					}
				}
			}
		}

		// now we might smooth things out a bit
		if(PerimeterRampDistance > 0) {
			// Debug: WritePNG( heightMapCreated, "height-0", true);
			// Debug: WritePNG( heightAlpha, "alpha-0", true);

			GeneratePerimeterHeightRampAndFlange(
				heightMap: heightMapCreated,
				blendStencil: heightAlpha,
				distance: PerimeterRampDistance);

			// Debug: WritePNG( heightMapCreated, "height-1", true);
			// Debug: WritePNG( heightAlpha, "alpha-1", true);
		}

		// apply the generated data (blend operation)
		for(int Tz = 0; Tz < Th; Tz++) {
			for(int Tx = 0; Tx < Tw; Tx++) {
				float fraction = heightAlpha[Tz, Tx];

				if(ApplyPerimeterRampCurve) {
					fraction = PerimeterRampCurve.Evaluate(fraction);
				}

				heightMapOriginal[Tz, Tx] = Mathf.Lerp(
					heightMapOriginal[Tz, Tx],
					heightMapCreated[Tz, Tx],
					fraction);
			}
		}

		terData.SetHeights(0, 0, heightMapOriginal);
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MatchTerrainToColliders))]
	public class MatchTerrainToCollidersEditor : Editor {
		public override void OnInspectorGUI() {
			MatchTerrainToColliders item = (MatchTerrainToColliders) target;

			DrawDefaultInspector();

			EditorGUILayout.BeginVertical();

			var buttonLabel = "Bring Terrain To Underside Of Collider";
			if(item.CastFromAbove) {
				buttonLabel = "Bring Terrain To Topside Of Collider";
			}

			if(GUILayout.Button(buttonLabel)) {
				item.BringTerrainToUndersideOfCollider();
			}

			EditorGUILayout.EndVertical();
		}
	}
#endif

	// debug stuff:
	void WritePNG(float[,] array, string filename, bool normalize = false) {
		int w = array.GetLength(0);
		int h = array.GetLength(1);

		Texture2D texture = new Texture2D(w, h);

		Color[] colors = new Color[w * h];

		// to colors
		{
			float min = 0;
			float max = 1;

			if(normalize) {
				min = 1;
				max = 0;
				for(int j = 0; j < h; j++) {
					for(int i = 0; i < w; i++) {
						float x = array[i, j];
						if(x < min)
							min = x;
						if(x > max)
							max = x;
					}
				}

				// no dynamic range present, disable normalization
				if(max <= min) {
					min = 0;
					max = 1;
				}
			}

			int n = 0;
			for(int j = 0; j < h; j++) {
				for(int i = 0; i < w; i++) {
					float x = array[i, j];
					x = x - min;
					x /= (max - min);
					colors[n] = new Color(x, x, x);
					n++;
				}
			}
		}

		texture.SetPixels(colors);
		texture.Apply();

		var bytes = texture.EncodeToPNG();

		DestroyImmediate(texture);

		filename = filename + ".png";

		System.IO.File.WriteAllBytes(filename, bytes);
	}

	// call this in lieu of doing the actual data
	void Debug_Microtest() {
		float[,] heights = new float[3, 3] {
			{ 0.0f, 0.0f, 0.0f, },
			{ 0.0f, 0.5f, 0.0f, },
			{ 0.0f, 0.0f, 0.0f, }
		};
		float[,] stencil = new float[3, 3] {
			{ 0.0f, 0.0f, 0.0f, },
			{ 0.0f, 1.0f, 0.0f, },
			{ 0.0f, 0.0f, 0.0f, }
		};

		{
			WritePNG(heights, "height-0", true);
			WritePNG(stencil, "alpha-0", true);

			GeneratePerimeterHeightRampAndFlange(
				heightMap: heights,
				blendStencil: stencil,
				distance: PerimeterRampDistance);

			WritePNG(heights, "height-1", true);
			WritePNG(stencil, "alpha-1", true);
		}
	}
}