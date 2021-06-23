//using Microsoft.Azure.Kinect.Sensor;
using K4AdotNet.Sensor;
using UnityEngine;
using System.Threading;
using System;
using K4AdotNet.Samples.Unity;
using K4AdotNet.Record;
using CLARTE.Threads.APC;
using CLARTE.Dev.Profiling;

public class Kinect4ASensor : MonoBehaviour
{
	public bool useRecordedVideo;

	// Event callbacks
	private MonoBehaviourCall behaviourCall;
	public delegate void KinectEvent();
	public event KinectEvent kinectOpened;
	public event KinectEvent kinectClosed;

	public event EventHandler<CaptureEventArgs> CaptureReady;
	
	#region Members
    [Header("Camera settings")]
    [SerializeField]
    private ImageFormat imageFormat = ImageFormat.ColorBgra32; //ImageFormat.ColorBGRA32;
    public ColorResolution colorResolution = ColorResolution.R720p;
    public DepthMode depthMode = DepthMode.NarrowViewUnbinned; //DepthMode.NFOV_Unbinned;
    public bool synchronizedImagesOnly = true;
    public FrameRate cameraFPS = FrameRate.Thirty; //FPS _cameraFPS = FPS.FPS30;

	[SerializeField]
	private string filename;
	public bool loopPlayback = true;

	// Kinect values
	private int deviceID = 0;
	private Playback playback;
	private Device kinect;
	private Transformation transformation;
	private Calibration calibration = new Calibration();
	private Capture capture = new Capture();

	// Event values
	private CaptureEventArgs eventCapture;
	private bool isCaptureDirty = false;
	
	private bool kinectIsOn = false;    // remplacer par device.isconnected() ?
	Chrono timePlaying = new Chrono();

	// Threading values
	protected Thread captureThread;
	protected readonly System.Object doneLock = new System.Object();
	private bool done = true;

	#endregion

	#region Getter

	public ImageFormat ImageFormat { get => imageFormat; }

	public Transformation GetTransformation()
	{
		return transformation;
	}

	public Calibration GetCalibration()
	{
		return calibration;
	}
	#endregion

	#region Monobehaviour callbacks
	private void OnEnable()
	{
		done = false;
		behaviourCall = MonoBehaviourCall.Instance;
		captureThread = new Thread(UpdateCapture);
		captureThread.Start();
	}

	private void OnDisable()
	{
		lock (doneLock)
		{
			done = true;
		}

		if (captureThread != null)
		{
			captureThread.Join();
		}     
    }

    private void Update()
    {
        lock (capture)
        {
            if (isCaptureDirty)
            {
                eventCapture = new CaptureEventArgs(capture);
                isCaptureDirty = false;
            }
        }

        // Don't throw event in lock to avoid slowing it down
        if (eventCapture != null)
        {
            CaptureReady?.Invoke(this, eventCapture);
            eventCapture = null;
        }

		lock(doneLock)
		{
			if(done)
			{
				enabled = false;
			}
		}
    }
	#endregion 

	#region Private methods
	/// <summary>
	/// Open device stream if it has not been opened already 
	/// </summary>
	private void StartCamera()
    {
        if (!kinectIsOn)
        {
            DeviceConfiguration deviceConfiguration = new DeviceConfiguration
            {
                ColorFormat = imageFormat,
                ColorResolution = colorResolution,
                DepthMode = depthMode,
                SynchronizedImagesOnly = synchronizedImagesOnly,
                CameraFps = cameraFPS
            };

            kinect = Device.Open(deviceID);
            kinect.StartCameras(deviceConfiguration);
            kinect.GetCalibration(depthMode, colorResolution, out calibration);

			kinectIsOn = true;
		}
	}

	/// <summary>
	/// Open kinect video playback 
	/// </summary>
	private void OpenVideo()
	{
		if (!string.IsNullOrEmpty(filename))
		{
			playback = new Playback(filename);

			if (playback != null)
			{
				playback.GetCalibration(out calibration);

				playback.SetColorConversion(imageFormat);

				timePlaying.Reset();
				timePlaying.Start();
			}
		}
	}

	/// <summary>
	/// Stop kinect if it has not been stopped already
	/// </summary>
	private void StopCamera()
	{
		if (kinectIsOn)
		{
			kinectIsOn = false;
			kinect.StopCameras();
			kinect.Dispose();
			kinect = null;
		}
	}

	/// <summary>
	/// Close video playback
	/// </summary>
	private void CloseVideo()
	{
		timePlaying.Stop();

		playback.Dispose();
	}

	/// <summary>
	/// Start capture flow depending on method choosen
	/// </summary>
	private void Init()
	{
		if (useRecordedVideo)
		{
			OpenVideo();
		}
		else
		{
			StartCamera();
		}
	}

	/// <summary>
	/// Stop capture flow
	/// </summary>
	private void Close()
	{
		if (useRecordedVideo)
		{
			CloseVideo();
		}
		else
		{
			StopCamera();
		}
	}

	/// <summary>
	/// Retrieve the next capture
	/// </summary>
	/// <param name="capture"></param>
	/// <returns> true if the capture depth image and color image exist </returns>
	private bool TryGetCapture(out Capture capture)
	{
		if (useRecordedVideo)
		{
			long elapsed_ms = (long)(timePlaying.GetElapsedTime() * 1000000.0);

			bool seek = playback.TrySeekTimestamp(new K4AdotNet.Microseconds64(elapsed_ms), PlaybackSeekOrigin.Begin);
			bool get_capture = playback.TryGetNextCapture(out capture);

			if(!(seek && get_capture))
			{
				if(loopPlayback)
				{
					playback.SeekTimestamp(0, PlaybackSeekOrigin.Begin);
					
					timePlaying.Restart();

					playback.TryGetNextCapture(out capture);
				}
				else
				{
					return false;
				}
			}

			return capture.DepthImage != null && capture.ColorImage != null;
		}
		else
		{
			int timeout = 1000;
			return kinect.TryGetCapture(out capture, K4AdotNet.Timeout.FromMilliseconds(timeout));
		}
	}

	/// <summary>
	/// Thread continuously updating the capture until sensor is done
	/// </summary>
	private void UpdateCapture()
	{
		Init();

		behaviourCall?.Call(() => kinectOpened?.Invoke());

		transformation = calibration.CreateTransformation();

		Capture _capture = new Capture();

		bool _done = false;

		while (!_done)
		{
			if (TryGetCapture(out _capture))
			{
				if (_capture != null)    // nécessaire?
				{
					lock (capture)
					{
						capture = _capture;
						isCaptureDirty = true;
					}
				}
			}

			lock (doneLock)
			{
				_done = done;
			}

			Thread.Sleep(0);
		}

		Close();
		behaviourCall?.Call(() => kinectClosed?.Invoke());
	}
	#endregion

}
