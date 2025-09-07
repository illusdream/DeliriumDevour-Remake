using System;

namespace ilsActionEditor
{
    public class BBValue_String : BaseBBValue<string>,IEquatable<BBValue_String>
    {
        public BBValue_String(string value) : base(value)
        {
        }

        public bool Equals(BBValue_String other)
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
            return Equals((BBValue_String) obj);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public static bool operator ==(BBValue_String lhs, BBValue_String rhs)
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
        public static bool operator !=(BBValue_String lhs, BBValue_String rhs)
        {
            return !(lhs == rhs);
        }
    }
}