// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using FX2.Game.Beatmap;
using NUnit.Framework;

namespace FX2.Tests
{
    [TestFixture]
    public class TestBeatmap
    {
        static Stream OpenTestMap(string mapName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            var resourceName = assembly.GetName().Name + $".TestMaps.{mapName}.ksh";

            return assembly.GetManifestResourceStream(resourceName);
        }

        [Test]
        public void TestKSHBeatmap1()
        {
            using(var file = OpenTestMap("soflan"))
            {
                // Load only metadata
                BeatmapKSH kshBeatmap = new BeatmapKSH(file, true);
                Assert.IsEmpty(kshBeatmap.Measures);
                Assert.IsNotEmpty(kshBeatmap.Options);
                Assert.AreEqual(20, kshBeatmap.Options.Count);
                Assert.AreEqual("ノリニクシティ・マックス！混乱帝国そふらんカレー＼こんなあおにゃんありえなーーい!!／", kshBeatmap.Options["illustrator"]);
                Assert.AreEqual("32-259", kshBeatmap.Options["t"]);
                Assert.AreEqual("140d", kshBeatmap.Options["ver"]);
            }

            using(var file = OpenTestMap("soflan"))
            {
                // Load again but fully this time
                BeatmapKSH kshBeatmap = new BeatmapKSH(file, false);
                Assert.IsNotEmpty(kshBeatmap.Measures);

                var firstMeasure = kshBeatmap.Measures[0];
                Assert.AreEqual(1, firstMeasure.Ticks.Length);

                var firstTick = firstMeasure.Ticks[0];
                Assert.AreEqual(2, firstTick.Options.Count);
                Assert.AreEqual("230", firstTick.Options["t"]);

                Assert.AreEqual(BeatmapKSH.Tick.LaserNone, firstTick.Lasers[0]);
                Assert.AreEqual(BeatmapKSH.Tick.LaserNone, firstTick.Lasers[1]);
            }
        }

        [Test]
        public void TestKSHBeatmap2()
        {
            using(var file = OpenTestMap("C18H27NO3"))
            {
                BeatmapKSH kshBeatmap = new BeatmapKSH(file);
                Assert.IsNotEmpty(kshBeatmap.Options);
                Assert.AreEqual(19, kshBeatmap.Options.Count);
                Assert.AreEqual("C18H27NO3", kshBeatmap.Options["title"]);
                Assert.AreEqual("210", kshBeatmap.Options["t"]);
                Assert.AreEqual("160", kshBeatmap.Options["ver"]);
                Assert.AreEqual("zMFGXvEzqCnbudCN.png", kshBeatmap.Options["jacket"]);

                Assert.IsNotEmpty(kshBeatmap.Measures);
                var m0 = kshBeatmap.Measures[0];
                Assert.AreEqual(1, m0.Ticks.Length);
                var t0 = m0.Ticks[0];
                for(int i = 0; i < 6; i++)
                    Assert.AreEqual('0', t0.Button[i]);
            }
        }

        [Test]
        public void TestTimingPoint()
        {
            TimingPoint testPoint = new TimingPoint();
            testPoint.Offset = 0.2;
            testPoint.BPM = 200.0;

            // 4/4 measure
            double bpmStep = 60.0 / 200.0 * 4;
            Assert.AreEqual(0.2, testPoint.GetMeasureOffset(0));
            Assert.AreEqual(0.2 + bpmStep, testPoint.GetMeasureOffset(1));

            // 3/4 measure
            testPoint.Numerator = 3;
            bpmStep = 60.0 / 200.0 * 3;
            Assert.AreEqual(0.2 + bpmStep, testPoint.GetMeasureOffset(1));

            // 6/4 measure
            testPoint.Numerator = 6;
            Assert.AreEqual(0.2 + bpmStep * 2, testPoint.GetMeasureOffset(1));

            // 6/6 measure
            testPoint.Denominator = 6;
            bpmStep = 60.0 / 200.0 * 4;
            Assert.AreEqual(0.2 + bpmStep, testPoint.GetMeasureOffset(1));

            // 3/6 measure
            testPoint.Numerator = 3;
            bpmStep = 60.0 / 200.0 * 4 / 6 * 3;
            Assert.AreEqual(0.2 + bpmStep, testPoint.GetMeasureOffset(1));

            // 5/8 measure
            testPoint.Numerator = 5;
            testPoint.Denominator = 8;
            double stepLength = 60 / testPoint.BPM * 4 / 8;
            Assert.AreEqual(0.2 + stepLength * 5, testPoint.GetMeasureOffset(1));

            // 3/8 measure
            testPoint.Numerator = 3;
            testPoint.Denominator = 8;
            stepLength = 60 / testPoint.BPM * 4 / 8;
            Assert.AreEqual(0.2 + stepLength * 3, testPoint.GetMeasureOffset(1));
        }

