// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Refers to the generic <see cref="Object"/> type, cast to <see cref="IReference{TObjectType}"/> to try and obtain a more specific object
    /// </summary>
    public class ObjectReference
    {
        public Measure Measure { get; }

        public Object Object { get; }

        public ObjectReference()
        {
        }

        public ObjectReference(Object obj, Measure measure)
        {
            this.Object = obj;
            this.Measure = measure;
        }

        public double AbsolutePosition => Object.GetAbsolutePosition(Measure);

        public override string ToString()
        {
            return $"Object Reference {Object} - Measure {Measure.Index}";
        }

        protected bool Equals(ObjectReference other)
        {
            return EqualityComparer<Object>.Default.Equals(Object, other.Object);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;

            return Equals((ObjectReference)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Object>.Default.GetHashCode(Object);
        }
    }
}