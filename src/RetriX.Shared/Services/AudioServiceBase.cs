using LibRetriX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public abstract class AudioServiceBase : IAudioService
    {
        protected abstract Task CreateResourcesAsync(uint sampleRate);
        protected abstract void DestroyResources();
        protected abstract void StartPlayback();
        protected abstract void StopPlayback();
        protected abstract void SetAudioGain(double AudioGain);
        protected abstract void SetAudioEcho(bool AudioEcho);
        protected abstract void SetAudioReverb(bool AudioReverb);
        public abstract void TryStartNoGCRegionCall();
        public abstract void EndNoGCRegionCall();

        public bool AudioMuteGlobal { get; set; }
        public bool VideoOnlyGlobal { get; set; }
        public bool SmartFrameDelay { get; set; }
        public bool EnableGCPrevent { get; set; }
        public bool FrameFailed = false;
        public string FrameFailedError = "";

        protected const uint NumChannels = 2;
        protected const uint SampleSizeBytes = sizeof(short);

        private const uint NullSampleRate = 0;
        public const int MaxSamplesQueueSize = 44100 * 4;
        public const uint GCReserveSize = MaxSamplesQueueSize;
        public const uint GCReserveSizeMax = 300000000;
        private const float PlaybackDelaySeconds = 0.01f; //Have some buffer to avoid crackling
        private const float MaxAllowedDelaySeconds = 0.1f; //Limit maximum delay

        protected Queue<short> SamplesBuffer = new Queue<short>(MaxSamplesQueueSize);
        public bool isGameStarted { get; set; }
        public bool gameIsPaused { get; set; }

        private int MinNumSamplesForPlayback { get; set; } = 0;
        private int MaxNumSamplesForTargetDelay { get; set; } = 0;

        private Task ResourcesCreationTask { get; set; }
        private bool IsPlaying { get; set; }
        private IPlatformService PlatformService { get; }

        private uint sampleRate = NullSampleRate;
        private uint SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                MinNumSamplesForPlayback = (int)(sampleRate * PlaybackDelaySeconds);
                MaxNumSamplesForTargetDelay = (int)(sampleRate * MaxAllowedDelaySeconds);
            }
        }

        Random random = new Random();
        public bool ShouldDelayNextFrame
        {
            get
            {
                try
                {
                    if (SampleRate == NullSampleRate || (AudioMuteGlobal || VideoOnlyGlobal) || FrameFailed)
                    {
                        return false; //Allow core a chance to init timings by runnning
                    }

                    /*if (SmartFrameDelay)
                    {
                        return random.Next(0, 30) == 2;
                    }*/
                    lock (SamplesBuffer)
                    {
                        if (SmartFrameDelay)
                        {
                            var delayState = SamplesBuffer.Count >= MaxNumSamplesForTargetDelay;
                            if (delayState)
                            {
                                SamplesBuffer.Clear();
                            }
                            return false;
                        }
                        else
                        {
                            return SamplesBuffer.Count >= MaxNumSamplesForTargetDelay;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public Task InitAsync()
        {
            SamplesBuffer = new Queue<short>(MaxSamplesQueueSize);
            return Task.CompletedTask;
        }

        public Task DeinitAsync()
        {
            Stop();
            if (ResourcesCreationTask == null)
            {
                DestroyResources();
            }

            SampleRate = NullSampleRate;
            return Task.CompletedTask;
        }

        public async void TimingChanged(SystemTimings timings)
        {
            try
            {
                uint sampleRate = (uint)timings.SampleRate;
                if (SampleRate == sampleRate || ResourcesCreationTask != null)
                {
                    return;
                }

                SampleRate = sampleRate;

                Stop();
                DestroyResources();
                try
                {
                    ResourcesCreationTask = CreateResourcesAsync(SampleRate);
                    await ResourcesCreationTask.ConfigureAwait(false);
                    ResourcesCreationTask = null;
                }
                catch
                {
                    DestroyResources();
                    SampleRate = NullSampleRate;
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        
        public uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames)
        {
            if(SamplesBuffer == null || !isGameStarted)
            {
                return 0;
            }
            try
            {
                var numSrcSamples = (uint)numFrames * NumChannels;
                var bufferRemainingCapacity = Math.Max(0, MaxSamplesQueueSize - SamplesBuffer.Count);
                var numSamplesToCopy = Math.Min(numSrcSamples, bufferRemainingCapacity);
                lock (SamplesBuffer)
                {
                    if ((!AudioMuteGlobal && !VideoOnlyGlobal))
                    {
                        for (var i = 0; i < numSamplesToCopy; i++)
                        {
                            SamplesBuffer.Enqueue(data[i]);
                        }
                    }
                    //SamplesBuffer = new Queue<short>(data.ToArray());
                    if (ResourcesCreationTask == null && !IsPlaying && SamplesBuffer.Count >= MinNumSamplesForPlayback)
                    {
                        StartPlayback();
                        IsPlaying = true;
                    }
                }
                return numFrames;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return 0;
            }
        }

        public void Start()
        {
            if (ResourcesCreationTask == null && !IsPlaying && SamplesBuffer.Count >= MinNumSamplesForPlayback)
            {
                StartPlayback();
                IsPlaying = true;
            }
        }
        public void Stop()
        {
            try
            {
                if (ResourcesCreationTask == null && IsPlaying)
                {
                    StopPlayback();
                    IsPlaying = false;
                }
                lock (SamplesBuffer)
                {
                    SamplesBuffer.Clear();
                    try
                    {
                        SamplesBuffer = null;
                        SamplesBuffer = new Queue<short>(MaxSamplesQueueSize);
                    }catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public int GetSamplesBufferCount()
        {
            if(SamplesBuffer == null)
            {
                return 0;
            }
            return SamplesBuffer.Count;
        }
        public int GetMaxSamplesBufferCount()
        {
            return (int)MaxSamplesQueueSize;
        }

        public void ChangeAudioGain(double AudioGain)
        {
            SetAudioGain(AudioGain);
        }
        public void AddAudioEcho(bool AudioEcho)
        {
            SetAudioEcho(AudioEcho);
        }
        public void AddAudioReverb(bool AudioReverb)
        {
            SetAudioReverb(AudioReverb);
        }
        public string GetFrameFailedMessage()
        {
            return FrameFailedError;
        }
        public void SetGCPrevent(bool GCPreventState)
        {
            EnableGCPrevent = GCPreventState;
        }
    }
    public class PinnedBuffer : IDisposable
    {
        public GCHandle Handle { get; }
        public short[] Data { get; private set; }

        public IntPtr Ptr
        {
            get
            {
                return Handle.AddrOfPinnedObject();
            }
        }

        public PinnedBuffer(short[] bytes)
        {
            Data = bytes;
            Handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Handle.Free();
                Data = null;
            }
        }
    }
}
