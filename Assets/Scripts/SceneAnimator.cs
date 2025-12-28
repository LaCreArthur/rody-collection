using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // convert

public class SceneAnimator : MonoBehaviour {

	public GameManager gm;
	SpriteRenderer spriteRenderer;
	public Sprite baseFrame;
	public List<Sprite> frames;
	public bool isSpeaking;

	private int fps = 5;
	int currentFrame = 0;
	public int sumDial = 0;
	public int firstDial = 0;
	bool increasing = true;

	float frameTime;
	float frameTimer = 0;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		isSpeaking = false;
		frameTime = 1 / (float)fps;
		spriteRenderer.sprite = baseFrame;	
	}

	void Update()
	{
		if (isSpeaking) {
			if (frameTimer < frameTime)
			{
				frameTimer += Time.deltaTime;
			}
			else
			{
				// Debug.Log("SceneAnimator : currentframe: " + currentFrame + ", firstDial: " + firstDial + ", currentDialIndex: " + gm.sm.currentDialIndex + ", sumDial: "+ sumDial);
				spriteRenderer.sprite = frames[currentFrame]; // change the sprite
				
				if (frames.Count > 1) {
					// there are frames for animation	
					if (sumDial > 1 && frames.Count > 3) { 
						// there are more than one non mastico dial AND there are more than 3 frame of dialogue
						if (gm.sm.currentDialIndex == firstDial )
							UpdateFrame(1, 3); // play 1,2,3,2,1,...
						else 
							UpdateFrame(4, frames.Count);	// play 4,..,frames.Count,...,4,...
					}
					else {
						// there is only one non mastico dial or not enough frames, play all the frames
						UpdateFrame(1, frames.Count);	// play 1,..,frames.Count,...,1,...
					}
				}
				frameTimer = 0;
			}
		}
		else {
			spriteRenderer.sprite = baseFrame;
		}
	}

	/// <summary>
	/// assign to currentFrame the next frame in the cycle [iFirst,..., iLast, ..., iFirst, ...]
	/// </summary>
	/// <param name="iFirst"> first frame of the cycle
	/// <param name="iLast"> first frame of the cycle
	void UpdateFrame(int iFirst, int iLast) {
		
		if (currentFrame > iFirst - 1) { 
			if (currentFrame < iLast - 1) {
				currentFrame = increasing ? currentFrame + 1 : currentFrame - 1;
			}
			else { // currentFrame == last frame
				increasing = false;
				currentFrame = iLast - 2;
			}
		}
		else { // currentFrame == 0
			increasing = true;
			currentFrame = iFirst;
		}
	}

}
