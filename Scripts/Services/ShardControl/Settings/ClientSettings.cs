using System;
using Server;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
    public sealed class ClientSettings : BaseSettings
    {
        #region Variables
        private bool _AutoDetectClient;
        private string _ClientPath;
        private OldClientResponse _OldClientResponse;
        private ClientVersion _RequiredVersion;
        private bool _AllowRegular, _AllowUOTD, _AllowGod;
        private TimeSpan _AgeLeniency;
        private TimeSpan _GameTimeLeniency;
        private TimeSpan _KickDelay;

        [CommandProperty(AccessLevel.Administrator)]
        public bool AutoDetectClient
        {
            get
            {
                return _AutoDetectClient;
            }
            set
            {
                _AutoDetectClient = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public string ClientPath
        {
            get
            {
                return _ClientPath;
            }
            set
            {
                _ClientPath = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public OldClientResponse OldClientResponse
        {
            get
            {
                return _OldClientResponse;
            }
            set
            {
                _OldClientResponse = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public ClientVersion RequiredClientVersion
        {
            get
            {
                return _RequiredVersion;
            }
            set
            {
                _RequiredVersion = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AllowRegular
        {
            get
            {
                return _AllowRegular;
            }
            set
            {
                _AllowRegular = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AllowUOTD
        {
            get
            {
                return _AllowUOTD;
            }
            set
            {
                _AllowUOTD = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AllowGod
        {
            get
            {
                return _AllowGod;
            }
            set
            {
                _AllowGod = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan AgeLeniency
        {
            get
            {
                return _AgeLeniency;
            }
            set
            {
                _AgeLeniency = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan GameTimeLeniency
        {
            get
            {
                return _GameTimeLeniency;
            }
            set
            {
                _GameTimeLeniency = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan KickDelay
        {
            get
            {
                return _KickDelay;
            }
            set
            {
                _KickDelay = value;
            }
        }
        #endregion

        public ClientSettings(bool autoDetectClient = false, string clientPath = null,
            OldClientResponse oldClientResponse = OldClientResponse.LenientKick,
            ClientVersion requiredVersion = null, bool allowRegular = true, bool allowUOTD = true,
            bool allowGod = true, TimeSpan ageLeniency = TimeSpan.FromDays(10.0),
            TimeSpan gameTimeLeniency = TimeSpan.FromHours(25.0), TimeSpan kickDelay = TimeSpan.FromSeconds(30.0))
        {
            _AutoDetectClient = autoDetectClient;
            _ClientPath = clientPath;
            _OldClientResponse = oldClientResponse;
            _RequiredVersion = requiredVersion;
            _AllowRegular = allowRegular;
            _AllowUOTD = allowUOTD;
            _AllowGod = allowGod;
            _AgeLeniency = ageLeniency;
            _GameTimeLeniency = gameTimeLeniency;
            _KickDelay = kickDelay;
        }

        public override void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_AutoDetectClient);
            writer.Write(_ClientPath);
            writer.Write((byte)_OldClientResponse);

            writer.Write(_RequiredVersion.Major);
            writer.Write(_RequiredVersion.Minor);
            writer.Write(_RequiredVersion.Revision);
            writer.Write(_RequiredVersion.Patch);

            writer.Write(_AllowRegular);
            writer.Write(_AllowUOTD);
            writer.Write(_AllowGod);
            writer.Write(_AgeLeniency);
            writer.Write(_GameTimeLeniency);
            writer.Write(_KickDelay);
        }

        public ClientSettings(GenericReader reader)
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
                        _AutoDetectClient = reader.ReadBool();
                        _ClientPath = reader.ReadString();
                        _OldClientResponse = (OldClientResponse)reader.ReadByte();

                        _RequiredVersion = new ClientVersion(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());

                        _AllowRegular = reader.ReadBool();
                        _AllowUOTD = reader.ReadBool();
                        _AllowGod = reader.ReadBool();
                        _AgeLeniency = reader.ReadTimeSpan();
                        _GameTimeLeniency = reader.ReadTimeSpan();
                        _KickDelay = reader.ReadTimeSpan();
                        break;
                    }
            }
        }

        public override string ToString()
        {
            return @"Client Settings";
        }
    }
}