        [Test]
        public void TestRemoveInsert()
        {
            TimingPoint testPoint = new TimingPoint();
            var m0 = testPoint.AddMeasure();
            var m1 = testPoint.AddMeasure();
            var m2 = testPoint.AddMeasure();
            Assert.AreEqual(0, m0.Index);
            Assert.AreEqual(2, m2.Index);

            // Remove
            testPoint.RemoveMeasure(m1);
            Assert.AreEqual(0, m0.Index);
            Assert.AreEqual(1, m2.Index);

            // Insert
            testPoint.InsertMeasure(1, m1);
            Assert.AreEqual(0, m0.Index);
            Assert.AreEqual(1, m1.Index);
            Assert.AreEqual(2, m2.Index);
        }

        [Test]
        public void TestObjectPositions()
        {
            TimingPoint testPoint = new TimingPoint();
            testPoint.Offset = 0;
            testPoint.BPM = 200.0;
            testPoint.Numerator = 4;
            testPoint.Denominator = 4;

            var m0 = testPoint.AddMeasure();

            // 4 4th notes
            var b0 = new Button {Position = new TimeDivision(0, 4)};
            var b1 = new Button {Position = new TimeDivision(1, 4)};
            Assert.AreEqual(1.0 / 4.0, b1.Position.Relative);
            var b2 = new Button {Position = new TimeDivision(2, 4)};
            var b3 = new Button {Position = new TimeDivision(3, 4)};
            m0.Objects.Add(b2);
            m0.Objects.Add(b3);
            m0.Objects.Add(b1);
            m0.Objects.Add(b0);

            m0.Sort();
            Assert.AreEqual(0, m0.Objects.IndexOf(b0));
            Assert.AreEqual(3, m0.Objects.IndexOf(b3));

            // 8th note at 5
            var b25 = new Button {Position = new TimeDivision(5, 8)};
            m0.Objects.Add(b25);
            m0.Sort();

            Assert.Less(m0.Objects.IndexOf(b2), m0.Objects.IndexOf(b25));
            Assert.Less(m0.Objects.IndexOf(b25), m0.Objects.IndexOf(b3));

            Assert.AreEqual(60 / 200.0, b1.GetAbsolutePosition(m0));
            Assert.AreEqual(60 / 200.0 * 2, b2.GetAbsolutePosition(m0));
            Assert.AreEqual(60 / 400.0 * 5, b25.GetAbsolutePosition(m0));

            testPoint.InsertMeasure(0, new Measure());
            Assert.AreEqual(60 / 200.0 * 5, b1.GetAbsolutePosition(m0));
        }

        [Test]
        public void TestConvertedKSHBeatmap()
        {
            Beatmap beatmap;

            using(var file = OpenTestMap("C18H27NO3"))
            {
                // Load only metadata
                BeatmapKSH kshBeatmap = new BeatmapKSH(file);
                beatmap = new Beatmap(kshBeatmap);
            }

            Assert.AreEqual("RITSUKI", beatmap.Metadata.Illustrator);
            Assert.AreEqual("grv.ogg", beatmap.Metadata.AudioPath);
            Assert.IsEmpty(beatmap.Metadata.EffectedAudioPath);

            Assert.AreEqual(3, beatmap.TimingPoints.Count);

            var tp0 = beatmap.TimingPoints[0];
            Assert.AreEqual(210, tp0.BPM);
            Assert.AreEqual(4, tp0.Numerator);
            Assert.AreEqual(4, tp0.Denominator);

            var tp1 = beatmap.TimingPoints[1];
            Assert.AreEqual(1, tp1.Numerator);
            Assert.AreEqual(96, tp1.Denominator);

            Assert.IsNotEmpty(tp0.Measures);

            var m0 = tp0.Measures[0];
            Assert.IsEmpty(m0.Objects);

            var m1 = tp0.Measures[1];
            Assert.IsNotEmpty(m1.Objects);

            // 2 lasers starting here
            var obj0 = m1.Objects[0];
            var obj1 = m1.Objects[1];
            Assert.IsInstanceOf<LaserRoot>(obj0);
            Assert.IsInstanceOf<LaserRoot>(obj1);
            var l1 = obj1 as LaserRoot;
            Assert.AreEqual(1, l1.Index);
            Assert.AreEqual(1.0f, l1.HorizontalPosition); // Laser 1 starting on the right side

            var m3 = tp0.Measures[3];
            Assert.IsNotEmpty(m3.Objects);
        }

        [Test]
        public void TestBeatmapPlayback()
        {
            Beatmap beatmap;

            using(var file = OpenTestMap("C18H27NO3"))
            {
                // Load only metadata
                BeatmapKSH kshBeatmap = new BeatmapKSH(file);
                beatmap = new Beatmap(kshBeatmap);
            }

            var t0 = beatmap.TimingPoints[0];

            BeatmapPlayback playback = new BeatmapPlayback();
            playback.Beatmap = beatmap;
            playback.ViewDuration = t0.MeasureDuration * 2;

            // Store objects
            HashSet<ObjectReference> objectSet = new HashSet<ObjectReference>(playback.ObjectsInView);
            Assert.AreEqual(8, objectSet.Count); // 8 Laser points

            playback.ObjectLeft += reference => objectSet.Remove(reference);

            // Advance a few measures
            playback.Position = t0.GetMeasureOffset(10);

            // Check if objects went out of view
            Assert.IsEmpty(objectSet);

            // Check addition
            playback.ObjectEntered += reference => objectSet.Add(reference);
            playback.Position = 0.0;

            Assert.AreEqual(8, objectSet.Count); // 8 Laser points again
        }

