// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FX2.Game.Beatmap;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Lists;
using osu.Framework.Threading;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Win32;

namespace FX2.Game.Database
{
    /// <summary>
    /// Class for interacting with the beatmap database
    /// </summary>
    public class BeatmapDatabase : IDisposable
    {
        public const int Version = 1;
        public static readonly string DatabaseFilename = "database";

        private readonly DatabaseBlobSerializer serializer = new DatabaseBlobSerializer();
        private readonly ISQLitePlatform sqlitePlatform;
        private readonly string databasePath;

        private CancellationTokenSource cancellationTokenSource = null;

        private SQLiteConnection sqliteConnection;

        private readonly HashSet<string> searchPaths = new HashSet<string>();

        /// <summary>
        /// Scheduler that performs operations on the database on the main thread
        /// </summary>
        private readonly Scheduler databaseScheduler;

        /// <summary>
        /// The thread that will discover maps and push changes to the scheduler to process on the main thread
        /// </summary>
        private Thread scanThread;

        private int nextSetId = 0;
        private int nextDifficultyId = 0;

        /// <summary>
        /// An in memory list of sets
        /// </summary>
        private readonly Dictionary<int, SetIndex> sets = new Dictionary<int, SetIndex>(1000);

        /// <summary>
        /// An in memory list of difficulties
        /// </summary>
        private readonly Dictionary<int, DifficultyIndex> difficulties = new Dictionary<int, DifficultyIndex>(10000);

        /// <summary>
        /// A list of last write times that is to be used exculusively by the search thread
        /// </summary>
        private readonly Dictionary<string, DateTime> difficultyLastWriteTimes = new Dictionary<string, DateTime>();

        /// <summary>
        /// A list of sets by folder name
        /// </summary>
        private readonly Dictionary<string, SetIndex> setsByFolderName = new Dictionary<string, SetIndex>();

        public BeatmapDatabase(bool forceRebuild = false)
            : this(DatabaseFilename, forceRebuild)
        {
        }

        public BeatmapDatabase(string databaseFileName, bool forceRebuild = false)
        {
            databasePath = Path.Combine(Environment.CurrentDirectory, databaseFileName);
            sqlitePlatform = new SQLitePlatformWin32();

            OpenDatabase();

            databaseScheduler = new Scheduler(Thread.CurrentThread);

            // Check if database if valid or needs to be rebuild
            bool rebuild = forceRebuild;
            try
            {
                var settings = sqliteConnection.Table<DatabaseSettings>().FirstOrDefault();
                if(settings == null)
                    rebuild = true;
                else
                {
                    if(settings.Version != Version)
                        rebuild = true;
                }

            }
            catch(SQLiteException)
            {
                rebuild = true;
            }

            if(rebuild)
            {
                sqliteConnection.Close();
                File.Delete(databasePath); // Delete entire database
                OpenDatabase();
                sqliteConnection.CreateTable<DatabaseSettings>();
                sqliteConnection.CreateTable<SetIndex>();
                sqliteConnection.CreateTable<DifficultyIndex>();

                // Create settings
                sqliteConnection.CreateTable<DatabaseSettings>();
                sqliteConnection.Insert(new DatabaseSettings {Version = Version});
            }
            else
            {
                LoadIndex();
            }
        }

        public void Dispose()
        {
            EndSearching();
            sqliteConnection.Dispose();
        }

        /// <summary>
        /// True if a search for new/removed maps is currently running
        /// </summary>
        public bool IsSearchRunning { get; private set; } = false;

        /// <summary>
        /// All the beatmap sets
        /// </summary>
        public IReadOnlyDictionary<int, SetIndex> Sets => sets;

        /// <summary>
        /// All the difficulties
        /// </summary>
        public IReadOnlyDictionary<int, DifficultyIndex> Difficulties => difficulties;

        public event EventHandler<DifficultyChangedEventArgs> DifficultyChanged;
        public event EventHandler<SetChangedEventArgs> SetChanged;
        public event EventHandler SearchFinished;

