// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Linq;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Class that manages selecting subsets of data inside of a <see cref="Beatmap"/> and sending events for passed objects
    /// </summary>
    public class BeatmapPlayback
    {
        private Beatmap beatmap;
        private double position;
        private double endPosition;
        private double viewDuration = 2.0;

        private Measure currentMeasure;

        private List<ObjectReference> objectsInView = new List<ObjectReference>();
        private List<ObjectReference> activeObjects = new List<ObjectReference>();
        private List<Measure> measuresInView = new List<Measure>();
        private List<Measure> allMeasures = new List<Measure>();

        public Beatmap Beatmap
        {
            get { return beatmap; }
            set { SetBeatmap(value); }
        }

        public double Position
        {
            get { return position; }
            set
            {
                position = value;
                UpdateView();
            }
        }

        /// <summary>
        /// True if the position of this map is past the last object
        /// </summary>
        public bool HasEnded { get; private set; }

        /// <summary>
        /// Objects currently in view
        /// </summary>
        public IReadOnlyList<ObjectReference> ObjectsInView => objectsInView;
        
        /// <summary>
        /// Objects whose start/end lie around the current playback position, for hold/lasers
        /// </summary>
        public IReadOnlyList<ObjectReference> ActiveObjects => activeObjects;

        /// <summary>
        /// Measures currently in view
        /// </summary>
        public IReadOnlyList<Measure> MeasuresInView => measuresInView;

        /// <summary>
        /// All measures in the current beatmap
        /// </summary>
        public IReadOnlyList<Measure> Measures => allMeasures;

        /// <summary>
        /// First object in the beatmap
        /// </summary>
        public ObjectReference FirstObject { get; private set; }

        /// <summary>
        /// Last object in the beatmap
        /// </summary>
        public ObjectReference LastObject { get; private set; }

        /// <summary>
        /// View distance in seconds
        /// </summary>
        public double ViewDuration
        {
            get { return viewDuration; }
            set
            {
                viewDuration = value;
                ViewDurationChanged?.Invoke(this, null);
                UpdateView();
            }
        }

        public delegate void ObjectEventHandler(ObjectReference obj);

        public delegate void MeasureEventHandler(Measure measure);

        public event ObjectEventHandler ObjectEntered;
        public event ObjectEventHandler ObjectLeft;

        public event ObjectEventHandler ObjectActivated;
        public event ObjectEventHandler ObjectDeactivated;

        public event MeasureEventHandler MeasureEntered;
        public event MeasureEventHandler MeasureLeft;

        public event EventHandler ViewDurationChanged;

        /// <summary>
        /// Call this to notify that the current beamap's contents were changed
        /// </summary>
        public void OnBeatmapModified()
        {
            beatmap = null;
            currentMeasure = null; // Invalidate cached measure
            SetBeatmap(beatmap);
        }
        
        /// <summary>
        /// Sets the beatmap to use for source data
        /// </summary>
        protected void SetBeatmap(Beatmap beatmap)
        {
            // Reset state when beatmap is changed
            if(this.beatmap != beatmap)
            {
                this.beatmap = beatmap;
                foreach(var obj in objectsInView)
                    OnObjectLeft(obj);
                foreach(var obj in activeObjects)
                    OnObjectDeactivated(obj);

                objectsInView.Clear();
                allMeasures.Clear();
                currentMeasure = null;
                HasEnded = true;
                LastObject = null;
                FirstObject = null;

                // Stop here when beatmap is cleared
                if(this.beatmap == null)
                    return;

                beatmap.UpdateLaserIntervals();
            }

            UpdateView();

            allMeasures.Clear();
            foreach(var timingPoint in this.beatmap.TimingPoints)
            {
                allMeasures.AddRange(timingPoint.Measures);
            }

            // Find first object
            FirstObject = null;
            foreach(var measure in allMeasures)
            {
                foreach(var obj in measure.Objects)
                {
                    // TODO: Ignore events
                    FirstObject = new ObjectReference(obj, measure);
                    break;
                }

                if(FirstObject != null) break;
            }

            // Find last object
            LastObject = null;
            for(int m = allMeasures.Count - 1; m >= 0; m--)
            {
                var measure = allMeasures[m];
                for(int o = measure.Objects.Count - 1; o >= 0; o--)
                {
                    var obj = measure.Objects[o];

                    // TODO: Ignore events
                    LastObject = new ObjectReference(obj, measure);
                    break;
                }

                if(LastObject != null) break;
            }
        }

        /// <summary>
        /// Updates the list of visible objects
        /// </summary>
        protected void UpdateView()
        {
            if(beatmap == null)
                return;

            currentMeasure = SelectMeasure(position);

            var measure = currentMeasure;
            var timingPoint = measure.TimingPoint;
            endPosition = position + viewDuration;

            var addedObjects = new HashSet<ObjectReference>();
            var newMeasures = new List<Measure>();

            // Iterate over objects in view
            bool endReached = false;
            while(!endReached)
            {
                if(measure.AbsolutePosition > endPosition)
                    break;

                // Add measure to the list of current measure
                if(!measuresInView.Contains(measure))
                {
                    OnMeasureEntered(measure);
                }
                newMeasures.Add(measure);

                foreach(var obj in measure.Objects)
                {
                    var objRef = new ObjectReference(obj, measure);
                    if(!objectsInView.Contains(objRef))
                    {
                        if(GetObjectEndingPosition(objRef) < position || GetObjectStartingPosition(objRef) > endPosition)
                            continue;

                        double objectPosition = obj.GetAbsolutePosition(measure);
                        if(objectPosition >= endPosition)
                        {
                            endReached = true;
                            break;
                        }

                        // Add this object to the view list
                        if(!objectsInView.Contains(objRef))
                        {
                            OnObjectEntered(objRef);
                        }
                        addedObjects.Add(objRef);
                    }
                }

                // Also add crossing objects
                foreach(var objRef in measure.CrossingObjects)
                {
                    if(!objectsInView.Contains(objRef))
                    {
                        if(GetObjectEndingPosition(objRef) < position || GetObjectStartingPosition(objRef) > endPosition)
                            continue;

                        // Add this object to the view list
                        if(!objectsInView.Contains(objRef))
                        {
                            OnObjectEntered(objRef);
                        }
                        addedObjects.Add(objRef);
                    }
                }

                if(endReached) break;

                // Next measure
                if(measure.Next == null)
                {
                    while(true)
                    {
                        // Next timing point
                        if(timingPoint.Next == null)
                        {
                            endReached = true;
                            break;
                        }

                        timingPoint = timingPoint.Next;
                        if(timingPoint.Measures.Count > 0)
                        {
                            measure = timingPoint.Measures[0];
                            break;
                        }
                    }
                }
                else
                {
                    // Next measure
                    measure = measure.Next;
                }
            }

            MarkPassedObjects();
            objectsInView.AddRange(addedObjects);
            measuresInView = newMeasures;

            // Handle new active objects
            foreach(var obj in addedObjects)
            {
                var objStart = obj.AbsolutePosition;
                // Handle new active objects
                if(objStart < position)
                {
                    if(!activeObjects.Contains(obj))
                    {
                        OnObjectActivated(obj);
                        activeObjects.Add(obj);
                    }
                }
            }

            if(objectsInView.Count == 0)
            {
                // Update ended state
                HasEnded = LastObject == null || position > LastObject.AbsolutePosition;
            }
            else
            {
                // Can't have ended
                HasEnded = false;
            }
        }

        /// <summary>
        /// Select the timing point affecting the given position
        /// </summary>
        protected TimingPoint SelectTimingPoint(double position)
        {
            foreach(var timingPoint in beatmap.TimingPoints)
            {
                var next = timingPoint.Next;
                if(next == null)
                    return timingPoint;
                if(next.Offset > position)
                    return timingPoint;
            }

            return null;
        }

        /// <summary>
        /// Checks if the current measure is still valid, this acts as an optimization so that SelectMeasure doesn't have to do anything.
        /// </summary>
        protected bool IsCurrentMeasureValid(double position)
        {
            if(currentMeasure == null)
                return false;

            var measurePosition = currentMeasure.AbsolutePosition;
            if(measurePosition > position)
                return false;

            var measureDuration = currentMeasure.TimingPoint.MeasureDuration;
            if((measurePosition + measureDuration) < position)
                return false;

            // Skipped over to next timing point
            if(currentMeasure.TimingPoint.Next != null && currentMeasure.TimingPoint.Next.Offset < position)
                return false;

            // Skipped over to previous timing point
            if(currentMeasure.TimingPoint.Previous != null &&  currentMeasure.TimingPoint.Offset > position)
                return false;

            return true;
        }

        /// <summary>
        /// Select the measure containing the given position
        /// </summary>
        protected Measure SelectMeasure(double position)
        {
            if(IsCurrentMeasureValid(position))
                return currentMeasure;

            var timingPoint = SelectTimingPoint(position);
            if(timingPoint == null) return null;

            while(timingPoint.Measures.Count == 0)
            {
                timingPoint = timingPoint.Previous;
                if(timingPoint == null)
                    return null;
            }
            return SelectMeasure(position, timingPoint);
        }

        protected Measure SelectMeasure(double position, TimingPoint timingPoint)
        {
            for(int i = 0; i < timingPoint.Measures.Count;)
            {
                var current = timingPoint.Measures[i];
                i++;
                if(i >= timingPoint.Measures.Count)
                    return current;
                if(timingPoint.Measures[i].AbsolutePosition > position)
                    return current;
            }

            return null;
        }

        // TODO: These can be cached
        private double GetObjectStartingPosition(ObjectReference obj)
        {
            var laser = obj.Object as Laser;
            if(laser != null)
            {
                return laser.Root.AbsolutePosition;
            }

            var holdEnd = obj.Object as HoldEnd;
            if(holdEnd != null)
            {
                return holdEnd.StartingPoint.AbsolutePosition;
            }

            return obj.AbsolutePosition;
        }

        // TODO: These can be cached
        private double GetObjectEndingPosition(ObjectReference obj)
        {
            var laser = obj.Object as Laser;
            if(laser != null)
            {
                return laser.Next?.AbsolutePosition ?? obj.AbsolutePosition;
            }

            var hold = obj.Object as Hold;
            if(hold != null)
            {
                return hold.EndingPoint.AbsolutePosition;
            }

            return obj.AbsolutePosition;
        }

        private void OnObjectEntered(ObjectReference obj)
        {
            ObjectEntered?.Invoke(obj);
        }

        private void OnObjectLeft(ObjectReference obj)
        {
            ObjectLeft?.Invoke(obj);
        }

        private void OnObjectActivated(ObjectReference obj)
        {
            ObjectActivated?.Invoke(obj);
        }

        private void OnObjectDeactivated(ObjectReference obj)
        {
            ObjectDeactivated?.Invoke(obj);
        }

        private void OnMeasureEntered(Measure measure)
        {
            MeasureEntered?.Invoke(measure);
        }

        private void OnMeasureLeft(Measure measure)
        {
            MeasureLeft?.Invoke(measure);
        }

        private void MarkPassedObjects()
        {
            // Call OnObjectLeft on all passed objects
            // Also handle addition/removal of active objects
            for(int i = 0; i < objectsInView.Count;)
            {
                double startingPosition = GetObjectStartingPosition(objectsInView[i]);
                double endingPosition = GetObjectEndingPosition(objectsInView[i]);
                if(endingPosition < position ||
                   startingPosition > endPosition)
                {
                    if(ActiveObjects.Contains(objectsInView[i]))
                    {
                        OnObjectDeactivated(objectsInView[i]);
                        activeObjects.Remove(objectsInView[i]);
                    }

                    OnObjectLeft(objectsInView[i]);
                    objectsInView.RemoveAt(i);
                }
                else
                {
                    // Handle new active objects
                    if(startingPosition < position)
                    {
                        if(!activeObjects.Contains(objectsInView[i]))
                        {
                            OnObjectActivated(objectsInView[i]);
                            activeObjects.Add(objectsInView[i]);
                        }
                    }

                    i++;
                }
            }

            // Call OnMeasureLeft on all passed measures
            for(int i = 0; i < measuresInView.Count;)
            {
                var measureEnd = measuresInView[i].AbsolutePosition + measuresInView[i].TimingPoint.MeasureDuration;
                if(measureEnd < position ||
                   measuresInView[i].AbsolutePosition > endPosition)
                {
                    OnMeasureLeft(measuresInView[i]);
                    measuresInView.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}