        [Test]
        public void TestCompleteMapIteration()
        {
            Beatmap beatmap;

            using(var file = OpenTestMap("C18H27NO3"))
            {
                // Load only metadata
                BeatmapKSH kshBeatmap = new BeatmapKSH(file);
                beatmap = new Beatmap(kshBeatmap);
            }

            BeatmapPlayback playback = new BeatmapPlayback();
            playback.Beatmap = beatmap;
            playback.ViewDuration = 1.0;

            // Store objects
            HashSet<ObjectReference> objectSet = new HashSet<ObjectReference>(playback.ObjectsInView);
            int passedObjects = 0;

            playback.ObjectEntered += reference =>
            {
                Assert.That(objectSet, Has.No.Member(reference));
                objectSet.Add(reference);
                passedObjects++;
            };
            playback.ObjectLeft += reference =>
            {
                Assert.That(objectSet, Has.Member(reference));
                objectSet.Remove(reference);
            };

            // Simulate game loop
            double timeStep = 1.0 / 240.0;
            while(!playback.HasEnded)
            {
                playback.Position += timeStep;
            }

            Assert.Greater(playback.LastObject.AbsolutePosition, 70.0);
            Assert.Greater(playback.Position, playback.LastObject.AbsolutePosition);
            Assert.IsEmpty(objectSet);
        }

        [Test]
        public void TestLaserVisiblity()
        {
            Beatmap beatmap = new Beatmap();
            TimingPoint tp;
            beatmap.TimingPoints.Add(tp = new TimingPoint
            {
                BPM = 240.0f,
            });

            var measure0 = tp.AddMeasure();
            var measure1 = tp.AddMeasure();
            var measure2 = tp.AddMeasure();

            var laserRoot = new LaserRoot
            {
                HorizontalPosition = 0.0f,
                Index = 0,
                Position = new TimeDivision(0, 4)
            };
            laserRoot.Root = new ObjectReference(laserRoot, measure0);
            var laserEnd = new Laser
            {
                HorizontalPosition = 1.0f,
                Previous = laserRoot.Root,
                Position = new TimeDivision(0, 4)
            };
            laserEnd.Root = laserRoot.Root;
            laserRoot.Next = new ObjectReference(laserEnd, measure2);

            measure0.Objects.Add(laserRoot);
            measure2.Objects.Add(laserEnd);

            BeatmapPlayback playback = new BeatmapPlayback();
            playback.ViewDuration = 0.5f;
            playback.Position = 1.0f;
            playback.Beatmap = beatmap;

            var lr = new ObjectReference(laserRoot, measure0);
            Assert.Contains(lr, measure1.CrossingObjects);

            Assert.AreEqual(1, playback.MeasuresInView.Count);
            Assert.AreEqual(measure1, playback.MeasuresInView[0]);
            Assert.IsTrue(playback.ObjectsInView.Contains(lr));
        }

        [Test]
        public void TestActiveObjects()
        {
            Beatmap beatmap;

            using(var file = OpenTestMap("C18H27NO3"))
            {
                // Load only metadata
                BeatmapKSH kshBeatmap = new BeatmapKSH(file);
                beatmap = new Beatmap(kshBeatmap);
            }

            BeatmapPlayback playback = new BeatmapPlayback();
            playback.Beatmap = beatmap;
            playback.ViewDuration = 0.2;
            playback.Position = 3.0;

            Assert.AreEqual(2, playback.ActiveObjects.Count);

            playback.Position = 13.285;
            Assert.AreEqual(1, playback.ActiveObjects.Count);
            Assert.IsAssignableFrom(typeof(Laser), playback.ActiveObjects[0].Object);
            
            // Same test but with larger view duration
            playback.ViewDuration = 1.0;
            Assert.AreEqual(1, playback.ActiveObjects.Count);
            Assert.IsAssignableFrom(typeof(Laser), playback.ActiveObjects[0].Object);
            
            playback.Position = 25.285;
            Assert.AreEqual(3, playback.ActiveObjects.Count);
            var hold = playback.ActiveObjects.First(x => x.Object is Hold);
            Assert.IsNotNull(hold);

            // Test events
            int holdCount = 3;
            BeatmapPlayback.ObjectEventHandler handler = reference => { holdCount--; };
            playback.ObjectDeactivated += handler;
            playback.Position = 0.0;
            playback.ObjectDeactivated -= handler;

            Assert.AreEqual(0, holdCount);

            handler = reference => { holdCount++; };
            playback.ObjectActivated += handler;

            playback.Position = 35.571;
            Assert.AreEqual(2, holdCount);
        }

        [STAThread]
        static void Main()
        {
            TestBeatmap test = new TestBeatmap();
            test.TestLaserVisiblity();
        }
    }
}