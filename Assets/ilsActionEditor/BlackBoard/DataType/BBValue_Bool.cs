using System;

namespace ilsActionEditor
{
    public class BBValue_Bool : BaseBBValue<bool>,IEquatable<BBValue_Bool>
    {
        public BBValue_Bool(bool value) : base(value)
        {
        }

        public bool Equals(BBValue_Bool other)
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
            return Equals((BBValue_Bool) obj);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public static bool operator ==(BBValue_Bool lhs, BBValue_Bool rhs)
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
        public static bool operator !=(BBValue_Bool lhs, BBValue_Bool rhs)
        {
            return !(lhs == rhs);
        }
        
        
        public static bool operator >(BBValue_Bool lhs, BBValue_Bool rhs)
        {
            return false;
        }
        public static bool operator <(BBValue_Bool lhs, BBValue_Bool rhs)
        {
            return false;
        }
        public static bool operator >=(BBValue_Bool lhs, BBValue_Bool rhs)
        {
            return false;
        }
        public static bool operator <=(BBValue_Bool lhs, BBValue_Bool rhs)
        {
            return false;
        }
    }
}