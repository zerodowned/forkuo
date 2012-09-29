using System;
using Server;
using Server.Misc;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
    public sealed class AccountSettings : BaseSettings
    {
        #region Variables
        private int _AccountsPerIP;
        private int _HousesPerAccount;
        private int _MaxHousesPerAccount;
        private bool _AutoAccountCreation;
        private bool _RestrictDeletion;
        private TimeSpan _DeleteDelay;
        private PasswordProtection m_PasswordProtection;

        [CommandProperty(AccessLevel.Administrator)]
        public int AccountsPerIP
        {
            get
            {
                return _AccountsPerIP;
            }
            set
            {
                _AccountsPerIP = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int HousesPerAccount
        {
            get
            {
                return _HousesPerAccount;
            }
            set
            {
                _HousesPerAccount = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int MaxHousesPerAccount
        {
            get
            {
                return _MaxHousesPerAccount;
            }
            set
            {
                _MaxHousesPerAccount = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AutoAccountCreation
        {
            get
            {
                return _AutoAccountCreation;
            }
            set
            {
                _AutoAccountCreation = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool RestrictDeletion
        {
            get
            {
                return _RestrictDeletion;
            }
            set
            {
                _RestrictDeletion = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan DeleteDelay
        {
            get
            {
                return _DeleteDelay;
            }
            set
            {
                _DeleteDelay = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public PasswordProtection PasswordProtection
        {
            get
            {
                return m_PasswordProtection;
            }
            set
            {
                m_PasswordProtection = value;
            }
        }
        #endregion

        public AccountSettings(int accountsPerIP = 1, int housesPerAccount = 2,
            int maxHousesPerAccount = 4, bool autoAccountCreation = true,
            bool restrictDeletion = true, TimeSpan deleteDelay = TimeSpan.FromDays(7.0),
            PasswordProtection passwordProtection = PasswordProtection.NewCrypt)
        {
            _AccountsPerIP = accountsPerIP;
            _HousesPerAccount = housesPerAccount;
            _MaxHousesPerAccount = maxHousesPerAccount;
            _AutoAccountCreation = autoAccountCreation;
            _RestrictDeletion = restrictDeletion;
            _DeleteDelay = deleteDelay;
            m_PasswordProtection = passwordProtection;
        }

        public override void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_AccountsPerIP);
            writer.Write(_HousesPerAccount);
            writer.Write(_MaxHousesPerAccount);
            writer.Write(_AutoAccountCreation);
            writer.Write(_RestrictDeletion);
            writer.Write(_DeleteDelay);
            writer.Write((byte)m_PasswordProtection);
        }

        public AccountSettings(GenericReader reader)
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
                        _AccountsPerIP = reader.ReadInt();
                        _HousesPerAccount = reader.ReadInt();
                        _MaxHousesPerAccount = reader.ReadInt();
                        _AutoAccountCreation = reader.ReadBool();
                        _RestrictDeletion = reader.ReadBool();
                        _DeleteDelay = reader.ReadTimeSpan();
                        m_PasswordProtection = (PasswordProtection)reader.ReadByte();
                        break;
                    }
            }
        }

        public override string ToString()
        {
            return @"Account Settings";
        }
    }
}
