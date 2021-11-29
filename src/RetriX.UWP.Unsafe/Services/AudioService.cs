using RetriX.Shared.Services;
using RetriX.UWP.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.System;

namespace RetriX.UWP.Services
{
    public sealed class AudioService : AudioServiceBase
    {

        public AudioService()
        {

        }
        private AudioGraph graph;
        private AudioGraph Graph
        {
            get => graph;
            set { if (graph != value) { graph?.Dispose(); graph = value; } }
        }

        private AudioDeviceOutputNode outputNode;
        private AudioDeviceOutputNode OutputNode
        {
            get => outputNode;
            set { if (outputNode != value) { outputNode?.Dispose(); outputNode = value; } }
        }

        private AudioFrameInputNode inputNode;
        private AudioFrameInputNode InputNode
        {
            get => inputNode;
            set { if (inputNode != value) { inputNode?.Dispose(); inputNode = value; } }
        }

        EchoEffectDefinition echoEffect;
        ReverbEffectDefinition reverbEffect;
        protected override async Task CreateResourcesAsync(uint sampleRate)
        {
            try
            {
                var graphResult = await AudioGraph.CreateAsync(new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.GameMedia)).AsTask().ConfigureAwait(false);
                if (graphResult.Status != AudioGraphCreationStatus.Success)
                {
                    throw new Exception($"Unable to create audio graph: {graphResult.Status.ToString()}");
                }
                Graph = graphResult.Graph;
                Graph.Stop();

                var outNodeResult = await Graph.CreateDeviceOutputNodeAsync().AsTask().ConfigureAwait(false);
                if (outNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
                {
                    throw new Exception($"Unable to create device node: {outNodeResult.Status.ToString()}");
                }
                OutputNode = outNodeResult.DeviceOutputNode;

                var nodeProperties = Graph.EncodingProperties;
                nodeProperties.ChannelCount = NumChannels;
                nodeProperties.SampleRate = sampleRate;

                InputNode = Graph.CreateFrameInputNode(nodeProperties);
                InputNode.QuantumStarted += InputNodeQuantumStartedHandler;
                InputNode.AddOutgoingConnection(OutputNode);

                echoEffect = new EchoEffectDefinition(Graph);
                reverbEffect = new ReverbEffectDefinition(Graph);

                if (AudioGainRequested)
                {
                    SetAudioGain(AudioGainRequestedValue);
                }
                if (AudioEchoRequested)
                {
                    SetAudioEcho(AudioEchoRequestedValue);
                }
                if (AudioReverbRequested)
                {
                    SetAudioReverb(AudioReverbRequestedValue);
                }

            }
            catch (Exception e)
            {

            }
        }

        bool AudioGainRequested = false;
        double AudioGainRequestedValue = 1.0;
        protected override void SetAudioGain(double AudioGain)
        {
            try
            {
                AudioGainRequestedValue = AudioGain;
                if (InputNode != null)
                {
                    AudioGainRequested = false;
                    InputNode.OutgoingGain = AudioGain;
                }
                else
                {
                    AudioGainRequested = true;
                }
            }
            catch (Exception e)
            {

            }
        }

        bool AudioEchoRequested = false;
        bool AudioEchoRequestedValue = false;
        protected override void SetAudioEcho(bool AudioEcho)
        {
            try
            {
                if (InputNode != null)
                {
                    AudioEchoRequested = false;
                    lock (InputNode)
                    {
                        if (AudioEcho)
                        {
                            if (InputNode.EffectDefinitions != null && !InputNode.EffectDefinitions.Contains(echoEffect))
                            {
                                echoEffect.Delay = 200;
                                echoEffect.Feedback = .4;
                                echoEffect.WetDryMix = .2;
                                InputNode.EffectDefinitions.Add(echoEffect);
                                InputNode.OutgoingGain += 0.4;
                            }
                        }
                        else
                        {
                            if (InputNode.EffectDefinitions != null && InputNode.EffectDefinitions.Contains(echoEffect))
                            {
                                InputNode.EffectDefinitions.Remove(echoEffect);
                                InputNode.OutgoingGain = AudioGainRequestedValue;
                            }
                        }
                    }
                }
                else
                {
                    AudioEchoRequested = true;
                    AudioEchoRequestedValue = AudioEcho;
                }
            }
            catch (Exception e)
            {

            }
        }

