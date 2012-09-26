using System;
using Server;

namespace CustomsFramework
{
	public struct CustomSerial : IComparable, IComparable<CustomSerial>
	{
        private int _Serial;

        private static CustomSerial _LastCore = Zero;
        private static CustomSerial _LastModule = (int.MaxValue / 4);
        private static CustomSerial _LastService = (int)(int.MaxValue * 0.75);

        public static CustomSerial LastCore { get { return _LastCore; } }
        public static CustomSerial LastModule { get { return _LastModule; } }
        public static CustomSerial LastService { get { return _LastService; } }

        public static readonly CustomSerial MinusOne = new CustomSerial(-1);
        public static readonly CustomSerial Zero = new CustomSerial(0);

        public static CustomSerial NewCore
        {
            get
            {
                while (World.GetCore(_LastCore = (_LastCore + 1)) != null)
                {
                }

                return _LastCore;
            }
        }

        public static CustomSerial NewModule
        {
            get
            {
                while (World.GetModule(_LastModule = (_LastModule + 1)) != null)
                {
                }

                return _LastModule;
            }
        }

        public static CustomSerial NewService
        {
            get
            {
                while (World.GetService(_LastService = (_LastService + 1)) != null)
                {
                }

                return _LastService;
            }
        }

        private CustomSerial(int serial)
        {
            _Serial = serial;
        }

        public int Value
        {
            get
            {
                return _Serial;
            }
        }

        public bool IsCore
        {
            get
            {
                return (IsValid && _Serial < (int.MaxValue / 4));
            }
        }

        public bool IsModule
        {
            get
            {
                return (_Serial >= (int.MaxValue / 4) && _Serial < (int.MaxValue & 0.75));
            }
        }

        public bool IsService
        {
            get
            {
                return (_Serial >= (int.MaxValue * 0.75) && _Serial <= int.MaxValue);
            }
        }

        public bool IsValid
        {
            get
            {
                return (_Serial > 0);
            }
        }

        public override int GetHashCode()
        {
            return _Serial;
        }

        public int CompareTo(CustomSerial other)
        {
            return _Serial.CompareTo(other._Serial);
        }

        public int CompareTo(object other)
        {
            if (other is CustomSerial)
                return this.CompareTo((CustomSerial)other);
            else if (other == null)
                return -1;

            throw new ArgumentException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CustomSerial))
                return false;

            return ((CustomSerial)obj)._Serial == _Serial;
        }

        public static bool operator ==(CustomSerial first, CustomSerial second)
        {
            return first._Serial == second._Serial;
        }

        public static bool operator !=(CustomSerial first, CustomSerial second)
        {
            return first._Serial != second._Serial;
        }

        public static bool operator >(CustomSerial first, CustomSerial second)
        {
            return first._Serial > second._Serial;
        }

        public static bool operator <(CustomSerial first, CustomSerial second)
        {
            return first._Serial < second._Serial;
        }

        public static bool operator >=(CustomSerial first, CustomSerial second)
        {
            return first._Serial >= second._Serial;
        }

        public static bool operator <=(CustomSerial first, CustomSerial second)
        {
            return first._Serial <= second._Serial;
        }

        public override string ToString()
        {
            return String.Format("0x{0:X8}", _Serial);
        }

        public static implicit operator int(CustomSerial serial)
        {
            return serial._Serial;
        }

        public static implicit operator CustomSerial(int serial)
        {
            return new CustomSerial(serial);
        }
	}
}
