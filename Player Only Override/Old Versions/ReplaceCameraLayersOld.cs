using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;

namespace OrchidSeal.CameraOverride
{
    // Replace some layers of the screen camera with a separate camera using camera stacking.
    // (Camera.depth) This version uses two cameras and a dummy camera, which seems to be
    // unnecessary. Instead, use OrchidSeal.CameraOverride.ReplaceCameraLayers
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ReplaceCameraLayersOld : UdonSharpBehaviour
    {
        [SerializeField] private Camera cameraLeft;
        [SerializeField] private Camera cameraRight;
        // The projection matrix in VR is headset-specific! And VRChat doesn't provide a way to get
        // this through VRCCameraSettings. But cameras in the world do get the correct projection
        // matrix. So this dummy camera is used to get that data.
        [SerializeField] private Camera dummyCamera;
        [SerializeField] private Shader replacementShader;
        private Transform cameraTransformLeft;
        private Transform cameraTransformRight;

        // OnPreRender is only called if this script is on the same object as a Camera.
        private void OnPreRender()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            cameraLeft.allowHDR = screenCamera.AllowHDR;
            cameraLeft.aspect = screenCamera.Aspect;
            cameraLeft.farClipPlane = screenCamera.FarClipPlane;
            cameraLeft.fieldOfView = screenCamera.FieldOfView;
            cameraLeft.nearClipPlane = screenCamera.NearClipPlane;
            cameraLeft.useOcclusionCulling = screenCamera.UseOcclusionCulling;

            if (screenCamera.StereoEnabled)
            {
                cameraRight.allowHDR = screenCamera.AllowHDR;
                cameraRight.aspect = screenCamera.Aspect;
                cameraRight.farClipPlane = screenCamera.FarClipPlane;
                cameraRight.fieldOfView = screenCamera.FieldOfView;
                cameraRight.nearClipPlane = screenCamera.NearClipPlane;
                cameraRight.useOcclusionCulling = screenCamera.UseOcclusionCulling;
                
                cameraTransformLeft.SetPositionAndRotation(
                    VRCCameraSettings.GetEyePosition(Camera.StereoscopicEye.Left),
                    VRCCameraSettings.GetEyeRotation(Camera.StereoscopicEye.Left));
                cameraTransformRight.SetPositionAndRotation(
                    VRCCameraSettings.GetEyePosition(Camera.StereoscopicEye.Right),
                    VRCCameraSettings.GetEyeRotation(Camera.StereoscopicEye.Right));
                
                dummyCamera.aspect = screenCamera.Aspect;
                dummyCamera.farClipPlane = screenCamera.FarClipPlane;
                dummyCamera.fieldOfView = screenCamera.FieldOfView;
                dummyCamera.nearClipPlane = screenCamera.NearClipPlane;
                cameraLeft.projectionMatrix = dummyCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                cameraRight.projectionMatrix = dummyCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            }
            else
            {
                cameraTransformLeft.SetPositionAndRotation(screenCamera.Position, screenCamera.Rotation);
                cameraLeft.ResetProjectionMatrix();
            }
        }

        private void OnEnable()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            screenCamera.CullingMask = screenCamera.CullingMask & ~cameraLeft.cullingMask;
        }

        private void OnDisable()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            screenCamera.CullingMask = screenCamera.CullingMask | cameraLeft.cullingMask;
        }

        private void Start()
        {
            cameraTransformLeft = cameraLeft.transform;
            cameraTransformRight = cameraRight.transform;

            if (replacementShader)
            {
                cameraLeft.SetReplacementShader(replacementShader, "RenderType");
                cameraRight.SetReplacementShader(replacementShader, "RenderType");
            }

            cameraLeft.enabled = true;

            if (VRCCameraSettings.ScreenCamera.StereoEnabled)
            {
                cameraRight.enabled = true;
            }
        }
    }
}
