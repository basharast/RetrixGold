using LibRetriX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string FrameFailedError="";

        protected const uint NumChannels = 2;
        protected const uint SampleSizeBytes = sizeof(short);

        private const uint NullSampleRate = 0;
        public const uint MaxSamplesQueueSize = 44100 * 4;
        public const uint GCReserveSize = MaxSamplesQueueSize;
        public const uint GCReserveSizeMax = 300000000;
        private const float PlaybackDelaySeconds = 0.2f; //Have some buffer to avoid crackling
        private const float MaxAllowedDelaySeconds = 0.4f; //Limit maximum delay

        protected Queue<short> SamplesBuffer = new Queue<short>(MaxSamplesQueueSize);

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

                try { 
                if (SampleRate == NullSampleRate || (AudioMuteGlobal || VideoOnlyGlobal) || FrameFailed)
                {
                    return false; //Allow core a chance to init timings by runnning
                }

                if (SmartFrameDelay)
                {
                   
                    return random.Next(0, 30) == 2;
                }
                lock (SamplesBuffer)
                {
                    return SamplesBuffer.Count >= MaxNumSamplesForTargetDelay;
                }
                }catch(Exception e)
                {
                    return false;
                }
            }
        }

        public Task InitAsync()
        {
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
                if (PlatformService != null) { 
                PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames)
        {
            try
            {
                var numSrcSamples = (uint)numFrames * NumChannels;
                var bufferRemainingCapacity = Math.Max(0, MaxSamplesQueueSize - SamplesBuffer.Count);
                var numSamplesToCopy = Math.Min(numSrcSamples, bufferRemainingCapacity);
                lock (SamplesBuffer)
                {
                    if( (!AudioMuteGlobal && !VideoOnlyGlobal)) { 
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
                if (PlatformService != null) { 
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
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null) { 
                PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public int GetSamplesBufferCount()
        {
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

    public class Queue<T>
    {
        T[] nodes;
        int current;
        int emptySpot;
        public int Count = 0;

        public Queue(uint size)
        {
            nodes = new T[size];
            this.current = 0;
            this.emptySpot = 0;
        }

        public void Enqueue(T value)
        {
            nodes[emptySpot] = value;
            emptySpot++;
            if (emptySpot >= nodes.Length)
            {
                emptySpot = 0;
            }
            Interlocked.Increment(ref Count);
        }
        public T Dequeue()
        {
            int ret = current;
            current++;
            if (current >= nodes.Length)
            {
                current = 0;
            }
            Interlocked.Decrement(ref Count);
            return nodes[ret];
        }
        public void Clear()
        {
            Interlocked.Exchange(ref Count, 0);
            Interlocked.Exchange(ref emptySpot, 0);
            Interlocked.Exchange(ref current, 0);
            nodes.Initialize();
        }
    }
}
