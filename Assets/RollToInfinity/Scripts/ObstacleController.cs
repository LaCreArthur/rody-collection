using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// this script hide the obstacle if it is behind and above the player 
/// it do this by switching the obstacle material to a transparent one
///</summary>
public class ObstacleController : MonoBehaviour {

	public Material mat_normal;
	public Material mat_transparent;
	public bool isMoving;
	[Range(-3.5f, 3.5f)]
	public float min;
	[Range(-3.5f, 3.5f)]
	public float max;

	static Transform s_playerTransform;
	MeshRenderer meshRenderer;
	bool isGoingRight;

	void Start () {
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = mat_normal;
		if (s_playerTransform == null)
			s_playerTransform = GameObject.FindWithTag("Player").transform;
		isGoingRight = Random.value > 0.5f;
	}
	
	void Update () {
		// make mat_transparent the obstacles that are before the player and in altitude
		if (transform.position.y > 1.0f && transform.position.z + 1f < s_playerTransform.position.z) {
			meshRenderer.material = mat_transparent;
		}

		if (isMoving) {
			// if it is at the max, stop going to the right
			if (transform.position.x >= max) isGoingRight = false;
			// if it is at the min, stop going to the left
			if (transform.position.x <= min) isGoingRight = true;

			// if it is going to the right
			if (transform.position.x < max && isGoingRight)
				transform.position += (Vector3.right * Time.deltaTime * 2.0f);
			
			// if it is going to the left
			if (transform.position.x > min && !isGoingRight)
				transform.position += (-Vector3.right * Time.deltaTime * 2.0f);
		}
	}
}
