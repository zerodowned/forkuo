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

            return this._Serial.CompareTo(other.Serial);
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
            return this.Name;
        }

        internal int _TypeID;

        int ISerializable.TypeReference
        {
            get
            {
                return this._TypeID;
            }
        }

        int ISerializable.SerialIdentity
        {
            get
            {
                return this._Serial;
            }
        }

        public virtual string Name
        {
            get
            {
                return @"Base Module";
            }
        }
        public virtual string Description
        {
            get
            {
                return "Base Module, inherit from this class and override all interface items.";
            }
        }
        public virtual string Version
        {
            get
            {
                return "1.0";
            }
        }
        public virtual AccessLevel EditLevel
        {
            get
            {
                return AccessLevel.Developer;
            }
        }
        public virtual Gump SettingsGump
        {
            get
            {
                return null;
            }
        }

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
                return this._Deleted;
            }
            set
            {
                this._Deleted = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public CustomSerial Serial
        {
            get
            {
                return this._Serial;
            }
            set
            {
                this._Serial = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public Mobile LinkedMobile
        {
            get
            {
                return this._LinkedMobile;
            }
            set
            {
                this._LinkedMobile = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public Item LinkedItem
        {
            get
            {
                return this._LinkedItem;
            }
            set
            {
                this._LinkedItem = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public DateTime CreatedTime
        {
            get
            {
                return this._CreatedTime;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public DateTime LastEditedTime
        {
            get
            {
                return this._LastEditedTime;
            }
        }

        public BaseModule(CustomSerial serial)
        {
            this._Serial = serial;

            Type moduleType = this.GetType();
            this._TypeID = World._ModuleTypes.IndexOf(moduleType);

            if (this._TypeID == -1)
            {
                World._ModuleTypes.Add(moduleType);
                this._TypeID = World._ModuleTypes.Count - 1;
            }
        }

        public BaseModule()
        {
            this._Serial = CustomSerial.NewModule;

            World.AddModule(this);

            Type moduleType = this.GetType();
            this._TypeID = World._ModuleTypes.IndexOf(moduleType);

            if (this._TypeID == -1)
            {
                World._ModuleTypes.Add(moduleType);
                this._TypeID = World._ModuleTypes.Count - 1;
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
            this._LastEditedTime = DateTime.Now;
        }

        public virtual bool LinkMobile(Mobile from)
        {
            if (this._LinkedMobile != null)
                return false;
            else if (this._LinkedMobile == from)
                return false;
            else
            {
                this._LinkedMobile = from;
                this.Update();
                return true;
            }
        }

        public virtual bool LinkItem(Item item)
        {
            if (this._LinkedItem == null)
                return false;
            else if (this._LinkedItem == item)
                return false;
            else
            {
                this._LinkedItem = item;
                this.Update();
                return true;
            }
        }

        public virtual bool UnlinkMobile()
        {
            if (this._LinkedMobile == null)
                return false;
            else
            {
                this._LinkedMobile = null;
                this.Update();
                return true;
            }
        }

        public virtual bool UnlinkItem()
        {
            if (this._LinkedItem == null)
                return false;
            else
            {
                this._LinkedItem = null;
                this.Update();
                return true;
            }
        }

        public virtual void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(this._Deleted);
            writer.Write(this._LinkedMobile);
            writer.Write(this._LinkedItem);
            writer.Write(this._CreatedTime);
            writer.Write(this._LastEditedTime);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        this._Deleted = reader.ReadBool();
                        this._LinkedMobile = reader.ReadMobile();
                        this._LinkedItem = reader.ReadItem();
                        this._CreatedTime = reader.ReadDateTime();
                        this._LastEditedTime = reader.ReadDateTime();
                        break;
                    }
            }
        }
    }
}