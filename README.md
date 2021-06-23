Unity plugin to retrieve MS Kinect for Azure data. This plugin relies on [K4A.Net](https://github.com/bibigone/k4a.net) 
library and on Microsoft bodytracking([nuget package]((https://www.nuget.org/packages/Microsoft.Azure.Kinect.BodyTracking/))) library.

Bodytracking dependencies must be downloaded following the instructions available in the [K4A.Net](https://github.com/bibigone/k4a.net) readme.

### Use in Unity
In order to use Video dataflow elements, Bodytracking elements and PointCloudRenderer from mesh reconstruction, 
the scene needs to have KinectSensor component on one of its GameObjects. <br>
The manager raises events that will be handled by relevant scripts.
