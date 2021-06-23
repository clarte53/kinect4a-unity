using CLARTE.Threads.DataFlow.Unity;
using CLARTE.Video;
using K4AdotNet.Samples.Unity;
using System.Threading;
using UnityEngine;

public class Kinect4AFrameProvider : DataProvider<Frame>
{
	#region Members
	public Kinect4ASensor kinectSensor;
	private Kinect4AFrameCapture kinectCapture;
	#endregion

	#region Monobehaviour callbacks
	protected override void OnEnable()
	{
		base.OnEnable();
		if (kinectSensor == null)
		{
			Debug.LogError("Kinect sensor field is not set");
			enabled = false;
			return;
		}
		kinectSensor.CaptureReady += HandleCapture;
	}

	// Update is called once per frame
	protected override void OnDisable()
	{
		base.OnDisable();
		kinectCapture.Dispose();
		if (kinectSensor != null) kinectSensor.CaptureReady -= HandleCapture;
	}
	#endregion

	#region Public Method
	protected override Frame CreateData()
	{
		if (kinectCapture == null)
		{
			Thread.Sleep(2000); // Wait for 2 seconds to see if kinectSensor is running
			if (kinectCapture == null)
			{
				Debug.LogError("KinectSensor did not provide Capture for 2 seconds, disabling provider function");
				DataCreator.Stop(false);
				return null;
			}
			else
			{
				return kinectCapture.NextFrame();
			}
		}
		else
		{
			return kinectCapture.NextFrame();
		}
	}
	#endregion

	#region Private Method
	/// <summary>
	/// Function handling the events raised by the KinectSensor when a new capture is available
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="captureArg"> Object containing the new capture</param>
	private void HandleCapture(object sender, CaptureEventArgs captureArg)
	{
		if (kinectCapture == null)
		{
			kinectCapture = new Kinect4AFrameCapture(kinectSensor.GetCalibration(), kinectSensor.ImageFormat);
		}
		kinectCapture.SetCapture(captureArg.Capture);
	}
	#endregion
}
