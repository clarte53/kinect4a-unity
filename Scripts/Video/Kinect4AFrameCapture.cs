using K4AdotNet.Sensor;
using System;
using System.Threading;
using UnityEngine;

public class Kinect4AFrameCapture : IDisposable
{
	#region Members
	private int camWidth;
	private int camHeight;
	private TextureFormat textureFormat;

	private Capture capture;
	#endregion

	#region Constructor / Destructor
	public Kinect4AFrameCapture(Calibration calibration, ImageFormat imageFormat)
	{
		camWidth = calibration.ColorCameraCalibration.ResolutionWidth;
		camHeight = calibration.ColorCameraCalibration.ResolutionHeight;
		if (camWidth == 0 || camHeight == 0)
		{
			throw new ArgumentException("Calibration is not set");
		}

		switch (imageFormat)
		{
			case ImageFormat.ColorBgra32: textureFormat = TextureFormat.BGRA32; break;
			case ImageFormat.ColorYUY2: textureFormat = TextureFormat.YUY2; break;
			default: throw new ApplicationException(string.Format("Texture format {0} not supported.", imageFormat));
		}
	}

	public void Dispose() { }
	#endregion

	#region Setter
	public void SetCapture(Capture capture)
	{
		this.capture = capture;
	}
	#endregion

	#region Public Method
	public Kinect4AFrame NextFrame()
	{
		while (capture == null)
		{
			Thread.Sleep(0);
		}
		return new Kinect4AFrame(camWidth, camHeight, textureFormat, capture);
	}
	#endregion
}
