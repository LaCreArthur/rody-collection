using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// this script sets up two buttons on the low half  of the screen to control the player by touch
/// click on the low left part of the screen to move the player to the left and vise versa
///</summary>

public class TouchButtonController : MonoBehaviour {

	public GameObject player;

	Rigidbody playerRb;
	PlayerController playerController;

	void Start () {
		GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height / 2);
		playerRb = player.GetComponent<Rigidbody>();
		playerController = player.GetComponent<PlayerController>();
	}

	public void OnClick(bool isRight) {
		// Direct velocity for snappy control (matches PlayerController)
		float x = isRight ? playerController.sensitivity : -playerController.sensitivity;
		Vector3 vel = playerRb.linearVelocity;
		vel.x = x;
		playerRb.linearVelocity = vel;
	}
}