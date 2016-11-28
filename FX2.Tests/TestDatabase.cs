// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FX2.Game.Beatmap;
using FX2.Game.Database;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace FX2.Tests
{
    // TODO: Fix these tests when running from resharper because they use files in the project folder
    public class TestDatabase
    {
        [Ignore("Fix local database testing")]
        [Test]
        public void TestScanningAndReloading()
        {
            SetIndex testSet;

            using(BeatmapDatabase database = new BeatmapDatabase("test_database", true))
            {
                string testPath = Path.Combine(Environment.CurrentDirectory, "TestMaps");

                // Record changes so they can be verified for this test
                HashSet<DifficultyIndex> difficulties = new HashSet<DifficultyIndex>();
                HashSet<SetIndex> sets = new HashSet<SetIndex>();
                
                database.DifficultyChanged += (sender, args) =>
                {
                    if(args.Type == DatabaseEventType.Added)
                        difficulties.Add(args.Difficulty);
                };
                
                database.SetChanged += (sender, args) =>
                {
                    if(args.Type == DatabaseEventType.Added)
                        sets.Add(args.Set);
                };

                database.AddSearchPath(testPath);
                database.StartSearching();
                while(database.IsSearchRunning)
                {
                    Thread.Sleep(10);
                    database.Update();
                }

                // Make sure all tasks completed
                Thread.Sleep(10);
                database.Update();

                testSet = database.Sets[0];

                // Ensure all 4 test sets are detected
                Assert.AreEqual(4, sets.Count);
            }

            // Test database reloads
            using(BeatmapDatabase database = new BeatmapDatabase("test_database"))
            {
                // Ensure all 4 test sets are detected
                Assert.AreEqual(4, database.Sets.Count);

                var testSet1 = database.Sets[0];

                Assert.AreEqual(testSet1.Difficulties.Count, testSet.Difficulties.Count);

                var diff = testSet.Difficulties[0];
                var diff1 = testSet1.Difficulties[0];

                Assert.AreEqual(diff.Path, diff1.Path);
                Assert.AreEqual(diff.MetaData.Artist, diff1.MetaData.Artist);
                Assert.AreEqual(diff.LastWriteTime, diff1.LastWriteTime);
            }
        }

        [Ignore("Fix local database testing")]
        [Test]
        public void TestLargeDatabase()
        {
            Stopwatch timer = new Stopwatch();

            using(BeatmapDatabase database = new BeatmapDatabase("large_database"))
            {
                string testPath = @"."; // Note: replace this with your own large KShoot folder for this test to be useful

                timer.Start();
                database.AddSearchPath(testPath);
                database.StartSearching();
                while(database.IsSearchRunning)
                {
                    Thread.Sleep(10);
                    database.Update();
                }

                // Make sure all tasks completed
                Thread.Sleep(10);
                database.Update();

                timer.Stop();
                Debug.WriteLine($"Finished scanning in {timer.Elapsed}, found {database.Sets.Count} maps and {database.Difficulties.Count} difficulties");
            }
        }

        public static void Main()
        {
            TestDatabase test = new TestDatabase();
            test.TestLargeDatabase();
        }
    }
}
