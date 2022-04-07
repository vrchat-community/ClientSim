
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    // Listens to Events:
    // - ClientSimOnTrackingScaleUpdateEvent
    // TODO split into local and remote versions
    [AddComponentMenu("")]
    public class ClientSimPlayerAvatarManager : ClientSimBehaviour, IClientSimPlayerAvatarDataProvider
    {
        [SerializeField] 
        private Animator avatarAnimator;
        
        private IClientSimEventDispatcher _eventDispatcher;
        
        // TODO initialize with option for Generic or Humanoid
        // TODO better initialization options for Local vs Remote
        public void Initialize(IClientSimEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            
            _eventDispatcher.Subscribe<ClientSimOnTrackingScaleUpdateEvent>(OnTrackingScaleUpdate);
        }

        private void OnDestroy()
        {
            _eventDispatcher?.Unsubscribe<ClientSimOnTrackingScaleUpdateEvent>(OnTrackingScaleUpdate);
        }

        #region ClientSim Events

        private void OnTrackingScaleUpdate(ClientSimOnTrackingScaleUpdateEvent trackingEvent)
        {
            transform.localScale = trackingEvent.trackingScale * Vector3.one;
        }

        #endregion

        #region IClientSimPlayerAvatarDataProvider

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            if (avatarAnimator == null)
            {
                return null;
            }
            
            return avatarAnimator.GetBoneTransform(bone);
        }
        
        public Quaternion GetBoneRotation(HumanBodyBones bone)
        {
            if (avatarAnimator == null)
            {
                return Quaternion.identity;
            }

            Transform boneTransform = GetBoneTransform(bone);
            return boneTransform ? boneTransform.rotation : Quaternion.identity;
        }

        public Vector3 GetBonePosition(HumanBodyBones bone)
        {
            if (avatarAnimator == null)
            {
                return Vector3.zero;
            }
            
            Transform boneTransform = GetBoneTransform(bone);
            return boneTransform ? boneTransform.position : Vector3.zero;
        }

        #endregion
    }
}