using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibRetriX.RetroBindings
{
    internal static class VFSHandler
    {
        private class VFSStream : IDisposable
        {
            public string Path { get; set; }
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

        public const int SupportedVFSVersion = 1;

        public static OpenFileStreamDelegate OpenFileStream { get; set; }
        public static CloseFileStreamDelegate CloseFileStream { get; set; }

        private static VFSInterface VFSInterface { get; }
        public static IntPtr VFSInterfacePtr { get; }

        static VFSHandler()
        {
            try { 
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
                Rename = VFSRenameHandler
            };

            VFSInterfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf(VFSInterface));
            Marshal.StructureToPtr(VFSInterface, VFSInterfacePtr, false);
            }
            catch (Exception e)
            {

            }
        }

        private static VFSStream StreamFromIntPtr(IntPtr ptr)
        {
            return (VFSStream)GCHandle.FromIntPtr(ptr).Target;
        }

        private static IntPtr VFSGetPathHandler(IntPtr stream)
        {
            var vfsStream = StreamFromIntPtr(stream);
            return vfsStream.PathUnmanaged;
        }

        private static IntPtr VFSOpenHandler(IntPtr path, uint mode, uint hints)
        {
            try { 
            var output = IntPtr.Zero;
            if (path == IntPtr.Zero)
            {
                return output;
            }

            var pathStr = Marshal.PtrToStringAnsi(path);
            var access = FileAccess.Read;
            if((mode & Constants.RETRO_VFS_FILE_ACCESS_WRITE) == Constants.RETRO_VFS_FILE_ACCESS_WRITE)
            {
                access = FileAccess.Write;
            }
            if ((mode & Constants.RETRO_VFS_FILE_ACCESS_READ_WRITE) == Constants.RETRO_VFS_FILE_ACCESS_READ_WRITE)
            {
                access = FileAccess.ReadWrite;
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
            var handle = GCHandle.Alloc(vfsStream);
            output = GCHandle.ToIntPtr(handle);
            return output;
            }
            catch (Exception e)
            {
                return IntPtr.Zero;
            }
        }

        private static int VFSCloseHandler(IntPtr stream)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                CloseFileStream?.Invoke(vfsStream.BackingStream);
                vfsStream.Dispose();
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }

            return VFSSuccessCode;
        }

        private static long VFSGetSizeHandler(IntPtr stream)
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

        private static long VFSGetPositionHandler(IntPtr stream)
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

        private static long VFSSetPositionHandler(IntPtr stream, long offset, int seekPosition)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                switch(seekPosition)
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

        public static long VFSReadHandler(IntPtr stream, IntPtr buffer, ulong len)
        {
            try
            {
                var vfsStream = StreamFromIntPtr(stream);
                var nativeBuffer = new byte[len];
                var readBytes = vfsStream.BackingStream.Read(nativeBuffer, 0, nativeBuffer.Length);
                Marshal.Copy(nativeBuffer, 0, buffer, readBytes);
                return readBytes;
            }
            catch (Exception e)
            {
                return VFSErrorCode;
            }
        }

        private static long VFSWriteHandler(IntPtr stream, IntPtr buffer, long len)
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

        private static int VFSFlushHandler(IntPtr stream)
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

        private static int VFSDeleteHandler(IntPtr path)
        {
            return VFSErrorCode;
        }

        private static int VFSRenameHandler(IntPtr oldPath, IntPtr newPath)
        {
            return VFSErrorCode;
        }
    }
}
