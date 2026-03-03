using UnityEngine;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Sits on the same GameObject as the Animator (e.g. SpeechBubble) and relays
    /// Animation Events up to the NegotiationPanel on a parent object.
    ///
    /// Unity Animation Events can only call methods on components that share the
    /// same GameObject as the Animator, so this thin bridge is the standard pattern
    /// for child-object animators.
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        private NegotiationPanel _negotiationPanel;

        private void Awake()
        {
            _negotiationPanel = GetComponentInParent<NegotiationPanel>();

            if (_negotiationPanel == null)
                Debug.LogWarning("[AnimationEventRelay] No NegotiationPanel found in parent hierarchy.", this);
        }

        /// <summary>
        /// Wire this to the Animation Event on the last frame of the reaction clip.
        /// </summary>
        public void OnReactionAnimationComplete()
        {
            _negotiationPanel?.OnReactionAnimationComplete();
        }
    }
}
