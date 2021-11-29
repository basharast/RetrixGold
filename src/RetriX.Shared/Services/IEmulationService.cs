using LibRetriX;
using RetriX.Shared.StreamProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public enum InjectedInputTypes
    {
        DeviceIdJoypadB = 0,
        DeviceIdJoypadY = 1,
        DeviceIdJoypadSelect = 2,
        DeviceIdJoypadStart = 3,
        DeviceIdJoypadUp = 4,
        DeviceIdJoypadDown = 5,
        DeviceIdJoypadLeft = 6,
        DeviceIdJoypadRight = 7,
        DeviceIdJoypadA = 8,
        DeviceIdJoypadX = 9,
        DeviceIdJoypadC = 10,
        DeviceIdJoypadZ = 11,
        DeviceIdJoypadL = 12,
        DeviceIdJoypadR = 13,
        DeviceIdJoypadR2 = 14,
        DeviceIdJoypadL2 = 15,
        DeviceIdPointerPressed = 38,
        DeviceIdPointerX = 36,
        DeviceIdPointerY = 37,
        DeviceIdMouseLeft = 22,
        DeviceIdMouseRight = 23,
        DeviceIdMouseX = 20,
        DeviceIdMouseY = 21,
    };

    public interface IEmulationService
    {
        Task<bool> StartGameAsync(ICore core, IStreamProvider streamProvider, string mainFilePath);

        Task ResetGameAsync();
        Task StopGameAsync();

        Task PauseGameAsync();
        Task ResumeGameAsync();

        string GetGameID();
        string GetGameName();

        Task<bool> SaveGameStateAsync(uint slotID, bool showMessage = true);
        Task<bool> LoadGameStateAsync(uint slotID);

        void InjectInputPlayer1(InjectedInputTypes inputType);

        event EventHandler GameStarted;
        event EventHandler GameStopped;
        event EventHandler GameLoaded;
        event EventHandler<Exception> GameRuntimeExceptionOccurred;
        void UpdateCoreOption(string optionName, uint optionValue);
        uint GetCoreOptionValue(string optionName);
        IDictionary<string, CoreOption> GetCoreOptions();

        ObservableCollection<string> GetCoreLogsList();
        bool CorePaused { get; set; }
        bool isGameLoaded();

        void SetFPSCounterState(bool FPSCounterState);
        int GetFPSCounterValue();
        void SetVideoOnlyState(bool VideoOnlyState);
        void SetAudioOnlyState(bool AudioOnlyState);
        void SetSkipFramesState(bool SkipFramesState);
        void SetSkipFramesRandomState(bool SkipFramesState);
        int GetSamplesBufferCount();

    }
}
