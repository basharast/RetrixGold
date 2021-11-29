﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LibRetriX.Test
{
    [Collection(nameof(CoreTestBase))]
    public abstract class CoreTestBase
    {
        protected abstract ICore InstanceTarget();

        protected Lazy<ICore> TargetLazy { get; }
        protected ICore Target => TargetLazy.Value;

        protected CoreTestBase(Func<ICore> coreInstancer)
        {
            TargetLazy = new Lazy<ICore>(InstanceTarget, LazyThreadSafetyMode.PublicationOnly);
        }

        [Fact]
        public void CoreInfoIsRead()
        {
            Assert.NotNull(Target.Name);
            Assert.NotNull(Target.Version);
            Assert.NotNull(Target.SupportedExtensions);

            Assert.NotNull(Target.Options);
            foreach (var i in Target.Options)
            {
                Assert.NotEmpty(i.Key);

                var value = i.Value;
                Assert.NotNull(value);
                Assert.NotEmpty(value.Description);
                Assert.NotEmpty(value.Values);
                foreach (var j in value.Values)
                {
                    Assert.NotEmpty(j);
                }
            }

            Assert.NotNull(Target.FileDependencies);
            foreach (var i in Target.FileDependencies)
            {
                Assert.False(string.IsNullOrEmpty(i.Description) || string.IsNullOrWhiteSpace(i.Description));
                Assert.False(string.IsNullOrEmpty(i.Name) || string.IsNullOrWhiteSpace(i.Name));
                Assert.Equal(32, i.MD5.Length);
            }
        }

        public async virtual Task LoadingRomWorks(string romName)
        {
            var romPath = await SetupTestROMLoading(romName);

            //Cold load, with initialization
            var loadResult = Target.LoadGame(romPath);

            Assert.True(loadResult);
            Assert.NotEqual(PixelFormats.Unknown, Target.PixelFormat);

            var geometry = Target.Geometry;
            Assert.NotEqual(0, geometry.AspectRatio);
            Assert.NotEqual(0U, geometry.BaseWidth);
            Assert.NotEqual(0U, geometry.BaseHeight);
            Assert.NotEqual(0U, geometry.MaxHeight);
            Assert.NotEqual(0U, geometry.MaxWidth);

            var timings = Target.Timings;
            Assert.NotEqual(0, timings.FPS);
            Assert.NotEqual(0, timings.SampleRate);

            //Load other game without reinitializing
            loadResult = Target.LoadGame(romPath);
            Assert.True(loadResult);

            //Unload and deinit
            Target.UnloadGame();

            //Reload with initialization
            loadResult = Target.LoadGame(romPath);
            Assert.True(loadResult);
        }

        public async virtual Task ExecutionWorks(string romName)
        {
            var numPollInputCalled = 0;
            var numGetInputStateCalled = 0;
            var numRenderVideoFrameCalled = 0;
            var numRenderAudioFrameCalled = 0;

            Target.PollInput = () => numPollInputCalled++;

            Target.GetInputState += (d, e) =>
            {
                numGetInputStateCalled++;
                return 0;
            };

            Target.RenderVideoFrame += (d, e, f, g) =>
            {
                Assert.True(e > 0);
                Assert.True(f > 0);
                Assert.True(g > 0);
                numRenderVideoFrameCalled++;
            };

            Target.RenderAudioFrames += (d, e) =>
            {
                numRenderAudioFrameCalled++;
                return 0;
            };

            var romPath = await SetupTestROMLoading(romName);
            var loadResult = Target.LoadGame(romPath);
            Assert.True(loadResult);

            var numFramesToRun = 1;//10 * Target.Timings.FPS;
            for (var i = 0; i < numFramesToRun; i++)
            {
                Target.RunFrame();
            }

            Assert.True(numPollInputCalled == numFramesToRun);
            Assert.True(numGetInputStateCalled >= numFramesToRun);
            Assert.True(numRenderVideoFrameCalled == numFramesToRun);
            Assert.True(numRenderAudioFrameCalled >= numFramesToRun);
        }

        private async Task<string> SetupTestROMLoading(string romName)
        {
            var romsFolder = await Plugin.FileSystem.CrossFileSystem.Current.InstallLocation.GetDirectoryAsync("TestRoms");

            var provider = new StreamProvider("Test\\", romsFolder);
            Target.SystemRootPath = "Test";
            Target.SaveRootPath = Target.SystemRootPath;
            Target.OpenFileStream = provider.OpenFileStream;
            Target.CloseFileStream = provider.CloseFileStream;

            var romPath = $"{provider.HandledScheme}{romName}";
            return romPath;
        }
    }
}
