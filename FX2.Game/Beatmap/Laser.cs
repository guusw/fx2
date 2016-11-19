// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Special object for the first laser object
    /// </summary>
    public class LaserRoot : Laser
    {
        private byte index;

        /// <summary>
        /// True if this laser part uses the extended range
        /// </summary>
        public new bool IsExtended;
        
        /// <summary>
        /// Index of the laser
        /// </summary>
        public new int Index
        {
            get { return index; }
            set { index = (byte)value; }
        }
        
        public override string ToString()
        {
            return $"{base.ToString()}, Root, {nameof(IsExtended)}: {IsExtended}";
        }
    }

    /// <summary>
    /// A laser control point
    /// </summary>
    public class Laser : Object
    {
        /// <summary>
        /// Horizontal position of the laser on the track
        /// </summary>
        public float HorizontalPosition;

        /// <summary>
        /// The root laser object
        /// </summary>
        public ObjectReference Root;

        /// <summary>
        /// The previous control point in a connected laser part
        /// </summary>
        public ObjectReference Previous;

        /// <summary>
        /// The next control point in a connected laser part
        /// </summary>
        public ObjectReference Next;

        /// <summary>
        /// True if this laser part uses the extended range
        /// </summary>
        public bool IsExtended
        {
            get
            {
                var root = this as LaserRoot;
                if(root == null)
                    root = Root?.Object as LaserRoot;
                return root?.IsExtended ?? false;
            }
        }

        /// <summary>
        /// Index of the laser
        /// </summary>
        public int Index
        {
            get
            {
                var root = this as LaserRoot;
                if(root == null)
                    root = Root?.Object as LaserRoot;
                return root?.Index ?? -1;
            }
        }

        /// <summary>
        /// Is the segment following this control point an instant segment
        /// </summary>
        public bool IsInstant()
        {
            if(Next == null)
                return false;

            var nextLaser = Next.Object as Laser;
            return nextLaser.Previous.Measure == Next.Measure && Next.Object.Position == Position;
        }

        public override string ToString()
        {
            return $"Laser {Index} ({HorizontalPosition})";
        }
    }
}