using LibRetriX;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public enum FullScreenChangeType { Enter, Exit, Toggle };

    public enum MousePointerVisibility { Visible, Hidden };

    public interface IPlatformService
    {
        bool FullScreenChangingPossible { get; }
        bool IsFullScreenMode { get; }
        bool TouchScreenAvailable { get; }
        bool ShouldDisplayTouchGamepad { get; }
        bool HandleGameplayKeyShortcuts { get; set; }
        bool  GameNoticeShowed { get; set; }
        bool IsCoresLoaded { get; set; }
        bool DialogInProgress { get; set; }

        Task<bool> ChangeFullScreenStateAsync(FullScreenChangeType changeType);
        void ChangeMousePointerVisibility(MousePointerVisibility visibility);
        void ForceUIElementFocus();

        void CopyToClipboard(string content);

        event EventHandler<FullScreenChangeEventArgs> FullScreenChangeRequested;

        event EventHandler PauseToggleRequested;
        event EventHandler XBoxMenuRequested;
        event EventHandler QuickSaveRequested;
        event EventHandler SavesListRequested;
        event EventHandler ChangeToXBoxModeRequested;
        bool ShowNotification(string text, int time);
        bool ShowNotificationMain(string text, int time);
        void RestoreGamesListState(double currentIndex);
        void SaveGamesListState();
        Task<List<byte[]>> getShader();
        Task<List<byte[]>> getOverlay();

        double veScroll { get; }
        event EventHandler<GameStateOperationEventArgs> GameStateOperationRequested;
        string GetMemoryUsage();
        void PlayNotificationSound(string TargetSound);
        void StopNotificationSound(string TargetSound);
        void ShowErrorMessage(Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
        
        Task<string> GetFileToken(string FileLocation);
        Dictionary<string, List<string[]>> GetGamesRecents();
        Task AddGameToRecents(string system, string fileInfo, bool RootNeeded,string gameID, long totalTime, bool delete);
        int GetGameOpenCount(string SystemName, string GameLocation);
        string GetGameLocation(string SystemName, string GameLocation);
        bool GetGameRootNeeded(string SystemName, string GameLocation);
        long GetGamePlayedTime(string SystemName, string GameLocation);
        Task RetriveRecents();
        void ClearRecentsBySystem(string SystemName);
        void AddNewSystemOpenCount(string SystemName);
        int GetPlaysCount(string SystemName);
        int GetGamePlaysCount(string SystemName, string GameName);
        string GetGameID(string SystemName, string GameName);
        bool IsAppStartedByFile { get; set; }
        bool XBoxMode { get; set; }
        byte[] ConvertBytesToBitmap(int height, int width, byte[] bytes);
        void InvokeStopHandler(Object rootFrame);
        void SetStopHandler(EventHandler eventHandler);
        void DeSetStopHandler(EventHandler eventHandler);
        void SetHideSavesListHandler(EventHandler eventHandler);
        void DeSetHideSavesListHandler(EventHandler eventHandler);
        void SetSavesListActive(bool SavesListActiveState);


        void SetSubPageState(bool SubPageState);
        void SetHideCoreOptionsHandler(EventHandler eventHandler);
        void DeSetHideCoreOptionsHandler(EventHandler eventHandler);
        void SetCoreOptionsState(bool SubPageState);

        void SetResetSelectionHandler(EventHandler eventHandler);
        void SetFilterModeState(bool SubPageState);
        void SetRotateDegree(int degree);
        Task<IDirectoryInfo> GetRecentsLocationAsync();

        void InitialServices();
        void SetGameStopInProgress(bool StopState);

        Task<IDirectoryInfo> PickDirectory(string systemName, bool reSelect = false);
        bool CheckDirectoryToken(string token);
        Task<IDictionary<string, ArchiveData>> GetFilesStreams(Stream stream, string HandledScheme);
    }
}