using LibRetriX;
using System;
using System.Collections.Generic;

namespace RetriX.Shared.Services
{
    public enum TextureFilterTypes { NearestNeighbor, Bilinear };

    public interface IVideoService : IInitializable
    {
        event EventHandler RequestRunCoreFrame;

        void GeometryChanged(GameGeometry geometry);
        void PixelFormatChanged(PixelFormats format);
        void RotationChanged(Rotations rotation);
        void TimingsChanged(SystemTimings timings);
        void RenderVideoFrame(ReadOnlySpan<byte> data, uint width, uint height, uint pitch);
        void SetFilter(int filterType);
        void PlayAudioOnly(bool AudioOnlyState);
        void SetAliased(bool AliasedState);
        int GetFrameRate();
        int TotalEffects();
        void SetShowFPS(bool ShowFPS);
        int SetEffect(string EffectName, bool EffectState, double EffectValue1 = 0, double EffectValue2 = 0, double EffectValue3 = 0, double EffectValue4 = 0, int ForceOrder = -1);
        int SetEffect(string EffectName, bool EffectState, List<byte[]> EffectValue1, int ForceOrder = -1);
        bool isShaderActive();
        bool isOverlayActive();
    }
}
