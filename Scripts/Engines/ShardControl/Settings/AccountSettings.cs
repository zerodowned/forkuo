using System;
using Server;
using Server.Misc;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
    public sealed class AccountSettings : BaseSettings
    {
        #region Variables
        private int m_AccountsPerIP;
        private int m_HousesPerAccount;
        private int m_MaxHousesPerAccount;
        private bool m_AutoAccountCreation;
        private bool m_RestrictDeletion;
        private TimeSpan m_DeleteDelay;
        private PasswordProtection m_PasswordProtection;

        [CommandProperty(AccessLevel.Administrator)]
        public int AccountsPerIP
        {
            get
            {
                return m_AccountsPerIP;
            }
            set
            {
                m_AccountsPerIP = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int HousesPerAccount
        {
            get
            {
                return m_HousesPerAccount;
            }
            set
            {
                m_HousesPerAccount = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int MaxHousesPerAccount
        {
            get
            {
                return m_MaxHousesPerAccount;
            }
            set
            {
                m_MaxHousesPerAccount = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AutoAccountCreation
        {
            get
            {
                return m_AutoAccountCreation;
            }
            set
            {
                m_AutoAccountCreation = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool RestrictDeletion
        {
            get
            {
                return m_RestrictDeletion;
            }
            set
            {
                m_RestrictDeletion = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan DeleteDelay
        {
            get
            {
                return m_DeleteDelay;
            }
            set
            {
                m_DeleteDelay = value;
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
            m_AccountsPerIP = accountsPerIP;
            m_HousesPerAccount = housesPerAccount;
            m_MaxHousesPerAccount = maxHousesPerAccount;
            m_AutoAccountCreation = autoAccountCreation;
            m_RestrictDeletion = restrictDeletion;
            m_DeleteDelay = deleteDelay;
            m_PasswordProtection = passwordProtection;
        }

        public override void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(m_AccountsPerIP);
            writer.Write(m_HousesPerAccount);
            writer.Write(m_MaxHousesPerAccount);
            writer.Write(m_AutoAccountCreation);
            writer.Write(m_RestrictDeletion);
            writer.Write(m_DeleteDelay);
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
                        m_AccountsPerIP = reader.ReadInt();
                        m_HousesPerAccount = reader.ReadInt();
                        m_MaxHousesPerAccount = reader.ReadInt();
                        m_AutoAccountCreation = reader.ReadBool();
                        m_RestrictDeletion = reader.ReadBool();
                        m_DeleteDelay = reader.ReadTimeSpan();
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
