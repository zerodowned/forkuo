using System;
using Server;
using Server.Gumps;

namespace CustomsFramework
{
    public class BaseCore : ICustomsEntity, IComparable<BaseCore>, ISerializable
    {
        #region CompareTo
        public int CompareTo(ICustomsEntity other)
        {
            if (other == null)
                return -1;

            return this._Serial.CompareTo(other.Serial);
        }

        public int CompareTo(BaseCore other)
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

        private bool _Enabled;
        private bool _Deleted;
        private CustomSerial _Serial;

        [CommandProperty(AccessLevel.Developer)]
        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
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

        [CommandProperty(AccessLevel.Developer)]
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

        public virtual string Name
        {
            get
            {
                return @"Base Core";
            }
        }
        public virtual string Description
        {
            get
            {
                return @"Base Core, inherit from this class and override the interface items.";
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

        public BaseCore(CustomSerial serial)
        {
            this._Serial = serial;
            
            Type coreType = this.GetType();
            this._TypeID = World._CoreTypes.IndexOf(coreType);

            if (this._TypeID == -1)
            {
                World._CoreTypes.Add(coreType);
                this._TypeID = World._CoreTypes.Count - 1;
            }
        }

        public BaseCore()
        {
            this._Serial = CustomSerial.NewCore;

            World.AddCore(this);

            Type coreType = this.GetType();
            this._TypeID = World._CoreTypes.IndexOf(coreType);

            if (this._TypeID == -1)
            {
                World._CoreTypes.Add(coreType);
                this._TypeID = World._CoreTypes.Count - 1;
            }
            Console.WriteLine("A Core has been created.");
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

            // Version 0
            writer.Write(this._Deleted);
            writer.Write(this._Enabled);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        this._Deleted = reader.ReadBool();
                        this._Enabled = reader.ReadBool();
                        break;
                    }
            }
        }
    }
}