using LibRetriX;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using RetriX.Shared.Services;
using RetriX.UWP.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace RetriX.UWP
{
    public sealed class VideoService
    {
        public event EventHandler RequestRunCoreFrame;

        private CanvasAnimatedControl renderPanel;

        public CanvasAnimatedControl RenderPanel
        {
            get => renderPanel;
            set
            {
                if (renderPanel == value)
                {
                    return;
                }

                RenderTargetManager.Dispose();

                if (renderPanel != null)
                {
                    renderPanel.Update -= RenderPanelUpdate;
                    renderPanel.Draw -= RenderPanelDraw;
                    renderPanel.GameLoopStopped -= RenderPanelLoopStopping;
                }

                renderPanel = value;
                if (renderPanel != null)
                {
                    RenderPanel.ClearColor = Color.FromArgb(0xff, 0, 0, 0);
                    renderPanel.Update += RenderPanelUpdate;
                    renderPanel.Draw += RenderPanelDraw;
                    renderPanel.GameLoopStopped += RenderPanelLoopStopping;
                }
            }
        }

        private readonly RenderTargetManager RenderTargetManager = new RenderTargetManager();

        private TaskCompletionSource<object> InitTCS;

        public Task InitAsync()
        {
            if (InitTCS == null)
            {
                InitTCS = new TaskCompletionSource<object>();
            }

            return InitTCS.Task;
        }

        public Task DeinitAsync()
        {
            RenderPanel = null;
            return Task.CompletedTask;
        }

        public void RenderVideoFrame(IntPtr data, uint width, uint height, uint pitch)
        {
            if (RenderPanel == null)
            {
                return;
            }

            RenderTargetManager.UpdateFromCoreOutput(RenderPanel, data, width, height, pitch);
        }

        public void PlayAudioOnly(bool AudioOnlyState)
        {
            RenderTargetManager.PlayAudioOnly = AudioOnlyState;
        }
        public void SetAliased(bool AliasedState)
        {
            RenderTargetManager.Aliased = AliasedState;
        }
        public void GeometryChanged(GameGeometry geometry)
        {
            RenderTargetManager.CurrentGeometry = geometry;
        }

        public float GetAspectRatio()
        {
            return RenderTargetManager.RenderTargetAspectRatio;
        }

        public void PixelFormatChanged(PixelFormats format)
        {
            RenderTargetManager.CurrentPixelFormat = format;
        }

        public int GetFrameRate()
        {
            return RenderTargetManager.FrameRate;
        }
        public void SetShowFPS(bool ShowFPS)
        {
            RenderTargetManager.ShowFPSCounter = ShowFPS;
        }

        public static long TargetTimeTicks = 0;
        public void TimingsChanged(SystemTimings timings)
        {
            if (RenderPanel == null)
            {
                return;
            }

            var targetTimeTicks = (long)(TimeSpan.TicksPerSecond / timings.FPS);
            TargetTimeTicks = targetTimeTicks;
            RenderPanel.TargetElapsedTime = TimeSpan.FromTicks(targetTimeTicks);
        }

        public void RotationChanged(Rotations rotation)
        {
            RenderTargetManager.CurrentRotation = rotation;
        }

        public void SetFilter(int filterType)
        {
            RenderTargetManager.RenderTargetFilterType = filterType;
        }

        private void RenderPanelLoopStopping(ICanvasAnimatedControl sender, object args)
        {
            RenderPanel = null;
        }

        public object UpdateEffects(object drawingSession, object RenderTarget, double viewportWidth, double viewportHeight, object interpolation)
        {
            return RenderTargetManager.UpdateEffects((CanvasDrawingSession)drawingSession, (CanvasBitmap)RenderTarget, viewportWidth, viewportHeight, (CanvasImageInterpolation)interpolation);
        }
        private void RenderPanelUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            try
            {
                if (InitTCS != null)
                {
                    InitTCS.SetResult(null);
                    InitTCS = null;
                }

                RequestRunCoreFrame?.Invoke(this, EventArgs.Empty);
            }catch(Exception ex)
            {

            }
        }

        public int TotalEffects()
        {
            if (RenderTargetManager?.RenderEffectsList != null)
            {
                return RenderTargetManager.RenderEffectsList.Count;
            }
            else
            {
                return 0;
            }
        }
        public float RenderTargetAspectRatio()
        {
            return RenderTargetManager.aspectRatio;
        }
        public object GetTransformMattrix()
        {
            return RenderTargetManager.transformMatrix;
        }
        public object GetDestinationSize()
        {
            return RenderTargetManager.destinationSize;
        }
        public object GetRenderTarget()
        {
            return RenderTargetManager.RenderTarget;
        }

        private void RenderPanelDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            RenderTargetManager.Render(args.DrawingSession, sender);
        }
        public int SetEffect(string EffectName, bool EffectState, double EffectValue1 = 0, double EffectValue2 = 0, double EffectValue3 = 0, double EffectValue4 = 0, int ForceOrder = -1)
        {
            int effectOrder = -1;
            if (!EffectState)
            {
                RemoveEffect(EffectName);
            }
            else
            {
                RenderEffect RenderEffect = new RenderEffect(EffectName, EffectValue1, EffectValue2, EffectValue3, EffectValue4);
                effectOrder = UpdateOrAddEffect(RenderEffect, ForceOrder);
            }
            return effectOrder;
        }

        public int SetEffect(string EffectName, bool EffectState, List<byte[]> EffectValue1, int ForceOrder = -1, int currentBlendMode = -1)
        {
            int effectOrder = -1;
            if (!EffectState)
            {
                RemoveEffect(EffectName);
            }
            else
            {
                RenderEffect RenderEffect = new RenderEffect(EffectName, EffectValue1, currentBlendMode);
                effectOrder = UpdateOrAddEffect(RenderEffect, ForceOrder);
            }
            return effectOrder;
        }
        public void RemoveEffect(string Name)
        {
            try
            {
                for (int i = 0; i < RenderTargetManager.RenderEffectsList.Count; i++)
                {
                    if (RenderTargetManager.RenderEffectsList[i].Name.Equals(Name))
                    {
                        RenderTargetManager.RenderEffectsList.RemoveAt(i);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public int UpdateOrAddEffect(RenderEffect effect, int ForceOrder = -1)
        {
            int effectOrder = -1;
            try
            {
                bool EffectFound = false;

                for (int i = 0; i < RenderTargetManager.RenderEffectsList.Count; i++)
                {
                    if (RenderTargetManager.RenderEffectsList[i].Name.Equals(effect.Name))
                    {
                        RenderTargetManager.RenderEffectsList[i].Value1 = effect.Value1;
                        RenderTargetManager.RenderEffectsList[i].Value2 = effect.Value2;
                        RenderTargetManager.RenderEffectsList[i].Value3 = effect.Value3;
                        RenderTargetManager.RenderEffectsList[i].Value4 = effect.Value4;
                        RenderTargetManager.RenderEffectsList[i].Values1 = effect.Values1;
                        RenderTargetManager.RenderEffectsList[i].BlendModeState = effect.BlendModeState;
                        RenderTargetManager.RenderEffectsList[i].CurrentBlendMode = effect.CurrentBlendMode;
                        RenderTargetManager.RenderEffectsList[i].Updated = true;
                        EffectFound = true;
                        break;
                    }
                }
                if (!EffectFound)
                {
                    if (ForceOrder > -1)
                    {
                        effect.Order = ForceOrder;
                    }
                    else
                    {
                        effect.Order = RenderTargetManager.RenderEffectsList.Count;
                    }
                    effectOrder = effect.Order;
                    RenderTargetManager.RenderEffectsList.Add(effect);
                }
            }
            catch (Exception ex)
            {

            }
            return effectOrder;
        }
        public bool isShaderActive()
        {
            bool shaderState = false;

            try
            {
                foreach(var eItem in RenderTargetManager.RenderEffectsList)
                {
                    if (eItem.Name.Equals("PixelShaderEffect"))
                    {
                        shaderState = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return shaderState;
        }    
        public bool isOverlayActive()
        {
            bool overlayState = false;

            try
            {
                foreach(var eItem in RenderTargetManager.RenderEffectsList)
                {
                    if (eItem.Name.Equals("OverlayEffect"))
                    {
                        overlayState = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return overlayState;
        }
    }
}
