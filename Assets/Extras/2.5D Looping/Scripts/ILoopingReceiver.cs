using UnityEngine;

namespace Kamgam.Looping25D
{
    public interface ILoopingReceiver
    {
        bool IsValid();
        Vector3 GetPosition();
        void SetPositionZ(float posZ);
    }
}