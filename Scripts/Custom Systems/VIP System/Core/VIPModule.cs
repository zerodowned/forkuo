using System;
using Server;
using Server.Gumps;

namespace CustomsFramework.Systems.VIPSystem
{
    public enum VIPTier
    {
        None,
        Bronze,
        Silver,
        Gold
    }

	public class VIPModule : BaseModule
	{
        public override string Name
        {
            get
            {
                if (LinkedMobile != null)
                    return String.Format(@"VIP Module - {0}", LinkedMobile.Name);
                else
                    return @"Unlinked VIP Module";
            }
        }

        public override string Description
        {
            get
            {
                if (LinkedMobile != null)
                    return String.Format(@"VIP Module that is linked to {0}, was linked on {1}, and expires on {2}", LinkedMobile.Name, 0, 0);
                else
                    return @"Unlinked VIP Module";
            }
        }

        public override string Version
        {
            get
            {
                return VIPCore.SystemVersion;
            }
        }

        public override AccessLevel EditLevel
        {
            get
            {
                return AccessLevel.Developer;
            }
        }
        
        public override Gump SettingsGump
        {
            get
            {
                return base.SettingsGump; // TODO: Create a settings gump.
            }
        }

        private bool _Canceled;
        private DateTime _TimeStarted;
        private TimeSpan _ServicePeriod;

        private VIPTier _Tier;
        private Bonuses _Bonuses;

        [CommandProperty(AccessLevel.Developer)]
        public bool Canceled
        {
            get
            {
                return _Canceled;
            }
            set
            {
                _Canceled = value;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public DateTime TimeStarted
        {
            get
            {
                return _TimeStarted;
            }
            set
            {
                _TimeStarted = value;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public TimeSpan ServicePeriod
        {
            get
            {
                return _ServicePeriod;
            }
            set
            {
                _ServicePeriod = value;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public VIPTier Tier
        {
            get
            {
                return _Tier;
            }
            set
            {
                _Tier = value;
                SetTier(value);
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonuses Bonuses
        {
            get
            {
                return _Bonuses;
            }
            set
            {
                _Bonuses = value;
            }
        }

        public VIPModule(Mobile from, BaseVIPDeed deed) : base()
        {
            _Canceled = false;
            _TimeStarted = DateTime.MinValue;
            _ServicePeriod = TimeSpan.Zero;

            if (deed != null)
            {
                _Tier = deed.Tier;
                _Bonuses = deed.Bonuses;
            }

            LinkMobile(from);
        }

        public VIPModule(CustomSerial serial)
            : base(serial)
        {
        }

        public void SetTier(VIPTier tier)
        {
            if (tier == VIPTier.None)
            {
                foreach (Bonus bonus in _Bonuses)
                {
                    bonus.Enabled = false;
                }
            }
            else if (tier == VIPTier.Bronze)
            {
                _Bonuses[0].Enabled = true;
                _Bonuses[1].Enabled = true;
                _Bonuses[2].Enabled = true;
                _Bonuses[3].Enabled = true;
                _Bonuses[4].Enabled = true;
            }
            else if (tier == VIPTier.Silver)
            {
                _Bonuses[5].Enabled = true;
                _Bonuses[6].Enabled = true;
                _Bonuses[7].Enabled = true;
                _Bonuses[8].Enabled = true;
                _Bonuses[9].Enabled = true;
            }
            else if (tier == VIPTier.Gold)
            {
                _Bonuses[10].Enabled = true;
                _Bonuses[11].Enabled = true;
                _Bonuses[12].Enabled = true;
                _Bonuses[13].Enabled = true;
                _Bonuses[14].Enabled = true;
            }
        }

        public override void Prep()
        {
            base.Prep();

            Check();
        }

        public void Check()
        {
            if (!LinkedMobile.Deleted || LinkedMobile != null)
            {
                switch (_Tier)
                {
                    case VIPTier.None:
                        {
                            foreach (Bonus bonus in _Bonuses)
                            {
                                if (bonus.TimeStarted + bonus.ServicePeriod >= DateTime.Now)
                                {
                                    bonus.Enabled = false;
                                    bonus.ServicePeriod = TimeSpan.Zero;
                                    bonus.TimeStarted = DateTime.MinValue;
                                }
                            }
                            break;
                        }
                    case VIPTier.Bronze:
                        {
                            if (_TimeStarted + _ServicePeriod >= DateTime.Now)
                            {
                                _TimeStarted = DateTime.MinValue;
                                _ServicePeriod = TimeSpan.Zero;
                            }

                            _Canceled = true;
                            LinkedMobile.AccessLevel = AccessLevel.Player;

                            goto case VIPTier.None;
                        }
                    case VIPTier.Silver:
                        {
                            goto case VIPTier.Bronze;
                        }
                    case VIPTier.Gold:
                        {
                            goto case VIPTier.Bronze;
                        }
                }
            }
            else
            {
                _Canceled = true;
            }
        }

        public override void Update()
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Canceled);
            writer.Write(_TimeStarted);
            writer.Write(_ServicePeriod);
            _Bonuses.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _Canceled = reader.ReadBool();
                        _TimeStarted = reader.ReadDateTime();
                        _ServicePeriod = reader.ReadTimeSpan();
                        _Bonuses = new Bonuses(reader);
                        break;
                    }
            }
        }
	}
}
