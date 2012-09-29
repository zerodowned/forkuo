using System;
using Server;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
	public class MainSettings
    {
        #region Variables
        private AccountSettings _AccountSettings;
        private SaveSettings _SaveSettings;
        private ClientSettings _ClientSettings;

        [CommandProperty(AccessLevel.Administrator)]
        public AccountSettings AccountSettings
        {
            get
            {
                return _AccountSettings;
            }
            set
            {
                _AccountSettings = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public SaveSettings SaveSettings
        {
            get
            {
                return _SaveSettings;
            }
            set
            {
                _SaveSettings = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public ClientSettings ClientSettings
        {
            get
            {
                return _ClientSettings;
            }
            set
            {
                _ClientSettings = value;
            }
        }
        #endregion

        public MainSettings()
        {
            _AccountSettings = new AccountSettings();
            _SaveSettings = new SaveSettings();
            _ClientSettings = new ClientSettings();
        }

        public void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            _AccountSettings.Serialize(writer);
            _SaveSettings.Serialize(writer);
            _ClientSettings.Serialize(writer);
        }

        public MainSettings(GenericReader reader)
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
                        _AccountSettings = new AccountSettings(reader);
                        _SaveSettings = new SaveSettings(reader);
                        _ClientSettings = new ClientSettings(reader);
                        break;
                    }
            }
        }
    }
}
