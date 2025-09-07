using System;

namespace ilsActionEditor
{
    public class BBValue_Float : BaseBBValue<float>,IEquatable<BBValue_Float>
    {
        public BBValue_Float(float value) : base(value)
        {
        }

        public bool Equals(BBValue_Float other)
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
            return Equals((BBValue_Float) obj);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public static bool operator ==(BBValue_Float lhs, BBValue_Float rhs)
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
        public static bool operator !=(BBValue_Float lhs, BBValue_Float rhs)
        {
            return !(lhs == rhs);
        }
        
        
        public static bool operator >(BBValue_Float lhs, BBValue_Float rhs)
        {
            return lhs.GetValue() > rhs.GetValue();
        }
        public static bool operator <(BBValue_Float lhs, BBValue_Float rhs)
        {
            return lhs.GetValue() < rhs.GetValue();
        }
        public static bool operator >=(BBValue_Float lhs, BBValue_Float rhs)
        {
            return lhs.GetValue() >= rhs.GetValue();
        }
        public static bool operator <=(BBValue_Float lhs, BBValue_Float rhs)
        {
            return lhs.GetValue() <= rhs.GetValue();
        }
    }
}