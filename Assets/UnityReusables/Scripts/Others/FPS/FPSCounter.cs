using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class FPSCounter : MonoBehaviour {

	public int frameRange = 30;

	public IntVariable averageFPS;
	public IntVariable highFPS;
	public IntVariable lowFPS;
	
	int[] fpsBuffer;
	int fpsBufferIndex;

	void Update () {
		if (fpsBuffer == null || fpsBuffer.Length != frameRange) {
			InitializeBuffer();
		}
		UpdateBuffer();
		CalculateFPS();
	}

	void InitializeBuffer () {
		if (frameRange <= 0) {
			frameRange = 1;
		}
		fpsBuffer = new int[frameRange];
		fpsBufferIndex = 0;
	}

	void UpdateBuffer () {
		fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
		if (fpsBufferIndex >= frameRange) {
			fpsBufferIndex = 0;
		}
	}

	void CalculateFPS () {
		int sum = 0;
		int highest = 0;
		int lowest = int.MaxValue;
		for (int i = 0; i < frameRange; i++) {
			int fps = fpsBuffer[i];
			sum += fps;
			if (fps > highest) {
				highest = fps;
			}
			if (fps < lowest) {
				lowest = fps;
			}
		}
		averageFPS.v = (int)((float)sum / frameRange);
		highFPS.v = highest;
		lowFPS.v = lowest;
	}
}