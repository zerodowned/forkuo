using System;
using Server;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
    public sealed class GeneralSettings : BaseSettings
    {
        #region Variables
        private string _ShardName;
        private bool _AutoDetect;
        private string _Address;
        private int _Port;
        private Expansion _Expansion;
        private AccessLevel _MaxPlayerLevel;
        private AccessLevel _LowestStaffLevel;
        private AccessLevel _LowestOwnerLevel;

        [CommandProperty(AccessLevel.Owner)]
        public string ShardName
        {
            get
            {
                return _ShardName;
            }
            set
            {
                _ShardName = value;
            }
        }

        [CommandProperty(AccessLevel.Owner)]
        public bool AutoDetect
        {
            get
            {
                return _AutoDetect;
            }
            set
            {
                _AutoDetect = value;
            }
        }

        [CommandProperty(AccessLevel.Owner)]
        public string Address
        {
            get
            {
                return _Address;
            }
            set
            {
                _Address = value;
            }
        }

        [CommandProperty(AccessLevel.Owner)]
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
            }
        }

        [CommandProperty(AccessLevel.Owner)]
        public Expansion Expansion
        {
            get
            {
                return _Expansion;
            }
            set
            {
                _Expansion = value;
            }
        }
        #endregion

        public GeneralSettings(string shardName = "My Shard", bool autoDetect = true,
            string address = null, int port = 2593, Expansion expansion = Expansion.SA)
        {
            _ShardName = shardName;
            _AutoDetect = autoDetect;
            _Address = address;
            _Port = port;
            _Expansion = expansion;
        }

        public override void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_ShardName);
            writer.Write(_AutoDetect);
            writer.Write(_Address);
            writer.Write(_Port);
            writer.Write((byte)_Expansion);
        }

        public GeneralSettings(GenericReader reader)
        {
            Deserialize(reader);
        }

        protected sealed override void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _ShardName = reader.ReadString();
                        _AutoDetect = reader.ReadBool();
                        _Address = reader.ReadString();
                        _Port = reader.ReadInt();
                        _Expansion = (Expansion)reader.ReadByte();
                        break;
                    }
            }
        }

        public override string ToString()
        {
            return @"General Settings";
        }
	}
}