        bool AudioReverbRequested = false;
        bool AudioReverbRequestedValue = false;
        protected override void SetAudioReverb(bool AudioReverb)
        {
            try
            {
                if (InputNode != null)
                {
                    lock (InputNode)
                    {
                        if (AudioReverb)
                        {
                            if (InputNode.EffectDefinitions != null && !InputNode.EffectDefinitions.Contains(reverbEffect))
                            {
                                reverbEffect.RoomSize = 0.7;
                                reverbEffect.DecayTime = 1;
                                reverbEffect.ReverbGain = 1;
                                reverbEffect.WetDryMix = 65;
                                InputNode.EffectDefinitions.Add(reverbEffect);
                                InputNode.OutgoingGain += 0.4;
                            }
                        }
                        else
                        {
                            if (InputNode.EffectDefinitions != null && InputNode.EffectDefinitions.Contains(reverbEffect))
                            {
                                InputNode.EffectDefinitions.Remove(reverbEffect);
                                InputNode.OutgoingGain = AudioGainRequestedValue;
                            }
                        }
                    }
                }
                else
                {
                    AudioReverbRequested = true;
                    AudioReverbRequestedValue = AudioReverb;
                }
            }
            catch (Exception e)
            {

            }
        }


        protected override void DestroyResources()
        {
            try
            {
                InputNode = null;
                OutputNode = null;
                Graph = null;
            }
            catch (Exception e)
            {

            }
        }

        protected override void StartPlayback()
        {
            try
            {
                if (Graph != null)
                {
                    Graph.Start();
                }
            }
            catch (Exception e)
            {

            }
        }

        protected override void StopPlayback()
        {
            try
            {
                if (Graph != null)
                {
                    Graph.Stop();
                }
            }
            catch (Exception e)
            {

            }
        }

        GCLatencyMode oldMode = GCSettings.LatencyMode;

        public override void TryStartNoGCRegionCall()
        {
            try
            {
                /*try
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                }
                catch (Exception ex)
                {

                }
                try
                {
                    GCSettings.LatencyMode = GCLatencyMode.Batch;
                }
                catch (Exception ex)*/
                {
                    if (EnableGCPrevent)
                    {
                        /* try
                         {
                             var appMemory = (long)MemoryManager.AppMemoryUsage;
                             appMemory += 10000;
                             GC.TryStartNoGCRegion(appMemory, true);
                         }
                         catch (Exception exx)
                         {
                             try
                             {
                                 GC.TryStartNoGCRegion(GCReserveSizeMax, true);
                             }
                             catch (Exception exxx)
                             {
                                 GC.TryStartNoGCRegion(GCReserveSize, true);
                             }
                         }*/
                        GC.TryStartNoGCRegion(GCReserveSize, true);
                    }
                }
            }
            catch (Exception ea)
            {
                FrameFailed = false;
                FrameFailedError = ea.Message;
            }
        }

        public override void EndNoGCRegionCall()
        {
            try
            {
                if (!EnableGCPrevent)
                {
                    GC.EndNoGCRegion();
                }
                // ALWAYS set the latency mode back
                //GCSettings.LatencyMode = oldMode;
            }
            catch (Exception es)
            {
                //FrameFailed = false;
                //FrameFailedError = es.Message;
            }
        }
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        private AudioBuffer buffer;
        private IMemoryBufferReference reference;
        private unsafe void InputNodeQuantumStartedHandler(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            try
            {
                if ((!AudioMuteGlobal && !VideoOnlyGlobal))
                {
                    var requiredSamples = args.RequiredSamples;
                    if (requiredSamples < 1)
                        return;

                    // Buffer size is (number of samples) * (size of each sample)
                    // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
                    uint bufferSizeElements = (uint)requiredSamples * NumChannels;
                    uint bufferSizeBytes = bufferSizeElements * sizeof(float);
                    AudioFrame frame = new AudioFrame(bufferSizeBytes);

                    using (buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
                    {
                        using (reference = buffer.CreateReference())
                        {
                            byte* dataInBytes;
                            uint capacityInBytes;
                            float* dataInFloat;

                            // Get the buffer from the AudioFrame
                            ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                            // Cast to float since the data we are generating is float
                            dataInFloat = (float*)dataInBytes;
                            lock (SamplesBuffer)
                            {
                                var numElementsToCopy = Math.Min(bufferSizeElements, SamplesBuffer.Count);

                                for (var i = 0; i < numElementsToCopy; i++)
                                {
                                    var converted = (float)SamplesBuffer.Dequeue() / short.MaxValue;
                                    dataInFloat[i] = converted;
                                }
                                //Should we not have enough samples in buffer, set the remaing data in audio frame to zeros
                                for (var i = numElementsToCopy; i < bufferSizeElements; i++)
                                {
                                    dataInFloat[i] = 0f;
                                }

                            }
                        }
                    }

                    sender.AddFrame(frame);

                    FrameFailed = false;
                    FrameFailedError = "";
                }
            }
            catch (Exception e)
            {
                FrameFailed = true;
                FrameFailedError = e.Message;
            }
        }


    }

}
