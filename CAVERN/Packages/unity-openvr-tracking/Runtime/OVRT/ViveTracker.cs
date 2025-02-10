using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Spelunx.OVRT
{
    public sealed class ViveTracker : OVRT_TrackedDevice
    {
        // an enum representing the possible bindings a vive tracker can be assigned to
        // The tracker -> binding assignment should be done on the CAVERN computer
        public enum SteamVRPoseBindings
        {

            [InspectorName("None")]
            TrackerRole_None,
            [InspectorName("Handed")]
            TrackerRole_Handed, // TODO: Check that this works
            [InspectorName("Any Hand")]
            TrackerControllerRole_Invalid, // TODO: Check that this works
            [InspectorName("Left Hand")]
            TrackerControllerRole_LeftHand, // TODO: Check that this works
            [InspectorName("Right Hand")]
            TrackerControllerRole_RightHand, // TODO: Check that this works
            [InspectorName("Left Foot")]
            TrackerRole_LeftFoot,
            [InspectorName("Right Foot")]
            TrackerRole_RightFoot,
            [InspectorName("Left Shoulder")]
            TrackerRole_LeftShoulder,
            [InspectorName("Right Shoulder")]
            TrackerRole_RightShoulder,
            [InspectorName("Left Elbow")]
            TrackerRole_LeftElbow,
            [InspectorName("Right Elbow")]
            TrackerRole_RightElbow,
            [InspectorName("Left Knee")]
            TrackerRole_LeftKnee,
            [InspectorName("Right Knee")]
            TrackerRole_RightKnee,
            [InspectorName("Left Wrist")]
            TrackerRole_LeftWrist,
            [InspectorName("Right Wrist")]
            TrackerRole_RightWrist,
            [InspectorName("Left Ankle")]
            TrackerRole_LeftAnkle,
            [InspectorName("Right Ankle")]
            TrackerRole_RightAnkle,
            [InspectorName("Wrist")]
            TrackerRole_Wrist,
            [InspectorName("Chest")]
            TrackerRole_Chest,
            [InspectorName("Camera")]
            TrackerRole_Camera,
            [InspectorName("Keyboard")]
            TrackerRole_Keyboard,

        }

        [Tooltip("Specify a binding from SteamVR for this tracker.")]
        public SteamVRPoseBindings binding;

        [Tooltip("If not set, relative to parent")]

        private UnityAction<string, TrackedDevicePose_t, int> _onNewBoundPoseAction;
        private UnityAction _onTrackerRolesChanged;

        private Quaternion rotationAlignment = Quaternion.identity;

        private void OnDeviceConnected(int index, bool connected)
        {
            if (DeviceIndex == index && !connected)
            {
                IsConnected = false;
            }
        }

        private void OnNewBoundPose(string binding, TrackedDevicePose_t pose, int deviceIndex)
        {
            if (this.binding.ToString() != binding)
                return;

            IsValid = false;

            if (DeviceIndex != deviceIndex)
            {
                DeviceIndex = deviceIndex;
                onDeviceIndexChanged.Invoke(DeviceIndex);
            }
            IsConnected = pose.bDeviceIsConnected;

            if (!pose.bDeviceIsConnected)
                return;

            if (!pose.bPoseIsValid)
                return;

            IsValid = true;

            var rigidTransform = new OVRT.OVRT_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(rigidTransform.pos);
                if (Input.GetKey(KeyCode.C))
                {
                    // calibrate
                    rotationAlignment = Quaternion.Inverse(origin.rotation * rigidTransform.rot);
                }
                transform.rotation = origin.rotation * rigidTransform.rot * rotationAlignment;
            }
            else
            {
                transform.localPosition = rigidTransform.pos;
                transform.localRotation = rigidTransform.rot;
            }
        }

        private void OnTrackerRolesChanged()
        {
            IsValid = false;
            IsConnected = false;
        }

        private void OnButtonPressed(int deviceIndex, EVRButtonId button, bool pressed)
        {
            Debug.Log($"{deviceIndex}\t{button}\t{pressed}");
        }

        private void Awake()
        {
            _onNewBoundPoseAction += OnNewBoundPose;
            _onDeviceConnectedAction += OnDeviceConnected;
            _onTrackerRolesChanged += OnTrackerRolesChanged;
        }

        private void OnEnable()
        {
            OVRT.OVRT_Events.NewBoundPose.AddListener(_onNewBoundPoseAction);
            OVRT.OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnectedAction);
            OVRT.OVRT_Events.TrackerRolesChanged.AddListener(_onTrackerRolesChanged);
            OVRT.OVRT_Events.ButtonPressed.AddListener(OnButtonPressed);
        }

        private void OnDisable()
        {
            OVRT.OVRT_Events.NewBoundPose.RemoveListener(_onNewBoundPoseAction);
            OVRT.OVRT_Events.TrackedDeviceConnected.RemoveListener(_onDeviceConnectedAction);
            OVRT.OVRT_Events.TrackerRolesChanged.RemoveListener(_onTrackerRolesChanged);
            OVRT.OVRT_Events.ButtonPressed.RemoveListener(OnButtonPressed);
            IsValid = false;
            IsConnected = false;
        }
    }
}