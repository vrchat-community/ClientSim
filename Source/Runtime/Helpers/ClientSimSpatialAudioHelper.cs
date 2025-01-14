using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class ClientSimSpatialAudioHelper : ONSPAudioSource
    {
        private const float EPS = 1e-3f;

        private VRC_SpatialAudioSource _spatialAudioSource;
        private AudioSource _audioSource;
        private bool _useAudioSourceCurve;
        private ONSPAudioSource _onsp;
        private bool _forceUpdate = true;
        private bool _updateONSPParams;

        public static void InitializeAudio(VRC_SpatialAudioSource obj)
        {
#if UNITY_EDITOR
            // VRC_SpatialAudioSource executes in editor, meaning it will try to initialize even outside of playmode.
            // This code is to prevent adding the ClientSim helper in these cases.
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || 
                UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(obj))
            {
                return;
            }
#endif
            
            ClientSimSpatialAudioHelper spatialAudio = obj.GetComponent<ClientSimSpatialAudioHelper>();
            if (spatialAudio != null)
            {
                DestroyImmediate(spatialAudio);
            }
            
            spatialAudio = obj.gameObject.AddComponent<ClientSimSpatialAudioHelper>();
            spatialAudio.PreventComponentFromSaving();
            spatialAudio.SetSpatializer(obj);
        }

        private void SetSpatializer(VRC_SpatialAudioSource obj)
        {
            _spatialAudioSource = obj;
            _audioSource = GetComponent<AudioSource>();
            _onsp = this;
            _forceUpdate = true;

            UpdateSettings();
        }

        private void Start()
        {
            // Catch Helper not initialized.
            if (_spatialAudioSource == null)
            {
                this.LogWarning($"Destroying uninitialized Helper. Object: {Tools.GetGameObjectPath(gameObject)}");
                DestroyImmediate(this);
            }
        }

        private void OnEnable()
        {
            // ONSP needs to reapply audio settings everytime the object is enabled.
            _forceUpdate = true;
            _updateONSPParams = true;
        }

        // Late update to help with testing
        private void LateUpdate()
        {
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            if (_spatialAudioSource == null)
            {
                _spatialAudioSource = GetComponent<VRC_SpatialAudioSource>();
                if (_spatialAudioSource == null)
                {
                    Destroy(this);
                }
                SetSpatializer(_spatialAudioSource);  
                return;
            }

            // Check if we need to make changes.
            if (
                _onsp.EnableSpatialization != _spatialAudioSource.EnableSpatialization ||
                _onsp.Gain != _spatialAudioSource.Gain ||
                _onsp.Near != _spatialAudioSource.Near ||
                _onsp.Far != _spatialAudioSource.Far ||
                _useAudioSourceCurve != _spatialAudioSource.UseAudioSourceVolumeCurve
            ) {
                _forceUpdate = true;
                _updateONSPParams = true;
            }
            
            _onsp.EnableSpatialization = _spatialAudioSource.EnableSpatialization;
            _onsp.Gain = _spatialAudioSource.Gain;
            _useAudioSourceCurve = _spatialAudioSource.UseAudioSourceVolumeCurve;
            _onsp.Near = _spatialAudioSource.Near;
            _onsp.Far = _spatialAudioSource.Far;
            _onsp.VolumetricRadius = _spatialAudioSource.VolumetricRadius;

            // In unity 2022 - updating ONSP params every frame can cause a logspam
            // This is a workaround to only update when needed
            if (_updateONSPParams)
            {
                _onsp.SetParameters(ref _audioSource);
                _updateONSPParams = false;
            }
            
            if (!_onsp.EnableSpatialization)
            {
                return;
            }

            if (!_forceUpdate)
            {
                return;
            }
            
            _forceUpdate = false;

            if (!_spatialAudioSource.UseAudioSourceVolumeCurve)
            {
                float near = _onsp.VolumetricRadius + _onsp.Near;
                float far = _onsp.VolumetricRadius + Mathf.Max(near, _onsp.Far + EPS);

                _audioSource.maxDistance = far;
                
                CreateRolloffCurve(near, far);
                CreateSpatialCurve(near, far);
            }
        }

        // Create volume rolloff curve where Volumetric + near is volume 1, then 2^-x fall off to far.
        private void CreateRolloffCurve(float near, float far)
        {
            _audioSource.rolloffMode = AudioRolloffMode.Custom;

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(new Keyframe(near, 1));
            int max = 8;
            for (int loc = 1; loc < max; ++loc)
            {
                float time = near + Mathf.Pow(2, loc - max) * (far - near);
                float value = Mathf.Pow(2.2f, -loc);
                curve.AddKey(new Keyframe(time, value));
            }
            curve.AddKey(new Keyframe(far, 0));

            for (int i = 0; i < curve.length; ++i)
            {
                curve.SmoothTangents(i, 0);
            }

            _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        }

        // Create spatial blend curve so that it goes from (Setting) to 3d from min to max
        private void CreateSpatialCurve(float near, float far)
        {
            AnimationCurve spatialCurve = new AnimationCurve();
            spatialCurve.AddKey(0, _audioSource.spatialBlend);
            spatialCurve.AddKey(_onsp.VolumetricRadius, _audioSource.spatialBlend);

            Keyframe nearFrame = new Keyframe(near + EPS, 1);
            nearFrame.outTangent = 0;
            spatialCurve.AddKey(nearFrame);

            Keyframe farFrame = new Keyframe(far, 1);
            farFrame.inTangent = 0;
            spatialCurve.AddKey(farFrame);

            _audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatialCurve);
        }
    }
}