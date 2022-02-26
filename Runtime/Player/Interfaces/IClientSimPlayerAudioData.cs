namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerAudioData
    {
        void SetAvatarAudioVolumetricRadius(float value);
        void SetAvatarAudioNearRadius(float value);
        void SetAvatarAudioFarRadius(float value);
        void SetAvatarAudioGain(float value);
        void SetAvatarAudioForceSpatial(bool value);
        void SetAvatarAudioCustomCurve(bool value);
        void SetVoiceLowpass(bool value);
        void SetVoiceVolumetricRadius(float value);
        void SetVoiceDistanceFar(float value);
        void SetVoiceDistanceNear(float value);
        void SetVoiceGain(float value);
    }
}