        public void StartSearching()
        {
            if(IsSearchRunning)
                return; // Search thread already running

            // Fill list of last write times
            difficultyLastWriteTimes.Clear();
            foreach(var diff in difficulties.Values)
            {
                difficultyLastWriteTimes.Add(diff.Path, diff.LastWriteTime);
            }

            IsSearchRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            scanThread = new Thread(() => RunBeatmapScanner(cancellationTokenSource.Token));
            scanThread.Start();
        }

        public void EndSearching()
        {
            if(!IsSearchRunning)
                return;

            using(cancellationTokenSource)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Token.WaitHandle.WaitOne();
                scanThread.Join();
            }
        }

        /// <summary>
        /// Adds a new search path
        /// </summary>
        /// <remarks>Paths can't be added or removed while running</remarks>
        /// <param name="searchPath"></param>
        public void AddSearchPath(string searchPath)
        {
            if(IsSearchRunning)
                throw new InvalidOperationException("Can't add paths while running");

            searchPaths.Add(searchPath);
        }

        public void RemoveSearchPath(string searchPath)
        {
            if(IsSearchRunning)
                throw new InvalidOperationException("Can't remove paths while running");

            searchPaths.Remove(searchPath);
        }

        private void OpenDatabase()
        {
            sqliteConnection?.Dispose();
            sqliteConnection = new SQLiteConnection(sqlitePlatform, databasePath, serializer: serializer, storeDateTimeAsTicks: true);
        }

        private void LoadIndex()
        {
            nextSetId = 0;
            nextDifficultyId = 0;

            sqliteConnection.Table<SetIndex>().ForEach(x =>
            {
                sets.Add(x.Id, x);
                setsByFolderName.Add(x.RootPath, x);

                nextSetId = Math.Max(nextSetId, x.Id + 1);
            });

            sqliteConnection.Table<DifficultyIndex>().ForEach(x =>
            {
                SetIndex set;
                if(sets.TryGetValue(x.SetId, out set))
                {
                    difficulties.Add(x.Id, x);

                    // Add sets to it's appropriate difficulty
                    sets[x.SetId].Difficulties.Add(x);

                    nextDifficultyId = Math.Max(nextDifficultyId, x.Id + 1);
                } 
            });
        }
        
        /// <summary>
        /// Processes pending database operations synchronously
        /// </summary>
        public void Update()
        {
            int tasks = databaseScheduler.Update();
            if(tasks > 0)
            {
                Debug.WriteLine($"Performed {tasks} scheduled database tasks");
            }
        }

        private void RunBeatmapScanner(CancellationToken token)
        {
            List<FileInfo> mapFiles = new List<FileInfo>();

            // Files that should be found in the result so they are not considered for removal
            HashSet<string> filesToRemove = new HashSet<string>(difficultyLastWriteTimes.Keys);

            // Scan folder layout
            foreach(var searchPath in searchPaths)
            {
                foreach(var file in Directory.EnumerateFiles(searchPath, "*.ksh", SearchOption.AllDirectories))
                {
                    if(token.IsCancellationRequested)
                        return;

                    var fileInfo = new FileInfo(file);
                    mapFiles.Add(fileInfo);
                    filesToRemove.Remove(fileInfo.FullName); // Do not remove this map, it still exists
                }
            }

            // Process removals
            databaseScheduler.Add(() => RemoveDifficulties(filesToRemove));

            // Process found map files
            foreach(var mapFile in mapFiles)
            {
                if(!IsDifficultyAddedOrChanged(mapFile))
                    continue; // Skip unchanged maps

                // Try to load map metadata
                try
                {
                    var stream = File.OpenRead(mapFile.FullName);
                    BeatmapKsh mapKsh = new BeatmapKsh(stream, true);
                    Beatmap.Beatmap map = new Beatmap.Beatmap(mapKsh);
                    databaseScheduler.Add(() => { AddDifficulty(mapFile, map.Metadata); });
                }
                catch(BeatmapParserException)
                {
                    Debug.WriteLine($"Corrupted map [{mapFile.Name}], not adding it to the database");
                    if(difficultyLastWriteTimes.ContainsKey(mapFile.FullName))
                    {
                        // Difficulty existed, remove it now since it is corrupt
                        databaseScheduler.Add(() => RemoveDifficulties(new HashSet<string> {mapFile.FullName}));
                    }
                }
            }

            // Notify finished
            databaseScheduler.Add(() => SearchFinished?.Invoke(this, null));
            IsSearchRunning = false;
        }

