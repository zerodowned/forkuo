using System;
using Server;
using Server.Gumps;

namespace CustomsFramework
{
    public class BaseModule : ICustomsEntity, IComparable<BaseModule>, ISerializable
    {
        #region CompareTo
        public int CompareTo(ICustomsEntity other)
        {
            if (other == null)
                return -1;

            return _Serial.CompareTo(other.Serial);
        }

        public int CompareTo(BaseModule other)
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

        public virtual string Name { get { return @"Base Module"; } }
        public virtual string Description { get { return "Base Module, inherit from this class and override all interface items."; } }
        public virtual string Version { get { return "1.0"; } }
        public virtual AccessLevel EditLevel { get { return AccessLevel.Developer; } }
        public virtual Gump SettingsGump { get { return null; } }

        private bool _Deleted;

        private CustomSerial _Serial;
        private Mobile _LinkedMobile;
        private Item _LinkedItem;

        private DateTime _CreatedTime;
        private DateTime _LastEditedTime;

        [CommandProperty(AccessLevel.Administrator)]
        public bool Deleted
        {
            get
            {
                return _Deleted;
            }
            set
            {
                _Deleted = value;
            }
        }

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

        [CommandProperty(AccessLevel.Administrator)]
        public Mobile LinkedMobile
        {
            get
            {
                return _LinkedMobile;
            }
            set
            {
                _LinkedMobile = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public Item LinkedItem
        {
            get
            {
                return _LinkedItem;
            }
            set
            {
                _LinkedItem = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public DateTime CreatedTime
        {
            get
            {
                return _CreatedTime;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public DateTime LastEditedTime
        {
            get
            {
                return _LastEditedTime;
            }
        }

        public BaseModule(CustomSerial serial)
        {
            _Serial = serial;

            Type moduleType = this.GetType();
            _TypeID = World._ModuleTypes.IndexOf(moduleType);

            if (_TypeID == -1)
            {
                World._ModuleTypes.Add(moduleType);
                _TypeID = World._ModuleTypes.Count - 1;
            }
        }

        public BaseModule()
        {
            _Serial = CustomSerial.NewModule;

            World.AddModule(this);

            Type moduleType = this.GetType();
            _TypeID = World._ModuleTypes.IndexOf(moduleType);

            if (_TypeID == -1)
            {
                World._ModuleTypes.Add(moduleType);
                _TypeID = World._ModuleTypes.Count - 1;
            }
        }

        public virtual void Prep()
        {

        }

        public virtual void Delete()
        {

        }

        public virtual void Update()
        {
            _LastEditedTime = DateTime.Now;
        }

        public virtual bool LinkMobile(Mobile from)
        {
            if (_LinkedMobile != null)
                return false;
            else if (_LinkedMobile == from)
                return false;
            else
            {
                _LinkedMobile = from;
                Update();
                return true;
            }
        }

        public virtual bool LinkItem(Item item)
        {
            if (_LinkedItem == null)
                return false;
            else if (_LinkedItem == item)
                return false;
            else
            {
                _LinkedItem = item;
                Update();
                return true;
            }
        }

        public virtual bool UnlinkMobile()
        {
            if (_LinkedMobile == null)
                return false;
            else
            {
                _LinkedMobile = null;
                Update();
                return true;
            }
        }

        public virtual bool UnlinkItem()
        {
            if (_LinkedItem == null)
                return false;
            else
            {
                _LinkedItem = null;
                Update();
                return true;
            }
        }

        public virtual void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Deleted);
            writer.Write(_LinkedMobile);
            writer.Write(_LinkedItem);
            writer.Write(_CreatedTime);
            writer.Write(_LastEditedTime);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _Deleted = reader.ReadBool();
                        _LinkedMobile = reader.ReadMobile();
                        _LinkedItem = reader.ReadItem();
                        _CreatedTime = reader.ReadDateTime();
                        _LastEditedTime = reader.ReadDateTime();
                        break;
                    }
            }
        }
    }
}
