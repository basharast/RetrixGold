using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Visualizer.UI.DSP;
using Visualizer.UI.Spectrum;
using WinUniversalTool;
using RetriX.UWP.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Pages;

namespace Visualizer.UI
{
    public class AudioGraphProvider : BindableBase, IAudioProvider
    {
        #region Fields
        private BasicSpectrumProvider _spectrumProvider;
        private GamePlayerViewModel VM;
        #endregion


        #region Properties

        public BasicSpectrumProvider SpectrumProvider
        {
            get { return _spectrumProvider; }
        }

        #endregion


        #region ctor

        public AudioGraphProvider(GamePlayerViewModel ViewModel)
        {
            try
            {
                VM = ViewModel;
                var channelCount = AudioService.Graph != null ? (int)AudioService.Graph.EncodingProperties.ChannelCount : 1;
                var sampleRate = AudioService.Graph != null ? (int)AudioService.Graph.EncodingProperties.SampleRate : 1;
                _spectrumProvider = new BasicSpectrumProvider(channelCount, sampleRate, (FftSize)GamePlayerView.FFTSizeArray[GamePlayerView.FftSizeValue]);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion


        #region Methods
        public unsafe void ProcessFrameOutput(float* dataInFloat, int n, int total)
        {
            if (SpectrumProvider != null && VM != null && !VM.GameIsPaused && !VM.GameStopInProgress && VM.isGameStarted && !VM.FailedToLoadGame && VM.AVDebug)
            {
                try
                {
                    SpectrumProvider.Add(GamePlayerView.LeftChannelState ? dataInFloat[n] : 0, GamePlayerView.RightChannelState ? (n + 1 < total ? dataInFloat[n + 1] : 0) : 0);
                }
                catch (Exception ex)
                {

                }
            }
        }
        #endregion


        #region Event handlers

        public bool GetFftData(float[] fftDataBuffer)
        {
            try
            {
                if (SpectrumProvider == null) return false;
                return SpectrumProvider.GetFftData(fftDataBuffer);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int GetFftFrequencyIndex(int frequency)
        {
            try
            {
                if (SpectrumProvider == null || AudioService.Graph == null) return 0;

                var fftSize = (int)SpectrumProvider.FftSize;
                var f = AudioService.Graph.EncodingProperties.SampleRate / 2.0;
                return (int)(frequency / f * (fftSize / 2.0));
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion
    }

}
