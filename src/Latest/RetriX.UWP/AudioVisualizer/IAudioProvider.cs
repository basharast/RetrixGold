using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Visualizer.UI.Spectrum;
using Windows.Media;

namespace Visualizer.UI
{
    public interface IAudioProvider : ISpectrumProvider
    {
       unsafe void ProcessFrameOutput(float* dataInFloat, int n, int total);
    }
}