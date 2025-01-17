using System;

namespace VRC.SDK3.ClientSim
{
    [Serializable]
    public class ClientSimPlayerAudioData : IClientSimPlayerAudioData
    {
        public float voiceVolumetricRadius = 0;
        public float voiceDistanceNear = 0;
        public float voiceDistanceFar = 25;
        public float voiceGain = 15;
        public bool voiceLowpass = false;

        public float avatarAudioVolumetricRadius = 40;
        public float avatarAudioNearRadius = 40;
        public float avatarAudioFarRadius = 40;
        public float avatarAudioGain = 10;
        public bool avatarAudioCustomCurve;
        public bool avatarAudioForceSpatial;

        #region Player Audio

        public void SetVoiceGain(float value) => voiceGain = value;
        public void SetVoiceDistanceNear(float value) => voiceDistanceNear = value;
        public void SetVoiceDistanceFar(float value) => voiceDistanceFar = value;
        public void SetVoiceVolumetricRadius(float value) => voiceVolumetricRadius = value;
        public void SetVoiceLowpass(bool value) => voiceLowpass = value;

        public float GetVoiceGain() => voiceGain;
        public float GetVoiceDistanceNear() => voiceDistanceNear;
        public float GetVoiceDistanceFar() => voiceDistanceFar;
        public float GetVoiceVolumetricRadius() => voiceVolumetricRadius;
        public bool GetVoiceLowpass() => voiceLowpass;
        #endregion

        #region Avatar Audio

        public void SetAvatarAudioVolumetricRadius(float value)
        {
            avatarAudioVolumetricRadius = value;
        }

        public void SetAvatarAudioNearRadius(float value)
        {
            avatarAudioNearRadius = value;
        }

        public void SetAvatarAudioFarRadius(float value)
        {
            avatarAudioFarRadius = value;
        }

        public void SetAvatarAudioGain(float value)
        {
            avatarAudioGain = value;
        }

        public void SetAvatarAudioForceSpatial(bool value)
        {
            avatarAudioForceSpatial = value;
        }

        public void SetAvatarAudioCustomCurve(bool value)
        {
            avatarAudioCustomCurve = value;
        }
        
        #endregion
    }
}