using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;

namespace OrchidSeal.CameraOverride
{
    // Override the screen camera with in-world cameras, which provides access to render textures.
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OverrideCamera : UdonSharpBehaviour
    {
        [SerializeField] private Camera cameraLeft;
        [SerializeField] private Camera cameraRight;
        // The projection matrix in VR is headset-specific! And VRChat doesn't provide a way to get
        // this through VRCCameraSettings. But cameras in the world do get the correct projection
        // matrix. So this dummy camera is used to get that data.
        [SerializeField] private Camera dummyCamera;
        [SerializeField] private Material cameraOverrideMaterial;
        [SerializeField] private Shader replacementShader;
        [SerializeField] private UnityEngine.UI.RawImage debugLeftEyeImage;
        private Transform cameraTransformLeft;
        private Transform cameraTransformRight;

        // OnWillRenderObject is only called if this script is on the same object as a Renderer.
        private void OnWillRenderObject()
        {
            VRCCameraSettings.GetCurrentCamera(out var internalCamera, out var externalCamera);
            var screenCamera = VRCCameraSettings.ScreenCamera;

            // OnWillRenderObject will be called for any camera, but we only want to override the screen.
            if (internalCamera != screenCamera) return;
            
            cameraLeft.allowHDR = screenCamera.AllowHDR;
            cameraLeft.aspect = screenCamera.Aspect;
            cameraLeft.backgroundColor = screenCamera.BackgroundColor;
            cameraLeft.clearFlags = screenCamera.ClearFlags;
            cameraLeft.farClipPlane = screenCamera.FarClipPlane;
            cameraLeft.fieldOfView = screenCamera.FieldOfView;
            cameraLeft.nearClipPlane = screenCamera.NearClipPlane;
            cameraLeft.useOcclusionCulling = screenCamera.UseOcclusionCulling;

            if (screenCamera.StereoEnabled)
            {
                cameraRight.allowHDR = screenCamera.AllowHDR;
                cameraRight.aspect = screenCamera.Aspect;
                cameraRight.backgroundColor = screenCamera.BackgroundColor;
                cameraRight.clearFlags = screenCamera.ClearFlags;
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
                if (replacementShader)
                {
                    cameraLeft.RenderWithShader(replacementShader, "RenderType");
                    cameraRight.RenderWithShader(replacementShader, "RenderType");
                }
                else
                {
                    cameraLeft.Render();
                    cameraRight.Render();
                }
            }
            else
            {
                cameraTransformLeft.SetPositionAndRotation(screenCamera.Position, screenCamera.Rotation);
                cameraLeft.ResetProjectionMatrix();
                if (replacementShader)
                {
                    cameraLeft.RenderWithShader(replacementShader, "RenderType");
                }
                else
                {
                    cameraLeft.Render();
                }
            }
        }

        private void Start()
        {
            cameraTransformLeft = cameraLeft.transform;
            cameraTransformRight = cameraRight.transform;
            
            var screenCamera = VRCCameraSettings.ScreenCamera;
            const int depthBits = 32;
            var colorFormat = screenCamera.AllowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            var leftEyeTexture = new RenderTexture(screenCamera.PixelWidth, screenCamera.PixelHeight, depthBits, colorFormat);
            cameraLeft.targetTexture = leftEyeTexture;
            cameraOverrideMaterial.SetTexture("_CameraLeftEyeTexture", leftEyeTexture);
            if (debugLeftEyeImage) debugLeftEyeImage.texture = cameraLeft.targetTexture;

            if (screenCamera.StereoEnabled)
            {
                var rightEyeTexture = new RenderTexture(screenCamera.PixelWidth, screenCamera.PixelHeight, depthBits, colorFormat);
                cameraRight.targetTexture = rightEyeTexture;
                cameraOverrideMaterial.SetTexture("_CameraRightEyeTexture", rightEyeTexture);
            }
        }

        private void Update()
        {
            // Place the quad in front of the player so it triggers OnWillRenderObject. The
            // positioning isn't very important because the shader does a separate calculation to
            // place it more precisely over the screen.
            transform.SetPositionAndRotation(cameraTransformLeft.position + cameraTransformLeft.forward, cameraTransformLeft.rotation);
        }
    }
}
