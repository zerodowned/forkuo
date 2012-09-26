﻿using System;
using Server;
using System.Collections;
using System.Collections.Generic;

namespace CustomsFramework.Systems.VIPSystem
{
    public enum BonusName
    {
        ResProtection = 0,
        ToolbarAccess = 1,
        BasicCommands = 2,
        GainIncrease = 3,
        FreeCorpseReturn = 4,

        FullLRC = 5,
        BankIncrease = 6,
        LifeStoneNoUses = 7,
        LootGoldFromGround = 8,
        DoubleResources = 9,

        LootGoldFromCorpses = 10,
        GlobalBankCommands = 11,
        SmartGrabBags = 12,
        FreeHouseDecoration = 13, // Added
        UnlimitedTools = 14
    }

    [PropertyObject]
    public class Bonus
    {
        private Bonuses _Bonuses;
        private BonusInfo _Info;
        private bool _Enabled;
        private DateTime _TimeStarted;
        private TimeSpan _ServicePeriod;

        public override string ToString()
        {
 	        return String.Format("[{0}: {1}]", Name, Enabled);
        }

        public Bonus(Bonuses bonuses, BonusInfo info, GenericReader reader)
        {
            _Bonuses = bonuses;
            _Info = info;

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _Enabled = reader.ReadBool();
                        _TimeStarted = reader.ReadDateTime();
                        _ServicePeriod = reader.ReadTimeSpan();
                        break;
                    }
            }
        }

        public Bonus(Bonuses bonuses, BonusInfo info, bool enabled)
        {
            _Bonuses = bonuses;
            _Info = info;
            _Enabled = enabled;
            _TimeStarted = DateTime.MinValue;
            _ServicePeriod = TimeSpan.Zero;
        }

        public Bonus(Bonuses bonuses, BonusInfo info)
        {
            _Bonuses = bonuses;
            _Info = info;
            _Enabled = false;
            _TimeStarted = DateTime.MinValue;
            _ServicePeriod = TimeSpan.Zero;
        }

        public void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Enabled);
            writer.Write(_TimeStarted);
            writer.Write(_ServicePeriod);
        }

        public Bonuses Bonuses
        {
            get
            {
                return _Bonuses;
            }
        }

        public BonusName BonusName
        {
            get
            {
                return (BonusName)_Info.BonusID;
            }
        }

        public int BonusID
        {
            get
            {
                return _Info.BonusID;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public string Name
        {
            get
            {
                return _Info.BonusName;
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
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
    }

    public class BonusInfo
    {
        private int _BonusID;
        private string _BonusName;
        private string _BonusDescription;

        public BonusInfo(int bonusID, string name, string description)
        {
            _BonusID = bonusID;
            _BonusName = name;
            _BonusDescription = description;
        }

        public int BonusID
        {
            get
            {
                return _BonusID;
            }
            set
            {
                _BonusID = value;
            }
        }

        public string BonusName
        {
            get
            {
                return _BonusName;
            }
            set
            {
                _BonusName = value;
            }
        }

        public string BonusDescription
        {
            get
            {
                return _BonusDescription;
            }
            set
            {
                _BonusDescription = value;
            }
        }

        private static BonusInfo[] _Table = new BonusInfo[15]
        {
            new BonusInfo(0, "Ressurection Protection", "An optional 30 second ressurection protection."),
            new BonusInfo(1, "Toolbar Access", "Access to the [Toolbar command, for quick easy commands."),
            new BonusInfo(2, "Basic Commands", "Access to basic VIP commands."),
            new BonusInfo(3, "Skill & Stat Gain Increase", "Faster skill and stat gains."),
            new BonusInfo(4, "Free Corpse Retrieval", "No fee for getting your corpse back."),
            new BonusInfo(5, "Full LRC", "No regs needed to cast spells."),
            new BonusInfo(6, "Bank Size Increase", "Get more room in your bank box."),
            new BonusInfo(7, "Unlimited Life Stone Uses", "Life Stones will not consume uses when you use them."),
            new BonusInfo(8, "Loot Gold From Ground", "Ledger will loot gold you walk over, automatically."),
            new BonusInfo(9, "Double Resource Gain", "Double resources, no matter what map you're on."),
            new BonusInfo(10, "Loot Gold From Corpses", "Ledger will loot gold from near-by kills, automatically."),
            new BonusInfo(11, "Global Bank Commands", "Access your bank account from anywhere."),
            new BonusInfo(12, "Smart Grab Bags", "Setup grab bags with separate lists."),
            new BonusInfo(13, "Free House Decoration", "House commits don't cost anything."),
            new BonusInfo(14, "Unlimited Tool Uses", "Non-crafting tools will not consume uses when you use them."),
        };

        public static BonusInfo[] Table
        {
            get
            {
                return _Table;
            }
            set
            {
                _Table = value;
            }
        }
    }

    [PropertyObject]
    public class Bonuses : IEnumerable<Bonus>
    {
        private Bonus[] _Bonuses;

        [CommandProperty(AccessLevel.Developer)]
        public Bonus ResProtection
        {
            get
            {
                return this[BonusName.ResProtection];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus ToolbarAccess
        {
            get
            {
                return this[BonusName.ToolbarAccess];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus BasicCommands
        {
            get
            {
                return this[BonusName.BasicCommands];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus GainIncrease
        {
            get
            {
                return this[BonusName.GainIncrease];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus FreeCorpseReturn
        {
            get
            {
                return this[BonusName.FreeCorpseReturn];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus FullLRC
        {
            get
            {
                return this[BonusName.FullLRC];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus BankIncrease
        {
            get
            {
                return this[BonusName.BankIncrease];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus LifeStoneNoUses
        {
            get
            {
                return this[BonusName.LifeStoneNoUses];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus LootGoldFromGround
        {
            get
            {
                return this[BonusName.LootGoldFromGround];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus DoubleResources
        {
            get
            {
                return this[BonusName.DoubleResources];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus LootGoldFromCorpses
        {
            get
            {
                return this[BonusName.LootGoldFromCorpses];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus GlobalBankCommands
        {
            get
            {
                return this[BonusName.GlobalBankCommands];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus SmartGrabBags
        {
            get
            {
                return this[BonusName.SmartGrabBags];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus FreeHouseDecoration
        {
            get
            {
                return this[BonusName.FreeHouseDecoration];
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.Developer)]
        public Bonus UnlimitedTools
        {
            get
            {
                return this[BonusName.UnlimitedTools];
            }
            set
            {
            }
        }

        public int Length
        {
            get
            {
                return _Bonuses.Length;
            }
        }

        public Bonus this[BonusName name]
        {
            get
            {
                return this[(int)name];
            }
        }

        public Bonus this[int bonusID]
        {
            get
            {
                if (bonusID < 0 || bonusID >= _Bonuses.Length)
                    return null;

                Bonus bonus = _Bonuses[bonusID];

                if (bonus == null)
                    _Bonuses[bonusID] = bonus = new Bonus(this, BonusInfo.Table[bonusID]);

                return bonus;
            }
        }

        public override string ToString()
        {
 	        return "...";
        }

        public Bonuses()
        {
            BonusInfo[] info = BonusInfo.Table;
            _Bonuses = new Bonus[info.Length];

            for (int i = 0; i < info.Length; ++i)
                _Bonuses[i] = new Bonus(this, info[i]);
        }

        public void StartBonuses()
        {
            VIPCore core = World.GetCore(typeof(VIPCore)) as VIPCore;

            if (core != null)
            {
                foreach (Bonus bonus in _Bonuses)
                {
                    if (bonus.Enabled)
                    {
                        bonus.TimeStarted = DateTime.Now;
                        bonus.ServicePeriod = core.ServiceTimespan;
                    }
                }
            }
        }

        public Bonuses(GenericReader reader)
        {
            Deserialize(reader);
        }

        public void Serialize(GenericWriter writer)
        {
            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_Bonuses.Length);

            for (int i = 0; i < _Bonuses.Length; ++i)
            {
                _Bonuses[i].Serialize(writer);
            }
        }

        private void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        BonusInfo[] info = BonusInfo.Table;
                        _Bonuses = new Bonus[info.Length];

                        int count = reader.ReadInt();

                        for (int i = 0; i < count; ++i)
                        {
                            _Bonuses[i] = new Bonus(this, info[i], reader);
                        }

                        break;
                    }
            }
        }

        public IEnumerator<Bonus> GetEnumerator()
        {
            foreach (Bonus bonus in _Bonuses)
            {
                yield return bonus;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
