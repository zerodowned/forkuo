using System;
using Server;
using Server.Gumps;

namespace CustomsFramework
{
    public class BaseService : ICustomsEntity, IComparable<BaseService>, ISerializable
    {
        #region CompareTo
        public int CompareTo(ICustomsEntity other)
        {
            if (other == null)
                return -1;

            return _Serial.CompareTo(other.Serial);
        }

        public int CompareTo(BaseService other)
        {
            return this.CompareTo((ICustomsEntity)other);
        }

        public int CompareTo(object other)
        {
            if (other == null || other is ICustomsEntity)
                return this.CompareTo((ICustomsEntity)other);

            throw new ArgumentException();
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        internal int _TypeID;

        int ISerializable.TypeReference
        {
            get
            {
                return _TypeID;
            }
        }

        int ISerializable.SerialIdentity
        {
            get
            {
                return _Serial;
            }
        }

        private CustomSerial _Serial;

        [CommandProperty(AccessLevel.Administrator)]
        public CustomSerial Serial
        {
            get
            {
                return _Serial;
            }
            set
            {
                _Serial = value;
            }
        }

        public virtual string Name { get { return @"Base Service"; } }
        public virtual string Description { get { return @"Base Service, inherit from this class and override the interface items."; } }
        public virtual string Version { get { return "1.0"; } }
        public virtual AccessLevel EditLevel { get { return AccessLevel.Developer; } }
        public virtual Gump SettingsGump { get { return null; } }

        public BaseService(CustomSerial serial)
        {
            _Serial = serial;

            Type serviceType = this.GetType();
            _TypeID = World._ServiceTypes.IndexOf(serviceType);

            if (_TypeID == -1)
            {
                World._ServiceTypes.Add(serviceType);
                _TypeID = World._ServiceTypes.Count - 1;
            }
        }

        public BaseService()
        {
            _Serial = CustomSerial.NewService;

            World.AddService(this);

            Type serviceType = this.GetType();
            _TypeID = World._ServiceTypes.IndexOf(serviceType);

            if (_TypeID == -1)
            {
                World._ServiceTypes.Add(serviceType);
                _TypeID = World._ServiceTypes.Count - 1;
            }
        }

        public virtual void Prep()
        {

        }

        public virtual void Delete()
        {

        }

        public virtual void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            //Version 0
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {

                        break;
                    }
            }
        }
    }
}