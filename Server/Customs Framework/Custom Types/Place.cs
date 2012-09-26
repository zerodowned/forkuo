using CustomsFramework;

namespace Server
{
    public class Place
    {
        private Map _Map;
        private Point3D _Location;

        [CommandProperty(AccessLevel.Decorator)]
        public Map Map
        {
            get
            {
                return _Map;
            }
            set
            {
                _Map = value;
            }
        }

        [CommandProperty(AccessLevel.Decorator)]
        public Point3D Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
            }
        }

        public Place()
        {
            _Map = Map.Internal;
            _Location = new Point3D(0, 0, 0);
        }

        public Place(Map map, Point3D location)
        {
            _Map = map;
            _Location = location;
        }

        public void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Map);
            writer.Write(_Location);
        }

        public Place(GenericReader reader)
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
                        _Map = reader.ReadMap();
                        _Location = reader.ReadPoint3D();
                        break;
                    }
            }
        }
    }
}
