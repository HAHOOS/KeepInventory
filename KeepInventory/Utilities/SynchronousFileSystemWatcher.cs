using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using MelonLoader;

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

        private readonly List<EventArgs> _Queue = [];

        /// <summary>
        /// Event queue
        /// </summary>
        public IReadOnlyList<EventArgs> Queue => _Queue.AsReadOnly();

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
            _watcher.Renamed += (sender, e) => _Queue.Add(e);
            _watcher.Created += (sender, e) => _Queue.Add(e);
            _watcher.Deleted += (sender, e) => _Queue.Add(e);
            _watcher.Changed += (sender, e) => _Queue.Add(e);
            _watcher.Disposed += (sender, e) => _Queue.Add(e);

            _watcher.Error += (sender, e) => _Queue.Add(e);
            MelonEvents.OnUpdate.Subscribe(Update);
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
                        if (@event is RenamedEventArgs renamed)
                        {
                            Renamed?.Invoke(this, renamed);
                        }
                        else if (@event is FileSystemEventArgs fse_args)
                        {
                            if (fse_args.ChangeType == WatcherChangeTypes.Created)
                                Created?.Invoke(this, fse_args);
                            else if (fse_args.ChangeType == WatcherChangeTypes.Deleted)
                                Deleted?.Invoke(this, fse_args);
                            else if (fse_args.ChangeType == WatcherChangeTypes.Changed)
                                Changed?.Invoke(this, fse_args);
                        }
                        else if (@event is ErrorEventArgs error)
                        {
                            Error?.Invoke(this, error);
                        }
                        else if (@event is EventArgs args)
                        {
                            try
                            {
                                Disposed?.Invoke(this, args);
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"SynchronousFileSystemWatcher | An unexpected error has occurred while running Disposed event, exception:\n{ex}");
                            }
                            finally
                            {
                                Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"SynchronousFileSystemWatcher | An unexpected error has occurred while triggering file system watcher events, exception:\n{ex}");
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
            try
            {
                _watcher.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
            MelonEvents.OnUpdate.Unsubscribe(Update);
            GC.SuppressFinalize(this);
        }
    }
}