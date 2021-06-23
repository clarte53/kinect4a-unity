using UnityEngine;
using CLARTE.Video;
using K4AdotNet.Sensor;
using System;

public class Kinect4AFrame : Frame
{
    #region Contructors

    /// <summary>
    /// Create blanc frame
    /// </summary>
    /// <param name="width"> image width</param>
    /// <param name="height"> image height</param>
    /// <param name="format"> image color format</param>
    public Kinect4AFrame(int width, int height, TextureFormat format) : base(width, height, format) { }

    /// <summary>
    /// Create new frame with raw data as byte
    /// </summary>
    /// <param name="width"> image width</param>
    /// <param name="height"> image height</param>
    /// <param name="format"> image color format</param>
    /// <param name="data"> raw byte data</param>
    public Kinect4AFrame(int width, int height, TextureFormat format, byte[] data) : base(width, height, format)
    {
        if (data.Length != Data.Length)
        {
            throw new InvalidDataFrame(string.Format("Data provided has a length of {0} but should be {1}", data.Length, Data.Length));
        }
        Buffer.BlockCopy(data, 0, Data, 0, data.Length);
    }

    /// <summary>
    /// Create new frame where data is store in capture color image
    /// </summary>
    /// <param name="width"> image width</param>
    /// <param name="height"> image height</param>
    /// <param name="format"> image color format</param>
    /// <param name="capture"> capture acquired from kinect</param>
    public Kinect4AFrame(int width, int height, TextureFormat format, Capture capture) : base(width, height, format)
    {
        if(capture.ColorImage.WidthPixels != width || capture.ColorImage.HeightPixels != height)
        {
            throw new InvalidDataFrame(string.Format("Capture color image size ({0},{1}) does not match arguments ({2},{3})",
                capture.ColorImage.WidthPixels, capture.ColorImage.HeightPixels, width, height));
        }
        capture.ColorImage.CopyTo(Data);
    }

    /// <summary>
    /// Create new frame where data is store in image
    /// </summary>
    /// <param name="width"> image width</param>
    /// <param name="height"> image height</param>
    /// <param name="format"> image color format</param>
    /// <param name="image"> Microsoft image from kinect </param>
    public Kinect4AFrame(int width, int height, TextureFormat format, Image image) : base(width, height, format)
    {
        if (image.WidthPixels != width || image.HeightPixels != height)
        {
            throw new InvalidDataFrame(string.Format("Image size ({0},{1}) does not match arguments ({2},{3})",
                image.WidthPixels, image.HeightPixels, width, height));
        }
        image.CopyTo(Data);
    }
    #endregion
}
