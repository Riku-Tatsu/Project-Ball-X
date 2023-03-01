using UnityEngine;
using Kamgam.Looping25D.Helpers;

namespace Kamgam.Looping25D
{
    /// <summary>
    /// A fairly simple camera follow script.
    /// </summary>
    public class Cameraman2D : MonoBehaviour
    {
        [Tooltip("Base camera offset while standing still.")]
        public Vector3 Offset = new Vector3(0f, 1.0f, -5f);

        [Tooltip("X = max offset due to zoom on the Y-axis.\nY = max offset due to zoom on the Z-axis.")]
        public Vector2 SpeedZoomOffset = new Vector2(2.5f, 5f);

        [Tooltip("X = min (at which the zooming starts), Y = max (at which the zooming is capped)")]
        public Vector2 SpeedZoomMinMaxVelocity = new Vector2(0, 13f);

        /// <summary>
        /// Used to verage the velocity of the objectToTrack. Using the velocity directly would be too "jittery".
        /// </summary>
        protected Vector2AverageQueue velocityAverage = new Vector2AverageQueue(30);

        protected Transform cameraToMove;
        protected Rigidbody2D objectToTrack;

        public void SetCameraToMove(Transform obj)
        {
            this.cameraToMove = obj;
        }

        public void SetObjectToTrack(Rigidbody2D obj)
        {
            this.objectToTrack = obj;
        }
        
        void LateUpdate()
        {
            // update average velocity
            velocityAverage.Enqueue(objectToTrack.velocity);
            velocityAverage.UpdateAverage();

            // calc final offset based on the velocity average.
            var offset = Offset;
            float delta = SpeedZoomMinMaxVelocity.y - SpeedZoomMinMaxVelocity.x;
            float zoomFactor = (velocityAverage.Average().magnitude - SpeedZoomMinMaxVelocity.x) / delta;
            offset.y += zoomFactor * SpeedZoomOffset.x;
            offset.z -= zoomFactor * SpeedZoomOffset.y;

            // move camera
            if (objectToTrack != null && cameraToMove != null)
            {
                // use LateUpdate for tracking if the rigidbody is interpolating
                if (objectToTrack.interpolation != RigidbodyInterpolation2D.None)
                    cameraToMove.position = objectToTrack.transform.position + offset;
            }
        }

        // Use this if you are not using interpolation on your physics objects.
        // Don't know what I am talking about? Then read this:
        // https://forum.unity.com/threads/camera-following-rigidbody.171343/#post-2491001
        /*
        Vector3 tmpPos = Vector3.zero;
        private void FixedUpdate()
        {
            if (objectToTrack != null && cameraToMove != null)
            {
                // use FixedUpdate for tracking if the rigidbody is not interpolating
                if (objectToTrack.interpolation == RigidbodyInterpolation2D.None)
                {
                    tmpPos.x = objectToTrack.position.x;
                    tmpPos.y = objectToTrack.position.y;
                    tmpPos.z = objectToTrack.transform.position.z;

                    cameraToMove.position = tmpPos + Offset;
                }
            }
        }*/
    }
}
