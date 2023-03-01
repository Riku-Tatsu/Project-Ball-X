using Kamgam.Looping25D.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Looping25D
{
    /// <summary>
    /// The looping will detect any object with a ILooingReceiver which enters the 
    /// ControlZPositionTriggerArea trigger. Once and object has enterd the Looping will
    /// calculate a z position based on the objects current x/y position. This is then set
    /// via the SetPositionZ().
    /// <br /><br/>
    /// Left to Right Colliders are activated by default or if an object enters the
    /// LeftToRightColliderTriggers. The same logic applies to the RightToLeftColliderTriggers.
    /// <br /><br/>
    /// The ZFieldPointsInside and ZFieldPointsOutside exist to calculate the z position. The
    /// closer and object is to the ZField the closer it will get to the fields z pos.
    /// </summary>
    public class Looping25D : MonoBehaviour, ITrigger2DReceiver
    {
        public Trigger2D ControlZPositionTriggerArea;

        public Trigger2D LeftToRightColliderTriggerA;
        public Trigger2D LeftToRightColliderTriggerB;

        public Trigger2D RightToLeftColliderTriggerA;
        public Trigger2D RightToLeftColliderTriggerB;

        public PolygonCollider2D LeftToRightCollider;
        public PolygonCollider2D RightToLeftCollider;

        public List<Transform> ZFieldPointsInside
        {
            get
            {
                return IsLeftToRight ?
                    ZFieldPointsInsideLeftToRight :
                    ZFieldPointsInsideRightToLeft;
            }
        }

        public bool IsLeftToRight
        {
            get => LeftToRightCollider.enabled;
        }

        [Tooltip("The inner z field points if moving from left to right.")]
        public List<Transform> ZFieldPointsInsideLeftToRight = new List<Transform>();

        [Tooltip("The inner z field points if moving from right to left.")]
        public List<Transform> ZFieldPointsInsideRightToLeft = new List<Transform>();

        public List<Transform> ZFieldPointsOutside = new List<Transform>();

        /// <summary>
        /// The Receiver is the object being controlled and the InitialZPos (float)
        /// is the z pos which the object had before the looping started controlling it.
        /// </summary>
        protected class ControlledObject
        {
            public ILoopingReceiver Receiver;
            public float InitialZPos;

            public ControlledObject(ILoopingReceiver receiver)
            {
                Receiver = receiver;
                InitialZPos = receiver.GetPosition().z;
            }

            public void SetPositionZ(float z)
            {
                Receiver.SetPositionZ(z);
            }

            public void ResetPositionZ()
            {
                Receiver.SetPositionZ(InitialZPos);
            }
        }

        protected List<ControlledObject> zControlledObjects = new List<ControlledObject>();

        protected bool tmpWasSecondColliderEnabledAtCheckpoint;

        public void OnCheckpointReached()
        {
            tmpWasSecondColliderEnabledAtCheckpoint = RightToLeftCollider.enabled;
        }

        protected void resetControlledZ()
        {
            if (zControlledObjects.Count > 0)
            {
                foreach (var obj in zControlledObjects)
                    obj.ResetPositionZ();
                zControlledObjects.Clear();
            }
        }

        public void ResetState(bool restartFromBeginning)
        {
            resetControlledZ();

            if (restartFromBeginning)
            {
                LeftToRightCollider.enabled = true;
                RightToLeftCollider.enabled = false;
            }
            else
            {
                LeftToRightCollider.enabled = !tmpWasSecondColliderEnabledAtCheckpoint;
                RightToLeftCollider.enabled = tmpWasSecondColliderEnabledAtCheckpoint;
            }
        }

        public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
        {
            // triggers
            if (trigger == LeftToRightColliderTriggerA)
            {
                LeftToRightColliderTriggerB.gameObject.SetActive(false);
                RightToLeftColliderTriggerB.gameObject.SetActive(true);
            }
            else if (trigger == RightToLeftColliderTriggerA)
            {
                LeftToRightColliderTriggerB.gameObject.SetActive(true);
                RightToLeftColliderTriggerB.gameObject.SetActive(false);
            }

            // colliders
            if (trigger == LeftToRightColliderTriggerA || trigger == LeftToRightColliderTriggerB)
            {
                LeftToRightCollider.enabled = true;
                RightToLeftCollider.enabled = false;
            }
            else if (trigger == RightToLeftColliderTriggerA || trigger == RightToLeftColliderTriggerB)
            {
                LeftToRightCollider.enabled = false;
                RightToLeftCollider.enabled = true;
            }

            if (trigger == ControlZPositionTriggerArea)
            {
                var receiver = getReceiverFromCollider(other);
                if (receiver != null && !IsControlling(receiver))
                {
                    zControlledObjects.Add(
                        new ControlledObject(receiver)
                        );
                }
            }
        }

        public bool IsControlling(ILoopingReceiver receiver)
        {
            if (zControlledObjects.Count <= 0)
                return false;

            foreach (var obj in zControlledObjects)
                if (obj.Receiver == receiver)
                    return true;

            return false;
        }

        protected ControlledObject getControlled(ILoopingReceiver receiver)
        {
            if (zControlledObjects.Count <= 0)
                return null;

            foreach (var obj in zControlledObjects)
                if (obj.Receiver == receiver)
                    return obj;

            return null;
        }

        public void RemoveControlled(ILoopingReceiver receiver)
        {
            if (zControlledObjects.Count <= 0)
                return;

            for (int i = zControlledObjects.Count-1; i >= 0; i--)
            {
                if (zControlledObjects[i].Receiver == receiver)
                    zControlledObjects.RemoveAt(i);
            }
        }

        protected ILoopingReceiver getReceiverFromCollider(Collider2D collider)
        {
            var receiver = collider.GetComponentInChildren<ILoopingReceiver>();

            // Not found in children? Then search in parent.
            if (receiver == null)
            {
                receiver = collider.GetComponentInParent<ILoopingReceiver>();
            }

            return receiver;
        }

        public void OnCustomTriggerExit2D(Trigger2D trigger, Collider2D other)
        {
            if (trigger == ControlZPositionTriggerArea)
            {
                var receiver = getReceiverFromCollider(other);
                if (receiver != null)
                {
                    var controlled = getControlled(receiver);
                    if (controlled != null)
                        controlled.ResetPositionZ();

                    RemoveControlled(receiver);
                }
            }
        }

        public void OnCustomTriggerStay2D(Trigger2D trigger, Collider2D other)
        {
        }

        public void Update()
        {
            if (zControlledObjects != null && zControlledObjects.Count > 0)
            {
                removeDeletedReceivers();

                foreach (var obj in zControlledObjects)
                {
                    float positionZ = getZValue(obj.Receiver.GetPosition());
                    obj.SetPositionZ(positionZ);
                }
            }
        }

        protected void removeDeletedReceivers()
        {
            for (int i = zControlledObjects.Count - 1; i >= 0; i--)
            {
                if (zControlledObjects[i] == null || zControlledObjects[i].Receiver == null || !zControlledObjects[i].Receiver.IsValid())
                {
                    zControlledObjects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Calculates the new z position according to the current x/y position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected float getZValue(Vector3 position)
        {
            float sqrDistance = Mathf.Infinity;
            Vector3 closestPoint = Vector3.zero;
            Vector3 tmp;
            float tmpSqrDistance;
            if (ZFieldPointsInside != null && ZFieldPointsInside.Count > 0)
            {
                for (int i = 0; i < ZFieldPointsInside.Count - 1; i++)
                {
                    tmp = closestPointOnLine(ZFieldPointsInside[i].position, ZFieldPointsInside[i + 1].position, position);
                    tmpSqrDistance = Vector3.SqrMagnitude(tmp - position);
                    if (tmpSqrDistance < sqrDistance)
                    {
                        sqrDistance = tmpSqrDistance;
                        closestPoint = tmp;
                    }
                }
                for (int i = 0; i < ZFieldPointsOutside.Count - 1; i++)
                {
                    tmp = closestPointOnLine(ZFieldPointsOutside[i].position, ZFieldPointsOutside[i + 1].position, position);
                    tmpSqrDistance = Vector3.SqrMagnitude(tmp - position);
                    if (tmpSqrDistance < sqrDistance)
                    {
                        sqrDistance = tmpSqrDistance;
                        closestPoint = tmp;
                    }
                }
            }

            return closestPoint.z;
        }

        protected Vector3 closestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            var vVector1 = vPoint - vA;
            var vVector2 = (vB - vA).normalized;

            var d = Vector3.Distance(vA, vB);
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return vA;

            if (t >= d)
                return vB;

            return vA + vVector2 * t;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (ZFieldPointsInside != null && ZFieldPointsInside.Count > 0)
            {
                if (!UnityEditor.EditorApplication.isPlaying || IsLeftToRight)
                {
                    if (ZFieldPointsInsideLeftToRight.Count > 0)
                    {
                        Gizmos.color = Color.blue;
                        for (int i = 0; i < ZFieldPointsInsideLeftToRight.Count - 1; i++)
                        {
                            Gizmos.DrawLine(ZFieldPointsInsideLeftToRight[i].position, ZFieldPointsInsideLeftToRight[i + 1].position);
                            Gizmos.DrawSphere(ZFieldPointsInsideLeftToRight[i].position, 0.2f);
                        }
                        Gizmos.DrawSphere(ZFieldPointsInsideLeftToRight[ZFieldPointsInsideLeftToRight.Count - 1].position, 0.2f);
                    }
                }

                if (!UnityEditor.EditorApplication.isPlaying || !IsLeftToRight)
                {
                    if (ZFieldPointsInsideRightToLeft.Count > 0)
                    {
                        Gizmos.color = Color.cyan;
                        for (int i = 0; i < ZFieldPointsInsideRightToLeft.Count - 1; i++)
                        {
                            Gizmos.DrawLine(ZFieldPointsInsideRightToLeft[i].position, ZFieldPointsInsideRightToLeft[i + 1].position);
                            Gizmos.DrawSphere(ZFieldPointsInsideRightToLeft[i].position, 0.2f);
                        }
                        Gizmos.DrawSphere(ZFieldPointsInsideRightToLeft[ZFieldPointsInsideRightToLeft.Count - 1].position, 0.2f);
                    }
                }
            }

            if (ZFieldPointsOutside != null && ZFieldPointsOutside.Count > 0)
            {
                Gizmos.color = Color.white;
                if (ZFieldPointsOutside.Count > 0)
                {
                    for (int i = 0; i < ZFieldPointsOutside.Count - 1; i++)
                    {
                        if (ZFieldPointsOutside[i].gameObject.activeSelf)
                        {
                            Gizmos.DrawLine(ZFieldPointsOutside[i].position, ZFieldPointsOutside[i + 1].position);
                            Gizmos.DrawSphere(ZFieldPointsOutside[i].position, 0.2f);
                        }
                    }
                    Gizmos.DrawSphere(ZFieldPointsOutside[ZFieldPointsOutside.Count - 1].position, 0.2f);
                }
            }
        }
#endif
    }
}
