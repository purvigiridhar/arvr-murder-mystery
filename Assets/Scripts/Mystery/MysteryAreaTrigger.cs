using UnityEngine;

namespace MurderMystery
{
    [RequireComponent(typeof(Collider))]
    public sealed class MysteryAreaTrigger : MonoBehaviour
    {
        private MysteryManager mysteryManager;
        private MysteryLocation location;

        public void Initialize(
            MysteryManager manager,
            MysteryLocation areaLocation)
        {
            mysteryManager = manager;
            location = areaLocation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mysteryManager == null)
            {
                Debug.LogError(
                    $"MysteryAreaTrigger on '{name}' is not initialized.",
                    this);
                enabled = false;
                return;
            }

            if (other.GetComponentInParent<FirstPersonController>() != null)
                mysteryManager.EnterLocation(location);
        }

        private void OnTriggerExit(Collider other)
        {
            if (mysteryManager != null &&
                other.GetComponentInParent<FirstPersonController>() != null)
            {
                mysteryManager.ExitLocation(location);
            }
        }
    }
}
