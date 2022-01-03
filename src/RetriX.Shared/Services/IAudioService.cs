using LibRetriX;
using System;

namespace RetriX.Shared.Services
{
    public interface IAudioService : IInitializable
    {
        bool ShouldDelayNextFrame { get; }
        void TimingChanged(SystemTimings timings);
        uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames);
        void Stop();
        void Start();
        int GetSamplesBufferCount();
        int GetMaxSamplesBufferCount();
        void ChangeAudioGain(double AudioGain);
        void AddAudioEcho(bool AudioEcho);
        void AddAudioReverb(bool AudioReverb);
        void SetGCPrevent(bool GCPreventState);
        bool AudioMuteGlobal { get; set; }
        bool VideoOnlyGlobal { get; set; }
        bool SmartFrameDelay { get; set; }
        bool EnableGCPrevent { get; set; }
        bool isGameStarted { get; set; }
        bool gameIsPaused { get; set; }

        string GetFrameFailedMessage ();
        void TryStartNoGCRegionCall();
        void EndNoGCRegionCall();
    }
}