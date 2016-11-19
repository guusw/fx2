// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Single note button
    /// </summary>
    public class Button : Object
    {
        private byte index;

        /// <summary>
        /// Index of the button
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = (byte)value; }
        }

        public override string ToString()
        {
            return $"Button {Index}";
        }
    }
}