using System;
using System.Collections;
using System.Collections.Generic;
using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.CannedEvil;
using Server.Engines.Craft;
using Server.Engines.Help;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Fifth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Spellweaving;
using Server.Targeting;

namespace Server.Mobiles
{
    #region Enums
    [Flags]
    public enum PlayerFlag // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
    {
        None = 0x00000000,
        Glassblowing = 0x00000001,
        Masonry = 0x00000002,
        SandMining = 0x00000004,
        StoneMining = 0x00000008,
        ToggleMiningStone = 0x00000010,
        KarmaLocked = 0x00000020,
        AutoRenewInsurance = 0x00000040,
        UseOwnFilter = 0x00000080,
        PublicMyRunUO = 0x00000100,
        PagingSquelched = 0x00000200,
        Young = 0x00000400,
        AcceptGuildInvites = 0x00000800,
        DisplayChampionTitle = 0x00001000,
        HasStatReward = 0x00002000
    }

    public enum NpcGuild
    {
        None,
        MagesGuild,
        WarriorsGuild,
        ThievesGuild,
        RangersGuild,
        HealersGuild,
        MinersGuild,
        MerchantsGuild,
        TinkersGuild,
        TailorsGuild,
        FishermensGuild,
        BardsGuild,
        BlacksmithsGuild
    }

    public enum SolenFriendship
    {
        None,
        Red,
        Black
    }
    #endregion

    public partial class PlayerMobile : Mobile, IHonorTarget
    {
        private class CountAndTimeStamp
        {
            private int m_Count;
            private DateTime m_Stamp;

            public CountAndTimeStamp()
            {
            }

            public DateTime TimeStamp
            {
                get
                {
                    return this.m_Stamp;
                }
            }
            public int Count
            {
                get
                {
                    return this.m_Count;
                }
                set
                {
                    this.m_Count = value;
                    this.m_Stamp = DateTime.Now;
                }
            }
        }

        private DesignContext m_DesignContext;

        private NpcGuild m_NpcGuild;
        private DateTime m_NpcGuildJoinTime;
        private DateTime m_NextBODTurnInTime;
        private TimeSpan m_NpcGuildGameTime;
        private PlayerFlag m_Flags;
        private int m_StepsTaken;
        private int m_Profession;
        private bool m_IsStealthing; // IsStealthing should be moved to Server.Mobiles
        private bool m_IgnoreMobiles; // IgnoreMobiles should be moved to Server.Mobiles
        private int m_NonAutoreinsuredItems; // number of items that could not be automaitically reinsured because gold in bank was not enough
        private bool m_NinjaWepCooldown;
        /*
        * a value of zero means, that the mobile is not executing the spell. Otherwise,
        * the value should match the BaseMana required
        */
        private int m_ExecutesLightningStrike; // move to Server.Mobiles??

        private DateTime m_LastOnline;
        private Server.Guilds.RankDefinition m_GuildRank;

        private int m_GuildMessageHue, m_AllianceMessageHue;

        private List<Mobile> m_AutoStabled;
        private List<Mobile> m_AllFollowers;
        private List<Mobile> m_RecentlyReported;

        #region Getters & Setters

        public List<Mobile> RecentlyReported
        {
            get
            {
                return this.m_RecentlyReported;
            }
            set
            {
                this.m_RecentlyReported = value;
            }
        }

        public List<Mobile> AutoStabled
        {
            get
            {
                return this.m_AutoStabled;
            }
        }

        public bool NinjaWepCooldown
        {
            get
            {
                return this.m_NinjaWepCooldown;
            }
            set
            {
                this.m_NinjaWepCooldown = value;
            }
        }

        public List<Mobile> AllFollowers
        {
            get
            {
                if (this.m_AllFollowers == null)
                    this.m_AllFollowers = new List<Mobile>();
                return this.m_AllFollowers;
            }
        }