        private bool IsDifficultyAddedOrChanged(FileInfo mapFile)
        {
            DateTime lastWriteTime;
            if(!difficultyLastWriteTimes.TryGetValue(mapFile.FullName, out lastWriteTime))
                return true;

            return lastWriteTime != mapFile.LastWriteTimeUtc;
        }

        private SetIndex GetOrCreateSet(FileInfo mapFile, BeatmapMetadata mapMetadata)
        {
            SetIndex set;
            if(!setsByFolderName.TryGetValue(mapFile.DirectoryName, out set))
            {
                // Create a new set with the given metadata
                set = new SetIndex();
                set.Id = nextSetId++;
                set.Artist = mapMetadata.Artist;
                set.Creator = mapMetadata.Creator;
                set.Title = mapMetadata.Title;
                set.RootPath = mapFile.DirectoryName;

                sets.Add(set.Id, set);
                setsByFolderName.Add(set.RootPath, set);

                // Update database
                sqliteConnection.Insert(set);

                // Notify
                SetChanged?.Invoke(this, new SetChangedEventArgs {Set = set, Type = DatabaseEventType.Added});
            }
            return set;
        }

        private void RemoveDifficulties(HashSet<string> difficultiesToRemove)
        {
            foreach(var diffToRemove in difficultiesToRemove)
            {
                string folder = Path.GetDirectoryName(diffToRemove);
                SetIndex set;
                if(setsByFolderName.TryGetValue(folder, out set))
                {
                    var diff = set.Difficulties.FirstOrDefault(x => x.Path == diffToRemove);
                    difficulties.Remove(diff.Id);
                    set.Difficulties.Remove(diff);
                    if(set.Difficulties.Count == 0)
                    {
                        // Remove set, since it is empty now
                        setsByFolderName.Remove(set.RootPath);
                        sets.Remove(set.Id);

                        // Update database
                        sqliteConnection.Delete<SetIndex>(set.Id);

                        // Notify
                        SetChanged?.Invoke(this, new SetChangedEventArgs {Set = set, Type = DatabaseEventType.Removed});
                    }
                }
            }
        }

        private void AddDifficulty(FileInfo mapFile, BeatmapMetadata mapMetadata)
        {
            var set = GetOrCreateSet(mapFile, mapMetadata);
            var existing = set.Difficulties.FirstOrDefault(x => x.Path == mapFile.FullName);

            // Update existing or create new
            DifficultyIndex difficulty = existing ?? new DifficultyIndex();

            if(existing == null)
            {
                difficulty.Id = nextDifficultyId++;
                difficulty.SetId = set.Id;
                difficulty.Path = mapFile.FullName;
                difficulties.Add(difficulty.Id, difficulty);
                set.Difficulties.Add(difficulty);
            }

            difficulty.LastWriteTime = mapFile.LastWriteTimeUtc;
            difficulty.MetaData = mapMetadata;

            if(existing != null)
            {
                // Update database
                sqliteConnection.Update(difficulty);
            }
            else
            {
                // Insert into database
                sqliteConnection.Insert(difficulty);
            }

            // Notify
            DifficultyChanged?.Invoke(this,
                new DifficultyChangedEventArgs()
                {
                    Difficulty = difficulty,
                    Type = existing != null ? DatabaseEventType.Changed : DatabaseEventType.Added
                });
        }
    }
}