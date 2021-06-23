using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Samples.Unity;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class ColliderSkeletonProvider : MonoBehaviour, IInitializable
{
	/// <summary>
	/// Code highly based on K4A.Net plugin example (https://github.com/bibigone/k4a.net)
	/// </summary>
	#region Members
	public Kinect4ASensor kinectSensor;
    public TrackerProcessingMode processingMode;
    public bool useLiteDnnVersion;
    private Tracker tracker;
    private string modelPath;

    public bool IsInitializationComplete { get; private set; }
    public bool IsAvailable { get; private set; }

    public event EventHandler<SkeletonEventArgs> SkeletonUpdated;
    #endregion

    #region Monobehaviour callbacks
    private IEnumerator Start()
    {
		if(kinectSensor == null)
		{
			Debug.LogError("KinectSensor field is not assigned");
			enabled = false;
		} else
		{
			yield return new WaitForSeconds(2);

			Task<Tuple<bool, string>> task = Task.Run(() =>
			{
				bool initialized = Sdk.TryInitializeBodyTrackingRuntime(processingMode, out string message);
				return Tuple.Create(initialized, message);
			});
			yield return new WaitUntil(() => task.IsCompleted);

            modelPath = useLiteDnnVersion ? Sdk.BODY_TRACKING_DNN_MODEL_LITE_FILE_NAME : Sdk.BODY_TRACKING_DNN_MODEL_FILE_NAME;

			bool is_available = false;
			try
			{
				Tuple<bool, string> result = task.Result;
				is_available = result.Item1;
				if (!is_available)
				{
					Debug.Log($"Cannot initialize body tracking: {result.Item2}");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Exception on {nameof(Sdk.TryInitializeBodyTrackingRuntime)}\r\n{ex}");
			}

			if (is_available)
			{
				kinectSensor.CaptureReady += KinectSensor_CaptureReady;
			}

			IsAvailable = is_available;
		}
        
    }

    private void OnDestroy()
    {
        IsAvailable = false;

        kinectSensor.CaptureReady -= KinectSensor_CaptureReady;
        tracker?.Dispose();
    }

    private void Update()
    {
        if (IsInitializationComplete && IsAvailable)
        {
            if (tracker.TryPopResult(out BodyFrame body_frame))
            {
                using (body_frame)
                {
                    if (body_frame.BodyCount > 0)
                    {
                        body_frame.GetBodySkeleton(0, out Skeleton skeleton);
                        SkeletonUpdated?.Invoke(this, new SkeletonEventArgs(skeleton));
                    }
                    else
                    {
                        SkeletonUpdated?.Invoke(this, SkeletonEventArgs.Empty);
                    }
                }
            }
        }
    }
    #endregion

    #region Private Method
    /// <summary>
    /// Function handling the events raised by the KinectSensor when a new capture is available
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="captureArg"> Object containing the new capture</param>
    private void KinectSensor_CaptureReady(object sender, CaptureEventArgs e)
    {
        if(!IsInitializationComplete)
        {
            var calibration = kinectSensor.GetCalibration();
            TrackerConfiguration conf = TrackerConfiguration.Default;
            conf.ProcessingMode = processingMode;
            conf.ModelPath = modelPath;
            tracker = new Tracker(in calibration, conf);
            IsInitializationComplete = true;
        }

        if (IsInitializationComplete && IsAvailable)
        {
            tracker.TryEnqueueCapture(e.Capture);
        }
    }
	#endregion
}

