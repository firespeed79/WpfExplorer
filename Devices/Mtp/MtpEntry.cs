﻿using ExplorerCtrl;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace Devices.Mtp
{
    [DebuggerDisplay("{Type} - {FullName}")]
    public class MtpEntry : IEntry
    {
        private MtpDevice device;
        private MediaFileSystemInfo entry;

        internal MtpEntry(MtpDevice device, MediaFileSystemInfo entry)
        {
            this.device = device;
            this.entry = entry;
        }

        #region IExplorerItem

        #pragma warning disable 414, 67
        public event EventHandler<RefreshEventArgs> Refresh;

        public string Name
        {
            get
            {
                return this.entry.Name;
            }
            set
            {
                if (this.IsDirectory)
                {
                    MediaDirectoryInfo fileInfo = this.entry as MediaDirectoryInfo;
                    //fileInfo.MoveTo("xx");
                }
                else
                {
                    MediaFileInfo fileInfo = this.entry as MediaFileInfo;
                    //fileInfo.MoveTo("xx");
                }

            }
        }
       
        public string FullName
        {
            get { return this.entry.FullName; }
        }

        public string Link
        {
            // links not sopported for MTP
            get { return string.Empty; }
        }
              
        public long Size
        {
            get { return (long)this.entry.Length; }
        }

        public DateTime? Date
        {
            get { return this.entry.LastWriteTime;  }
        }

        public ExplorerItemType Type
        {
            get { return this.IsDirectory ? ExplorerItemType.Directory : ExplorerItemType.File; }
        }

        public ImageSource Icon 
        {
            get
            {
                switch (this.Type)
                {
                    case ExplorerItemType.Directory:
                        return DeviceIcons.Folder;
                    case ExplorerItemType.Link:
                        return DeviceIcons.Link;
                    case ExplorerItemType.File:
                        return DeviceIcons.File;
                    default:
                        return null;
                }
            }
        }

        public bool HasChildren
        {
            get
            {
                return this.Children.Any();
            }
        }

        public bool IsDirectory
        {
            get { return this.entry.Attributes.HasFlag(MediaFileAttributes.Directory) || this.entry.Attributes.HasFlag(MediaFileAttributes.Object); }
        }
        
        //public IDevice Device
        //{
        //    get
        //    {
        //        return (IDevice)this.device;
        //    }
        //}

        //public IEnumerable<IEntry> GetFolders()
        //{
        //    MediaDirectoryInfo dir = this.entry as MediaDirectoryInfo;
        //    if (dir == null)
        //    {
        //        throw new Exception();
        //    }
        //    return dir.EnumerateDirectories().Select(f => new MtpEntry(this.device, f)).OrderBy(f => f.Name);
        //}

        public IEnumerable<IExplorerItem> Children
        {
            get
            {
                MediaDirectoryInfo dir = this.entry as MediaDirectoryInfo;
                //if (dir == null)
                //{
                //    throw new Exception();
                //}
                return dir?.EnumerateFileSystemInfos().Select(f => new MtpEntry(this.device, f)).OrderBy(f => f.Name);
            }
        }

        //bool FolderExist(string path);

        public void CreateFolder(string folderName)
        {
            string path = Path.Combine(this.FullName, folderName);
            this.device.device.CreateDirectory(path);
        }

        public void Pull(string path, Stream stream)
        {
            this.device.device.DownloadFile(path, stream);
        }

        public void Push(Stream stream, string path)
        {
            this.device.device.UploadFile(stream, path);
        }

        #endregion

        #region IEntry

        public bool CanDelete { get { return true; } }
        public bool CanCreateFolder { get { return this.IsDirectory; } }
        public bool CanCreateLink { get { return false; } }
        public bool CanRename { get { return true; } }

        public void CreateLink(string linkName, string linkPath)
        {
            throw new NotSupportedException();
        }
        
        public void Delete()
        {
            if (this.device.device.DirectoryExists(this.FullName))
            {
                this.device.device.DeleteDirectory(this.FullName);
            }
            else if (this.device.device.FileExists(this.FullName))
            {
                this.device.device.DeleteFile(this.FullName);
            }
        }

        public void Rename(string newName)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
