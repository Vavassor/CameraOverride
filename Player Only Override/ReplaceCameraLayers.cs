using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;

namespace OrchidSeal.CameraOverride
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ReplaceCameraLayers : UdonSharpBehaviour
    {
        [SerializeField] private Camera overrideCamera;
        [SerializeField] private Shader replacementShader;
        private Transform overrideCameraTransform;
        
        private void OnDisable()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            screenCamera.CullingMask = screenCamera.CullingMask | overrideCamera.cullingMask;
        }
        
        private void OnEnable()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            screenCamera.CullingMask = screenCamera.CullingMask & ~overrideCamera.cullingMask;
        }
        
        // OnPreRender is only called if this script is on the same object as a Camera.
        private void OnPreRender()
        {
            var screenCamera = VRCCameraSettings.ScreenCamera;
            overrideCamera.allowHDR = screenCamera.AllowHDR;
            overrideCamera.aspect = screenCamera.Aspect;
            overrideCamera.farClipPlane = screenCamera.FarClipPlane;
            overrideCamera.fieldOfView = screenCamera.FieldOfView;
            overrideCamera.nearClipPlane = screenCamera.NearClipPlane;
            overrideCamera.useOcclusionCulling = screenCamera.UseOcclusionCulling;
            overrideCameraTransform.SetPositionAndRotation(screenCamera.Position, screenCamera.Rotation);
            overrideCamera.ResetProjectionMatrix();
        }

        private void Start()
        {
            overrideCameraTransform = overrideCamera.transform;
            
            if (replacementShader)
            {
                overrideCamera.SetReplacementShader(replacementShader, "RenderType");
            }

            overrideCamera.enabled = true;
        }
    }
}
