# **Customs Framework** #

---

**Info:** The _Customs Framework_ was designed **for** developers to utilize. This is to prevent systems from being designed to use the _Item class_ as a base class. Thus freeing up memory and resources, as you won't be serializing all the extra crap that Item has. It's designed to be easy to use (little to no learning curve,) and easy to extend.

**Why?** - I believe that developers or the average person that decides to create things for <sub>RunUO</sub> **ForkUO** should be able to easily develop nice and robust systems that are entirely drag and drop. This would help alleviate supporting said systems so much, and streamline the install process of community developed systems.

**WTF is it...?** - The Customs Framework mostly consists of three new base classes. Which are **BaseCore**, **BaseModule**, and **BaseService**. Developers can use these new base classes by simply inheriting from either one. They allow developers to easily serialize and handle their own data. Each new class serving it's own purpose. These new classes _will_ be managed by a global system at some point, for easier management of systems and modules of data.


---


### Base Core ###

**Usage:** BaseCore is meant to be used as the main Core/Engine of a new system. By default it only has two values that get serialized when the core is created. Which is Enabled, and Deleted (both for internal system usage.) Cores do need to be initialized in a certain way, depending on how it's used.

Ideally it should be initialized in a way so that there is only **one** instance of a type of core. Although you could technically create multiple cores of the same type.

Good example of creating your own core would be the **VIPCore**.

```
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
        ...
```

Notice how the Initialize() method is used to create a single instance of VIPCore. Initialize is actually called by the script compiler, meaning the core is created before any data read or used.

In this example, **VIPCore** is used to hold all needed variables of the VIP System. So only one instance would ever be needed. If you examine the system, you'll notice it references the first VIPCore that the framework returns.

All **Cores** can override a handful of default properties, which are meant for internal system usage. Although they can be used within your own system too.

```
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
        ...
```

As you see, this core has it's own set of custom properties that are used within the rest of the system. It even manages a list of it's own **VIPModules** which inherits from _BaseModule_.

All **Cores** must implement a default constructor. Which in this case would be the **VIPCore() : Base()** constructor. The default constructor should always call the base constructor, like so.

You **must** also implement the CustomSerial constructor that is listed above for any core you create. This is similar to how you'd create an Item or MObile. So these aren't big changes.

```
        public override void Prep() // Called after all cores are loaded
        {
            Server.EventSink.Login += new LoginEventHandler(VIPHook_Login);
            Server.EventSink.Logout += new LogoutEventHandler(VIPHook_Logout);
            Server.EventSink.Disconnected += new DisconnectedEventHandler(VIPHook_Disconnected);
            CommandSystem.Register("VIP", AccessLevel.VIP, new CommandEventHandler(Command_VIP));
        }
        ...
```

This is another method that **Cores** (and the other two new base types) can override. Prep() is called after _all_ cores are loaded from disk. You should use this instead of the Initialize() method for most cases.

```
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
```

Surprise, surprise. Serializing your data for your own systems is as easy as it is to save data to items or mobiles. There are a few advantages to using this to save data, compared to tagging it onto a custom item.

Main advantage, is that your data can exist without the need to ever create an instance of an item. Meaning you won't have to worry about keeping track of this item, or worry about it being deleted. Although you could still create "master" items for systems as you've done in the past and add your custom core (or specific properties linked from your core) to your item...which is far safer.


---


**More updates to the wiki later...**