        public Server.Guilds.RankDefinition GuildRank
        {
            get
            {
                if (this.AccessLevel >= AccessLevel.GameMaster)
                    return Server.Guilds.RankDefinition.Leader;
                else
                    return this.m_GuildRank;
            }
            set
            {
                this.m_GuildRank = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GuildMessageHue
        {
            get
            {
                return this.m_GuildMessageHue;
            }
            set
            {
                this.m_GuildMessageHue = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AllianceMessageHue
        {
            get
            {
                return this.m_AllianceMessageHue;
            }
            set
            {
                this.m_AllianceMessageHue = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Profession
        {
            get
            {
                return this.m_Profession;
            }
            set
            {
                this.m_Profession = value;
            }
        }

        public int StepsTaken
        {
            get
            {
                return this.m_StepsTaken;
            }
            set
            {
                this.m_StepsTaken = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsStealthing // IsStealthing should be moved to Server.Mobiles
        {
            get
            {
                return this.m_IsStealthing;
            }
            set
            {
                this.m_IsStealthing = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IgnoreMobiles // IgnoreMobiles should be moved to Server.Mobiles
        {
            get
            {
                return this.m_IgnoreMobiles;
            }
            set
            {
                if (this.m_IgnoreMobiles != value)
                {
                    this.m_IgnoreMobiles = value;
                    this.Delta(MobileDelta.Flags);
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public NpcGuild NpcGuild
        {
            get
            {
                return this.m_NpcGuild;
            }
            set
            {
                this.m_NpcGuild = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NpcGuildJoinTime
        {
            get
            {
                return this.m_NpcGuildJoinTime;
            }
            set
            {
                this.m_NpcGuildJoinTime = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextBODTurnInTime
        {
            get
            {
                return this.m_NextBODTurnInTime;
            }
            set
            {
                this.m_NextBODTurnInTime = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOnline
        {
            get
            {
                return this.m_LastOnline;
            }
            set
            {
                this.m_LastOnline = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastMoved
        {
            get
            {
                return this.LastMoveTime;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NpcGuildGameTime
        {
            get
            {
                return this.m_NpcGuildGameTime;
            }
            set
            {
                this.m_NpcGuildGameTime = value;
            }
        }

        private int m_ToTItemsTurnedIn;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToTItemsTurnedIn
        {
            get
            {
                return this.m_ToTItemsTurnedIn;
            }
            set
            {
                this.m_ToTItemsTurnedIn = value;
            }
        }

        private int m_ToTTotalMonsterFame;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToTTotalMonsterFame
        {
            get
            {
                return this.m_ToTTotalMonsterFame;
            }
            set
            {
                this.m_ToTTotalMonsterFame = value;
            }
        }

        public int ExecutesLightningStrike
        {
            get
            {
                return this.m_ExecutesLightningStrike;
            }
            set
            {
                this.m_ExecutesLightningStrike = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToothAche
        {
            get
            {
                return CandyCane.GetToothAche(this);
            }
            set
            {
                CandyCane.SetToothAche(this, value);
            }
        }

        #endregion

        #region PlayerFlags
        public PlayerFlag Flags
        {
            get
            {
                return this.m_Flags;
            }
            set
            {
                this.m_Flags = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PagingSquelched
        {
            get
            {
                return this.GetFlag(PlayerFlag.PagingSquelched);
            }
            set
            {
                this.SetFlag(PlayerFlag.PagingSquelched, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Glassblowing
        {
            get
            {
                return this.GetFlag(PlayerFlag.Glassblowing);
            }
            set
            {
                this.SetFlag(PlayerFlag.Glassblowing, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Masonry
        {
            get
            {
                return this.GetFlag(PlayerFlag.Masonry);
            }
            set
            {
                this.SetFlag(PlayerFlag.Masonry, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SandMining
        {
            get
            {
                return this.GetFlag(PlayerFlag.SandMining);
            }
            set
            {
                this.SetFlag(PlayerFlag.SandMining, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool StoneMining
        {
            get
            {
                return this.GetFlag(PlayerFlag.StoneMining);
            }
            set
            {
                this.SetFlag(PlayerFlag.StoneMining, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningStone
        {
            get
            {
                return this.GetFlag(PlayerFlag.ToggleMiningStone);
            }
            set
            {
                this.SetFlag(PlayerFlag.ToggleMiningStone, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool KarmaLocked
        {
            get
            {
                return this.GetFlag(PlayerFlag.KarmaLocked);
            }
            set
            {
                this.SetFlag(PlayerFlag.KarmaLocked, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AutoRenewInsurance
        {
            get
            {
                return this.GetFlag(PlayerFlag.AutoRenewInsurance);
            }
            set
            {
                this.SetFlag(PlayerFlag.AutoRenewInsurance, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnFilter
        {
            get
            {
                return this.GetFlag(PlayerFlag.UseOwnFilter);
            }
            set
            {
                this.SetFlag(PlayerFlag.UseOwnFilter, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PublicMyRunUO
        {
            get
            {
                return this.GetFlag(PlayerFlag.PublicMyRunUO);
            }
            set
            {
                this.SetFlag(PlayerFlag.PublicMyRunUO, value);
                this.InvalidateMyRunUO();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AcceptGuildInvites
        {
            get
            {
                return this.GetFlag(PlayerFlag.AcceptGuildInvites);
            }
            set
            {
                this.SetFlag(PlayerFlag.AcceptGuildInvites, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasStatReward
        {
            get
            {
                return this.GetFlag(PlayerFlag.HasStatReward);
            }
            set
            {
                this.SetFlag(PlayerFlag.HasStatReward, value);
            }
        }
        #endregion

        #region Auto Arrow Recovery
        private Dictionary<Type, int> m_RecoverableAmmo = new Dictionary<Type, int>();

        public Dictionary<Type, int> RecoverableAmmo
        {
            get
            {
                return this.m_RecoverableAmmo;
            }
        }

        public void RecoverAmmo()
        {
            if (Core.SE && this.Alive)
            {
                foreach (KeyValuePair<Type, int> kvp in this.m_RecoverableAmmo)
                {
                    if (kvp.Value > 0)
                    {
                        Item ammo = null;

                        try
                        {
                            ammo = Activator.CreateInstance(kvp.Key) as Item;
                        }
                        catch
                        {
                        }

                        if (ammo != null)
                        {
                            string name = ammo.Name;
                            ammo.Amount = kvp.Value;

                            if (name == null)
                            {
                                if (ammo is Arrow)
                                    name = "arrow";
                                else if (ammo is Bolt)
                                    name = "bolt";
                            }

                            if (name != null && ammo.Amount > 1)
                                name = String.Format("{0}s", name);

                            if (name == null)
                                name = String.Format("#{0}", ammo.LabelNumber);

                            this.PlaceInBackpack(ammo);
                            this.SendLocalizedMessage(1073504, String.Format("{0}\t{1}", ammo.Amount, name)); // You recover ~1_NUM~ ~2_AMMO~.
                        }
                    }
                }

                this.m_RecoverableAmmo.Clear();
            }
        }

        #endregion

        private DateTime m_AnkhNextUse;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AnkhNextUse
        {
            get
            {
                return this.m_AnkhNextUse;
            }
            set
            {
                this.m_AnkhNextUse = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DisguiseTimeLeft
        {
            get
            {
                return DisguiseTimers.TimeRemaining(this);
            }
        }

        private DateTime m_PeacedUntil;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime PeacedUntil
        {
            get
            {
                return this.m_PeacedUntil;
            }
            set
            {
                this.m_PeacedUntil = value;
            }
        }

        #region Scroll of Alacrity
        private DateTime m_AcceleratedStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AcceleratedStart
        {
            get
            {
                return this.m_AcceleratedStart;
            }
            set
            {
                this.m_AcceleratedStart = value;
            }
        }

        private SkillName m_AcceleratedSkill;

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName AcceleratedSkill
        {
            get
            {
                return this.m_AcceleratedSkill;
            }
            set
            {
                this.m_AcceleratedSkill = value;
            }
        }
        #endregion

        public static Direction GetDirection4(Point3D from, Point3D to)
        {
            int dx = from.X - to.X;
            int dy = from.Y - to.Y;

            int rx = dx - dy;
            int ry = dx + dy;

            Direction ret;

            if (rx >= 0 && ry >= 0)
                ret = Direction.West;
            else if (rx >= 0 && ry < 0)
                ret = Direction.South;
            else if (rx < 0 && ry < 0)
                ret = Direction.East;
            else
                ret = Direction.North;

            return ret;
        }

        public override bool OnDroppedItemToWorld(Item item, Point3D location)
        {
            if (!base.OnDroppedItemToWorld(item, location))
                return false;

            IPooledEnumerable mobiles = this.Map.GetMobilesInRange(location, 0);

            foreach (Mobile m in mobiles)
            {
                if (m.Z >= location.Z && m.Z < location.Z + 16)
                {
                    mobiles.Free();
                    return false;
                }
            }

            mobiles.Free();

            BounceInfo bi = item.GetBounce();

            if (bi != null)
            {
                Type type = item.GetType();

                if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
                {
                    object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

                    if (objs != null && objs.Length > 0)
                    {
                        FlipableAttribute fp = objs[0] as FlipableAttribute;

                        if (fp != null)
                        {
                            int[] itemIDs = fp.ItemIDs;

                            Point3D oldWorldLoc = bi.m_WorldLoc;
                            Point3D newWorldLoc = location;

                            if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
                            {
                                Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

                                if (itemIDs.Length == 2)
                                {
                                    switch ( dir )
                                    {
                                        case Direction.North:
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                        case Direction.West:
                                            item.ItemID = itemIDs[1];
                                            break;
                                    }
                                }
                                else if (itemIDs.Length == 4)
                                {
                                    switch ( dir )
                                    {
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                            item.ItemID = itemIDs[1];
                                            break;
                                        case Direction.North:
                                            item.ItemID = itemIDs[2];
                                            break;
                                        case Direction.West:
                                            item.ItemID = itemIDs[3];
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override int GetPacketFlags()
        {
            int flags = base.GetPacketFlags();

            if (this.m_IgnoreMobiles)
                flags |= 0x10;

            return flags;
        }

        public override int GetOldPacketFlags()
        {
            int flags = base.GetOldPacketFlags();

            if (this.m_IgnoreMobiles)
                flags |= 0x10;

            return flags;
        }

        public bool GetFlag(PlayerFlag flag)
        {
            return ((this.m_Flags & flag) != 0);
        }

        public void SetFlag(PlayerFlag flag, bool value)
        {
            if (value)
                this.m_Flags |= flag;
            else
                this.m_Flags &= ~flag;
        }

        public DesignContext DesignContext
        {
            get
            {
                return this.m_DesignContext;
            }
            set
            {
                this.m_DesignContext = value;
            }
        }

        public static void Initialize()
        {
            if (FastwalkPrevention)
                PacketHandlers.RegisterThrottler(0x02, new ThrottlePacketCallback(MovementThrottle_Callback));

            EventSink.Login += new LoginEventHandler(OnLogin);
            EventSink.Logout += new LogoutEventHandler(OnLogout);
            EventSink.Connected += new ConnectedEventHandler(EventSink_Connected);
            EventSink.Disconnected += new DisconnectedEventHandler(EventSink_Disconnected);

            if (Core.SE)
            {
                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(CheckPets));
            }
        }

        private static void CheckPets()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)m;

                    if (((!pm.Mounted || (pm.Mount != null && pm.Mount is EtherealMount)) && (pm.AllFollowers.Count > pm.AutoStabled.Count)) ||
                        (pm.Mounted && (pm.AllFollowers.Count > (pm.AutoStabled.Count + 1))))
                    {
                        pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
                    }
                }
            }
        }

        public override void OnSkillInvalidated(Skill skill)
        {
            if (Core.AOS && skill.SkillName == SkillName.MagicResist)
                this.UpdateResistances();
        }

        public override int GetMaxResistance(ResistanceType type)
        {
            if (this.IsStaff())
                return int.MaxValue;

            int max = base.GetMaxResistance(type);

            if (type != ResistanceType.Physical && 60 < max && Spells.Fourth.CurseSpell.UnderEffect(this))
                max = 60;

            if (Core.ML && this.Race == Race.Elf && type == ResistanceType.Energy)
                max += 5; //Intended to go after the 60 max from curse

            return max;
        }

        protected override void OnRaceChange(Race oldRace)
        {
            this.ValidateEquipment();
            this.UpdateResistances();
        }

        public override int MaxWeight
        {
            get
            {
                return (((Core.ML && this.Race == Race.Human) ? 100 : 40) + (int)(3.5 * this.Str));
            }
        }

        private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

        public override void OnNetStateChanged()
        {
            this.m_LastGlobalLight = -1;
            this.m_LastPersonalLight = -1;
        }

        public override void ComputeBaseLightLevels(out int global, out int personal)
        {
            global = LightCycle.ComputeLevelFor(this);

            bool racialNightSight = (Core.ML && this.Race == Race.Elf);

            if (this.LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
                personal = 21;
            else
                personal = this.LightLevel;
        }

        public override void CheckLightLevels(bool forceResend)
        {
            NetState ns = this.NetState;

            if (ns == null)
                return;

            int global, personal;

            this.ComputeLightLevels(out global, out personal);

            if (!forceResend)
                forceResend = (global != this.m_LastGlobalLight || personal != this.m_LastPersonalLight);

            if (!forceResend)
                return;

            this.m_LastGlobalLight = global;
            this.m_LastPersonalLight = personal;

            ns.Send(GlobalLightLevel.Instantiate(global));
            ns.Send(new PersonalLightLevel(this, personal));
        }

        public override int GetMinResistance(ResistanceType type)
        {
            int magicResist = (int)(this.Skills[SkillName.MagicResist].Value * 10);
            int min = int.MinValue;

            if (magicResist >= 1000)
                min = 40 + ((magicResist - 1000) / 50);
            else if (magicResist >= 400)
                min = (magicResist - 400) / 15;

            if (min > MaxPlayerResistance)
                min = MaxPlayerResistance;

            int baseMin = base.GetMinResistance(type);

            if (min < baseMin)
                min = baseMin;

            return min;
        }

        public override void OnManaChange(int oldValue)
        {
            base.OnManaChange(oldValue);
            if (this.m_ExecutesLightningStrike > 0)
            {
                if (this.Mana < this.m_ExecutesLightningStrike)
                {
                    LightningStrike.ClearCurrentMove(this);
                }
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            Mobile from = e.Mobile;

            CheckAtrophies(from);

            if (AccountHandler.LockdownLevel > AccessLevel.VIP)
            {
                string notice;

                Accounting.Account acct = from.Account as Accounting.Account;

                if (acct == null || !acct.HasAccess(from.NetState))
                {
                    if (from.IsPlayer())
                        notice = "The server is currently under lockdown. No players are allowed to log in at this time.";
                    else
                        notice = "The server is currently under lockdown. You do not have sufficient access level to connect.";

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Disconnect), from);
                }
                else if (from.AccessLevel >= AccessLevel.Administrator)
                {
                    notice = "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
                }
                else
                {
                    notice = "The server is currently under lockdown. You have sufficient access level to connect.";
                }

                from.SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
                return;
            }

            if (from is PlayerMobile)
                ((PlayerMobile)from).ClaimAutoStabledPets();
        }

        private bool m_NoDeltaRecursion;

        public void ValidateEquipment()
        {
            if (this.m_NoDeltaRecursion || this.Map == null || this.Map == Map.Internal)
                return;

            if (this.Items == null)
                return;

            this.m_NoDeltaRecursion = true;
            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(ValidateEquipment_Sandbox));
        }

        private void ValidateEquipment_Sandbox()
        {
            try
            {
                if (this.Map == null || this.Map == Map.Internal)
                    return;

                List<Item> items = this.Items;

                if (items == null)
                    return;

                bool moved = false;

                int str = this.Str;
                int dex = this.Dex;
                int intel = this.Int;

                #region Factions
                int factionItemCount = 0;
                #endregion

                Mobile from = this;

                #region Ethics
                Ethics.Ethic ethic = Ethics.Ethic.Find(from);
                #endregion

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                        continue;

                    Item item = items[i];

                    #region Ethics
                    if ((item.SavedFlags & 0x100) != 0)
                    {
                        if (item.Hue != Ethics.Ethic.Hero.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x100;
                        }
                        else if (ethic != Ethics.Ethic.Hero)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    else if ((item.SavedFlags & 0x200) != 0)
                    {
                        if (item.Hue != Ethics.Ethic.Evil.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x200;
                        }
                        else if (ethic != Ethics.Ethic.Evil)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    #endregion

                    if (item is BaseWeapon)
                    {
                        BaseWeapon weapon = (BaseWeapon)item;

                        bool drop = false;

                        if (dex < weapon.DexRequirement)
                            drop = true;
                        else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
                            drop = true;
                        else if (intel < weapon.IntRequirement)
                            drop = true;
                        else if (weapon.RequiredRace != null && weapon.RequiredRace != this.Race)
                            drop = true;

                        if (drop)
                        {
                            string name = weapon.Name;

                            if (name == null)
                                name = String.Format("#{0}", weapon.LabelNumber);

                            from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
                            from.AddToBackpack(weapon);
                            moved = true;
                        }
                    }
                    else if (item is BaseArmor)
                    {
                        BaseArmor armor = (BaseArmor)item;

                        bool drop = false;

                        if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (armor.RequiredRace != null && armor.RequiredRace != this.Race)
                        {
                            drop = true;
                        }
                        else
                        {
                            int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
                            int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
                            int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

                            if (dex < dexReq || (dex + dexBonus) < 1)
                                drop = true;
                            else if (str < strReq || (str + strBonus) < 1)
                                drop = true;
                            else if (intel < intReq || (intel + intBonus) < 1)
                                drop = true;
                        }

                        if (drop)
                        {
                            string name = armor.Name;

                            if (name == null)
                                name = String.Format("#{0}", armor.LabelNumber);

                            if (armor is BaseShield)
                                from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
                            else
                                from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(armor);
                            moved = true;
                        }
                    }
                    else if (item is BaseClothing)
                    {
                        BaseClothing clothing = (BaseClothing)item;

                        bool drop = false;

                        if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (clothing.RequiredRace != null && clothing.RequiredRace != this.Race)
                        {
                            drop = true;
                        }
                        else
                        {
                            int strBonus = clothing.ComputeStatBonus(StatType.Str);
                            int strReq = clothing.ComputeStatReq(StatType.Str);

                            if (str < strReq || (str + strBonus) < 1)
                                drop = true;
                        }

                        if (drop)
                        {
                            string name = clothing.Name;

                            if (name == null)
                                name = String.Format("#{0}", clothing.LabelNumber);

                            from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(clothing);
                            moved = true;
                        }
                    }

                    FactionItem factionItem = FactionItem.Find(item);

                    if (factionItem != null)
                    {
                        bool drop = false;

                        Faction ourFaction = Faction.Find(this);

                        if (ourFaction == null || ourFaction != factionItem.Faction)
                            drop = true;
                        else if (++factionItemCount > FactionItem.GetMaxWearables(this))
                            drop = true;

                        if (drop)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                        }
                    }
                }

                if (moved)
                    from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                this.m_NoDeltaRecursion = false;
            }
        }

        public override void Delta(MobileDelta flag)
        {
            base.Delta(flag);

            if ((flag & MobileDelta.Stat) != 0)
                this.ValidateEquipment();

            if ((flag & (MobileDelta.Name | MobileDelta.Hue)) != 0)
                this.InvalidateMyRunUO();
        }

        private static void Disconnect(object state)
        {
            NetState ns = ((Mobile)state).NetState;

            if (ns != null)
                ns.Dispose();
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            if (e.Mobile is PlayerMobile)
                ((PlayerMobile)e.Mobile).AutoStablePets();
        }

        private static void EventSink_Connected(ConnectedEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                pm.m_SessionStart = DateTime.Now;

                if (pm.m_Quest != null)
                    pm.m_Quest.StartTimer();

                pm.BedrollLogout = false;
                pm.LastOnline = DateTime.Now;
            }

            DisguiseTimers.StartTimer(e.Mobile);

            Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ClearSpecialMovesCallback), e.Mobile);
        }

        private static void ClearSpecialMovesCallback(object state)
        {
            Mobile from = (Mobile)state;

            SpecialMove.ClearAllMoves(from);
        }

        private static void EventSink_Disconnected(DisconnectedEventArgs e)
        {
            Mobile from = e.Mobile;
            DesignContext context = DesignContext.Find(from);

            if (context != null)
            {
                /* Client disconnected
                *  - Remove design context
                *  - Eject all from house
                *  - Restore relocated entities
                */
                // Remove design context
                DesignContext.Remove(from);

                // Eject all from house
                from.RevealingAction();

                foreach (Item item in context.Foundation.GetItems())
                    item.Location = context.Foundation.BanLocation;

                foreach (Mobile mobile in context.Foundation.GetMobiles())
                    mobile.Location = context.Foundation.BanLocation;

                // Restore relocated entities
                context.Foundation.RestoreRelocatedEntities();
            }

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                pm.m_GameTime += (DateTime.Now - pm.m_SessionStart);

                if (pm.m_Quest != null)
                    pm.m_Quest.StopTimer();

                pm.m_SpeechLog = null;
                pm.LastOnline = DateTime.Now;
            }

            DisguiseTimers.StopTimer(from);
        }

        public override void RevealingAction()
        {
            if (this.m_DesignContext != null)
                return;

            Spells.Sixth.InvisibilitySpell.RemoveTimer(this);

            base.RevealingAction();

            this.m_IsStealthing = false; // IsStealthing should be moved to Server.Mobiles
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Hidden
        {
            get
            {
                return base.Hidden;
            }
            set
            {
                base.Hidden = value;

                this.RemoveBuff(BuffIcon.Invisibility);	//Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

                if (!this.Hidden)
                {
                    this.RemoveBuff(BuffIcon.HidingAndOrStealth);
                }
                else // if( !InvisibilitySpell.HasTimer( this ) )
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));	//Hidden/Stealthing & You Are Hidden
                }
            }
        }

        public override void OnSubItemAdded(Item item)
        {
            if (this.AccessLevel < AccessLevel.GameMaster && item.IsChildOf(this.Backpack))
            {
                int maxWeight = WeightOverloading.GetMaxWeight(this);
                int curWeight = Mobile.BodyWeight + this.TotalWeight;

                if (curWeight > maxWeight)
                    this.SendLocalizedMessage(1019035, true, String.Format(" : {0} / {1}", curWeight, maxWeight));
            }
        }

        public override bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness)
        {
            if (this.m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
                return false;

            if ((target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || target is PlayerVendor || target is TownCrier)
            {
                if (message)
                {
                    if (target.Title == null)
                        this.SendMessage("{0} the vendor cannot be harmed.", target.Name);
                    else
                        this.SendMessage("{0} {1} cannot be harmed.", target.Name, target.Title);
                }

                return false;
            }

            return base.CanBeHarmful(target, message, ignoreOurBlessedness);
        }

        public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
        {
            if (this.m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
                return false;

            return base.CanBeBeneficial(target, message, allowDead);
        }

        public override bool CheckContextMenuDisplay(IEntity target)
        {
            return (this.m_DesignContext == null);
        }

        public override void OnItemAdded(Item item)
        {
            base.OnItemAdded(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                this.Hits = this.Hits;
                this.Stam = this.Stam;
                this.Mana = this.Mana;
            }

            if (this.NetState != null)
                this.CheckLightLevels(false);

            this.InvalidateMyRunUO();
        }

        public override void OnItemRemoved(Item item)
        {
            base.OnItemRemoved(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                this.Hits = this.Hits;
                this.Stam = this.Stam;
                this.Mana = this.Mana;
            }

            if (this.NetState != null)
                this.CheckLightLevels(false);

            this.InvalidateMyRunUO();
        }

        public override double ArmorRating
        {
            get
            {
                //BaseArmor ar;
                double rating = 0.0;

                this.AddArmorRating(ref rating, NeckArmor);
                this.AddArmorRating(ref rating, HandArmor);
                this.AddArmorRating(ref rating, HeadArmor);
                this.AddArmorRating(ref rating, ArmsArmor);
                this.AddArmorRating(ref rating, LegsArmor);
                this.AddArmorRating(ref rating, ChestArmor);
                this.AddArmorRating(ref rating, ShieldArmor);

                return this.VirtualArmor + this.VirtualArmorMod + rating;
            }
        }

        private void AddArmorRating(ref double rating, Item armor)
        {
            BaseArmor ar = armor as BaseArmor;

            if (ar != null && (!Core.AOS || ar.ArmorAttributes.MageArmor == 0))
                rating += ar.ArmorRatingScaled;
        }

        #region [Stats]Max
        [CommandProperty(AccessLevel.GameMaster)]
        public override int HitsMax
        {
            get
            {
                int strBase;
                int strOffs = this.GetStatOffset(StatType.Str);

                if (Core.AOS)
                {
                    strBase = this.Str;	//this.Str already includes GetStatOffset/str
                    strOffs = AosAttributes.GetValue(this, AosAttribute.BonusHits);

                    if (Core.ML && strOffs > 25 && this.IsPlayer())
                        strOffs = 25;

                    if (AnimalForm.UnderTransformation(this, typeof(BakeKitsune)) || AnimalForm.UnderTransformation(this, typeof(GreyWolf)))
                        strOffs += 20;
                }
                else
                {
                    strBase = this.RawStr;
                }

                return (strBase / 2) + 50 + strOffs;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int StamMax
        {
            get
            {
                return base.StamMax + AosAttributes.GetValue(this, AosAttribute.BonusStam);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int ManaMax
        {
            get
            {
                return base.ManaMax + AosAttributes.GetValue(this, AosAttribute.BonusMana) + ((Core.ML && this.Race == Race.Elf) ? 20 : 0);
            }
        }
        #endregion

        #region Stat Getters/Setters

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Str
        {
            get
            {
                if (Core.ML && this.IsPlayer())
                    return Math.Min(base.Str, 150);

                return base.Str;
            }
            set
            {
                base.Str = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Int
        {
            get
            {
                if (Core.ML && this.IsPlayer())
                    return Math.Min(base.Int, 150);

                return base.Int;
            }
            set
            {
                base.Int = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Dex
        {
            get
            {
                if (Core.ML && this.IsPlayer())
                    return Math.Min(base.Dex, 150);

                return base.Dex;
            }
            set
            {
                base.Dex = value;
            }
        }

        #endregion

        public override bool Move(Direction d)
        {
            NetState ns = this.NetState;

            if (ns != null)
            {
                if (this.HasGump(typeof(ResurrectGump)))
                {
                    if (this.Alive)
                    {
                        this.CloseGump(typeof(ResurrectGump));
                    }
                    else
                    {
                        this.SendLocalizedMessage(500111); // You are frozen and cannot move.
                        return false;
                    }
                }
            }

            TimeSpan speed = this.ComputeMovementSpeed(d);

            bool res;

            if (!this.Alive)
                Server.Movement.MovementImpl.IgnoreMovableImpassables = true;

            res = base.Move(d);

            Server.Movement.MovementImpl.IgnoreMovableImpassables = false;

            if (!res)
                return false;

            this.m_NextMovementTime += speed;

            return true;
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            DesignContext context = this.m_DesignContext;

            if (context == null)
                return base.CheckMovement(d, out newZ);

            HouseFoundation foundation = context.Foundation;

            newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int newX = this.X, newY = this.Y;
            Movement.Movement.Offset(d, ref newX, ref newY);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            return (newX >= startX && newY >= startY && newX < endX && newY < endY && this.Map == foundation.Map);
        }

        public override bool AllowItemUse(Item item)
        {
            #region Dueling
            if (this.m_DuelContext != null && !this.m_DuelContext.AllowItemUse(this, item))
                return false;
            #endregion

            return DesignContext.Check(this);
        }

        public SkillName[] AnimalFormRestrictedSkills
        {
            get
            {
                return this.m_AnimalFormRestrictedSkills;
            }
        }

        private SkillName[] m_AnimalFormRestrictedSkills = new SkillName[]
        {
            SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics,
            SkillName.Inscribe, SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking,
            SkillName.Provocation, SkillName.RemoveTrap, SkillName.SpiritSpeak, SkillName.Stealing,
            SkillName.TasteID
        };

        public override bool AllowSkillUse(SkillName skill)
        {
            if (AnimalForm.UnderTransformation(this))
            {
                for (int i = 0; i < this.m_AnimalFormRestrictedSkills.Length; i++)
                {
                    if (this.m_AnimalFormRestrictedSkills[i] == skill)
                    {
                        this.SendLocalizedMessage(1070771); // You cannot use that skill in this form.
                        return false;
                    }
                }
            }

            #region Dueling
            if (this.m_DuelContext != null && !this.m_DuelContext.AllowSkillUse(this, skill))
                return false;
            #endregion

            return DesignContext.Check(this);
        }

        private bool m_LastProtectedMessage;
        private int m_NextProtectionCheck = 10;

        public virtual void RecheckTownProtection()
        {
            this.m_NextProtectionCheck = 10;

            Regions.GuardedRegion reg = (Regions.GuardedRegion)this.Region.GetRegion(typeof(Regions.GuardedRegion));
            bool isProtected = (reg != null && !reg.IsDisabled());

            if (isProtected != this.m_LastProtectedMessage)
            {
                if (isProtected)
                    this.SendLocalizedMessage(500112); // You are now under the protection of the town guards.
                else
                    this.SendLocalizedMessage(500113); // You have left the protection of the town guards.

                this.m_LastProtectedMessage = isProtected;
            }
        }

        public override void MoveToWorld(Point3D loc, Map map)
        {
            base.MoveToWorld(loc, map);

            this.RecheckTownProtection();
        }

        public override void SetLocation(Point3D loc, bool isTeleport)
        {
            if (!isTeleport && this.IsPlayer())
            {
                // moving, not teleporting
                int zDrop = (this.Location.Z - loc.Z);

                if (zDrop > 20) // we fell more than one story
                    this.Hits -= ((zDrop / 20) * 10) - 5; // deal some damage; does not kill, disrupt, etc
            }

            base.SetLocation(loc, isTeleport);

            if (isTeleport || --this.m_NextProtectionCheck == 0)
                this.RecheckTownProtection();
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from == this)
            {
                if (this.m_Quest != null)
                    this.m_Quest.GetContextMenuEntries(list);

                if (this.Alive && InsuranceEnabled)
                {
                    list.Add(new CallbackEntry(6201, new ContextCallback(ToggleItemInsurance)));

                    if (this.AutoRenewInsurance)
                        list.Add(new CallbackEntry(6202, new ContextCallback(CancelRenewInventoryInsurance)));
                    else
                        list.Add(new CallbackEntry(6200, new ContextCallback(AutoRenewInventoryInsurance)));
                }

                BaseHouse house = BaseHouse.FindHouseAt(this);

                if (house != null)
                {
                    if (this.Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
                        list.Add(new CallbackEntry(6204, new ContextCallback(GetVendor)));

                    if (house.IsAosRules && !this.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone))) // Dueling
                        list.Add(new CallbackEntry(6207, new ContextCallback(LeaveHouse)));
                }

                if (this.m_JusticeProtectors.Count > 0)
                    list.Add(new CallbackEntry(6157, new ContextCallback(CancelProtection)));

                if (this.Alive)
                    list.Add(new CallbackEntry(6210, new ContextCallback(ToggleChampionTitleDisplay)));
            }
            if (from != this)
            {
                if (this.Alive && Core.Expansion >= Expansion.AOS)
                {
                    Party theirParty = from.Party as Party;
                    Party ourParty = this.Party as Party;

                    if (theirParty == null && ourParty == null)
                    {
                        list.Add(new AddToPartyEntry(from, this));
                    }
                    else if (theirParty != null && theirParty.Leader == from)
                    {
                        if (ourParty == null)
                        {
                            list.Add(new AddToPartyEntry(from, this));
                        }
                        else if (ourParty == theirParty)
                        {
                            list.Add(new RemoveFromPartyEntry(from, this));
                        }
                    }
                }

                BaseHouse curhouse = BaseHouse.FindHouseAt(this);

                if (curhouse != null)
                {
                    if (this.Alive && Core.Expansion >= Expansion.AOS && curhouse.IsAosRules && curhouse.IsFriend(from))
                        list.Add(new EjectPlayerEntry(from, this));
                }
            }
        }

        private void CancelProtection()
        {
            for (int i = 0; i < this.m_JusticeProtectors.Count; ++i)
            {
                Mobile prot = this.m_JusticeProtectors[i];

                string args = String.Format("{0}\t{1}", this.Name, prot.Name);

                prot.SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
                this.SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
            }

            this.m_JusticeProtectors.Clear();
        }

        #region Insurance

        private void ToggleItemInsurance()
        {
            if (!this.CheckAlive())
                return;

            this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
            this.SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
        }

        private bool CanInsure(Item item)
        {
            if (((item is Container) && !(item is BaseQuiver)) || item is BagOfSending || item is KeyRing)
                return false;

            if ((item is Spellbook && item.LootType == LootType.Blessed) || item is Runebook || item is PotionKeg || item is Sigil)
                return false;

            if (item.Stackable)
                return false;

            if (item.LootType == LootType.Cursed)
                return false;

            if (item.ItemID == 0x204E) // death shroud
                return false;

            return true;
        }

        private void ToggleItemInsurance_Callback(Mobile from, object obj)
        {
            if (!this.CheckAlive())
                return;

            Item item = obj as Item;

            if (item == null || !item.IsChildOf(this))
            {
                this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                this.SendLocalizedMessage(1060871, "", 0x23); // You can only insure items that you have equipped or that are in your backpack
            }
            else if (item.Insured)
            {
                item.Insured = false;

                this.SendLocalizedMessage(1060874, "", 0x35); // You cancel the insurance on the item

                this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                this.SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
            }
            else if (!this.CanInsure(item))
            {
                this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                this.SendLocalizedMessage(1060869, "", 0x23); // You cannot insure that
            }
            else if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied || item.BlessedFor == from)
            {
                this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                this.SendLocalizedMessage(1060870, "", 0x23); // That item is blessed and does not need to be insured
                this.SendLocalizedMessage(1060869, "", 0x23); // You cannot insure that
            }
            else
            {
                if (!item.PayedInsurance)
                {
                    if (Banker.Withdraw(from, 600))
                    {
                        this.SendLocalizedMessage(1060398, "600"); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                        item.PayedInsurance = true;
                    }
                    else
                    {
                        this.SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        return;
                    }
                }

                item.Insured = true;

                this.SendLocalizedMessage(1060873, "", 0x23); // You have insured the item

                this.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                this.SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
            }
        }

        private void AutoRenewInventoryInsurance()
        {
            if (!this.CheckAlive())
                return;

            this.SendLocalizedMessage(1060881, "", 0x23); // You have selected to automatically reinsure all insured items upon death
            this.AutoRenewInsurance = true;
        }

        private void CancelRenewInventoryInsurance()
        {
            if (!this.CheckAlive())
                return;

            if (Core.SE)
            {
                if (!this.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
                    this.SendGump(new CancelRenewInventoryInsuranceGump(this));
            }
            else
            {
                this.SendLocalizedMessage(1061075, "", 0x23); // You have cancelled automatically reinsuring all insured items upon death
                this.AutoRenewInsurance = false;
            }
        }

        private class CancelRenewInventoryInsuranceGump : Gump
        {
            private PlayerMobile m_Player;

            public CancelRenewInventoryInsuranceGump(PlayerMobile player) : base(250, 200)
            {
                this.m_Player = player;

                this.AddBackground(0, 0, 240, 142, 0x13BE);
                this.AddImageTiled(6, 6, 228, 100, 0xA40);
                this.AddImageTiled(6, 116, 228, 20, 0xA40);
                this.AddAlphaRegion(6, 6, 228, 142);

                this.AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false); // You are about to disable inventory insurance auto-renewal.

                this.AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                this.AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                this.AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                this.AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!this.m_Player.CheckAlive())
                    return;

                if (info.ButtonID == 1)
                {
                    this.m_Player.SendLocalizedMessage(1061075, "", 0x23); // You have cancelled automatically reinsuring all insured items upon death
                    this.m_Player.AutoRenewInsurance = false;
                }
                else
                {
                    this.m_Player.SendLocalizedMessage(1042021); // Cancelled.
                }
            }
        }

        #endregion

        private void GetVendor()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (this.CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
            {
                this.CloseGump(typeof(ReclaimVendorGump));
                this.SendGump(new ReclaimVendorGump(house));
            }
        }

        private void LeaveHouse()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
                this.Location = house.BanLocation;
        }

        private delegate void ContextCallback();

        private class CallbackEntry : ContextMenuEntry
        {
            private ContextCallback m_Callback;

            public CallbackEntry(int number, ContextCallback callback) : this(number, -1, callback)
            {
            }

            public CallbackEntry(int number, int range, ContextCallback callback) : base(number, range)
            {
                this.m_Callback = callback;
            }

            public override void OnClick()
            {
                if (this.m_Callback != null)
                    this.m_Callback();
            }
        }

        public override void DisruptiveAction()
        {
            if (this.Meditating)
            {
                this.RemoveBuff(BuffIcon.ActiveMeditation);
            }

            base.DisruptiveAction();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this == from && !this.Warmode)
            {
                IMount mount = this.Mount;

                if (mount != null && !DesignContext.Check(this))
                    return;
            }

            base.OnDoubleClick(from);
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
            if (DesignContext.Check(this))
                base.DisplayPaperdollTo(to);
        }

        private static bool m_NoRecursion;

        public override bool CheckEquip(Item item)
        {
            if (!base.CheckEquip(item))
                return false;

            #region Dueling
            if (this.m_DuelContext != null && !this.m_DuelContext.AllowItemEquip(this, item))
                return false;
            #endregion

            #region Factions
            FactionItem factionItem = FactionItem.Find(item);

            if (factionItem != null)
            {
                Faction faction = Faction.Find(this);

                if (faction == null)
                {
                    this.SendLocalizedMessage(1010371); // You cannot equip a faction item!
                    return false;
                }
                else if (faction != factionItem.Faction)
                {
                    this.SendLocalizedMessage(1010372); // You cannot equip an opposing faction's item!
                    return false;
                }
                else
                {
                    int maxWearables = FactionItem.GetMaxWearables(this);

                    for (int i = 0; i < this.Items.Count; ++i)
                    {
                        Item equiped = this.Items[i];

                        if (item != equiped && FactionItem.Find(equiped) != null)
                        {
                            if (--maxWearables == 0)
                            {
                                this.SendLocalizedMessage(1010373); // You do not have enough rank to equip more faction items!
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion

            if (this.AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && this.HasTrade)
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null)
                {
                    if (bounce.m_Parent is Item)
                    {
                        Item parent = (Item)bounce.m_Parent;

                        if (parent == this.Backpack || parent.IsChildOf(this.Backpack))
                            return true;
                    }
                    else if (bounce.m_Parent == this)
                    {
                        return true;
                    }
                }

                this.SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
                return false;
            }

            return true;
        }

        public override bool CheckTrade(Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            int msgNum = 0;

            if (cont == null)
            {
                if (to.Holding != null)
                    msgNum = 1062727; // You cannot trade with someone who is dragging something.
                else if (this.HasTrade)
                    msgNum = 1062781; // You are already trading with someone else!
                else if (to.HasTrade)
                    msgNum = 1062779; // That person is already involved in a trade
            }

            if (msgNum == 0)
            {
                if (cont != null)
                {
                    plusItems += cont.TotalItems;
                    plusWeight += cont.TotalWeight;
                }

                if (this.Backpack == null || !this.Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
                    msgNum = 1004040; // You would not be able to hold this if the trade failed.
                else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
                    msgNum = 1004039; // The recipient of this trade would not be able to carry this.
                else
                    msgNum = CheckContentForTrade(item);
            }

            if (msgNum != 0)
            {
                if (message)
                    this.SendLocalizedMessage(msgNum);

                return false;
            }

            return true;
        }

        private static int CheckContentForTrade(Item item)
        {
            if (item is TrapableContainer && ((TrapableContainer)item).TrapType != TrapType.None)
                return 1004044; // You may not trade trapped items.

            if (SkillHandlers.StolenItem.IsStolen(item))
                return 1004043; // You may not trade recently stolen items.

            if (item is Container)
            {
                foreach (Item subItem in item.Items)
                {
                    int msg = CheckContentForTrade(subItem);

                    if (msg != 0)
                        return msg;
                }
            }

            return 0;
        }

        public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
        {
            if (!base.CheckNonlocalDrop(from, item, target))
                return false;

            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            Container pack = this.Backpack;
            if (from == this && this.HasTrade && (target == pack || target.IsChildOf(pack)))
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null && bounce.m_Parent is Item)
                {
                    Item parent = (Item)bounce.m_Parent;

                    if (parent == pack || parent.IsChildOf(pack))
                        return true;
                }

                this.SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
                return false;
            }

            return true;
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            this.CheckLightLevels(false);

            #region Dueling
            if (this.m_DuelContext != null)
                this.m_DuelContext.OnLocationChanged(this);
            #endregion

            DesignContext context = this.m_DesignContext;

            if (context == null || m_NoRecursion)
                return;

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            int newX = this.X, newY = this.Y;
            int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            if (newX >= startX && newY >= startY && newX < endX && newY < endY && this.Map == foundation.Map)
            {
                if (this.Z != newZ)
                    this.Location = new Point3D(this.X, this.Y, newZ);

                m_NoRecursion = false;
                return;
            }

            this.Location = new Point3D(foundation.X, foundation.Y, newZ);
            this.Map = foundation.Map;

            m_NoRecursion = false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m is BaseCreature && !((BaseCreature)m).Controlled)
                return (!this.Alive || !m.Alive || this.IsDeadBondedPet || m.IsDeadBondedPet) || (this.Hidden && this.IsStaff());

            #region Dueling
            if (this.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)) && m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                if (pm.DuelContext == null || pm.DuelPlayer == null || !pm.DuelContext.Started || pm.DuelContext.Finished || pm.DuelPlayer.Eliminated)
                    return true;
            }
            #endregion

            return base.OnMoveOver(m);
        }

        public override bool CheckShove(Mobile shoved)
        {
            if (this.m_IgnoreMobiles || TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
                return true;
            else
                return base.CheckShove(shoved);
        }

        protected override void OnMapChange(Map oldMap)
        {
            if ((this.Map != Faction.Facet && oldMap == Faction.Facet) || (this.Map == Faction.Facet && oldMap != Faction.Facet))
                this.InvalidateProperties();

            #region Dueling
            if (this.m_DuelContext != null)
                this.m_DuelContext.OnMapChanged(this);
            #endregion

            DesignContext context = this.m_DesignContext;

            if (context == null || m_NoRecursion)
                return;

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            if (this.Map != foundation.Map)
                this.Map = foundation.Map;

            m_NoRecursion = false;
        }

        public override void OnBeneficialAction(Mobile target, bool isCriminal)
        {
            if (this.m_SentHonorContext != null)
                this.m_SentHonorContext.OnSourceBeneficialAction(target);

            base.OnBeneficialAction(target, isCriminal);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            int disruptThreshold;

            if (!Core.AOS)
                disruptThreshold = 0;
            else if (from != null && from.Player)
                disruptThreshold = 18;
            else
                disruptThreshold = 25;

            if (amount > disruptThreshold)
            {
                BandageContext c = BandageContext.GetContext(this);

                if (c != null)
                    c.Slip();
            }

            if (Confidence.IsRegenerating(this))
                Confidence.StopRegenerating(this);

            WeightOverloading.FatigueOnDamage(this, amount);

            if (this.m_ReceivedHonorContext != null)
                this.m_ReceivedHonorContext.OnTargetDamaged(from, amount);
            if (this.m_SentHonorContext != null)
                this.m_SentHonorContext.OnSourceDamaged(from, amount);

            if (willKill && from is PlayerMobile)
                Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(((PlayerMobile)from).RecoverAmmo));

            base.OnDamage(amount, from, willKill);
        }

        public override void Resurrect()
        {
            bool wasAlive = this.Alive;

            base.Resurrect();

            if (this.Alive && !wasAlive)
            {
                Item deathRobe = new DeathRobe();

                if (!this.EquipItem(deathRobe))
                    deathRobe.Delete();
            }
        }

        public override double RacialSkillBonus
        {
            get
            {
                if (Core.ML && this.Race == Race.Human)
                    return 20.0;

                return 0;
            }
        }

        public override void OnWarmodeChanged()
        {
            if (!this.Warmode)
                Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(RecoverAmmo));
        }

        private Mobile m_InsuranceAward;
        private int m_InsuranceCost;
        private int m_InsuranceBonus;

        private List<Item> m_EquipSnapshot;

        public List<Item> EquipSnapshot
        {
            get
            {
                return this.m_EquipSnapshot;
            }
        }

        private bool FindItems_Callback(Item item)
        {
            if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
            {
                if (this.Backpack != item.ParentEntity)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool OnBeforeDeath()
        {
            NetState state = this.NetState;

            if (state != null)
                state.CancelAllTrades();

            this.DropHolding();

            if (this.Backpack != null && !this.Backpack.Deleted)
            {
                List<Item> ilist = this.Backpack.FindItemsByType<Item>(FindItems_Callback);

                for (int i = 0; i < ilist.Count; i++)
                {
                    this.Backpack.AddItem(ilist[i]);
                }
            }

            this.m_EquipSnapshot = new List<Item>(this.Items);

            this.m_NonAutoreinsuredItems = 0;
            this.m_InsuranceCost = 0;
            this.m_InsuranceAward = base.FindMostRecentDamager(false);

            if (this.m_InsuranceAward is BaseCreature)
            {
                Mobile master = ((BaseCreature)this.m_InsuranceAward).GetMaster();

                if (master != null)
                    this.m_InsuranceAward = master;
            }

            if (this.m_InsuranceAward != null && (!this.m_InsuranceAward.Player || this.m_InsuranceAward == this))
                this.m_InsuranceAward = null;

            if (this.m_InsuranceAward is PlayerMobile)
                ((PlayerMobile)this.m_InsuranceAward).m_InsuranceBonus = 0;

            if (this.m_ReceivedHonorContext != null)
                this.m_ReceivedHonorContext.OnTargetKilled();
            if (this.m_SentHonorContext != null)
                this.m_SentHonorContext.OnSourceKilled();

            this.RecoverAmmo();

            return base.OnBeforeDeath();
        }

        private bool CheckInsuranceOnDeath(Item item)
        {
            if (InsuranceEnabled && item.Insured)
            {
                #region Dueling
                if (this.m_DuelPlayer != null && this.m_DuelContext != null && this.m_DuelContext.Registered && this.m_DuelContext.Started && !this.m_DuelPlayer.Eliminated)
                    return true;
                #endregion

                if (this.AutoRenewInsurance)
                {
                    int cost = (this.m_InsuranceAward == null ? 600 : 300);

                    if (Banker.Withdraw(this, cost))
                    {
                        this.m_InsuranceCost += cost;
                        item.PayedInsurance = true;
                        this.SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                    }
                    else
                    {
                        this.SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        item.PayedInsurance = false;
                        item.Insured = false;
                        this.m_NonAutoreinsuredItems++;
                    }
                }
                else
                {
                    item.PayedInsurance = false;
                    item.Insured = false;
                }

                if (this.m_InsuranceAward != null)
                {
                    if (Banker.Deposit(this.m_InsuranceAward, 300))
                    {
                        if (this.m_InsuranceAward is PlayerMobile)
                            ((PlayerMobile)this.m_InsuranceAward).m_InsuranceBonus += 300;
                    }
                }

                return true;
            }

            return false;
        }

        public override DeathMoveResult GetParentMoveResultFor(Item item)
        {
            if (this.CheckInsuranceOnDeath(item))
                return DeathMoveResult.MoveToBackpack;

            DeathMoveResult res = base.GetParentMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && this.Young)
                res = DeathMoveResult.MoveToBackpack;

            return res;
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            if (this.CheckInsuranceOnDeath(item))
                return DeathMoveResult.MoveToBackpack;

            DeathMoveResult res = base.GetInventoryMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && this.Young)
                res = DeathMoveResult.MoveToBackpack;

            return res;
        }

        public override void OnDeath(Container c)
        {
            if (this.m_NonAutoreinsuredItems > 0)
            {
                this.SendLocalizedMessage(1061115);
            }

            base.OnDeath(c);

            this.m_EquipSnapshot = null;

            this.HueMod = -1;
            this.NameMod = null;
            this.SavagePaintExpiration = TimeSpan.Zero;

            this.SetHairMods(-1, -1);

            PolymorphSpell.StopTimer(this);
            IncognitoSpell.StopTimer(this);
            DisguiseTimers.RemoveTimer(this);

            this.EndAction(typeof(PolymorphSpell));
            this.EndAction(typeof(IncognitoSpell));

            MeerMage.StopEffect(this, false);

            SkillHandlers.StolenItem.ReturnOnDeath(this, c);

            if (this.m_PermaFlags.Count > 0)
            {
                this.m_PermaFlags.Clear();

                if (c is Corpse)
                    ((Corpse)c).Criminal = true;

                if (SkillHandlers.Stealing.ClassicMode)
                    this.Criminal = true;
            }

            if (this.Kills >= 5 && DateTime.Now >= this.m_NextJustAward)
            {
                Mobile m = this.FindMostRecentDamager(false);

                if (m is BaseCreature)
                    m = ((BaseCreature)m).GetMaster();

                if (m != null && m is PlayerMobile && m != this)
                {
                    bool gainedPath = false;

                    int pointsToGain = 0;

                    pointsToGain += (int)Math.Sqrt(this.GameTime.TotalSeconds * 4);
                    pointsToGain *= 5;
                    pointsToGain += (int)Math.Pow(this.Skills.Total / 250, 2);

                    if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
                    {
                        if (gainedPath)
                            m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
                        else
                            m.SendLocalizedMessage(1049363); // You have gained in Justice.

                        m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
                        m.PlaySound(0x1F7);

                        this.m_NextJustAward = DateTime.Now + TimeSpan.FromMinutes(pointsToGain / 3);
                    }
                }
            }

            if (this.m_InsuranceAward is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)this.m_InsuranceAward;

                if (pm.m_InsuranceBonus > 0)
                    pm.SendLocalizedMessage(1060397, pm.m_InsuranceBonus.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
            }

            Mobile killer = this.FindMostRecentDamager(true);

            if (killer is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)killer;

                Mobile master = bc.GetMaster();
                if (master != null)
                    killer = master;
            }

            if (this.Young && this.m_DuelContext == null)
            {
                if (this.YoungDeathTeleport())
                    Timer.DelayCall(TimeSpan.FromSeconds(2.5), new TimerCallback(SendYoungDeathNotice));
            }

            if (this.m_DuelContext == null || !this.m_DuelContext.Registered || !this.m_DuelContext.Started || this.m_DuelPlayer == null || this.m_DuelPlayer.Eliminated)
                Faction.HandleDeath(this, killer);

            Server.Guilds.Guild.HandleDeath(this, killer);

            #region Dueling
            if (this.m_DuelContext != null)
                this.m_DuelContext.OnDeath(this, c);
            #endregion

            if (this.m_BuffTable != null)
            {
                List<BuffInfo> list = new List<BuffInfo>();

                foreach (BuffInfo buff in this.m_BuffTable.Values)
                {
                    if (!buff.RetainThroughDeath)
                    {
                        list.Add(buff);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    this.RemoveBuff(list[i]);
                }
            }
        }

        private List<Mobile> m_PermaFlags;
        private List<Mobile> m_VisList;
        private Hashtable m_AntiMacroTable;
        private TimeSpan m_GameTime;
        private TimeSpan m_ShortTermElapse;
        private TimeSpan m_LongTermElapse;
        private DateTime m_SessionStart;
        private DateTime m_LastEscortTime;
        private DateTime m_LastPetBallTime;
        private DateTime m_NextSmithBulkOrder;
        private DateTime m_NextTailorBulkOrder;
        private DateTime m_SavagePaintExpiration;
        private SkillName m_Learning = (SkillName)(-1);

        public SkillName Learning
        {
            get
            {
                return this.m_Learning;
            }
            set
            {
                this.m_Learning = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan SavagePaintExpiration
        {
            get
            {
                TimeSpan ts = this.m_SavagePaintExpiration - DateTime.Now;

                if (ts < TimeSpan.Zero)
                    ts = TimeSpan.Zero;

                return ts;
            }
            set
            {
                this.m_SavagePaintExpiration = DateTime.Now + value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextSmithBulkOrder
        {
            get
            {
                TimeSpan ts = this.m_NextSmithBulkOrder - DateTime.Now;

                if (ts < TimeSpan.Zero)
                    ts = TimeSpan.Zero;

                return ts;
            }
            set
            {
                try
                {
                    this.m_NextSmithBulkOrder = DateTime.Now + value;
                }
                catch
                {
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTailorBulkOrder
        {
            get
            {
                TimeSpan ts = this.m_NextTailorBulkOrder - DateTime.Now;

                if (ts < TimeSpan.Zero)
                    ts = TimeSpan.Zero;

                return ts;
            }
            set
            {
                try
                {
                    this.m_NextTailorBulkOrder = DateTime.Now + value;
                }
                catch
                {
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastEscortTime
        {
            get
            {
                return this.m_LastEscortTime;
            }
            set
            {
                this.m_LastEscortTime = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastPetBallTime
        {
            get
            {
                return this.m_LastPetBallTime;
            }
            set
            {
                this.m_LastPetBallTime = value;
            }
        }

        public PlayerMobile()
        {
            this.m_AutoStabled = new List<Mobile>();

            this.m_VisList = new List<Mobile>();
            this.m_PermaFlags = new List<Mobile>();
            this.m_AntiMacroTable = new Hashtable();
            this.m_RecentlyReported = new List<Mobile>();

            this.m_BOBFilter = new Engines.BulkOrders.BOBFilter();

            this.m_GameTime = TimeSpan.Zero;
            this.m_ShortTermElapse = TimeSpan.FromHours(8.0);
            this.m_LongTermElapse = TimeSpan.FromHours(40.0);

            this.m_JusticeProtectors = new List<Mobile>();
            this.m_GuildRank = Guilds.RankDefinition.Lowest;

            this.m_ChampionTitles = new ChampionTitleInfo();

            this.InvalidateMyRunUO();
        }

        public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
        {
            if (this.Alive)
                return false;

            if (Core.ML && this.Skills[SkillName.SpiritSpeak].Value >= 100.0)
                return false;

            if (Core.AOS)
            {
                for (int i = 0; i < hears.Count; ++i)
                {
                    Mobile m = hears[i];

                    if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
                        return false;
                }
            }

            return base.MutateSpeech(hears, ref text, ref context);
        }

        public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
        {
            if (Guilds.Guild.NewGuildSystem && (type == MessageType.Guild || type == MessageType.Alliance))
            {
                Guilds.Guild g = this.Guild as Guilds.Guild;
                if (g == null)
                {
                    this.SendLocalizedMessage(1063142); // You are not in a guild!
                }
                else if (type == MessageType.Alliance)
                {
                    if (g.Alliance != null && g.Alliance.IsMember(g))
                    {
                        //g.Alliance.AllianceTextMessage( hue, "[Alliance][{0}]: {1}", this.Name, text );
                        g.Alliance.AllianceChat(this, text);
                        SendToStaffMessage(this, "[Alliance]: {0}", text);

                        this.m_AllianceMessageHue = hue;
                    }
                    else
                    {
                        this.SendLocalizedMessage(1071020); // You are not in an alliance!
                    }
                }
                else //Type == MessageType.Guild
                {
                    this.m_GuildMessageHue = hue;

                    g.GuildChat(this, text);
                    SendToStaffMessage(this, "[Guild]: {0}", text);
                }
            }
            else
            {
                base.DoSpeech(text, keywords, type, hue);
            }
        }

        private static void SendToStaffMessage(Mobile from, string text)
        {
            Packet p = null;

            foreach (NetState ns in from.GetClientsInRange(8))
            {
                Mobile mob = ns.Mobile;

                if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
                {
                    if (p == null)
                        p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));

                    ns.Send(p);
                }
            }

            Packet.Release(p);
        }

        private static void SendToStaffMessage(Mobile from, string format, params object[] args)
        {
            SendToStaffMessage(from, String.Format(format, args));
        }

        public override void Damage(int amount, Mobile from)
        {
            if (Spells.Necromancy.EvilOmenSpell.TryEndEffect(this))
                amount = (int)(amount * 1.25);

            Mobile oath = Spells.Necromancy.BloodOathSpell.GetBloodOath(from);

            /* Per EA's UO Herald Pub48 (ML):
            * ((resist spellsx10)/20 + 10=percentage of damage resisted)
            */

            if (oath == this)
            {
                amount = (int)(amount * 1.1);

                if (amount > 35 && from is PlayerMobile)  /* capped @ 35, seems no expansion */
                {
                    amount = 35;
                }

                if (Core.ML)
                {
                    from.Damage((int)(amount * (1 - (((from.Skills.MagicResist.Value * .5) + 10) / 100))), this);
                }
                else
                {
                    from.Damage(amount, this);
                }
            }

            if (from != null && this.Talisman is BaseTalisman)
            {
                BaseTalisman talisman = (BaseTalisman)this.Talisman;

                if (talisman.Protection != null && talisman.Protection.Type != null)
                {
                    Type type = talisman.Protection.Type;

                    if (type.IsAssignableFrom(from.GetType()))
                        amount = (int)(amount * (1 - (double)talisman.Protection.Amount / 100));
                }
            }

            base.Damage(amount, from);
        }

        #region Poison

        public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
        {
            if (!this.Alive)
                return ApplyPoisonResult.Immune;

            if (Spells.Necromancy.EvilOmenSpell.TryEndEffect(this))
                poison = PoisonImpl.IncreaseLevel(poison);

            ApplyPoisonResult result = base.ApplyPoison(from, poison);

            if (from != null && result == ApplyPoisonResult.Poisoned && this.PoisonTimer is PoisonImpl.PoisonTimer)
                (this.PoisonTimer as PoisonImpl.PoisonTimer).From = from;

            return result;
        }

        public override bool CheckPoisonImmunity(Mobile from, Poison poison)
        {
            if (this.Young && (this.DuelContext == null || !this.DuelContext.Started || this.DuelContext.Finished))
                return true;

            return base.CheckPoisonImmunity(from, poison);
        }

        public override void OnPoisonImmunity(Mobile from, Poison poison)
        {
            if (this.Young && (this.DuelContext == null || !this.DuelContext.Started || this.DuelContext.Finished))
                this.SendLocalizedMessage(502808); // You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
            else
                base.OnPoisonImmunity(from, poison);
        }

        #endregion

        public PlayerMobile(Serial s) : base(s)
        {
            this.m_VisList = new List<Mobile>();
            this.m_AntiMacroTable = new Hashtable();
            this.InvalidateMyRunUO();
        }

        public List<Mobile> VisibilityList
        {
            get
            {
                return this.m_VisList;
            }
        }

        public List<Mobile> PermaFlags
        {
            get
            {
                return this.m_PermaFlags;
            }
        }

        public override int Luck
        {
            get
            {
                return AosAttributes.GetValue(this, AosAttribute.Luck);
            }
        }

        public override bool IsHarmfulCriminal(Mobile target)
        {
            if (SkillHandlers.Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).m_PermaFlags.Count > 0)
            {
                int noto = Notoriety.Compute(this, target);

                if (noto == Notoriety.Innocent)
                    target.Delta(MobileDelta.Noto);

                return false;
            }

            if (target is BaseCreature && ((BaseCreature)target).InitialInnocent && !((BaseCreature)target).Controlled)
                return false;

            if (Core.ML && target is BaseCreature && ((BaseCreature)target).Controlled && this == ((BaseCreature)target).ControlMaster)
                return false;

            return base.IsHarmfulCriminal(target);
        }

        public bool AntiMacroCheck(Skill skill, object obj)
        {
            if (obj == null || this.m_AntiMacroTable == null || this.IsStaff())
                return true;

            Hashtable tbl = (Hashtable)this.m_AntiMacroTable[skill];
            if (tbl == null)
                this.m_AntiMacroTable[skill] = tbl = new Hashtable();

            CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
            if (count != null)
            {
                if (count.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.Now)
                {
                    count.Count = 1;
                    return true;
                }
                else
                {
                    ++count.Count;
                    if (count.Count <= SkillCheck.Allowance)
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                tbl[obj] = count = new CountAndTimeStamp();
                count.Count = 1;

                return true;
            }
        }

        private void RevertHair()
        {
            this.SetHairMods(-1, -1);
        }

        private Engines.BulkOrders.BOBFilter m_BOBFilter;

        public Engines.BulkOrders.BOBFilter BOBFilter
        {
            get
            {
                return this.m_BOBFilter;
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch ( version )
            {
                case 28:
                    {
                        this.m_PeacedUntil = reader.ReadDateTime();

                        goto case 27;
                    }
                case 27:
                    {
                        this.m_AnkhNextUse = reader.ReadDateTime();

                        goto case 26;
                    }
                case 26:
                    {
                        this.m_AutoStabled = reader.ReadStrongMobileList();

                        goto case 25;
                    }
                case 25:
                    {
                        int recipeCount = reader.ReadInt();

                        if (recipeCount > 0)
                        {
                            this.m_AcquiredRecipes = new Dictionary<int, bool>();

                            for (int i = 0; i < recipeCount; i++)
                            {
                                int r = reader.ReadInt();
                                if (reader.ReadBool())	//Don't add in recipies which we haven't gotten or have been removed
                                    this.m_AcquiredRecipes.Add(r, true);
                            }
                        }
                        goto case 24;
                    }
                case 24:
                    {
                        this.m_LastHonorLoss = reader.ReadDeltaTime();
                        goto case 23;
                    }
                case 23:
                    {
                        this.m_ChampionTitles = new ChampionTitleInfo(reader);
                        goto case 22;
                    }
                case 22:
                    {
                        this.m_LastValorLoss = reader.ReadDateTime();
                        goto case 21;
                    }
                case 21:
                    {
                        this.m_ToTItemsTurnedIn = reader.ReadEncodedInt();
                        this.m_ToTTotalMonsterFame = reader.ReadInt();
                        goto case 20;
                    }
                case 20:
                    {
                        this.m_AllianceMessageHue = reader.ReadEncodedInt();
                        this.m_GuildMessageHue = reader.ReadEncodedInt();

                        goto case 19;
                    }
                case 19:
                    {
                        int rank = reader.ReadEncodedInt();
                        int maxRank = Guilds.RankDefinition.Ranks.Length - 1;
                        if (rank > maxRank)
                            rank = maxRank;

                        this.m_GuildRank = Guilds.RankDefinition.Ranks[rank];
                        this.m_LastOnline = reader.ReadDateTime();
                        goto case 18;
                    }
                case 18:
                    {
                        this.m_SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

                        goto case 17;
                    }
                case 17: // changed how DoneQuests is serialized
                case 16:
                    {
                        this.m_Quest = QuestSerializer.DeserializeQuest(reader);

                        if (this.m_Quest != null)
                            this.m_Quest.From = this;

                        int count = reader.ReadEncodedInt();

                        if (count > 0)
                        {
                            this.m_DoneQuests = new List<QuestRestartInfo>();

                            for (int i = 0; i < count; ++i)
                            {
                                Type questType = QuestSerializer.ReadType(QuestSystem.QuestTypes, reader);
                                DateTime restartTime;

                                if (version < 17)
                                    restartTime = DateTime.MaxValue;
                                else
                                    restartTime = reader.ReadDateTime();

                                this.m_DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
                            }
                        }

                        this.m_Profession = reader.ReadEncodedInt();
                        goto case 15;
                    }
                case 15:
                    {
                        this.m_LastCompassionLoss = reader.ReadDeltaTime();
                        goto case 14;
                    }
                case 14:
                    {
                        this.m_CompassionGains = reader.ReadEncodedInt();

                        if (this.m_CompassionGains > 0)
                            this.m_NextCompassionDay = reader.ReadDeltaTime();

                        goto case 13;
                    }
                case 13: // just removed m_PayedInsurance list
                case 12:
                    {
                        this.m_BOBFilter = new Engines.BulkOrders.BOBFilter(reader);
                        goto case 11;
                    }
                case 11:
                    {
                        if (version < 13)
                        {
                            List<Item> payed = reader.ReadStrongItemList();

                            for (int i = 0; i < payed.Count; ++i)
                                payed[i].PayedInsurance = true;
                        }

                        goto case 10;
                    }
                case 10:
                    {
                        if (reader.ReadBool())
                        {
                            this.m_HairModID = reader.ReadInt();
                            this.m_HairModHue = reader.ReadInt();
                            this.m_BeardModID = reader.ReadInt();
                            this.m_BeardModHue = reader.ReadInt();
                        }

                        goto case 9;
                    }
                case 9:
                    {
                        this.SavagePaintExpiration = reader.ReadTimeSpan();

                        if (this.SavagePaintExpiration > TimeSpan.Zero)
                        {
                            this.BodyMod = (this.Female ? 184 : 183);
                            this.HueMod = 0;
                        }

                        goto case 8;
                    }
                case 8:
                    {
                        this.m_NpcGuild = (NpcGuild)reader.ReadInt();
                        this.m_NpcGuildJoinTime = reader.ReadDateTime();
                        this.m_NpcGuildGameTime = reader.ReadTimeSpan();
                        goto case 7;
                    }
                case 7:
                    {
                        this.m_PermaFlags = reader.ReadStrongMobileList();
                        goto case 6;
                    }
                case 6:
                    {
                        this.NextTailorBulkOrder = reader.ReadTimeSpan();
                        goto case 5;
                    }
                case 5:
                    {
                        this.NextSmithBulkOrder = reader.ReadTimeSpan();
                        goto case 4;
                    }
                case 4:
                    {
                        this.m_LastJusticeLoss = reader.ReadDeltaTime();
                        this.m_JusticeProtectors = reader.ReadStrongMobileList();
                        goto case 3;
                    }
                case 3:
                    {
                        this.m_LastSacrificeGain = reader.ReadDeltaTime();
                        this.m_LastSacrificeLoss = reader.ReadDeltaTime();
                        this.m_AvailableResurrects = reader.ReadInt();
                        goto case 2;
                    }
                case 2:
                    {
                        this.m_Flags = (PlayerFlag)reader.ReadInt();
                        goto case 1;
                    }
                case 1:
                    {
                        this.m_LongTermElapse = reader.ReadTimeSpan();
                        this.m_ShortTermElapse = reader.ReadTimeSpan();
                        this.m_GameTime = reader.ReadTimeSpan();
                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 26)
                            this.m_AutoStabled = new List<Mobile>();
                        break;
                    }
            }

            if (this.m_RecentlyReported == null)
                this.m_RecentlyReported = new List<Mobile>();

            // Professions weren't verified on 1.0 RC0
            if (!CharacterCreation.VerifyProfession(this.m_Profession))
                this.m_Profession = 0;

            if (this.m_PermaFlags == null)
                this.m_PermaFlags = new List<Mobile>();

            if (this.m_JusticeProtectors == null)
                this.m_JusticeProtectors = new List<Mobile>();

            if (this.m_BOBFilter == null)
                this.m_BOBFilter = new Engines.BulkOrders.BOBFilter();

            if (this.m_GuildRank == null)
                this.m_GuildRank = Guilds.RankDefinition.Member;	//Default to member if going from older version to new version (only time it should be null)

            if (this.m_LastOnline == DateTime.MinValue && this.Account != null)
                this.m_LastOnline = ((Account)this.Account).LastLogin;

            if (this.m_ChampionTitles == null)
                this.m_ChampionTitles = new ChampionTitleInfo();

            if (this.IsPlayer())
                this.m_IgnoreMobiles = true;

            List<Mobile> list = this.Stabled;

            for (int i = 0; i < list.Count; ++i)
            {
                BaseCreature bc = list[i] as BaseCreature;

                if (bc != null)
                    bc.IsStabled = true;
            }

            CheckAtrophies(this);

            if (this.Hidden)	//Hiding is the only buff where it has an effect that's serialized.
                this.AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
        }

        public override void Serialize(GenericWriter writer)
        {
            //cleanup our anti-macro table
            foreach (Hashtable t in this.m_AntiMacroTable.Values)
            {
                ArrayList remove = new ArrayList();
                foreach (CountAndTimeStamp time in t.Values)
                {
                    if (time.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.Now)
                        remove.Add(time);
                }

                for (int i = 0; i < remove.Count; ++i)
                    t.Remove(remove[i]);
            }

            this.CheckKillDecay();

            CheckAtrophies(this);

            base.Serialize(writer);

            writer.Write((int)28); // version

            writer.Write((DateTime)this.m_PeacedUntil);
            writer.Write((DateTime)this.m_AnkhNextUse);
            writer.Write(this.m_AutoStabled, true);

            if (this.m_AcquiredRecipes == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write(this.m_AcquiredRecipes.Count);

                foreach (KeyValuePair<int, bool> kvp in this.m_AcquiredRecipes)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }

            writer.WriteDeltaTime(this.m_LastHonorLoss);

            ChampionTitleInfo.Serialize(writer, this.m_ChampionTitles);

            writer.Write(this.m_LastValorLoss);
            writer.WriteEncodedInt(this.m_ToTItemsTurnedIn);
            writer.Write(this.m_ToTTotalMonsterFame);	//This ain't going to be a small #.

            writer.WriteEncodedInt(this.m_AllianceMessageHue);
            writer.WriteEncodedInt(this.m_GuildMessageHue);

            writer.WriteEncodedInt(this.m_GuildRank.Rank);
            writer.Write(this.m_LastOnline);

            writer.WriteEncodedInt((int)this.m_SolenFriendship);

            QuestSerializer.Serialize(this.m_Quest, writer);

            if (this.m_DoneQuests == null)
            {
                writer.WriteEncodedInt((int)0);
            }
            else
            {
                writer.WriteEncodedInt((int)this.m_DoneQuests.Count);

                for (int i = 0; i < this.m_DoneQuests.Count; ++i)
                {
                    QuestRestartInfo restartInfo = this.m_DoneQuests[i];

                    QuestSerializer.Write((Type)restartInfo.QuestType, QuestSystem.QuestTypes, writer);
                    writer.Write((DateTime)restartInfo.RestartTime);
                }
            }

            writer.WriteEncodedInt((int)this.m_Profession);

            writer.WriteDeltaTime(this.m_LastCompassionLoss);

            writer.WriteEncodedInt(this.m_CompassionGains);

            if (this.m_CompassionGains > 0)
                writer.WriteDeltaTime(this.m_NextCompassionDay);

            this.m_BOBFilter.Serialize(writer);

            bool useMods = (this.m_HairModID != -1 || this.m_BeardModID != -1);

            writer.Write(useMods);

            if (useMods)
            {
                writer.Write((int)this.m_HairModID);
                writer.Write((int)this.m_HairModHue);
                writer.Write((int)this.m_BeardModID);
                writer.Write((int)this.m_BeardModHue);
            }

            writer.Write(this.SavagePaintExpiration);

            writer.Write((int)this.m_NpcGuild);
            writer.Write((DateTime)this.m_NpcGuildJoinTime);
            writer.Write((TimeSpan)this.m_NpcGuildGameTime);

            writer.Write(this.m_PermaFlags, true);

            writer.Write(this.NextTailorBulkOrder);

            writer.Write(this.NextSmithBulkOrder);

            writer.WriteDeltaTime(this.m_LastJusticeLoss);
            writer.Write(this.m_JusticeProtectors, true);

            writer.WriteDeltaTime(this.m_LastSacrificeGain);
            writer.WriteDeltaTime(this.m_LastSacrificeLoss);
            writer.Write(this.m_AvailableResurrects);

            writer.Write((int)this.m_Flags);

            writer.Write(this.m_LongTermElapse);
            writer.Write(this.m_ShortTermElapse);
            writer.Write(this.GameTime);
        }

        public static void CheckAtrophies(Mobile m)
        {
            SacrificeVirtue.CheckAtrophy(m);
            JusticeVirtue.CheckAtrophy(m);
            CompassionVirtue.CheckAtrophy(m);
            ValorVirtue.CheckAtrophy(m);

            if (m is PlayerMobile)
                ChampionTitleInfo.CheckAtrophy((PlayerMobile)m);
        }

        public void CheckKillDecay()
        {
            if (this.m_ShortTermElapse < this.GameTime)
            {
                this.m_ShortTermElapse += TimeSpan.FromHours(8);
                if (this.ShortTermMurders > 0)
                    --this.ShortTermMurders;
            }

            if (this.m_LongTermElapse < this.GameTime)
            {
                this.m_LongTermElapse += TimeSpan.FromHours(40);
                if (this.Kills > 0)
                    --this.Kills;
            }
        }

        public void ResetKillTime()
        {
            this.m_ShortTermElapse = this.GameTime + TimeSpan.FromHours(8);
            this.m_LongTermElapse = this.GameTime + TimeSpan.FromHours(40);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SessionStart
        {
            get
            {
                return this.m_SessionStart;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan GameTime
        {
            get
            {
                if (this.NetState != null)
                    return this.m_GameTime + (DateTime.Now - this.m_SessionStart);
                else
                    return this.m_GameTime;
            }
        }

        public override bool CanSee(Mobile m)
        {
            if (m is CharacterStatue)
                ((CharacterStatue)m).OnRequestedAnimation(this);

            if (m is PlayerMobile && ((PlayerMobile)m).m_VisList.Contains(this))
                return true;

            if (this.m_DuelContext != null && this.m_DuelPlayer != null && !this.m_DuelContext.Finished && this.m_DuelContext.m_Tournament != null && !this.m_DuelPlayer.Eliminated)
            {
                Mobile owner = m;

                if (owner is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)owner;

                    Mobile master = bc.GetMaster();

                    if (master != null)
                        owner = master;
                }

                if (m.IsPlayer() && owner is PlayerMobile && ((PlayerMobile)owner).DuelContext != this.m_DuelContext)
                    return false;
            }

            return base.CanSee(m);
        }

        public override bool CanSee(Item item)
        {
            if (this.m_DesignContext != null && this.m_DesignContext.Foundation.IsHiddenToCustomizer(item))
                return false;

            return base.CanSee(item);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Faction faction = Faction.Find(this);

            if (faction != null)
                faction.RemoveMember(this);

            BaseHouse.HandleDeletion(this);

            DisguiseTimers.RemoveTimer(this);
        }

        public override bool NewGuildDisplay
        {
            get
            {
                return Server.Guilds.Guild.NewGuildSystem;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (this.Map == Faction.Facet)
            {
                PlayerState pl = PlayerState.Find(this);

                if (pl != null)
                {
                    Faction faction = pl.Faction;

                    if (faction.Commander == this)
                        list.Add(1042733, faction.Definition.PropName); // Commanding Lord of the ~1_FACTION_NAME~
                    else if (pl.Sheriff != null)
                        list.Add(1042734, "{0}\t{1}", pl.Sheriff.Definition.FriendlyName, faction.Definition.PropName); // The Sheriff of  ~1_CITY~, ~2_FACTION_NAME~
                    else if (pl.Finance != null)
                        list.Add(1042735, "{0}\t{1}", pl.Finance.Definition.FriendlyName, faction.Definition.PropName); // The Finance Minister of ~1_CITY~, ~2_FACTION_NAME~
                    else if (pl.MerchantTitle != MerchantTitle.None)
                        list.Add(1060776, "{0}\t{1}", MerchantTitles.GetInfo(pl.MerchantTitle).Title, faction.Definition.PropName); // ~1_val~, ~2_val~
                    else
                        list.Add(1060776, "{0}\t{1}", pl.Rank.Title, faction.Definition.PropName); // ~1_val~, ~2_val~
                }
            }

            if (Core.ML)
            {
                for (int i = this.AllFollowers.Count - 1; i >= 0; i--)
                {
                    BaseCreature c = this.AllFollowers[i] as BaseCreature;

                    if (c != null && c.ControlOrder == OrderType.Guard)
                    {
                        list.Add(501129); // guarded
                        break;
                    }
                }
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            if (this.Map == Faction.Facet)
            {
                PlayerState pl = PlayerState.Find(this);

                if (pl != null)
                {
                    string text;
                    bool ascii = false;

                    Faction faction = pl.Faction;

                    if (faction.Commander == this)
                        text = String.Concat(this.Female ? "(Commanding Lady of the " : "(Commanding Lord of the ", faction.Definition.FriendlyName, ")");
                    else if (pl.Sheriff != null)
                        text = String.Concat("(The Sheriff of ", pl.Sheriff.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
                    else if (pl.Finance != null)
                        text = String.Concat("(The Finance Minister of ", pl.Finance.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
                    else
                    {
                        ascii = true;

                        if (pl.MerchantTitle != MerchantTitle.None)
                            text = String.Concat("(", MerchantTitles.GetInfo(pl.MerchantTitle).Title.String, ", ", faction.Definition.FriendlyName, ")");
                        else
                            text = String.Concat("(", pl.Rank.Title.String, ", ", faction.Definition.FriendlyName, ")");
                    }

                    int hue = (Faction.Find(from) == faction ? 98 : 38);

                    this.PrivateOverheadMessage(MessageType.Label, hue, ascii, text, from.NetState);
                }
            }

            base.OnSingleClick(from);
        }

        protected override bool OnMove(Direction d)
        {
            if (!Core.SE)
                return base.OnMove(d);

            if (this.IsStaff())
                return true;

            if (this.Hidden && DesignContext.Find(this) == null)	//Hidden & NOT customizing a house
            {
                if (!this.Mounted && this.Skills.Stealth.Value >= 25.0)
                {
                    bool running = (d & Direction.Running) != 0;

                    if (running)
                    {
                        if ((this.AllowedStealthSteps -= 2) <= 0)
                            this.RevealingAction();
                    }
                    else if (this.AllowedStealthSteps-- <= 0)
                    {
                        Server.SkillHandlers.Stealth.OnUse(this);
                    }
                }
                else
                {
                    this.RevealingAction();
                }
            }

            return true;
        }

        private bool m_BedrollLogout;

        public bool BedrollLogout
        {
            get
            {
                return this.m_BedrollLogout;
            }
            set
            {
                this.m_BedrollLogout = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Paralyzed
        {
            get
            {
                return base.Paralyzed;
            }
            set
            {
                base.Paralyzed = value;

                if (value)
                    this.AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827));	//Paralyze/You are frozen and can not move
                else
                    this.RemoveBuff(BuffIcon.Paralyze);
            }
        }

        #region Ethics
        private Ethics.Player m_EthicPlayer;

        [CommandProperty(AccessLevel.GameMaster)]
        public Ethics.Player EthicPlayer
        {
            get
            {
                return this.m_EthicPlayer;
            }
            set
            {
                this.m_EthicPlayer = value;
            }
        }
        #endregion

        #region Factions
        private PlayerState m_FactionPlayerState;

        public PlayerState FactionPlayerState
        {
            get
            {
                return this.m_FactionPlayerState;
            }
            set
            {
                this.m_FactionPlayerState = value;
            }
        }
        #endregion

        #region Dueling
        private Engines.ConPVP.DuelContext m_DuelContext;
        private Engines.ConPVP.DuelPlayer m_DuelPlayer;

        public Engines.ConPVP.DuelContext DuelContext
        {
            get
            {
                return this.m_DuelContext;
            }
        }

        public Engines.ConPVP.DuelPlayer DuelPlayer
        {
            get
            {
                return this.m_DuelPlayer;
            }
            set
            {
                bool wasInTourny = (this.m_DuelContext != null && !this.m_DuelContext.Finished && this.m_DuelContext.m_Tournament != null);

                this.m_DuelPlayer = value;

                if (this.m_DuelPlayer == null)
                    this.m_DuelContext = null;
                else
                    this.m_DuelContext = this.m_DuelPlayer.Participant.Context;

                bool isInTourny = (this.m_DuelContext != null && !this.m_DuelContext.Finished && this.m_DuelContext.m_Tournament != null);

                if (wasInTourny != isInTourny)
                    this.SendEverything();
            }
        }
        #endregion

        #region Quests
        private QuestSystem m_Quest;
        private List<QuestRestartInfo> m_DoneQuests;
        private SolenFriendship m_SolenFriendship;

        public QuestSystem Quest
        {
            get
            {
                return this.m_Quest;
            }
            set
            {
                this.m_Quest = value;
            }
        }

        public List<QuestRestartInfo> DoneQuests
        {
            get
            {
                return this.m_DoneQuests;
            }
            set
            {
                this.m_DoneQuests = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SolenFriendship SolenFriendship
        {
            get
            {
                return this.m_SolenFriendship;
            }
            set
            {
                this.m_SolenFriendship = value;
            }
        }
        #endregion

        #region MyRunUO Invalidation
        private bool m_ChangedMyRunUO;

        public bool ChangedMyRunUO
        {
            get
            {
                return this.m_ChangedMyRunUO;
            }
            set
            {
                this.m_ChangedMyRunUO = value;
            }
        }

        public void InvalidateMyRunUO()
        {
            if (!this.Deleted && !this.m_ChangedMyRunUO)
            {
                this.m_ChangedMyRunUO = true;
                Engines.MyRunUO.MyRunUO.QueueMobileUpdate(this);
            }
        }

        public override void OnKillsChange(int oldValue)
        {
            if (this.Young && this.Kills > oldValue)
            {
                Account acc = this.Account as Account;

                if (acc != null)
                    acc.RemoveYoungStatus(0);
            }

            this.InvalidateMyRunUO();
        }

        public override void OnGenderChanged(bool oldFemale)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnGuildChange(Server.Guilds.BaseGuild oldGuild)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnGuildTitleChange(string oldTitle)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnKarmaChange(int oldValue)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnFameChange(int oldValue)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnSkillChange(SkillName skill, double oldBase)
        {
            if (this.Young && this.SkillsTotal >= 4500)
            {
                Account acc = this.Account as Account;

                if (acc != null)
                    acc.RemoveYoungStatus(1019036); // You have successfully obtained a respectable skill level, and have outgrown your status as a young player!
            }

            this.InvalidateMyRunUO();
        }

        public override void OnAccessLevelChanged(AccessLevel oldLevel)
        {
            if (this.IsPlayer())
                this.IgnoreMobiles = false;
            else
                this.IgnoreMobiles = true;

            this.InvalidateMyRunUO();
        }

        public override void OnRawStatChange(StatType stat, int oldValue)
        {
            this.InvalidateMyRunUO();
        }

        public override void OnDelete()
        {
            if (this.m_ReceivedHonorContext != null)
                this.m_ReceivedHonorContext.Cancel();
            if (this.m_SentHonorContext != null)
                this.m_SentHonorContext.Cancel();

            this.InvalidateMyRunUO();
        }

        #endregion

        #region Fastwalk Prevention
        private static bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
        private static TimeSpan FastwalkThreshold = TimeSpan.FromSeconds(0.4); // Fastwalk prevention will become active after 0.4 seconds

        private DateTime m_NextMovementTime;

        public virtual bool UsesFastwalkPrevention
        {
            get
            {
                return (this.AccessLevel < AccessLevel.Counselor);
            }
        }

        public override TimeSpan ComputeMovementSpeed(Direction dir, bool checkTurning)
        {
            if (checkTurning && (dir & Direction.Mask) != (this.Direction & Direction.Mask))
                return Mobile.RunMount;	// We are NOT actually moving (just a direction change)

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null && context.Type == typeof(ReaperFormSpell))
                return Mobile.WalkFoot;

            bool running = ((dir & Direction.Running) != 0);

            bool onHorse = (this.Mount != null);

            AnimalFormContext animalContext = AnimalForm.GetContext(this);

            if (onHorse || (animalContext != null && animalContext.SpeedBoost))
                return (running ? Mobile.RunMount : Mobile.WalkMount);

            return (running ? Mobile.RunFoot : Mobile.WalkFoot);
        }

        public static bool MovementThrottle_Callback(NetState ns)
        {
            PlayerMobile pm = ns.Mobile as PlayerMobile;

            if (pm == null || !pm.UsesFastwalkPrevention)
                return true;

            if (pm.m_NextMovementTime == DateTime.MinValue)
            {
                // has not yet moved
                pm.m_NextMovementTime = DateTime.Now;
                return true;
            }

            TimeSpan ts = pm.m_NextMovementTime - DateTime.Now;

            if (ts < TimeSpan.Zero)
            {
                // been a while since we've last moved
                pm.m_NextMovementTime = DateTime.Now;
                return true;
            }

            return (ts < FastwalkThreshold);
        }

        #endregion

        #region Enemy of One
        private Type m_EnemyOfOneType;
        private bool m_WaitingForEnemy;

        public Type EnemyOfOneType
        {
            get
            {
                return this.m_EnemyOfOneType;
            }
            set
            {
                Type oldType = this.m_EnemyOfOneType;
                Type newType = value;

                if (oldType == newType)
                    return;

                this.m_EnemyOfOneType = value;

                this.DeltaEnemies(oldType, newType);
            }
        }

        public bool WaitingForEnemy
        {
            get
            {
                return this.m_WaitingForEnemy;
            }
            set
            {
                this.m_WaitingForEnemy = value;
            }
        }

        private void DeltaEnemies(Type oldType, Type newType)
        {
            foreach (Mobile m in this.GetMobilesInRange(18))
            {
                Type t = m.GetType();

                if (t == oldType || t == newType)
                {
                    NetState ns = this.NetState;

                    if (ns != null)
                    {
                        if (ns.StygianAbyss)
                        {
                            ns.Send(new MobileMoving(m, Notoriety.Compute(this, m)));
                        }
                        else
                        {
                            ns.Send(new MobileMovingOld(m, Notoriety.Compute(this, m)));
                        }
                    }
                }
            }
        }

        #endregion

        #region Hair and beard mods
        private int m_HairModID = -1, m_HairModHue;
        private int m_BeardModID = -1, m_BeardModHue;

        public void SetHairMods(int hairID, int beardID)
        {
            if (hairID == -1)
                this.InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
            else if (hairID != -2)
                this.InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);

            if (beardID == -1)
                this.InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
            else if (beardID != -2)
                this.InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
        }

        private void CreateHair(bool hair, int id, int hue)
        {
            if (hair)
            {
                //TODO Verification?
                this.HairItemID = id;
                this.HairHue = hue;
            }
            else
            {
                this.FacialHairItemID = id;
                this.FacialHairHue = hue;
            }
        }

        private void InternalRestoreHair(bool hair, ref int id, ref int hue)
        {
            if (id == -1)
                return;

            if (hair)
                this.HairItemID = 0;
            else
                this.FacialHairItemID = 0;

            //if( id != 0 )
            this.CreateHair(hair, id, hue);

            id = -1;
            hue = 0;
        }

        private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
        {
            if (storeID == -1)
            {
                storeID = hair ? this.HairItemID : this.FacialHairItemID;
                storeHue = hair ? this.HairHue : this.FacialHairHue;
            }
            this.CreateHair(hair, id, 0);
        }

        #endregion

        #region Virtues
        private DateTime m_LastSacrificeGain;
        private DateTime m_LastSacrificeLoss;
        private int m_AvailableResurrects;

        public DateTime LastSacrificeGain
        {
            get
            {
                return this.m_LastSacrificeGain;
            }
            set
            {
                this.m_LastSacrificeGain = value;
            }
        }
        public DateTime LastSacrificeLoss
        {
            get
            {
                return this.m_LastSacrificeLoss;
            }
            set
            {
                this.m_LastSacrificeLoss = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AvailableResurrects
        {
            get
            {
                return this.m_AvailableResurrects;
            }
            set
            {
                this.m_AvailableResurrects = value;
            }
        }

        private DateTime m_NextJustAward;
        private DateTime m_LastJusticeLoss;
        private List<Mobile> m_JusticeProtectors;

        public DateTime LastJusticeLoss
        {
            get
            {
                return this.m_LastJusticeLoss;
            }
            set
            {
                this.m_LastJusticeLoss = value;
            }
        }
        public List<Mobile> JusticeProtectors
        {
            get
            {
                return this.m_JusticeProtectors;
            }
            set
            {
                this.m_JusticeProtectors = value;
            }
        }

        private DateTime m_LastCompassionLoss;
        private DateTime m_NextCompassionDay;
        private int m_CompassionGains;

        public DateTime LastCompassionLoss
        {
            get
            {
                return this.m_LastCompassionLoss;
            }
            set
            {
                this.m_LastCompassionLoss = value;
            }
        }
        public DateTime NextCompassionDay
        {
            get
            {
                return this.m_NextCompassionDay;
            }
            set
            {
                this.m_NextCompassionDay = value;
            }
        }
        public int CompassionGains
        {
            get
            {
                return this.m_CompassionGains;
            }
            set
            {
                this.m_CompassionGains = value;
            }
        }

        private DateTime m_LastValorLoss;

        public DateTime LastValorLoss
        {
            get
            {
                return this.m_LastValorLoss;
            }
            set
            {
                this.m_LastValorLoss = value;
            }
        }

        private DateTime m_LastHonorLoss;
        private DateTime m_LastHonorUse;
        private bool m_HonorActive;
        private HonorContext m_ReceivedHonorContext;
        private HonorContext m_SentHonorContext;
        public DateTime m_hontime;

        public DateTime LastHonorLoss
        {
            get
            {
                return this.m_LastHonorLoss;
            }
            set
            {
                this.m_LastHonorLoss = value;
            }
        }
        public DateTime LastHonorUse
        {
            get
            {
                return this.m_LastHonorUse;
            }
            set
            {
                this.m_LastHonorUse = value;
            }
        }
        public bool HonorActive
        {
            get
            {
                return this.m_HonorActive;
            }
            set
            {
                this.m_HonorActive = value;
            }
        }
        public HonorContext ReceivedHonorContext
        {
            get
            {
                return this.m_ReceivedHonorContext;
            }
            set
            {
                this.m_ReceivedHonorContext = value;
            }
        }
        public HonorContext SentHonorContext
        {
            get
            {
                return this.m_SentHonorContext;
            }
            set
            {
                this.m_SentHonorContext = value;
            }
        }
        #endregion

        #region Young system
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Young
        {
            get
            {
                return this.GetFlag(PlayerFlag.Young);
            }
            set
            {
                this.SetFlag(PlayerFlag.Young, value);
                this.InvalidateProperties();
            }
        }

        public override string ApplyNameSuffix(string suffix)
        {
            if (this.Young)
            {
                if (suffix.Length == 0)
                    suffix = "(Young)";
                else
                    suffix = String.Concat(suffix, " (Young)");
            }

            #region Ethics
            if (this.m_EthicPlayer != null)
            {
                if (suffix.Length == 0)
                    suffix = this.m_EthicPlayer.Ethic.Definition.Adjunct.String;
                else
                    suffix = String.Concat(suffix, " ", this.m_EthicPlayer.Ethic.Definition.Adjunct.String);
            }
            #endregion

            if (Core.ML && this.Map == Faction.Facet)
            {
                Faction faction = Faction.Find(this);

                if (faction != null)
                {
                    string adjunct = String.Format("[{0}]", faction.Definition.Abbreviation);
                    if (suffix.Length == 0)
                        suffix = adjunct;
                    else
                        suffix = String.Concat(suffix, " ", adjunct);
                }
            }

            return base.ApplyNameSuffix(suffix);
        }

        public override TimeSpan GetLogoutDelay()
        {
            if (this.Young || this.BedrollLogout || TestCenter.Enabled)
                return TimeSpan.Zero;

            return base.GetLogoutDelay();
        }

        private DateTime m_LastYoungMessage = DateTime.MinValue;

        public bool CheckYoungProtection(Mobile from)
        {
            if (!this.Young)
                return false;

            if (this.Region is BaseRegion && !((BaseRegion)this.Region).YoungProtected)
                return false;

            if (from is BaseCreature && ((BaseCreature)from).IgnoreYoungProtection)
                return false;

            if (this.Quest != null && this.Quest.IgnoreYoungProtection(from))
                return false;

            if (DateTime.Now - this.m_LastYoungMessage > TimeSpan.FromMinutes(1.0))
            {
                this.m_LastYoungMessage = DateTime.Now;
                this.SendLocalizedMessage(1019067); // A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.
            }

            return true;
        }

        private DateTime m_LastYoungHeal = DateTime.MinValue;

        public bool CheckYoungHealTime()
        {
            if (DateTime.Now - this.m_LastYoungHeal > TimeSpan.FromMinutes(5.0))
            {
                this.m_LastYoungHeal = DateTime.Now;
                return true;
            }

            return false;
        }

        private static Point3D[] m_TrammelDeathDestinations = new Point3D[]
        {
            new Point3D(1481, 1612, 20),
            new Point3D(2708, 2153, 0),
            new Point3D(2249, 1230, 0),
            new Point3D(5197, 3994, 37),
            new Point3D(1412, 3793, 0),
            new Point3D(3688, 2232, 20),
            new Point3D(2578, 604, 0),
            new Point3D(4397, 1089, 0),
            new Point3D(5741, 3218, -2),
            new Point3D(2996, 3441, 15),
            new Point3D(624, 2225, 0),
            new Point3D(1916, 2814, 0),
            new Point3D(2929, 854, 0),
            new Point3D(545, 967, 0),
            new Point3D(3665, 2587, 0)
        };

        private static Point3D[] m_IlshenarDeathDestinations = new Point3D[]
        {
            new Point3D(1216, 468, -13),
            new Point3D(723, 1367, -60),
            new Point3D(745, 725, -28),
            new Point3D(281, 1017, 0),
            new Point3D(986, 1011, -32),
            new Point3D(1175, 1287, -30),
            new Point3D(1533, 1341, -3),
            new Point3D(529, 217, -44),
            new Point3D(1722, 219, 96)
        };

        private static Point3D[] m_MalasDeathDestinations = new Point3D[]
        {
            new Point3D(2079, 1376, -70),
            new Point3D(944, 519, -71)
        };

        private static Point3D[] m_TokunoDeathDestinations = new Point3D[]
        {
            new Point3D(1166, 801, 27),
            new Point3D(782, 1228, 25),
            new Point3D(268, 624, 15)
        };

        public bool YoungDeathTeleport()
        {
            if (this.Region.IsPartOf(typeof(Jail)) ||
                this.Region.IsPartOf("Samurai start location") ||
                this.Region.IsPartOf("Ninja start location") ||
                this.Region.IsPartOf("Ninja cave"))
                return false;

            Point3D loc;
            Map map;

            DungeonRegion dungeon = (DungeonRegion)this.Region.GetRegion(typeof(DungeonRegion));
            if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
            {
                loc = dungeon.EntranceLocation;
                map = dungeon.EntranceMap;
            }
            else
            {
                loc = this.Location;
                map = this.Map;
            }

            Point3D[] list;

            if (map == Map.Trammel)
                list = m_TrammelDeathDestinations;
            else if (map == Map.Ilshenar)
                list = m_IlshenarDeathDestinations;
            else if (map == Map.Malas)
                list = m_MalasDeathDestinations;
            else if (map == Map.Tokuno)
                list = m_TokunoDeathDestinations;
            else
                return false;

            Point3D dest = Point3D.Zero;
            int sqDistance = int.MaxValue;

            for (int i = 0; i < list.Length; i++)
            {
                Point3D curDest = list[i];

                int width = loc.X - curDest.X;
                int height = loc.Y - curDest.Y;
                int curSqDistance = width * width + height * height;

                if (curSqDistance < sqDistance)
                {
                    dest = curDest;
                    sqDistance = curSqDistance;
                }
            }

            this.MoveToWorld(dest, map);
            return true;
        }

        private void SendYoungDeathNotice()
        {
            this.SendGump(new YoungDeathNotice());
        }

        #endregion

        #region Speech log
        private SpeechLog m_SpeechLog;

        public SpeechLog SpeechLog
        {
            get
            {
                return this.m_SpeechLog;
            }
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (SpeechLog.Enabled && this.NetState != null)
            {
                if (this.m_SpeechLog == null)
                    this.m_SpeechLog = new SpeechLog();

                this.m_SpeechLog.Add(e.Mobile, e.Speech);
            }
        }

        #endregion

        #region Champion Titles
        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisplayChampionTitle
        {
            get
            {
                return this.GetFlag(PlayerFlag.DisplayChampionTitle);
            }
            set
            {
                this.SetFlag(PlayerFlag.DisplayChampionTitle, value);
            }
        }

        private ChampionTitleInfo m_ChampionTitles;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionTitleInfo ChampionTitles
        {
            get
            {
                return this.m_ChampionTitles;
            }
            set
            {
            }
        }

        private void ToggleChampionTitleDisplay()
        {
            if (!this.CheckAlive())
                return;

            if (this.DisplayChampionTitle)
                this.SendLocalizedMessage(1062419, "", 0x23); // You have chosen to hide your monster kill title.
            else
                this.SendLocalizedMessage(1062418, "", 0x23); // You have chosen to display your monster kill title.

            this.DisplayChampionTitle = !this.DisplayChampionTitle;
        }

        [PropertyObject]
        public class ChampionTitleInfo
        {
            public static TimeSpan LossDelay = TimeSpan.FromDays(1.0);
            public const int LossAmount = 90;

            private class TitleInfo
            {
                private int m_Value;
                private DateTime m_LastDecay;

                public int Value
                {
                    get
                    {
                        return this.m_Value;
                    }
                    set
                    {
                        this.m_Value = value;
                    }
                }
                public DateTime LastDecay
                {
                    get
                    {
                        return this.m_LastDecay;
                    }
                    set
                    {
                        this.m_LastDecay = value;
                    }
                }

                public TitleInfo()
                {
                }

                public TitleInfo(GenericReader reader)
                {
                    int version = reader.ReadEncodedInt();

                    switch( version )
                    {
                        case 0:
                            {
                                this.m_Value = reader.ReadEncodedInt();
                                this.m_LastDecay = reader.ReadDateTime();
                                break;
                            }
                    }
                }

                public static void Serialize(GenericWriter writer, TitleInfo info)
                {
                    writer.WriteEncodedInt((int)0); // version

                    writer.WriteEncodedInt(info.m_Value);
                    writer.Write(info.m_LastDecay);
                }
            }

            private TitleInfo[] m_Values;

            private int m_Harrower;	//Harrower titles do NOT decay

            public int GetValue(ChampionSpawnType type)
            {
                return this.GetValue((int)type);
            }

            public void SetValue(ChampionSpawnType type, int value)
            {
                this.SetValue((int)type, value);
            }

            public void Award(ChampionSpawnType type, int value)
            {
                this.Award((int)type, value);
            }

            public int GetValue(int index)
            {
                if (this.m_Values == null || index < 0 || index >= this.m_Values.Length)
                    return 0;

                if (this.m_Values[index] == null)
                    this.m_Values[index] = new TitleInfo();

                return this.m_Values[index].Value;
            }

            public DateTime GetLastDecay(int index)
            {
                if (this.m_Values == null || index < 0 || index >= this.m_Values.Length)
                    return DateTime.MinValue;

                if (this.m_Values[index] == null)
                    this.m_Values[index] = new TitleInfo();

                return this.m_Values[index].LastDecay;
            }

            public void SetValue(int index, int value)
            {
                if (this.m_Values == null)
                    this.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                if (value < 0)
                    value = 0;

                if (index < 0 || index >= this.m_Values.Length)
                    return;

                if (this.m_Values[index] == null)
                    this.m_Values[index] = new TitleInfo();

                this.m_Values[index].Value = value;
            }

            public void Award(int index, int value)
            {
                if (this.m_Values == null)
                    this.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                if (index < 0 || index >= this.m_Values.Length || value <= 0)
                    return;

                if (this.m_Values[index] == null)
                    this.m_Values[index] = new TitleInfo();

                this.m_Values[index].Value += value;
            }

            public void Atrophy(int index, int value)
            {
                if (this.m_Values == null)
                    this.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                if (index < 0 || index >= this.m_Values.Length || value <= 0)
                    return;

                if (this.m_Values[index] == null)
                    this.m_Values[index] = new TitleInfo();

                int before = this.m_Values[index].Value;

                if ((this.m_Values[index].Value - value) < 0)
                    this.m_Values[index].Value = 0;
                else
                    this.m_Values[index].Value -= value;

                if (before != this.m_Values[index].Value)
                    this.m_Values[index].LastDecay = DateTime.Now;
            }

            public override string ToString()
            {
                return "...";
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Pestilence
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.Pestilence);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.Pestilence, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Abyss
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.Abyss);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.Abyss, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Arachnid
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.Arachnid);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.Arachnid, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ColdBlood
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.ColdBlood);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.ColdBlood, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ForestLord
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.ForestLord);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.ForestLord, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int SleepingDragon
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.SleepingDragon);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.SleepingDragon, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int UnholyTerror
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.UnholyTerror);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.UnholyTerror, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int VerminHorde
            {
                get
                {
                    return this.GetValue(ChampionSpawnType.VerminHorde);
                }
                set
                {
                    this.SetValue(ChampionSpawnType.VerminHorde, value);
                }
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Harrower
            {
                get
                {
                    return this.m_Harrower;
                }
                set
                {
                    this.m_Harrower = value;
                }
            }

            public ChampionTitleInfo()
            {
            }

            public ChampionTitleInfo(GenericReader reader)
            {
                int version = reader.ReadEncodedInt();

                switch( version )
                {
                    case 0:
                        {
                            this.m_Harrower = reader.ReadEncodedInt();

                            int length = reader.ReadEncodedInt();
                            this.m_Values = new TitleInfo[length];

                            for (int i = 0; i < length; i++)
                            {
                                this.m_Values[i] = new TitleInfo(reader);
                            }

                            if (this.m_Values.Length != ChampionSpawnInfo.Table.Length)
                            {
                                TitleInfo[] oldValues = this.m_Values;
                                this.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                                for (int i = 0; i < this.m_Values.Length && i < oldValues.Length; i++)
                                {
                                    this.m_Values[i] = oldValues[i];
                                }
                            }
                            break;
                        }
                }
            }

            public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
            {
                writer.WriteEncodedInt((int)0); // version

                writer.WriteEncodedInt(titles.m_Harrower);

                int length = titles.m_Values.Length;
                writer.WriteEncodedInt(length);

                for (int i = 0; i < length; i++)
                {
                    if (titles.m_Values[i] == null)
                        titles.m_Values[i] = new TitleInfo();

                    TitleInfo.Serialize(writer, titles.m_Values[i]);
                }
            }

            public static void CheckAtrophy(PlayerMobile pm)
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                    return;

                if (t.m_Values == null)
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if ((t.GetLastDecay(i) + LossDelay) < DateTime.Now)
                    {
                        t.Atrophy(i, LossAmount);
                    }
                }
            }

            public static void AwardHarrowerTitle(PlayerMobile pm)	//Called when killing a harrower.  Will give a minimum of 1 point.
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                    return;

                if (t.m_Values == null)
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                int count = 1;

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.m_Values[i].Value > 900)
                        count++;
                }

                t.m_Harrower = Math.Max(count, t.m_Harrower);	//Harrower titles never decay.
            }
        }

        #endregion

        #region Recipes

        private Dictionary<int, bool> m_AcquiredRecipes;

        public virtual bool HasRecipe(Recipe r)
        {
            if (r == null)
                return false;

            return this.HasRecipe(r.ID);
        }

        public virtual bool HasRecipe(int recipeID)
        {
            if (this.m_AcquiredRecipes != null && this.m_AcquiredRecipes.ContainsKey(recipeID))
                return this.m_AcquiredRecipes[recipeID];

            return false;
        }

        public virtual void AcquireRecipe(Recipe r)
        {
            if (r != null)
                this.AcquireRecipe(r.ID);
        }

        public virtual void AcquireRecipe(int recipeID)
        {
            if (this.m_AcquiredRecipes == null)
                this.m_AcquiredRecipes = new Dictionary<int, bool>();

            this.m_AcquiredRecipes[recipeID] = true;
        }

        public virtual void ResetRecipes()
        {
            this.m_AcquiredRecipes = null;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int KnownRecipes
        {
            get
            {
                if (this.m_AcquiredRecipes == null)
                    return 0;

                return this.m_AcquiredRecipes.Count;
            }
        }

        #endregion

        #region Buff Icons

        public void ResendBuffs()
        {
            if (!BuffInfo.Enabled || this.m_BuffTable == null)
                return;

            NetState state = this.NetState;

            if (state != null && state.BuffIcon)
            {
                foreach (BuffInfo info in this.m_BuffTable.Values)
                {
                    state.Send(new AddBuffPacket(this, info));
                }
            }
        }

        private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

        public void AddBuff(BuffInfo b)
        {
            if (!BuffInfo.Enabled || b == null)
                return;

            this.RemoveBuff(b);	//Check & subsequently remove the old one.

            if (this.m_BuffTable == null)
                this.m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();

            this.m_BuffTable.Add(b.ID, b);

            NetState state = this.NetState;

            if (state != null && state.BuffIcon)
            {
                state.Send(new AddBuffPacket(this, b));
            }
        }

        public void RemoveBuff(BuffInfo b)
        {
            if (b == null)
                return;

            this.RemoveBuff(b.ID);
        }

        public void RemoveBuff(BuffIcon b)
        {
            if (this.m_BuffTable == null || !this.m_BuffTable.ContainsKey(b))
                return;

            BuffInfo info = this.m_BuffTable[b];

            if (info.Timer != null && info.Timer.Running)
                info.Timer.Stop();

            this.m_BuffTable.Remove(b);

            NetState state = this.NetState;

            if (state != null && state.BuffIcon)
            {
                state.Send(new RemoveBuffPacket(this, b));
            }

            if (this.m_BuffTable.Count <= 0)
                this.m_BuffTable = null;
        }

        #endregion

        public void AutoStablePets()
        {
            if (Core.SE && this.AllFollowers.Count > 0)
            {
                for (int i = this.m_AllFollowers.Count - 1; i >= 0; --i)
                {
                    BaseCreature pet = this.AllFollowers[i] as BaseCreature;

                    if (pet == null || pet.ControlMaster == null)
                        continue;

                    if (pet.Summoned)
                    {
                        if (pet.Map != this.Map)
                        {
                            pet.PlaySound(pet.GetAngerSound());
                            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(pet.Delete));
                        }
                        continue;
                    }

                    if (pet is IMount && ((IMount)pet).Rider != null)
                        continue;

                    if ((pet is PackLlama || pet is PackHorse || pet is Beetle || pet is HordeMinionFamiliar) && (pet.Backpack != null && pet.Backpack.Items.Count > 0))
                        continue;

                    if (pet is BaseEscortable)
                        continue;

                    pet.ControlTarget = null;
                    pet.ControlOrder = OrderType.Stay;
                    pet.Internalize();

                    pet.SetControlMaster(null);
                    pet.SummonMaster = null;

                    pet.IsStabled = true;

                    pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

                    this.Stabled.Add(pet);
                    this.m_AutoStabled.Add(pet);
                }
            }
        }

        public void ClaimAutoStabledPets()
        {
            if (!Core.SE || this.m_AutoStabled.Count <= 0)
                return;

            if (!this.Alive)
            {
                this.SendLocalizedMessage(1076251); // Your pet was unable to join you while you are a ghost.  Please re-login once you have ressurected to claim your pets.
                return;
            }

            for (int i = this.m_AutoStabled.Count - 1; i >= 0; --i)
            {
                BaseCreature pet = this.m_AutoStabled[i] as BaseCreature;

                if (pet == null || pet.Deleted)
                {
                    pet.IsStabled = false;

                    if (this.Stabled.Contains(pet))
                        this.Stabled.Remove(pet);

                    continue;
                }

                if ((this.Followers + pet.ControlSlots) <= this.FollowersMax)
                {
                    pet.SetControlMaster(this);

                    if (pet.Summoned)
                        pet.SummonMaster = this;

                    pet.ControlTarget = this;
                    pet.ControlOrder = OrderType.Follow;

                    pet.MoveToWorld(this.Location, this.Map);

                    pet.IsStabled = false;

                    pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

                    if (this.Stabled.Contains(pet))
                        this.Stabled.Remove(pet);
                }
                else
                {
                    this.SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
                }
            }

            this.m_AutoStabled.Clear();
        }
    }
}