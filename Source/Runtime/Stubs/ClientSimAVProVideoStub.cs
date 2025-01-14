using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Interfaces.AVPro;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    // This class does nothing for videos, but by creating one and returning it in the VRCAVProVideoPlayer.Initialize
    // callback, this prevents errors in Udon when calling Get IsReady and Get IsPlaying.
    public class ClientSimAVProVideoStub : IAVProVideoPlayerInternal
    {
        public static IAVProVideoPlayerInternal InitializePlayer(VRCAVProVideoPlayer player)
        {
            return new ClientSimAVProVideoStub(player);
        }
        
        public bool Loop { get; set; }
        public bool IsPlaying { get; }
        public bool IsReady { get; }
        public bool UseLowLatency { get; }

        public int VideoWidth { get; private set; }

        public int VideoHeight { get; private set; }

        public ClientSimAVProVideoStub(VRCAVProVideoPlayer videoPlayer)
        {
            IsPlaying = false;
            IsReady = false;
            UseLowLatency = videoPlayer.UseLowLatency;
            VideoWidth = videoPlayer.VideoWidth;
            VideoHeight = videoPlayer.VideoHeight;
        }
        
        public float GetTime()
        {
            return 0;
        }

        public float GetDuration()
        {
            return 0;
        }
        
        public void LoadURL(VRCUrl url) { }

        public void PlayURL(VRCUrl url) { }

        public void Play() { }

        public void Pause() { }

        public void Stop() { }

        public void SetTime(float value) { }
    }
}