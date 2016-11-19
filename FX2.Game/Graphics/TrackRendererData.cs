// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;

namespace FX2.Game.Graphics
{
    /// <summary>
    /// Base class for data associated with an object to render in a <see cref="TrackRenderer{TAssociatedData}"/>
    /// </summary>
    public abstract class TrackRendererData : IDisposable
    {
        /// <summary>
        /// Update the given object with a new Y position
        /// </summary>
        /// <param name="newPosition">The new Y position, relative to the bottom of the track, going up</param>
        public abstract void Update(float newPosition);

        /// <summary>
        /// Called when this object should no longer be rendered
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Called when the view duration was changed, by calling <see cref="TrackRendererBase.UpdateViewDuration"/>
        /// </summary>
        public virtual void UpdateViewDuration()
        {
        }
    }
}