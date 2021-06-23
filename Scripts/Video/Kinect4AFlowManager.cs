using UnityEngine;

public class Kinect4AFlowManager : MonoBehaviour
{
	public Kinect4ASensor kinectSensor;
	void OnEnable()
	{
		if (kinectSensor == null)
		{
			Debug.LogError("Kinect sensor field is not set");
			enabled = false;
			return;
		}

		kinectSensor.kinectOpened += KinectOpened;
		kinectSensor.kinectClosed += KinectStopped;
	}

	private void OnDisable()
	{
		if (kinectSensor != null)
		{
			kinectSensor.kinectOpened -= KinectOpened;
			kinectSensor.kinectClosed -= KinectStopped;
		}
	}

	private void KinectOpened()
	{
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(true);
		}
	}

	private void KinectStopped()
	{
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}
	}
}
