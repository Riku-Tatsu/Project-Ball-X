#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Kamgam.Looping25D.Helpers
{
    /// <summary>
    /// It will not trigger the EXIT event if the other object is destroyed or the collider is disabled.
    /// See: https://issuetracker.unity3d.com/issues/ontriggerexit2d-is-not-happening-when-other-colliding-object-is-beeing-destroyed?_ga=2.117158598.1137689064.1585670833-1663437565.1574337011
    ///      https://answers.unity.com/questions/396179/ontriggerexit-not-called-on-destroy.html
    /// </summary>
    public class Trigger2D : MonoBehaviour
    {
        public MonoBehaviour TriggerReceiver;

        [Tooltip("Only trigger colliders in those layers will be taken into account.")]
        public LayerMask TriggerLayers = ~0; // default to everything

        [Tooltip("The trigger will only execute if the colliding GameObject.name has the specified value (leave empty to disable). Especially useful for overlapping triggers.")]
        public string NameMatchFilter = "";

        [Tooltip("Usually the trigger sends one ENTER event for each collider entering it, even if another collider has already entered (same for exit). Set this TRUE to trigger ENTER only if no other collider is currently intersecting. EXIT will also only trigger once the last foreign collider has left. STAY events will not be affected. Beware to not destroy the other objects during collision or make sure you call Reset() after destruction.")]
        public bool BehaveLikeAnArea = false;

        [Tooltip("Useful if you spawn this trigger at a position intersecting a target. Normally this would trigger the ENTER event immediately. Set to true to prevent it (will reset to false after the ENTER event). This does not affect EXIT or STAY events, those will trigger as usual.")]
        public bool IgnoreNextEnter = false;

        [System.NonSerialized]
        public int NumOfIntersectingTriggers = 0;

        void OnTriggerEnter2D(Collider2D other)
        {
            // Triggers are collider based and thus updated in fixedUpdate.
            // We stop here to avoid handling events on disabled triggers.
            if (this.isActiveAndEnabled == false)
            {
                return;
            }
            if (TriggerReceiver != null)
            {
                if (0 != (TriggerLayers.value & 1 << other.gameObject.layer))
                {
                    if (NameMatchFilter.Length == 0 || string.Compare(other.gameObject.name, NameMatchFilter) == 0)
                    {
                        NumOfIntersectingTriggers++;
                        if (BehaveLikeAnArea == false || NumOfIntersectingTriggers == 1)
                        {
                            if (IgnoreNextEnter == false)
                            {
                                (TriggerReceiver as ITrigger2DReceiver).OnCustomTriggerEnter2D(this, other);
                            }
                            IgnoreNextEnter = false;
                        }
                    }
                }
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (this.isActiveAndEnabled == false)
            {
                return;
            }
            if (TriggerReceiver != null)
            {
                if (0 != (TriggerLayers.value & 1 << other.gameObject.layer))
                {
                    if (NameMatchFilter.Length == 0 || string.Compare(other.gameObject.name, NameMatchFilter) == 0)
                    {
                        (TriggerReceiver as ITrigger2DReceiver).OnCustomTriggerStay2D(this, other);
                    }
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (this.isActiveAndEnabled == false)
            {
                return;
            }
            if (TriggerReceiver != null)
            {
                if (0 != (TriggerLayers.value & 1 << other.gameObject.layer))
                {
                    if (NameMatchFilter.Length == 0 || string.Compare(other.gameObject.name, NameMatchFilter) == 0)
                    {
                        NumOfIntersectingTriggers = Mathf.Max(0, NumOfIntersectingTriggers - 1); // avoid negative values
                                                                                                 //Debug.Log( this.isActiveAndEnabled + "|" + this.transform.gameObject + ": " + NumOfIntersectingTriggers + " <   " + (NumOfIntersectingTriggers+1) + " " + other.gameObject.name);
                        if (BehaveLikeAnArea == false || NumOfIntersectingTriggers == 0)
                        {
                            (TriggerReceiver as ITrigger2DReceiver).OnCustomTriggerExit2D(this, other);
                        }
                    }
                }
            }
        }

        private void OnDisable()
        {
            NumOfIntersectingTriggers = 0;
        }

        public void Reset()
        {
            NumOfIntersectingTriggers = 0;
            IgnoreNextEnter = false;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                // auto assign trigger receiver
                if (TriggerReceiver == null)
                {
                    var receiverSelf = this.GetComponent<ITrigger2DReceiver>();
                    if (receiverSelf != null)
                    {
                        TriggerReceiver = receiverSelf as MonoBehaviour;
                    }
                }
                if (TriggerReceiver == null && this.transform.parent != null)
                {
                    var receiverParent = this.transform.parent.GetComponent<ITrigger2DReceiver>();
                    if (receiverParent != null)
                    {
                        TriggerReceiver = receiverParent as MonoBehaviour;
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1f, 0, 0.5f);
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(transform.position, 0.4f);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, 0.4f);
            }
        }
#endif
    }
}