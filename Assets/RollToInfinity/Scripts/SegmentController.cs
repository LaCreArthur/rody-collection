using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// Script to handle the procedural generation of the map through random segments and buildings instantiation
/// It uses a segment queue to track the instantiated segments
/// A segment n+2 is instantiated when the player reach the middle of the segment n, the fog of the game makes the instantiation invisible
/// At this point, the n-1 segment is destroyed.
///</summary>

public class SegmentController : MonoBehaviour {

	public Material buildingMat;
	public GameObject[] segBase; 
	public GameObject segParent;

	float offset;
	GameObject ground;
	Queue<GameObject> segQueue;
	GameObject currentSegment; 

	int segRand {
		get {
			// ensure the instantiation of empty ground until the game start
			if (RollGameManager.isStarted && segQueue.Count > 0)
				return Random.Range(1, segBase.Length - 1);
			else 
				return 0; // empty ground
		}
	}

	void Awake() {
		// ensure all segment bases are disable at startup
		foreach (GameObject seg in segBase) {
			seg.SetActive(false);
		}
	}

	void Start () {
		initSeg();
	}

	public void initSeg() {
		// create or reset the segment queue
		if (segQueue == null) segQueue = new Queue<GameObject>();
		else {
		while (segQueue.Count > 0)
			Destroy(segQueue.Dequeue());
		}

		offset = 0.0f;
		SetCurrentSegment();
		randomBuildings();

		// Unsubscribe first to prevent duplicate handlers on reset
		PlayerController.Moved -= NewSegment;
		PlayerController.Moved += NewSegment;
	}

	void NewSegment (int position) {
		// Handle a new segment if the player has crossed the current one
		if (position >= offset) {
			// increase the offset by the length of the ground
			ground = currentSegment.transform.GetChild(0).gameObject;
			offset += ground.GetComponent<Collider>().bounds.size.z;
			Debug.Log("offset is now: " + offset);

			SetCurrentSegment();

			// delete old segment
			if (segQueue.Count > 3) Destroy(segQueue.Dequeue());
			
			randomBuildings();
		}
	}

	GameObject instantiateBuilding(string name, int b_offset, bool isRight = false) {
		GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
		building.layer = 9;
		building.transform.SetParent(currentSegment.transform.GetChild(2).transform); // Should be `backgroundCity`
		building.name = name;
		building.transform.position = new Vector3((isRight)?12:-12, -6, 16 + b_offset + offset);
		building.transform.localScale = new Vector3(8, Random.Range(6.5f, 70.0f) , 8);
		building.GetComponent<MeshRenderer>().material = buildingMat;
		return building;
	}

	void SetCurrentSegment() {
			// Clone a randomly choosen segment at the offset position
			currentSegment = Instantiate(segBase[segRand], new Vector3(0, 0, offset), Quaternion.identity, segParent.transform);
			currentSegment.SetActive(true);
			// enqueue it to track the active segments
			segQueue.Enqueue(currentSegment);
	}

	void randomBuildings(int b_offset = 0) {
		for (int i=0; i<5; i++) {
			instantiateBuilding("leftBuilding" + i, i * 16 + b_offset);
			instantiateBuilding("rightBuilding" + i, i * 16 + b_offset, true);
		}
	}

	void OnDestroy()
	{
		PlayerController.Moved -= NewSegment;
		if (segQueue != null) {
			while (segQueue.Count > 0)
				Destroy(segQueue.Dequeue());
		}
	}
}
