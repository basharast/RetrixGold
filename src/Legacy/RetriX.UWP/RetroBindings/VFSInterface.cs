using System.Runtime.InteropServices;

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
    [StructLayout(LayoutKind.Sequential)]
    public struct VFSInterface
    {
        private VFSGetPathDelegate getPath;
        public VFSGetPathDelegate GetPath
        {
            get => getPath;
            set { getPath = value; }
        }

        private VFSOpenDelegate open;
        public VFSOpenDelegate Open
        {
            get => open;
            set { open = value; }
        }

        private VFSCloseDelegate close;
        public VFSCloseDelegate Close
        {
            get => close;
            set { close = value; }
        }

        private VFSGetSizeDelegate getSize;
        public VFSGetSizeDelegate GetSize
        {
            get => getSize;
            set { getSize = value; }
        }

        private VFSGetPositionDelegate getPosition;
        public VFSGetPositionDelegate GetPosition
        {
            get => getPosition;
            set { getPosition = value; }
        }

        private VFSSetPositionDelegate setPosition;
        public VFSSetPositionDelegate SetPosition
        {
            get => setPosition;
            set { setPosition = value; }
        }

        private VFSReadDelegate read;
        public VFSReadDelegate Read
        {
            get => read;
            set { read = value; }
        }

        private VFSWriteDelegate write;
        public VFSWriteDelegate Write
        {
            get => write;
            set { write = value; }
        }

        private VFSFlushDelegate flush;
        public VFSFlushDelegate Flush
        {
            get => flush;
            set { flush = value; }
        }

        private VFSDeleteDelegate delete;
        public VFSDeleteDelegate Delete
        {
            get => delete;
            set { delete = value; }
        }

        private VFSRenameDelegate rename;
        public VFSRenameDelegate Rename
        {
            get => rename;
            set { rename = value; }
        }

        /* VFS API v2 */
        private VFSTruncateDelegate truncate;
        public VFSTruncateDelegate Truncate
        {
            get => truncate;
            set { truncate = value; }
        }

        /* VFS API v3 */
        private VFSStatDelegate stat;
        public VFSStatDelegate Stat
        {
            get => stat;
            set { stat = value; }
        }

        private VFSMkdirDelegate mkdir;
        public VFSMkdirDelegate Mkdir
        {
            get => mkdir;
            set { mkdir = value; }
        }

        private VFSOpendirDelegate opendir;
        public VFSOpendirDelegate Opendir
        {
            get => opendir;
            set { opendir = value; }
        }

        private VFSReaddirDelegate readdir;
        public VFSReaddirDelegate Readdir
        {
            get => readdir;
            set { readdir = value; }
        }

        private VFSDirentGetNameDelegate dirent_get_name;
        public VFSDirentGetNameDelegate DirentGetName
        {
            get => dirent_get_name;
            set { dirent_get_name = value; }
        }

        private VFSDirentIsDirDelegate dirent_is_dir;
        public VFSDirentIsDirDelegate DirentIsDir
        {
            get => dirent_is_dir;
            set { dirent_is_dir = value; }
        }

        private VFSClosedirDelegate closedir;
        public VFSClosedirDelegate Closedir
        {
            get => closedir;
            set { closedir = value; }
        }
    };
}
