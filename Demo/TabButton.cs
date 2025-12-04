using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace OrchidSeal.CameraOverride
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TabButton : UdonSharpBehaviour
    {
        [SerializeField] private GameObject[] objectsToEnable;
        [SerializeField] private GameObject[] objectsToDisable;
        
        [PublicAPI]
        public void _OnChangeValue()
        {
            foreach (var gameObj in objectsToEnable)
            {
                gameObj.SetActive(true);
            }
            
            foreach (var gameObj in objectsToDisable)
            {
                gameObj.SetActive(false);
            }
        }
    }
}