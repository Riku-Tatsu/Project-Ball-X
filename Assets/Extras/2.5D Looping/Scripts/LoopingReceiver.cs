using UnityEngine;

namespace Kamgam.Looping25D
{
    /// <summary>
    /// The looping receiver is an example implementation of the ILoopingReceiver.
    /// The Looping25D calculates the z position for all ILoopingReceivers nearby
    /// and sets it by calling SetPosition(newPosition).
    /// </summary>
    public class LoopingReceiver : MonoBehaviour, ILoopingReceiver
    {
        public bool IsValid()
        {
            return this != null && gameObject != null;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void SetPositionZ(float posZ)
        {
            var pos = transform.position;
                pos.z = posZ;
            transform.position = pos;
        }
    }
}