using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace LibRetriX.RetroBindings
{
    public class VFSHandler
    {
        private class VFSStream : IDisposable
        {
            public string Path { get; set; }
            public long Size { get; set; }
            public IntPtr PathUnmanaged { get; private set; }
            public Stream BackingStream { get; private set; }

            public VFSStream(string path, Stream backingStream)
            {
                Path = path;
                PathUnmanaged = Marshal.StringToHGlobalAnsi(Path);
                BackingStream = backingStream;
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(PathUnmanaged);
            }
        }

        private const int VFSSuccessCode = 0;
        private const int VFSErrorCode = 1;

        public const int SupportedVFSVersion = 3;

        public OpenFileStreamDelegate OpenFileStream { get; set; }
        public CloseFileStreamDelegate CloseFileStream { get; set; }
        public DeleteHandlerDelegate DeleteHandler { get; set; }
        public RenameHandlerDelegate RenameHandler { get; set; }
        public TruncateHandlerDelegate TruncateHandler { get; set; }
        public StatHandlerDelegate StatHandler { get; set; }
        public MkdirHandlerDelegate MkdirHandler { get; set; }
        public OpendirHandlerDelegate OpendirHandler { get; set; }
        public ReaddirHandlerDelegate ReaddirHandler { get; set; }
        public DirentGetNameHandlerDelegate DirentGetNameHandler { get; set; }
        public DirentIsDirHandlerDelegate DirentIsDirHandler { get; set; }
        public ClosedirHandlerDelegate ClosedirHandler { get; set; }

        private VFSInterface VFSInterface { get; }
        public IntPtr VFSInterfacePtr { get; }

        public VFSHandler()
        {
            try
            {
                VFSInterface = new VFSInterface
                {
                    GetPath = VFSGetPathHandler,
                    Open = VFSOpenHandler,
                    Close = VFSCloseHandler,
                    GetSize = VFSGetSizeHandler,
                    GetPosition = VFSGetPositionHandler,
                    SetPosition = VFSSetPositionHandler,
                    Read = VFSReadHandler,
                    Write = VFSWriteHandler,
                    Flush = VFSFlushHandler,
                    Delete = VFSDeleteHandler,
                    Rename = VFSRenameHandler,
                    /* VFS API v2 */
                    Truncate = VFSTruncateDelegate,
                    /* VFS API v3 */
                    Stat = VFSStatDelegate,
                    Mkdir = VFSMkdirDelegate,
                    Opendir = VFSOpendirDelegate,
                    Readdir = VFSReaddirDelegate,
                    DirentGetName = VFSDirentGetNameDelegate,
                    DirentIsDir = VFSDirentIsDirDelegate,
                    Closedir = VFSClosedirDelegate
                };

                VFSInterfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf(VFSInterface));
                Marshal.StructureToPtr(VFSInterface, VFSInterfacePtr, false);
            }
            catch (Exception e)
            {

            }
        }

        private VFSStream StreamFromIntPtr(IntPtr ptr)
        {
            return (VFSStream)GCHandle.FromIntPtr(ptr).Target;
        }
        private VFSDir VFSDirFromIntPtr(IntPtr ptr)
        {
            return (VFSDir)GCHandle.FromIntPtr(ptr).Target;
        }

        private IntPtr VFSGetPathHandler(IntPtr stream)
        {
            var vfsStream = StreamFromIntPtr(stream);
            return vfsStream.PathUnmanaged;
        }

        static List<GCHandleCache> GCHandles = new List<GCHandleCache>();
        private IntPtr VFSOpenHandler(IntPtr path, uint mode, uint hints)
        {
            try
            {
                var output = IntPtr.Zero;
                if (path == IntPtr.Zero)
                {
                    return output;
                }

                var pathStr = Marshal.PtrToStringAnsi(path);
                var access = FileAccessMode.Read;
                if ((mode & Constants.RETRO_VFS_FILE_ACCESS_WRITE) == Constants.RETRO_VFS_FILE_ACCESS_WRITE)
                {
                    access = FileAccessMode.ReadWrite;
                }
                if ((mode & Constants.RETRO_VFS_FILE_ACCESS_READ_WRITE) == Constants.RETRO_VFS_FILE_ACCESS_READ_WRITE)
                {
                    access = FileAccessMode.ReadWrite;
                }

                var stream = default(Stream);
                try
                {
                    stream = OpenFileStream?.Invoke(pathStr, access);
                }
                catch
                {
                    return output;
                }

                if (stream == null)
                {
                    return output;
                }

                var vfsStream = new VFSStream(pathStr, stream);
                var handle = new GCHandleCache(pathStr, GCHandle.Alloc(vfsStream));
                GCHandles.Add(handle);
                output = GCHandle.ToIntPtr(handle.Handle);
                return output;
            }
            catch (Exception e)
            {
                return IntPtr.Zero;
            }
        }

        private int VFSCloseHandler(IntPtr stream)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                CloseFileStream?.Invoke(vfsStream?.BackingStream);
                for(int i=0;i< GCHandles.Count;i++)
                {
                    var gch = GCHandles[i];
                    if (gch.ID.Equals(vfsStream?.Path))
                    {
                        gch.Handle.Free();
                        GCHandles.Remove(gch);
                    }
                }
                int identificador = GC.GetGeneration(GCHandles);
                GC.Collect(identificador, GCCollectionMode.Forced);
                vfsStream?.Dispose();
                //GC.Collect();
                GC.SuppressFinalize(vfsStream);
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }

            return VFSSuccessCode;
        }

        private long VFSGetSizeHandler(IntPtr stream)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                return vfsStream.BackingStream.Length;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private long VFSGetPositionHandler(IntPtr stream)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                return vfsStream.BackingStream.Position;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private long VFSSetPositionHandler(IntPtr stream, long offset, int seekPosition)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                switch (seekPosition)
                {
                    case Constants.RETRO_VFS_SEEK_POSITION_CURRENT:
                        offset = vfsStream.BackingStream.Position + offset;
                        break;
                    case Constants.RETRO_VFS_SEEK_POSITION_END:
                        offset = vfsStream.BackingStream.Length - offset;
                        break;
                }

                vfsStream.BackingStream.Position = offset;
                return VFSSuccessCode;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        public long VFSReadHandler(IntPtr stream, IntPtr buffer, ulong len)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                var nativeBuffer = new byte[len];
                if (vfsStream != null)
                {
                    var readBytes = vfsStream.BackingStream.Read(nativeBuffer, 0, nativeBuffer.Length);
                    Marshal.Copy(nativeBuffer, 0, buffer, readBytes);
                    return readBytes;
                }
                return 0;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private long VFSWriteHandler(IntPtr stream, IntPtr buffer, long len)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                unsafe
                {
                    using (var unmanagedStream = new UnmanagedMemoryStream((byte*)buffer.ToPointer(), len))
                    {
                        unmanagedStream.CopyTo(vfsStream.BackingStream);
                    }
                }
                return len;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private int VFSFlushHandler(IntPtr stream)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                vfsStream.BackingStream.Flush();
                return VFSSuccessCode;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private int VFSDeleteHandler(IntPtr path)
        {
            try
            {
                var pathString = Marshal.PtrToStringAnsi(path);
                var state = DeleteHandler.Invoke(pathString);
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private int VFSRenameHandler(IntPtr oldPath, IntPtr newPath)
        {
            try
            {
                var pathOldString = Marshal.PtrToStringAnsi(oldPath);
                var pathNewString = Marshal.PtrToStringAnsi(newPath);
                var state = RenameHandler.Invoke(pathOldString, pathNewString);
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private long VFSTruncateDelegate(IntPtr stream, long length)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                var state = TruncateHandler.Invoke(vfsStream.BackingStream, length);
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }
        private int VFSStatDelegate(IntPtr path, IntPtr size)
        {
            try
            {
                var pathString = Marshal.PtrToStringAnsi(path);
                var state = StatHandler.Invoke(pathString, size);
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }
        private int VFSMkdirDelegate(IntPtr dir)
        {
            try
            {
                var pathString = Marshal.PtrToStringAnsi(dir);
                var state = MkdirHandler.Invoke(pathString);
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        static List<GCHandleFolderCache> GCFoldersHandles = new List<GCHandleFolderCache>();
        private IntPtr VFSOpendirDelegate(IntPtr name, bool include_hidden)
        {
            try
            {
                var pathString = Marshal.PtrToStringAnsi(name);
                var vfsDir = OpendirHandler.Invoke(pathString, include_hidden);
                if (vfsDir != null)
                {
                    var handle = new GCHandleFolderCache(pathString, GCHandle.Alloc(vfsDir));
                    GCFoldersHandles.Add(handle);
                    var output = GCHandle.ToIntPtr(handle.Handle);
                    return output;
                }
                return IntPtr.Zero;
            }
            catch (Exception e)
            {
                return IntPtr.Zero;
            }
        }
        private bool VFSReaddirDelegate(IntPtr rdir)
        {
            try
            {
                var vfsDir = VFSDirFromIntPtr(rdir);
                var state = ReaddirHandler.Invoke(vfsDir);
                return state;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private IntPtr VFSDirentGetNameDelegate(IntPtr rdir)
        {
            try
            {
                var vfsDir = VFSDirFromIntPtr(rdir);
                var state = DirentGetNameHandler.Invoke(vfsDir);
                var output = Marshal.StringToHGlobalAnsi(state);
                return output;
            }
            catch (Exception e)
            {
                return IntPtr.Zero;
            }
        }
        private bool VFSDirentIsDirDelegate(IntPtr rdir)
        {
            try
            {
                var vfsDir = VFSDirFromIntPtr(rdir);
                var state = DirentIsDirHandler.Invoke(vfsDir);
                return state;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private int VFSClosedirDelegate(IntPtr rdir)
        {
            try
            {
                var vfsDir = VFSDirFromIntPtr(rdir);
                var state = ClosedirHandler.Invoke(vfsDir);
                for (int i = 0; i < GCFoldersHandles.Count; i++)
                {
                    var gch = GCFoldersHandles[i];
                    if (gch.ID.Equals(vfsDir.DirPath))
                    {
                        gch.Handle.Free();
                        GCFoldersHandles.Remove(gch);
                    }
                }
                return state;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }
    }

    public class GCHandleCache
    {
        public string ID;
        public GCHandle Handle;
        public GCHandleCache(string id, GCHandle handle)
        {
            ID = id;
            Handle = handle;
        }
    }
    public class GCHandleFolderCache
    {
        public string ID;
        public GCHandle Handle;
        public GCHandleFolderCache(string id, GCHandle handle)
        {
            ID = id;
            Handle = handle;
        }
    }
}
