using Pipelines.Sockets.Unofficial;
using RetriX.Shared.Services;
using RetriX.UWP.Components;
using RetriX.UWP.Pages;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        private static AudioGraph graph;
        public static AudioGraph Graph
        {
            get => graph;
            set { if (graph != value) { graph?.Dispose(); graph = value; } }
        }

        private static AudioDeviceOutputNode outputNode;
        public static AudioDeviceOutputNode OutputNode
        {
            get => outputNode;
            set { if (outputNode != value) { outputNode?.Dispose(); outputNode = value; } }
        }
        public static AudioFrameInputNode inputNode;
        public static  AudioFrameInputNode InputNode
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
                if (InputNode != null)
                {
                    InputNode.QuantumStarted -= InputNodeQuantumStartedHandler;
                }
            }
            catch (Exception ex)
            {

            }
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

        public override void TryStartNoGCRegionCall()
        {
            try
            {
                if (EnableGCPrevent)
                {
                    GC.TryStartNoGCRegion(GCReserveSize, true);
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
                if (EnableGCPrevent)
                {
                    GC.EndNoGCRegion();
                }
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

        private int RequiredSamplesGlobal = 0;
        private unsafe void InputNodeQuantumStartedHandler(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            if (!isGameStarted || SamplesBuffer == null)
            {
                return;
            }
            RequiredSamplesGlobal = args.RequiredSamples;
            try
            {
                if ((!AudioMuteGlobal && !VideoOnlyGlobal))
                {
                    if (RequiredSamplesGlobal < 1)
                        return;

                    // Buffer size is (number of samples) * (size of each sample)
                    // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
                    uint bufferSizeElements = (uint)RequiredSamplesGlobal * NumChannels;

                    uint bufferSizeBytes = bufferSizeElements * sizeof(float);
                    AudioFrame frame = new AudioFrame(bufferSizeBytes);
                    using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
                    {
                        using (IMemoryBufferReference reference = buffer.CreateReference())
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
                                    if (GamePlayerView._audioProvider != null)
                                    {
                                        GamePlayerView._audioProvider.ProcessFrameOutput(dataInFloat, (int)i, (int)numElementsToCopy);
                                    }
                                }
                                //Should we not have enough samples in buffer, set the remaing data in audio frame to zeros
                                for (var i = numElementsToCopy; i < bufferSizeElements; i++)
                                {
                                    dataInFloat[i] = 0f;
                                    if (GamePlayerView._audioProvider != null)
                                    {
                                        GamePlayerView._audioProvider.ProcessFrameOutput(dataInFloat, (int)i, (int)bufferSizeElements);
                                    }
                                }
                            }
                            try
                            {
                                reference.Dispose();
                                GC.SuppressFinalize(reference);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        try
                        {
                            buffer.Dispose();
                            GC.SuppressFinalize(buffer);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    sender.AddFrame(frame);
                    
                    try
                    {
                        frame.Dispose();
                        GC.SuppressFinalize(frame);
                        GC.SuppressFinalize(SamplesBuffer);
                    }
                    catch (Exception ex)
                    {

                    }
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
