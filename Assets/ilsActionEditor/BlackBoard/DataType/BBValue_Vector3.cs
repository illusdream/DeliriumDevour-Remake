using System;
using UnityEngine;
using Object = System.Object;

namespace ilsActionEditor
{
    public class BBValue_Vector3 : BaseBBValue<Vector3>,IEquatable<BBValue_Vector3>
    {
        public BBValue_Vector3(Vector3 value) : base(value)
        {
        }

        public bool Equals(BBValue_Vector3 other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return this.Value == other.GetValue();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((BBValue_Vector3)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(BBValue_Vector3 lhs, BBValue_Vector3 rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(BBValue_Vector3 lhs, BBValue_Vector3 rhs)
        {
            return !(lhs == rhs);
        }

        
    }
}