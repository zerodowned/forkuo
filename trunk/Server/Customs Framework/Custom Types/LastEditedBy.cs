using System;
using CustomsFramework;

namespace Server
{
    public class LastEditedBy
    {
        private Mobile _Mobile;
        private DateTime _Time;

        [CommandProperty(AccessLevel.Decorator)]
        public Mobile Mobile
        {
            get
            {
                return _Mobile;
            }
            set
            {
                _Mobile = value;
            }
        }

        [CommandProperty(AccessLevel.Decorator)]
        public DateTime Time
        {
            get
            {
                return _Time;
            }
            set
            {
                _Time = value;
            }
        }

        public LastEditedBy(Mobile mobile)
        {
            _Mobile = mobile;
            _Time = DateTime.Now;
        }

        public void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Mobile);
            writer.Write(_Time);
        }

        public LastEditedBy(GenericReader reader)
        {
            Deserialize(reader);
        }

        private void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _Mobile = reader.ReadMobile();
                        _Time = reader.ReadDateTime();
                        break;
                    }
            }
        }
    }
}
