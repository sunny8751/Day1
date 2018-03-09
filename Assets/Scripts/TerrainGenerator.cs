using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	//public string type = "grass"; //grass, sand, snow
	public GameObject[] skyDecorations;

	GameObject[] decorations, items;

	//underground block
	GameObject centerPrefab;
	//left hill blocks
	GameObject leftPrefab;
	GameObject left2Prefab;
	//right hill blocks
	GameObject rightPrefab;
	GameObject right2Prefab;
	//level surface block
	GameObject midPrefab;

	GameObject water, waterTop;


	public static int chunkSize = 32, maxChunkExplored = 0;
	static int minX, maxX, minY = -3, maxY = 8;
	int heightL, heightR;

	static PerlinNoise pn;
	//length of each block
	public float length;

	void Awake () {
		long seed = Random.Range (1000000, 10000000);
		//seed = 7939704;
		print ("Seed: " + seed);
		pn = new PerlinNoise (seed);

		//loadResources ();

		GenerateBeginning ();
		GenerateChunk (0, "grass");
		GenerateChunk (1, "grass");
	}

	private void loadResources(string type) {
		centerPrefab = Resources.Load<GameObject> (type + "Center");
		leftPrefab = Resources.Load<GameObject> (type + "HillLeft");
		left2Prefab = Resources.Load<GameObject> (type + "HillLeft2");
		rightPrefab = Resources.Load<GameObject> (type + "HillRight");
		right2Prefab = Resources.Load<GameObject> (type + "HillRight2");
		midPrefab = Resources.Load<GameObject> (type + "Mid");

		if (type != "snow") {
			water = Resources.Load<GameObject> ("liquidWater");
			waterTop = Resources.Load<GameObject> ("liquidWaterTop_mid");
		} else {
			water = Resources.Load<GameObject> ("ice");
			waterTop = Resources.Load<GameObject> ("iceTop");
		}

		decorations = Resources.LoadAll<GameObject> (type + "Decorations");
		items = Resources.LoadAll<GameObject> (type + "Items");
		length = centerPrefab.GetComponent<SpriteRenderer> ().sprite.bounds.size.x;
	}

	public bool isWater(float playerX, int blockPos, bool useBlockPos) {
		//returns whether the terrain is water at a position x
		if (!useBlockPos) {
			blockPos = Mathf.RoundToInt ((playerX - chunkSize / 2) / length);
		}
		int columnHeight = getHeight (blockPos);
		int heightL = getHeight (blockPos - 1);
		int heightR = getHeight (blockPos + 1);
		return columnHeight == minY + 2 && heightL == columnHeight && heightR == columnHeight;
	}

	public void GenerateChunk (int chunkNumber, string _type) {
		GameObject chunk = new GameObject ("Chunk"+chunkNumber);
		chunk.tag = _type;
		loadResources (_type);
		minX = -chunkSize / 2 + chunkNumber * chunkSize;
		maxX = chunkSize / 2 + chunkNumber * chunkSize;
		int columnHeight;
		for (int i = minX; i < maxX; i++) {
			columnHeight = getHeight (i);

			if (i == minX) {
				heightL = getHeight (i - 1);
			}
			heightR = getHeight (i + 1);

			//sky decorations
			if (Random.Range (0, 10) == 0) {
				createObject (skyDecorations [Random.Range (0, skyDecorations.Length)], new Vector2 (i * length + chunkSize / 2, Random.Range (columnHeight + 1, 4)), chunk);
			}

			GameObject block = centerPrefab;
			for (int j = minY; j < columnHeight; j++) {
				Vector2 pos = new Vector2 (i * length + chunkSize / 2, j * length - 1.5f);
				//can change if it's the top 2 blocks
				if (j == columnHeight - 1) {
					//top block
					if (isWater(0, i, true)) {
						block = waterTop;
					} else if (heightL == columnHeight - 1) {
						//hill left
						block = leftPrefab;
					} else if (heightR == columnHeight - 1) {
						//hill left
						block = rightPrefab;
					} else {
						block = midPrefab;
						//add occasional decorations
						int n = Random.Range (0, 10);
						if (n == 0) {
							//terrain decorations
							createObject(decorations [Random.Range (0, decorations.Length)], pos + Vector2.up, chunk);
						} else if (n >= 8) {
							//items such as trees
							//see if chunk has ben instantiated before
							if (chunkNumber < maxChunkExplored) {
								//reload old items from gamehandler
								foreach (InstantiatedObject obj in GameHandler.getObjects(chunkNumber)) {
									createObject (obj.prefab, obj.pos, chunk);
								}
							} else {
								//add new items to the gamehandler
								GameObject item = items [Random.Range (0, items.Length)];
								createObject (item, pos + Vector2.up, chunk);
								GameHandler.addObject (chunkNumber, item, pos + Vector2.up);
							}
						}
					}
				} else if (j == columnHeight - 2) {
					//second to top block
					if (isWater(0, i, true)) {
						block = water;
					} else if (heightL == columnHeight - 1) {
						//hill left
						block = left2Prefab;
					} else if (heightR == columnHeight - 1) {
						//hill left
						block = right2Prefab;
					}
				}
				createObject (block, pos, chunk);
			}
			heightL = columnHeight;
			columnHeight = heightR;
		}
	}

	void GenerateBeginning () {
		loadResources ("grass");
		//generate the three flat columns and stone wall
		GameObject beginning = new GameObject("Beginning");
		GameObject wallPrefab = Resources.Load<GameObject> ("wall");

		int height = getHeight (-chunkSize / 2);

		//flat land
		for (int i = -3; i < 0; i++) {
			GameObject block = centerPrefab;
			for (int j = minY; j < height; j++) {
				Vector2 pos = new Vector2 (i * length - length * chunkSize / 2 + chunkSize / 2, j * length - 1.5f);
				if (j == height - 1) {
					block = midPrefab;
				}
				createObject (block, pos, beginning);
			}
		}
		//wall
		for (int j = minY; j < height + 8; j++) {
			Vector2 pos = new Vector2 (-4 * length - length * chunkSize / 2 + chunkSize / 2, j * length - 1.5f);
			if (j < height) {
				//ground
				createObject(centerPrefab, pos, beginning);
			} else {
				//stone
				createObject(wallPrefab, pos, beginning);
			}
		}
	}

	public void Recycle (int chunkNumber) {
		//"Destroy" chunk by disassembling parts for reuse
		Transform chunkTransform = GameObject.Find("Chunk"+chunkNumber).transform;
		Dictionary <string, ArrayList> dict = new Dictionary <string, ArrayList> ();
		for (int i = 0; i < chunkTransform.childCount; i++) {
			Transform part = chunkTransform.GetChild (i);
			//set disactive
			part.gameObject.SetActive(false);
			//add to the dictionary
			if (dict.ContainsKey (part.name.Substring(0, part.name.Length - 7))) {
				dict [part.name.Substring(0, part.name.Length - 7)].Add (part);
			} else {
				dict.Add (part.name.Substring(0, part.name.Length - 7), new ArrayList () { part });
			}
		}

		//transfer values in dict to gameobject
		foreach (string key in dict.Keys) {
			GameObject folder;
			if (GameObject.Find (key) == null) {
				folder = new GameObject (key);
			} else {
				folder = GameObject.Find (key);
			}
			foreach (Transform part in dict[key]) {
				part.parent = folder.transform;
			}
		}
		Destroy (chunkTransform.gameObject);
	}

	GameObject createObject(GameObject prefab, Vector3 pos, GameObject parent) {
		GameObject obj;
		//see if we can use recycled object
		if (GameObject.Find (prefab.name) != null && GameObject.Find (prefab.name).transform.childCount > 0) {
			obj = GameObject.Find (prefab.name).transform.GetChild (0).gameObject;
			obj.SetActive (true);
			obj.transform.position = pos;
		} else {
			obj = Instantiate (prefab, pos, Quaternion.identity) as GameObject;
		}
		obj.transform.parent = parent.transform;
		return obj;
	}

	public static int getHeight(int i) {
		return minY + 2 + pn.getNoise (i + chunkSize / 2, maxY - minY - 2);
	}

	public Vector2 getPlayerStartingPosition() {
		return new Vector2 (-3 * length - length * chunkSize / 2 + chunkSize / 2, getHeight(-chunkSize/2));
	}
}
