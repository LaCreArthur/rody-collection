using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
/// Script to handle the player states, collision, speed and health
/// It allows keyboard, pad and mobile device controle 
///</summary>

public class PlayerController : MonoBehaviour {

	public float startingSpeed;
	public float speedUpForce;
    public float acceleration;
    public float sensitivity;
    public Text touchSpeedUp;
	[HideInInspector] public Rigidbody rb;
	[HideInInspector] public float speed;
	[HideInInspector] public float levelMinSpeed;
	[HideInInspector] public int health;

	// used an event system to avoid circular reference with the GameManager and the SegmentController
	public delegate void playerEvent(int x);
	public static event playerEvent Picked;
	public static event playerEvent Moved;
	public static event playerEvent Damaged;
	public Camera rollCamera;

	float fov;
	bool isVulnerable;
	bool isBlinking;
	float pressTime;

    void Start () {
		isVulnerable = true;
		isBlinking = false;
		rb = GetComponent<Rigidbody>();
		health = 3;
		fov = rollCamera.fieldOfView;
		levelMinSpeed = speed = startingSpeed;
		pressTime = 1.0f;
	}
	
	void FixedUpdate() {
		
		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		touchSpeedUp.text = pressTime.ToString();

		#if UNITY_ANDROID
		if (Input.touchCount == 1) {
			Touch touch = Input.touches[0];
			if (touch.position.y < Screen.height / 2){
				pressTime += Time.deltaTime * sensitivity / 2;
				if (touch.position.x < Screen.width / 2) { // screen touched on the left
					moveHorizontal = -0.5f * pressTime ;
				}
				else moveHorizontal = 0.5f * pressTime; // screen touched on the right
			}
			else pressTime = 1.0f;
		}
		else pressTime = 1.0f;
		#elif UNITY_WEBGL || UNITY_EDITOR || UNITY_STANDALONE
		moveHorizontal *= sensitivity;
		#endif

		// Direct velocity for snappy horizontal control, physics for forward momentum
		Vector3 vel = rb.linearVelocity;
		vel.x = moveHorizontal;
		rb.linearVelocity = vel;

		// Forward force + slight downward to reduce flying time
		rb.AddForce(new Vector3(0f, -0.25f, 1.0f) * speed);
		
		if (Moved != null) Moved((int)transform.position.z); // send new position to SegmentController

		// increase speed over time
		speed += acceleration * Time.deltaTime;
		
		// field of view expansion through velocity
		rollCamera.fieldOfView = Mathf.Lerp(rollCamera.fieldOfView, fov + speed / 6, Time.deltaTime);

		// make the player blink if he touches a wall
		if (!isVulnerable && !isBlinking) {
			isBlinking = true;
			StartCoroutine(DoBlinks());			
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pick Up")) {
			StartCoroutine(DisablePickup(other.gameObject));
			if (Picked != null) Picked(1); // send event to GameManager to update score
		}

		if (other.gameObject.CompareTag("Heart")) {
			StartCoroutine(DisablePickup(other.gameObject));
			if (Picked != null) Picked(2);
			if (health < 7) health++;
			else {
				// if the player reach more than 6 HP, he gains 100 points, good luck!
				// also prevent health_bar glitch and cheats
				for (int i = 0; i < 100; i++) Picked(1);
			}
		}

		if (other.gameObject.CompareTag("Speed Up")) {
			other.gameObject.GetComponent<AudioSource>().Play();
			Vector3 movement = new Vector3(0.0f, 0.0f, speedUpForce);
			rb.AddForce(movement);
		}

        if (other.gameObject.CompareTag("GroundJoin")) {
			// avoid unwated jump on ground jointures
			rb.constraints = RigidbodyConstraints.FreezePositionY;
		}
	}
	void OnCollisionEnter(Collision collision)
    {
		// the hit only count if it occurs on the front of the obsacle to reduce the difficulty
        if (collision.gameObject.CompareTag("Obstacle") 
		&& collision.transform.position.y + 1.0f > transform.position.y // not hit from the top
		&& collision.transform.position.z > transform.position.z + 0.5f  // not hit from the sides
		&& isVulnerable){
			Debug.Log("Crashed ! Slowing down ...");
			collision.gameObject.GetComponent<AudioSource>().Play();
			speed -= (speed - levelMinSpeed) / 2; // slow down by half of gained speed 
			isVulnerable = false; // Invulnerable frames for 3s
			// lose an HP
			if (Damaged != null) Damaged(--health);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag("GroundJoin")) {
			rb.constraints = RigidbodyConstraints.None;
		}
	}

	IEnumerator DoBlinks() {
		Debug.Log("Invulnerable! ....");
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		for (int i=0; i<14; i++) {
			renderer.enabled = !renderer.enabled;
			yield return new WaitForSeconds(0.1f);
		}
		renderer.enabled = true;
		isBlinking = false;
		isVulnerable = true;
		Debug.Log(".... Not anymore !");
	}

	IEnumerator DisablePickup(GameObject pickup) {
		// avoid sound redondancy by lightly shifting the pich
		pickup.GetComponent<AudioSource>().pitch = Random.Range(0.975f, 1.025f);

		pickup.GetComponent<AudioSource>().Play();
		// hide the pickup
		pickup.GetComponent<MeshRenderer>().enabled = false;
		// wait untill the sound is played
		while(pickup.GetComponent<AudioSource>().isPlaying)
			yield return null;
		Destroy(pickup);
	}
}
