using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using K4AdotNet.Sensor;

[CustomEditor(typeof(Kinect4ASensor))]
public class Kinect4ASensorEditor : Editor
{
	Kinect4ASensor sensor;

	private SerializedProperty useRecordedVideoProperty;
	private SerializedProperty imageFormatProperty;
	private SerializedProperty colorResolutionProperty;
	private SerializedProperty depthModeProperty;
	private SerializedProperty cameraFPSProperty;
	private SerializedProperty synchronizedImagesOnlyProperty;

	private SerializedProperty filenameProperty;
	private SerializedProperty loopRecordingProperty;

	private string[] toolbarStrings = new string[] { "USB", "Video" };

	private GUILayoutOption[] optionsButtonBrowse = { GUILayout.MaxWidth(30) };
	private string[] filters = { "Kinect recording files", "mkv" }; //Filters used for browsing for a recording. 

	private void OnEnable()
	{
		sensor = (Kinect4ASensor)target;

		//Input Serialized Property
		useRecordedVideoProperty = serializedObject.FindProperty("useRecordedVideo");
		imageFormatProperty = serializedObject.FindProperty("imageFormat");
		colorResolutionProperty = serializedObject.FindProperty("colorResolution");
		depthModeProperty = serializedObject.FindProperty("depthMode");
		cameraFPSProperty = serializedObject.FindProperty("cameraFPS");
		synchronizedImagesOnlyProperty = serializedObject.FindProperty("synchronizedImagesOnly");

		filenameProperty = serializedObject.FindProperty("filename");
		loopRecordingProperty = serializedObject.FindProperty("loopPlayback");
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Space(15);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Input Type", GUILayout.Width(EditorGUIUtility.labelWidth));
		GUI.enabled = !Application.isPlaying;
		int inputType = GUILayout.Toolbar(useRecordedVideoProperty.boolValue ? 1 : 0, toolbarStrings, GUILayout.ExpandWidth(true));
		useRecordedVideoProperty.boolValue = inputType > 0 ? true : false;
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(5);

		switch (inputType)
		{
			case 0: // USB
				{
					GUIContent imageFormatLabel = new GUIContent("Image format", "Image color format");
					imageFormatProperty.enumValueIndex = (int)(ImageFormat)EditorGUILayout.EnumPopup(imageFormatLabel, (ImageFormat)imageFormatProperty.enumValueIndex);

					GUIContent colorResolutionLabel = new GUIContent("Color resolution", "Color image resolution");
					colorResolutionProperty.enumValueIndex = (int)(ColorResolution)EditorGUILayout.EnumPopup(colorResolutionLabel, (ColorResolution)colorResolutionProperty.enumValueIndex);

					GUIContent depthModeLabel = new GUIContent("Depth mode", "Depth image resolution and ratio");
					depthModeProperty.enumValueIndex = (int)(DepthMode)EditorGUILayout.EnumPopup(depthModeLabel, (DepthMode)depthModeProperty.enumValueIndex);

					GUIContent cameraFPSLabel = new GUIContent("Camera FPS", "Camera update rate");
					cameraFPSProperty.enumValueIndex = (int)(FrameRate)EditorGUILayout.EnumPopup(cameraFPSLabel, (FrameRate)cameraFPSProperty.enumValueIndex);

					GUIContent synchronizedImagesOnlyLabel = new GUIContent("Synchronize images only", "If enabled, captures with non synchronized images will be skipped");
					synchronizedImagesOnlyProperty.boolValue = EditorGUILayout.Toggle(synchronizedImagesOnlyLabel, synchronizedImagesOnlyProperty.boolValue);

					break;
				}

			case 1: // video
				{
					EditorGUILayout.BeginHorizontal();
					GUIContent fileNameLabel = new GUIContent("Recording file", "Kinect4A recording file");
					GUI.enabled = !Application.isPlaying;
					filenameProperty.stringValue = EditorGUILayout.TextField(fileNameLabel, filenameProperty.stringValue);
					GUIContent loadFilelabel = new GUIContent("...", "Browse for Kinect4A record.");
					if (GUILayout.Button(loadFilelabel, optionsButtonBrowse))
					{
						filenameProperty.stringValue = EditorUtility.OpenFilePanelWithFilters("Load Kinect4A recording", "", filters);
					}
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal();

					GUIContent imageFormatLabel = new GUIContent("Image format", "Convert video frames to the selected format");
					imageFormatProperty.enumValueIndex = (int)(ImageFormat)EditorGUILayout.EnumPopup(imageFormatLabel, (ImageFormat)imageFormatProperty.enumValueIndex);

					GUIContent loopRecordingLabel = new GUIContent("Loop", "If true, playback will restart when the end is reached");
					loopRecordingProperty.boolValue = EditorGUILayout.Toggle(loopRecordingLabel, loopRecordingProperty.boolValue);

					break;
				}
		}

		serializedObject.ApplyModifiedProperties();
	}
}