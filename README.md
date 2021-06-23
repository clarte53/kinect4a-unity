Unity plugin to retrieve MS Kinect for Azure data. This plugin relies on [K4A.Net](https://github.com/bibigone/k4a.net) 
library ([nuget package](https://www.nuget.org/packages/K4AdotNet/1.4.1)) and on 
Microsoft bodytracking([nuget package]((https://www.nuget.org/packages/Microsoft.Azure.Kinect.BodyTracking/))) library.

This plugin already contains the necessary libraries (handled by Git LFS).

### Use in Unity
In order to use Video dataflow elements, Bodytracking elements and PointCloudRenderer from mesh reconstruction, 
the scene needs to have KinectManager component on one of its GameObjects. <br>
The manager raises events that will be handled by relevant scripts.
