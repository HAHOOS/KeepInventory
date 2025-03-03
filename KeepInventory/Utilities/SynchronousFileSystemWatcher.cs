using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace KeepInventory.Utilities
{
    /// <summary>
    /// Class that makes the <see cref="FileSystemWatcher"/> run events on the main thread
    /// </summary>
    public class SynchronousFileSystemWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher = new();

        /// <inheritdoc cref="FileSystemWatcher.EnableRaisingEvents"/>
        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set => _watcher.EnableRaisingEvents = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.NotifyFilter"/>
        public NotifyFilters NotifyFilter
        {
            get => _watcher.NotifyFilter;
            set => _watcher.NotifyFilter = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.Filter"/>
        public string Filter
        {
            get => _watcher.Filter;
            set => _watcher.Filter = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.Filters"/>
        public Collection<string> Filters
        {
            get => _watcher.Filters;
        }

        /// <inheritdoc cref="FileSystemWatcher.Path"/>
        public string Path
        {
            get => _watcher.Path;
            set => _watcher.Path = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.Renamed"/>
        public event EventHandler<RenamedEventArgs> Renamed;

        /// <inheritdoc cref="FileSystemWatcher.Created"/>
        public event EventHandler<FileSystemEventArgs> Created;

        /// <inheritdoc cref="FileSystemWatcher.Deleted"/>
        public event EventHandler<FileSystemEventArgs> Deleted;

        /// <inheritdoc cref="FileSystemWatcher.Changed"/>
        public event EventHandler<FileSystemEventArgs> Changed;

        /// <inheritdoc cref="System.ComponentModel.Component.Disposed"/>
        public event EventHandler Disposed;

        /// <inheritdoc cref="FileSystemWatcher.Error"/>
        public event EventHandler<ErrorEventArgs> Error;

        private readonly List<WatcherEvent> _Queue = [];

        /// <summary>
        /// Event queue
        /// </summary>
        public IReadOnlyList<WatcherEvent> Queue => _Queue.AsReadOnly();

        /// <summary>
        /// Initialize a new instance of <see cref="SynchronousFileSystemWatcher"/>
        /// </summary>
        public SynchronousFileSystemWatcher()
            => Init();

        /// <summary>
        /// Initialize a new instance of <see cref="SynchronousFileSystemWatcher"/>
        /// </summary>
        /// <param name="path"><inheritdoc cref="FileSystemWatcher.Path"/></param>
        public SynchronousFileSystemWatcher(string path)
        {
            _watcher.Path = path;
            Init();
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SynchronousFileSystemWatcher"/>
        /// </summary>
        /// <param name="path"><inheritdoc cref="FileSystemWatcher.Path"/></param>
        /// <param name="filter"><inheritdoc cref="FileSystemWatcher.Filter"/></param>
        public SynchronousFileSystemWatcher(string path, string filter)
        {
            _watcher.Path = path;
            _watcher.Filter = filter;
            Init();
        }

        private void Init()
        {
            _watcher.Renamed += (sender, e) => _Queue.Add(new(e));
            _watcher.Created += (sender, e) => _Queue.Add(new(e));
            _watcher.Deleted += (sender, e) => _Queue.Add(new(e));
            _watcher.Changed += (sender, e) => _Queue.Add(new(e));
            _watcher.Disposed += (sender, e) => _Queue.Add(new());
            _watcher.Error += (sender, e) => _Queue.Add(new(e));
            Core.Update += Update;
        }

        private void Update()
        {
            if (Queue.Count > 0)
            {
                for (int i = Queue.Count - 1; i >= 0; i--)
                {
                    var @event = _Queue[i];
                    try
                    {
                        if (@event.ChangeType == WatcherChangeTypes.Created)
                            Created?.Invoke(this, @event.ToFileSystemEventArgs());
                        else if (@event.ChangeType == WatcherChangeTypes.Deleted)
                            Deleted?.Invoke(this, @event.ToFileSystemEventArgs());
                        else if (@event.ChangeType == WatcherChangeTypes.Renamed)
                            Renamed?.Invoke(this, @event.ToRenamedEventArgs());
                        else if (@event.ChangeType == WatcherChangeTypes.Changed)
                            Changed?.Invoke(this, @event.ToFileSystemEventArgs());
                        else if (@event.ChangeType == null && @event.Exception != null)
                            Error?.Invoke(this, @event.ToErrorEventArgs());
                        else if (@event.ChangeType == null)
                            Disposed?.Invoke(this, new());
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.Error($"An unexpected error has occurred while triggering file system watcher events, exception:\n{ex}");
                    }
                    finally
                    {
                        _Queue.RemoveAt(i);
                    }
                }
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            _watcher.Dispose();
            Core.Update -= Update;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A class holding parameters to be then converted into the requested <see cref="EventArgs"/>
    /// </summary>
    public class WatcherEvent
    {
        /// <inheritdoc cref="FileSystemEventArgs.ChangeType"/>
        public WatcherChangeTypes? ChangeType { get; private set; }

        /// <inheritdoc cref="FileSystemEventArgs.Name"/>
        public string Name { get; private set; }

        /// <inheritdoc cref="FileSystemEventArgs.FullPath"/>
        public string FullPath { get; private set; }

        /// <inheritdoc cref="RenamedEventArgs.OldName"/>
        public string OldName { get; private set; }

        /// <inheritdoc cref="RenamedEventArgs.OldFullPath"/>
        public string OldFullPath { get; private set; }

        /// <inheritdoc cref="ErrorEventArgs.GetException"/>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="WatcherEvent"/>
        /// </summary>
        /// <param name="args">The <see cref="FileSystemEventArgs"/> to hold parameters of</param>
        public WatcherEvent(FileSystemEventArgs args)
        {
            this.ChangeType = args.ChangeType;
            this.Name = args.Name;
            this.FullPath = args.FullPath;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="WatcherEvent"/>
        /// </summary>
        /// <param name="args">The <see cref="RenamedEventArgs"/> to hold parameters of</param>
        public WatcherEvent(RenamedEventArgs args)
        {
            this.ChangeType = args.ChangeType;
            this.Name = args.Name;
            this.FullPath = args.FullPath;

            this.OldFullPath = args.OldFullPath;
            this.OldName = args.OldName;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="WatcherEvent"/>
        /// </summary>
        /// <param name="args">The <see cref="ErrorEventArgs"/> to hold parameters of</param>
        public WatcherEvent(ErrorEventArgs args)
            => this.Exception = args.GetException();

        /// <summary>
        /// Initializes a new instance of <see cref="WatcherEvent"/>
        /// </summary>
        public WatcherEvent()
        {
        }

        private static bool IsFile(string path)
            => Path.HasExtension(path);

        /// <summary>
        /// Converts to <see cref="FileSystemEventArgs"/>
        /// </summary>
        public FileSystemEventArgs ToFileSystemEventArgs()
            => new(ChangeType ?? WatcherChangeTypes.All, IsFile(FullPath) ? Path.GetDirectoryName(FullPath) : FullPath, IsFile(FullPath) ? Name : null);

        /// <summary>
        /// Converts to <see cref="RenamedEventArgs"/>
        /// </summary>
        public RenamedEventArgs ToRenamedEventArgs()
            => new(ChangeType ?? WatcherChangeTypes.All, IsFile(FullPath) ? Path.GetDirectoryName(FullPath) : FullPath, IsFile(FullPath) ? Name : null, IsFile(FullPath) ? Name : null);

        /// <summary>
        /// Converts to <see cref="ErrorEventArgs"/>
        /// </summary>
        public ErrorEventArgs ToErrorEventArgs()
            => new(Exception);
    }
}