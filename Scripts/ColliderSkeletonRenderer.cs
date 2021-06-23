using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Samples.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ColliderSkeletonRenderer : MonoBehaviour
{
	#region Internal Type
	/// <summary>
	/// Class representing a bone, provides access to its transform, its parent and child JointType
	/// </summary>
	private class Bone
	{
		public JointType ParentJoint { get; }
		public JointType ChildJoint { get; }
		public Transform Transform { get; }

		public static Bone FromChildJoint(JointType child_joint, float bone_radius)
		{
			return new Bone(child_joint.GetParent(), child_joint, bone_radius);
		}

		public Bone(JointType parent_joint, JointType child_joint, float bone_radius)
		{
			ParentJoint = parent_joint;
			ChildJoint = child_joint;

			GameObject pos = new GameObject();
			pos.name = $"{parent_joint}->{child_joint}:pos";

			GameObject bone = new GameObject();
			bone.name = $"{parent_joint}->{child_joint}:bone";
			bone.transform.parent = pos.transform;
			bone.transform.localScale = new Vector3(0.033f, 0.5f, 0.033f);
			bone.transform.localPosition = 0.5f * Vector3.up;
			CapsuleCollider collider = bone.AddComponent<CapsuleCollider>();
			collider.radius = bone_radius;
			collider.height = 2f; // Set to 2 to touch parent and child joint.
			Transform = pos.transform;
		}


	}
	#endregion

	#region Members
	public ColliderSkeletonProvider skeletonProvider;

	private GameObject root;
	private IReadOnlyDictionary<JointType, Transform> joints;
	private IReadOnlyCollection<Bone> bones;
	private Transform head;
	#endregion

	#region Monobehaviour callbacks
	private void Awake()
	{
		if(skeletonProvider == null)
		{
			Debug.LogError("Skeleton Provider is not set");
			enabled = false;
			return;
		}
		root = new GameObject();
		root.name = "skeleton:root";
		root.transform.parent = transform;
		root.transform.localScale = Vector3.one;
		root.transform.localPosition = Vector3.zero;
		root.transform.localRotation = UnityEngine.Quaternion.identity;
		root.SetActive(false);

		CreateJoints();
		CreateBones();
		CreateHead();
	}

	private void OnEnable()
	{
		if (skeletonProvider != null)
		{
			skeletonProvider.SkeletonUpdated += SkeletonProvider_SkeletonUpdated;
		}
	}

	private void OnDisable()
	{
		if (skeletonProvider != null)
		{
			skeletonProvider.SkeletonUpdated -= SkeletonProvider_SkeletonUpdated;
		}
	}
	#endregion

	#region Private Methods
	#region Create Skeleton bones and joints
	/// <summary>
	/// Create a new gameObject with collider for each joint of the skeleton. Object size depends on joint type
	/// </summary>
	private void CreateJoints()
	{
		float joint_radius = 0.075f; // Default joint size

		// Joints are rendered as spheres
		joints = JointTypes.All
			.ToDictionary(
				jt => jt,
				jt =>
				{
					GameObject joint = new GameObject();
					joint.name = $"{jt.ToString()}:joint";
					joint.transform.parent = root.transform;
					joint.transform.localScale = joint_radius * Vector3.one;
					SphereCollider collider = joint.AddComponent<SphereCollider>();
					collider.radius = 0.5f;
					return joint.transform;
				});

		// Set slightly decreased size for some joints
		SetJointScale(0.05f, JointType.Neck, JointType.Head, JointType.ClavicleLeft, JointType.ClavicleRight);

		// Set increased size for spine joints
		SetJointScale(0.2f, JointType.SpineNavel, JointType.SpineChest, JointType.Pelvis);

		// Face joints don't need to be represented by colliders
		SetJointScale(0f, JointType.EyeLeft, JointType.EyeRight, JointType.Nose, JointType.EarLeft, JointType.EarRight);

		// Set greatly decreased size and specific colors for face joints
		SetJointScale(0.033f, JointType.HandLeft, JointType.HandTipLeft, JointType.ThumbLeft);

		// Set greatly decreased size and specific colors for face joints
		SetJointScale(0.033f, JointType.HandRight, JointType.HandTipRight, JointType.ThumbRight);

	}

	/// <summary>
	/// Set each param local scale
	/// </summary>
	/// <param name="scale"> New joint scale </param>
	/// <param name="joint_types"> Joints to update </param>
	private void SetJointScale(float scale, params JointType[] joint_types)
	{
		foreach (var jt in joint_types)
			joints[jt].localScale = scale * Vector3.one;
	}

	/// <summary>
	/// Represent skeleton bones using GameObjects with colliders . Bone collider radius depends on bone type. 
	/// </summary>
	private void CreateBones()
	{
		List<Bone> bones = new List<Bone>();

		// Spine
		CreateBones(bones, 3f, JointType.SpineNavel, JointType.SpineChest, JointType.Neck);
		// Head
		CreateBones(bones, 1.5f, JointType.Head);
		// Right arm
		CreateBones(bones, 1.5f, JointType.ClavicleRight, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight);
		// Right hand
		CreateBones(bones, 0.5f, JointType.HandRight, JointType.HandTipRight, JointType.ThumbRight);
		// Left arm
		CreateBones(bones, 1.5f, JointType.ClavicleLeft, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft);
		// Left hand
		CreateBones(bones, 0.5f, JointType.HandLeft, JointType.HandTipLeft, JointType.ThumbLeft);
		// Right leg
		CreateBones(bones, 2f, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight);
		// Left leg
		CreateBones(bones, 2f, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft);

		this.bones = bones;
		foreach (var b in this.bones)
		{
			b.Transform.parent = root.transform;
		}
	}

	/// <summary>
	/// Create new bones from arguments and add them to list
	/// </summary>
	/// <param name="list"> List to store created bones </param>
	/// <param name="bone_radius"> Radius of the capsule collider representing the bone </param>
	/// <param name="child_joints"> Joints from which bones are created </param>
	private static void CreateBones(ICollection<Bone> list, float bone_radius, params JointType[] child_joints)
	{
		foreach (var joint in child_joints)
		{
			list.Add(Bone.FromChildJoint(joint, bone_radius));
		}
	}

	/// <summary>
	/// Create head GameObject
	/// </summary>
	private void CreateHead()
	{
		GameObject head = new GameObject();
		head.AddComponent<SphereCollider>();
		head.transform.parent = root.transform;

		this.head = head.transform;
	}

	#endregion

	/// <summary>
	/// Handle events raised by ColliderSkeletonProvider
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"> Contains skeleton informations </param>
	private void SkeletonProvider_SkeletonUpdated(object sender, SkeletonEventArgs e)
	{
		if (e.Skeleton == null)
		{
			HideSkeleton();
		}
		else
		{
			RenderSkeleton(e.Skeleton.Value);
		}
	}

	#region Set position of Skeleton elements
	/// <summary>
	/// Update Joints, Bones and head using skeleton
	/// </summary>
	/// <param name="skeleton"> Skeleton containing joints informations </param>
	private void RenderSkeleton(Skeleton skeleton)
	{
		foreach (KeyValuePair<JointType, Transform> item in joints)
		{
			item.Value.localPosition = ConvertKinectPos(skeleton[item.Key].PositionMm);
		}

		foreach (Bone bone in bones)
		{
			PositionBone(bone, skeleton);
		}

		PositionHead(skeleton);

		root.SetActive(true);
	}

	/// <summary>
	///  Update bone position and direction using bone parent and child joint
	/// </summary>
	/// <param name="bone"> Bone to update </param>
	/// <param name="skeleton"> Skeleton containing joints informations</param>
	private static void PositionBone(Bone bone, Skeleton skeleton)
	{
		Vector3 parent_pos = ConvertKinectPos(skeleton[bone.ParentJoint].PositionMm);
		Vector3 direction = ConvertKinectPos(skeleton[bone.ChildJoint].PositionMm) - parent_pos;
		bone.Transform.localPosition = parent_pos;
		bone.Transform.localScale = new Vector3(1, direction.magnitude, 1);
		bone.Transform.localRotation = UnityEngine.Quaternion.FromToRotation(Vector3.up, direction);
	}

	/// <summary>
	/// Update head position, rotation and scale depending on head and ear joints
	/// </summary>
	/// <param name="skeleton"> Skeleton containing joints informations </param>
	private void PositionHead(Skeleton skeleton)
	{
		Vector3 head_pos = ConvertKinectPos(skeleton[JointType.Head].PositionMm);
		Vector3 ear_pos_right = ConvertKinectPos(skeleton[JointType.EarRight].PositionMm);
		Vector3 ear_pos_left = ConvertKinectPos(skeleton[JointType.EarLeft].PositionMm);
		Vector3 head_center = 0.5f * (ear_pos_right + ear_pos_left);
		float d = (ear_pos_right - ear_pos_left).magnitude;
		head.localPosition = head_center;
		head.localRotation = UnityEngine.Quaternion.FromToRotation(Vector3.up, head_center - head_pos);
		head.localScale = new Vector3(d, 2 * (head_center - head_pos).magnitude, d);
	}

	/// <summary>
	///  Kinect Y axis points down, so negate Y coordinate
	/// Scale to convert millimeters to meters
	/// https://docs.microsoft.com/en-us/azure/Kinect-dk/coordinate-systems
	/// Other transforms(positioning of the skeleton in the scene, mirroring)
	/// are handled by properties of ascendant GameObject's
	/// </summary>
	private static Vector3 ConvertKinectPos(Float3 pos)
	{
		return 0.001f * new Vector3(pos.X, -pos.Y, pos.Z);
	}

	/// <summary>
	/// Disable skeleton root
	/// </summary>
	private void HideSkeleton()
	{
		root.SetActive(false);
	}
	#endregion
	#endregion
}