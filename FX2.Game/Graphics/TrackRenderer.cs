// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using FX2.Game.Beatmap;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.Graphics
{
    /// <summary>
    /// Logic only track renderer, just keeps track of things to render using templated associated data. 
    /// It will also take care of rendering/loading common resources such as track textures, laser rendering, etc. 
    /// </summary>
    public abstract class TrackRendererBase
    {
        /// <summary>
        /// Current position of the view, in seconds
        /// </summary>
        public double Position;

        private double viewDuration;
        
        private readonly HashSet<TimeDivisionReference> pendingSplitRemovals = new HashSet<TimeDivisionReference>();
        private readonly HashSet<ObjectReference> pendingObjectRemovals = new HashSet<ObjectReference>();

        private Dictionary<TimeDivisionReference, TrackRendererData> splitObjects = new Dictionary<TimeDivisionReference, TrackRendererData>();
        private Dictionary<ObjectReference, TrackRendererData> objects = new Dictionary<ObjectReference, TrackRendererData>();

        /// <summary>
        /// Duration of the duration viewed, in seconds
        /// </summary>
        public double ViewDuration
        {
            get { return viewDuration; }
            set { viewDuration = value; UpdateViewDuration(); }
        }

        /// <summary>
        /// The base track texture
        /// </summary>
        public Texture TrackTexture { get; private set; }

        /// <summary>
        /// The split texture that indicates the start of a new measure
        /// </summary>
        public Texture SplitTexture { get; private set; }

        /// <summary>
        /// The regular button texture
        /// </summary>
        public Texture ButtonTexture { get; private set; }

        /// <summary>
        /// The regular hold button texture
        /// </summary>
        public Texture HoldButtonTexture { get; private set; }

        /// <summary>
        /// The FX button texture
        /// </summary>
        public Texture FXButtonTexture { get; private set; }

        /// <summary>
        /// The FX hold button texture
        /// </summary>
        public Texture FXHoldButtonTexture { get; private set; }

        /// <summary>
        /// The laser border texture
        /// </summary>
        public Texture LaserTexture { get; private set; }

        /// <summary>
        /// The laser colors
        /// </summary>
        public Color4[] LaserColors { get; private set; } = new Color4[2];

        /// <summary>
        /// Width of the playfield
        /// </summary>
        public float PlayfieldWidth { get; private set; } = 1.0f;

        /// <summary>
        /// Width of the track texture
        /// </summary>
        public float TrackWidth { get; private set; }

        /// <summary>
        /// Height of the track
        /// </summary>
        public float PlayfieldHeight { get; private set; } = 3.0f;

        /// <summary>
        /// Width of the area containing the buttons
        /// </summary>
        public float ButtonsWidth { get; private set; }

        /// <summary>
        /// Padding on buttons on the track
        /// </summary>
        public float ButtonPadding { get; private set; }

        /// <summary>
        /// Padding on lasers on the track
        /// </summary>
        public float LaserPadding { get; private set; }

        /// <summary>
        /// Width of a single laser track
        /// </summary>
        public float LaserWidth { get; private set; }

        public virtual void Load(BaseGame game)
        {
            // TODO: Load from skin

            // Start of the left laser track
            int trackStart = 33;
            int trackBorder = trackStart * 2;

            // Width of a single laser track
            int laserWidth = 138;
            // Width of a single button track
            int buttonWidth = 171;

            // Left and right padding on buttons
            int buttonPadding = 4;
            // Left and right padding on lasers
            int laserPadding = 4;
            
            SplitTexture = game.Textures.Get("split.png");

            TrackTexture = game.Textures.Get("track.png");
            int logicWidth = TrackTexture.Width - trackBorder;
            TrackWidth = (float)TrackTexture.Width / logicWidth;

            LaserWidth = (float)laserWidth / logicWidth * PlayfieldWidth;
            ButtonsWidth = PlayfieldWidth - LaserWidth * 2.0f;
            ButtonPadding = (float)buttonPadding / logicWidth * PlayfieldWidth;
            LaserPadding = (float)laserPadding / logicWidth * PlayfieldWidth;

            ButtonTexture = game.Textures.Get("button.png");
            HoldButtonTexture = game.Textures.Get("buttonhold.png");
            FXButtonTexture = game.Textures.Get("fxbutton.png");
            FXHoldButtonTexture = game.Textures.Get("fxbuttonhold.png");
            LaserTexture = game.Textures.Get("laser.png");

            // TODO: Move this to a skin settings file
            var rawTextures = game.Textures as ResourceStore<RawTexture>;
            var laserColors = rawTextures.Get("lasercolors.png");

            int b = 0;
            LaserColors[0] = new Color4(laserColors.Pixels[b++], laserColors.Pixels[b++],
                laserColors.Pixels[b++], laserColors.Pixels[b++]);
            LaserColors[1] = new Color4(laserColors.Pixels[b++], laserColors.Pixels[b++],
                laserColors.Pixels[b++], laserColors.Pixels[b++]);
        }
        
        /// <summary>
        /// Returns the placement of a button, relative to the left bottom corner of the playfield area
        /// </summary>
        /// <param name="buttonIndex">The index of the button (0-5), where 4-5 are FX buttons</param>
        /// <returns></returns>
        public Vector2 GetButtonPlacement(int buttonIndex)
        {
            if(buttonIndex < 4)
            {
                float offset = ButtonsWidth / 4.0f * buttonIndex + LaserWidth;
                return new Vector2(offset, 0);
            }
            else
            {
                float offset = ButtonsWidth / 2.0f * (buttonIndex - 4) + LaserWidth;
                return new Vector2(offset, 0);
            }
        }

        public float GetButtonWidth(int buttonIndex)
        {
            if(buttonIndex < 4)
            {
                return ButtonsWidth / 4.0f;
            }
            else
            {
                return ButtonsWidth / 2.0f;
            }
        }

        /// <summary>
        /// Gets the absolute laser position from a 0-1 track logic position, where 0 is left and 1 is right (the starting positions)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float GetLaserCenter(float position)
        {
            return LaserWidth * 0.5f + position * (TrackWidth - LaserWidth);
        }

        /// <summary>
        /// Adds or updates a new visible split to be rendered
        /// </summary>
        /// <param name="tdr">The time of this split</param>
        public void UpdateSplit(TimeDivisionReference tdr)
        {
            TrackRendererData existing;
            if(!splitObjects.TryGetValue(tdr, out existing))
            {
                existing = CreateSplitData(tdr);
                if(existing == null)
                    return;
                splitObjects.Add(tdr, existing);
            }
            else
            {
                pendingSplitRemovals.Remove(tdr);
            }

            existing.Update(GetPosition(tdr.AbsolutePosition));
        }

        public void UpdateObject(ObjectReference obj)
        {
            TrackRendererData existing;
            if(!objects.TryGetValue(obj, out existing))
            {
                existing = CreateObjectData(obj);
                if(existing == null)
                    return;
                objects.Add(obj, existing);
            }
            else
            {
                pendingObjectRemovals.Remove(obj);
            }

            existing.Update(GetPosition(obj.AbsolutePosition));
        }

        /// <summary>
        /// Call this after calling UpdateSplit/Object to update the visibility of objects
        /// </summary>
        public void Update()
        {
            // Process removals
            foreach(var splitRemoval in pendingSplitRemovals)
            {
                var obj = splitObjects[splitRemoval];
                obj.Dispose();
                splitObjects.Remove(splitRemoval);
            }
            pendingSplitRemovals.Clear();
            foreach(var objectRemoval in pendingObjectRemovals)
            {
                var obj = objects[objectRemoval];
                obj.Dispose();
                objects.Remove(objectRemoval);
            }
            pendingObjectRemovals.Clear();

            // Enqueue new removals if these objects are not updated before the next frame
            foreach(var splitObject in splitObjects)
            {
                pendingSplitRemovals.Add(splitObject.Key);
            }
            foreach(var obj in objects)
            {
                pendingObjectRemovals.Add(obj.Key);
            }
        }

        protected void UpdateViewDuration()
        {
            foreach(var obj in objects)
            {
                obj.Value.UpdateViewDuration();
            }
        }

        protected float GetPosition(double time)
        {
            return (float)((time - Position) / ViewDuration) * PlayfieldHeight;
        }

        protected abstract TrackRendererData CreateSplitData(TimeDivisionReference tdr);

        protected abstract TrackRendererData CreateObjectData(ObjectReference obj);
    }
}