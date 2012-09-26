using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Commands;
using Server.Items;

namespace CustomsFramework.Systems.VIPSystem
{
    public partial class VIPCore : BaseCore
    {
        public static void Initialize()
        {
            VIPCore core = World.GetCore(typeof(VIPCore)) as VIPCore;

            if (core == null)
            {
                core = new VIPCore();
                core.Prep();
            }
        }

        public const string SystemVersion = @"1.0";

        public override string Name { get { return @"VIP Core"; } }
        public override string Description { get { return @"Core that contains everything for the VIP system."; } }
        public override string Version { get { return SystemVersion; } }
        public override AccessLevel EditLevel { get { return AccessLevel.Developer; } }
        public override Gump SettingsGump { get { return null; } }

        public List<VIPModule> _VIPModules;

        private TimeSpan _ServiceTimespan;
        private double _ExchangeRate;

        private int _GoldFee;
        private int _SilverFee;
        private int _BronzeFee;
        private int _GoldBonusFee;
        private int _SilverBonusFee;
        private int _BronzeBonusFee;

        public TimeSpan ServiceTimespan
        {
            get
            {
                return _ServiceTimespan;
            }
            set
            {
                _ServiceTimespan = value;
            }
        }

        public double ExchangeRate
        {
            get
            {
                return _ExchangeRate;
            }
            set
            {
                _ExchangeRate = value;
            }
        }

        public int GoldFee
        {
            get
            {
                return _GoldFee;
            }
            set
            {
                _GoldFee = value;
            }
        }

        public int SilverFee
        {
            get
            {
                return _SilverFee;
            }
            set
            {
                _SilverFee = value;
            }
        }

        public int BronzeFee
        {
            get
            {
                return _BronzeFee;
            }
            set
            {
                _BronzeFee = value;
            }
        }

        public int GoldBonusFee
        {
            get
            {
                return _GoldBonusFee;
            }
            set
            {
                _GoldBonusFee = value;
            }
        }

        public int SilverBonusFee
        {
            get
            {
                return _SilverBonusFee;
            }
            set
            {
                _SilverBonusFee = value;
            }
        }

        public int BronzeBonusFee
        {
            get
            {
                return _BronzeBonusFee;
            }
            set
            {
                _BronzeBonusFee = value;
            }
        }

        public VIPCore() : base()
        {
            Enabled = true;

            _ServiceTimespan = TimeSpan.FromDays(30.0);
            _ExchangeRate = 0.1;

            _GoldFee = 500;
            _SilverFee = 250;
            _BronzeFee = 100;

            _GoldBonusFee = 250;
            _SilverBonusFee = 125;
            _BronzeBonusFee = 50;
        }

        public VIPCore(CustomSerial serial)
            : base(serial)
        {
        }

        public override void Prep() // Called after all cores are loaded
        {
            Server.EventSink.Login += new LoginEventHandler(VIPHook_Login);
            Server.EventSink.Logout += new LogoutEventHandler(VIPHook_Logout);
            Server.EventSink.Disconnected += new DisconnectedEventHandler(VIPHook_Disconnected);
            CommandSystem.Register("VIP", AccessLevel.VIP, new CommandEventHandler(Command_VIP));
        }

        public static void VIPHook_Login(LoginEventArgs e)
        {
            CheckModule(e.Mobile);
        }

        public static void VIPHook_Logout(LogoutEventArgs e)
        {
            CheckModule(e.Mobile);
        }

        public static void VIPHook_Disconnected(DisconnectedEventArgs e)
        {
            CheckModule(e.Mobile);
        }

        private static void CheckModule(Mobile from)
        {
            VIPModule module = from.GetModule(typeof(VIPModule)) as VIPModule;

            if (module != null)
                module.Check();
        }

        public int GetBalance(Mobile from)
        {
            int balance = 0;

            Container bank = from.FindBankNoCreate();
            Container backpack = from.Backpack;
            Item[] DonatorDeeds;

            if (bank != null)
            {
                DonatorDeeds = bank.FindItemsByType(typeof(DonatorDeed));

                for (int i = 0; i < DonatorDeeds.Length; i++)
                    balance += DonatorDeeds[i].Amount;
            }

            if (backpack != null)
            {
                DonatorDeeds = backpack.FindItemsByType(typeof(DonatorDeed));

                for (int i = 0; i < DonatorDeeds.Length; i++)
                    balance += DonatorDeeds[i].Amount;
            }

            return balance;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            Utilities.WriteVersion(writer, 0);

            // Version 0
            writer.Write(_ServiceTimespan);
            writer.Write(_ExchangeRate);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _ServiceTimespan = reader.ReadTimeSpan();
                        _ExchangeRate = reader.ReadDouble();
                        break;
                    }
            }
        }
    }
}
