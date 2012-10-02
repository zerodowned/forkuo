using System;
using System.Collections.Generic;
using Server;

namespace CustomsFramework.Systems.ShardControl
{
    [PropertyObject]
    public sealed class SaveSettings : BaseSettings
    {
        enum CompressionLevel
        {
            None = 0,
            Fast = 1,
            Low = 2,
            Normal = 3,
            High = 4,
            Ultra = 5,
        }

        #region Variables
        private bool m_SavesEnabled;
        private AccessLevel m_SaveAccessLevel;
        private SaveStrategy m_SaveStrategy;
        private bool m_AllowBackgroundWrite;
        private TimeSpan m_SaveDelay;
        private List<TimeSpan> m_WarningDelays;
        private int m_NoIOHour;

        private bool m_EnableEmergencyBackups;
        private int m_EmergencyBackupHour;
        private CompressionLevel m_CompressionLevel;

        [CommandProperty(AccessLevel.Administrator)]
        public bool SavesEnabled
        {
            get
            {
                return m_SavesEnabled;
            }
            set
            {
                m_SavesEnabled = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public AccessLevel SaveAccessLevel
        {
            get
            {
                return m_SaveAccessLevel;
            }
            set
            {
                m_SaveAccessLevel = value;
            }
        }


        [CommandProperty(AccessLevel.Administrator, true)]
        public SaveStrategy SaveStrategy
        {
            get
            {
                return m_SaveStrategy;
            }
            set
            {
                if (!Core.MultiProcessor && !(value is StandardSaveStrategy))
                    m_SaveStrategy = new StandardSaveStrategy();
                else
                {
                    if (Core.ProcessorCount == 2 && (value is DualSaveStrategy || value is DynamicSaveStrategy))
                        m_SaveStrategy = value;
                    else if (Core.ProcessorCount > 2)
                        m_SaveStrategy = value;
                }
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool AllowBackgroundWrite
        {
            get
            {
                return m_AllowBackgroundWrite;
            }
            set
            {
                m_AllowBackgroundWrite = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public TimeSpan SaveDelay
        {
            get
            {
                return m_SaveDelay;
            }
            set
            {
                m_SaveDelay = value;
            }
        }

        // Create a method to verify proper delay order
        [CommandProperty(AccessLevel.Administrator, true)]
        public List<TimeSpan> WarningDelays
        {
            get
            {
                return m_WarningDelays;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int NoIOHour
        {
            get
            {
                return m_NoIOHour;
            }
            set
            {
                m_NoIOHour = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool EnableEmergencyBackups
        {
            get
            {
                return m_EnableEmergencyBackups;
            }
            set
            {
                m_EnableEmergencyBackups = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int EmergencyBackupHour
        {
            get
            {
                return m_EmergencyBackupHour;
            }
            set
            {
                m_EmergencyBackupHour = value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public CompressionLevel Compression
        {
            get
            {
                return m_CompressionLevel;
            }
            set
            {
                m_CompressionLevel = value;
            }
        }
        #endregion

        public SaveSettings(bool savesEnabled = true, AccessLevel saveAccessLevel = AccessLevel.Administrator,
            SaveStrategy saveStrategy = SaveStrategy.Acquire(),  bool allowBackgroundWrite = false,
            TimeSpan saveDelay = TimeSpan.FromHours(1.0), int noIOHour = -1,
            List<TimeSpan> warningDelays = new List<TimeSpan>(){TimeSpan.FromMinutes(1.0), TimeSpan.FromSeconds(30.0)},
            bool enableEmergencyBackups = true, int emergencyBackupHour = 3,
            CompressionLevel compressionLevel = CompressionLevel.Normal)
        {
            m_SavesEnabled = savesEnabled;
            m_SaveAccessLevel = saveAccessLevel;
            m_SaveStrategy = saveStrategy;
            m_AllowBackgroundWrite = allowBackgroundWrite;
            m_SaveDelay = saveDelay;
            m_WarningDelays = warningDelays;
            m_NoIOHour = noIOHour;
            m_EnableEmergencyBackups = enableEmergencyBackups;
            m_EmergencyBackupHour = emergencyBackupHour;
            m_CompressionLevel = compressionLevel;
        }

        public override void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(m_SavesEnabled);
            writer.Write((byte)m_SaveAccessLevel);
            writer.Write((byte)Utilities.GetSaveType(m_SaveStrategy));
            writer.Write(m_AllowBackgroundWrite);
            writer.Write(m_SaveDelay);

            writer.Write(m_WarningDelays.Count);

            for (int i = 0; i < m_WarningDelays.Count; i++)
            {
                writer.Write(m_WarningDelays[i]);
            }

            writer.Write(m_NoIOHour);

            writer.Write(m_EnableEmergencyBackups);
            writer.Write(m_EmergencyBackupHour);
            writer.Write((byte)m_CompressionLevel);
        }

        public SaveSettings(GenericReader reader)
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
                        m_SavesEnabled = reader.ReadBool();
                        m_SaveAccessLevel = (AccessLevel)reader.ReadByte();
                        m_SaveStrategy = Utilities.GetSaveStrategy((SaveStrategyTypes)reader.ReadByte());
                        m_AllowBackgroundWrite = reader.ReadBool();
                        m_SaveDelay = reader.ReadTimeSpan();

                        m_WarningDelays = new List<TimeSpan>();
                        int count = reader.ReadInt();

                        for (int i = 0; i < count; i++)
                        {
                            m_WarningDelays.Add(reader.ReadTimeSpan());
                        }

                        m_NoIOHour = reader.ReadInt();

                        m_EnableEmergencyBackups = reader.ReadBool();
                        m_EmergencyBackupHour = reader.ReadInt();
                        m_CompressionLevel = (CompressionLevel)reader.ReadByte();
                        break;
                    }
            }
        }

        public override string ToString()
        {
            return @"Save Settings";
        }
    }
}
