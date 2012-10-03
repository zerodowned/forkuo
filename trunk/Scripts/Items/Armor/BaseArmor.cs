using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Factions;
using Server.Network;
using AMA = Server.Items.ArmorMeditationAllowance;
using AMT = Server.Items.ArmorMaterialType;

namespace Server.Items
{
    public abstract class BaseArmor : Item, IScissorable, IFactionItem, ICraftable, IWearableDurability
    {
        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get
            {
                return this.m_FactionState;
            }
            set
            {
                this.m_FactionState = value;

                if (this.m_FactionState == null)
                    this.Hue = CraftResources.GetHue(this.Resource);

                this.LootType = (this.m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

        /* Armor internals work differently now (Jun 19 2003)
        * 
        * The attributes defined below default to -1.
        * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
        * If not, the attribute value itself is used. Here's the list:
        *  - ArmorBase
        *  - StrBonus
        *  - DexBonus
        *  - IntBonus
        *  - StrReq
        *  - DexReq
        *  - IntReq
        *  - MeditationAllowance
        */

        // Instance values. These values must are unique to each armor piece.
        private int m_MaxHitPoints;
        private int m_HitPoints;
        private Mobile m_Crafter;
        private ArmorQuality m_Quality;
        private ArmorDurabilityLevel m_Durability;
        private ArmorProtectionLevel m_Protection;
        private CraftResource m_Resource;
        private bool m_Identified, m_PlayerConstructed;
        private int m_PhysicalBonus, m_FireBonus, m_ColdBonus, m_PoisonBonus, m_EnergyBonus;

        private AosAttributes m_AosAttributes;
        private AosArmorAttributes m_AosArmorAttributes;
        private AosSkillBonuses m_AosSkillBonuses;

        // Overridable values. These values are provided to override the defaults which get defined in the individual armor scripts.
        private int m_ArmorBase = -1;
        private int m_StrBonus = -1, m_DexBonus = -1, m_IntBonus = -1;
        private int m_StrReq = -1, m_DexReq = -1, m_IntReq = -1;
        private AMA m_Meditate = (AMA)(-1);

        public virtual bool AllowMaleWearer
        {
            get
            {
                return true;
            }
        }
        public virtual bool AllowFemaleWearer
        {
            get
            {
                return true;
            }
        }

        public abstract AMT MaterialType { get; }

        public virtual int RevertArmorBase
        {
            get
            {
                return this.ArmorBase;
            }
        }
        public virtual int ArmorBase
        {
            get
            {
                return 0;
            }
        }

        public virtual AMA DefMedAllowance
        {
            get
            {
                return AMA.None;
            }
        }
        public virtual AMA AosMedAllowance
        {
            get
            {
                return this.DefMedAllowance;
            }
        }
        public virtual AMA OldMedAllowance
        {
            get
            {
                return this.DefMedAllowance;
            }
        }

        public virtual int AosStrBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosDexBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosIntBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosStrReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosDexReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosIntReq
        {
            get
            {
                return 0;
            }
        }

        public virtual int OldStrBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldDexBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldIntBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldStrReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldDexReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldIntReq
        {
            get
            {
                return 0;
            }
        }

        public virtual bool CanFortify
        {
            get
            {
                return true;
            }
        }

        public override void OnAfterDuped(Item newItem)
        {
            BaseArmor armor = newItem as BaseArmor;

            if (armor == null)
                return;

            armor.m_AosAttributes = new AosAttributes(newItem, this.m_AosAttributes);
            armor.m_AosArmorAttributes = new AosArmorAttributes(newItem, this.m_AosArmorAttributes);
            armor.m_AosSkillBonuses = new AosSkillBonuses(newItem, this.m_AosSkillBonuses);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AMA MeditationAllowance
        {
            get
            {
                return (this.m_Meditate == (AMA)(-1) ? Core.AOS ? this.AosMedAllowance : this.OldMedAllowance : this.m_Meditate);
            }
            set
            {
                this.m_Meditate = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseArmorRating
        {
            get
            {
                if (this.m_ArmorBase == -1)
                    return this.ArmorBase;
                else
                    return this.m_ArmorBase;
            }
            set
            { 
                this.m_ArmorBase = value;
                this.Invalidate(); 
            }
        }

        public double BaseArmorRatingScaled
        {
            get
            {
                return (this.BaseArmorRating * this.ArmorScalar);
            }
        }

        public virtual double ArmorRating
        {
            get
            {
                int ar = this.BaseArmorRating;

                if (this.m_Protection != ArmorProtectionLevel.Regular)
                    ar += 10 + (5 * (int)this.m_Protection);

                switch ( this.m_Resource )
                {
                    case CraftResource.DullCopper:
                        ar += 2;
                        break;
                    case CraftResource.ShadowIron:
                        ar += 4;
                        break;
                    case CraftResource.Copper:
                        ar += 6;
                        break;
                    case CraftResource.Bronze:
                        ar += 8;
                        break;
                    case CraftResource.Gold:
                        ar += 10;
                        break;
                    case CraftResource.Agapite:
                        ar += 12;
                        break;
                    case CraftResource.Verite:
                        ar += 14;
                        break;
                    case CraftResource.Valorite:
                        ar += 16;
                        break;
                    case CraftResource.SpinedLeather:
                        ar += 10;
                        break;
                    case CraftResource.HornedLeather:
                        ar += 13;
                        break;
                    case CraftResource.BarbedLeather:
                        ar += 16;
                        break;
                }

                ar += -8 + (8 * (int)this.m_Quality);
                return this.ScaleArmorByDurability(ar);
            }
        }

        public double ArmorRatingScaled
        {
            get
            {
                return (this.ArmorRating * this.ArmorScalar);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrBonus
        {
            get
            {
                return (this.m_StrBonus == -1 ? Core.AOS ? this.AosStrBonus : this.OldStrBonus : this.m_StrBonus);
            }
            set
            {
                this.m_StrBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexBonus
        {
            get
            {
                return (this.m_DexBonus == -1 ? Core.AOS ? this.AosDexBonus : this.OldDexBonus : this.m_DexBonus);
            }
            set
            {
                this.m_DexBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntBonus
        {
            get
            {
                return (this.m_IntBonus == -1 ? Core.AOS ? this.AosIntBonus : this.OldIntBonus : this.m_IntBonus);
            }
            set
            {
                this.m_IntBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrRequirement
        {
            get
            {
                return (this.m_StrReq == -1 ? Core.AOS ? this.AosStrReq : this.OldStrReq : this.m_StrReq);
            }
            set
            {
                this.m_StrReq = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexRequirement
        {
            get
            {
                return (this.m_DexReq == -1 ? Core.AOS ? this.AosDexReq : this.OldDexReq : this.m_DexReq);
            }
            set
            {
                this.m_DexReq = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntRequirement
        {
            get
            {
                return (this.m_IntReq == -1 ? Core.AOS ? this.AosIntReq : this.OldIntReq : this.m_IntReq);
            }
            set
            {
                this.m_IntReq = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get
            {
                return this.m_Identified;
            }
            set
            {
                this.m_Identified = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get
            {
                return this.m_PlayerConstructed;
            }
            set
            {
                this.m_PlayerConstructed = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return this.m_Resource;
            }
            set
            {
                if (this.m_Resource != value)
                {
                    this.UnscaleDurability();

                    this.m_Resource = value;

                    if (!DefTailoring.IsNonColorable(this.GetType()))
                    {
                        this.Hue = CraftResources.GetHue(this.m_Resource);
                    }

                    this.Invalidate();
                    this.InvalidateProperties();

                    if (this.Parent is Mobile)
                        ((Mobile)this.Parent).UpdateResistances();

                    this.ScaleDurability();
                }
            }
        }

        public virtual double ArmorScalar
        {
            get
            {
                int pos = (int)this.BodyPosition;

                if (pos >= 0 && pos < m_ArmorScalars.Length)
                    return m_ArmorScalars[pos];

                return 1.0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get
            {
                return this.m_MaxHitPoints;
            }
            set
            {
                this.m_MaxHitPoints = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get 
            {
                return this.m_HitPoints;
            }
            set 
            {
                if (value != this.m_HitPoints && this.MaxHitPoints > 0)
                {
                    this.m_HitPoints = value;

                    if (this.m_HitPoints < 0)
                        this.Delete();
                    else if (this.m_HitPoints > this.MaxHitPoints)
                        this.m_HitPoints = this.MaxHitPoints;

                    this.InvalidateProperties();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get
            {
                return this.m_Crafter;
            }
            set
            {
                this.m_Crafter = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArmorQuality Quality
        {
            get
            {
                return this.m_Quality;
            }
            set
            {
                this.UnscaleDurability();
                this.m_Quality = value;
                this.Invalidate();
                this.InvalidateProperties();
                this.ScaleDurability();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArmorDurabilityLevel Durability
        {
            get
            {
                return this.m_Durability;
            }
            set
            {
                this.UnscaleDurability();
                this.m_Durability = value;
                this.ScaleDurability();
                this.InvalidateProperties();
            }
        }

        public virtual int ArtifactRarity
        {
            get
            {
                return 0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArmorProtectionLevel ProtectionLevel
        {
            get
            {
                return this.m_Protection;
            }
            set
            {
                if (this.m_Protection != value)
                {
                    this.m_Protection = value;

                    this.Invalidate();
                    this.InvalidateProperties();

                    if (this.Parent is Mobile)
                        ((Mobile)this.Parent).UpdateResistances();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes
        {
            get
            {
                return this.m_AosAttributes;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosArmorAttributes ArmorAttributes
        {
            get
            {
                return this.m_AosArmorAttributes;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get
            {
                return this.m_AosSkillBonuses;
            }
            set
            {
            }
        }

        public int ComputeStatReq(StatType type)
        {
            int v;

            if (type == StatType.Str)
                v = this.StrRequirement;
            else if (type == StatType.Dex)
                v = this.DexRequirement;
            else
                v = this.IntRequirement;

            return AOS.Scale(v, 100 - this.GetLowerStatReq());
        }

        public int ComputeStatBonus(StatType type)
        {
            if (type == StatType.Str)
                return this.StrBonus + this.Attributes.BonusStr;
            else if (type == StatType.Dex)
                return this.DexBonus + this.Attributes.BonusDex;
            else
                return this.IntBonus + this.Attributes.BonusInt;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PhysicalBonus
        {
            get
            {
                return this.m_PhysicalBonus;
            }
            set
            {
                this.m_PhysicalBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FireBonus
        {
            get
            {
                return this.m_FireBonus;
            }
            set
            {
                this.m_FireBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ColdBonus
        {
            get
            {
                return this.m_ColdBonus;
            }
            set
            {
                this.m_ColdBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonBonus
        {
            get
            {
                return this.m_PoisonBonus;
            }
            set
            {
                this.m_PoisonBonus = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyBonus
        {
            get
            {
                return this.m_EnergyBonus;
            }
            set
            {
                this.m_EnergyBonus = value;
                this.InvalidateProperties();
            }
        }

        public virtual int BasePhysicalResistance
        {
            get
            {
                return 0;
            }
        }
        public virtual int BaseFireResistance
        {
            get
            {
                return 0;
            }
        }
        public virtual int BaseColdResistance
        {
            get
            {
                return 0;
            }
        }
        public virtual int BasePoisonResistance
        {
            get
            {
                return 0;
            }
        }
        public virtual int BaseEnergyResistance
        {
            get
            {
                return 0;
            }
        }

        public override int PhysicalResistance
        {
            get
            {
                return this.BasePhysicalResistance + this.GetProtOffset() + this.GetResourceAttrs().ArmorPhysicalResist + this.m_PhysicalBonus;
            }
        }
        public override int FireResistance
        {
            get
            {
                return this.BaseFireResistance + this.GetProtOffset() + this.GetResourceAttrs().ArmorFireResist + this.m_FireBonus;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return this.BaseColdResistance + this.GetProtOffset() + this.GetResourceAttrs().ArmorColdResist + this.m_ColdBonus;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return this.BasePoisonResistance + this.GetProtOffset() + this.GetResourceAttrs().ArmorPoisonResist + this.m_PoisonBonus;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return this.BaseEnergyResistance + this.GetProtOffset() + this.GetResourceAttrs().ArmorEnergyResist + this.m_EnergyBonus;
            }
        }

        public virtual int InitMinHits
        {
            get
            {
                return 0;
            }
        }
        public virtual int InitMaxHits
        {
            get
            {
                return 0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArmorBodyType BodyPosition
        {
            get
            {
                switch ( this.Layer )
                {
                    default:
                    case Layer.Neck:
                        return ArmorBodyType.Gorget;
                    case Layer.TwoHanded:
                        return ArmorBodyType.Shield;
                    case Layer.Gloves:
                        return ArmorBodyType.Gloves;
                    case Layer.Helm:
                        return ArmorBodyType.Helmet;
                    case Layer.Arms:
                        return ArmorBodyType.Arms;

                    case Layer.InnerLegs:
                    case Layer.OuterLegs:
                    case Layer.Pants:
                        return ArmorBodyType.Legs;

                    case Layer.InnerTorso:
                    case Layer.OuterTorso:
                    case Layer.Shirt:
                        return ArmorBodyType.Chest;
                }
            }
        }

        public void DistributeBonuses(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                switch ( Utility.Random(5) )
                {
                    case 0:
                        ++this.m_PhysicalBonus;
                        break;
                    case 1:
                        ++this.m_FireBonus;
                        break;
                    case 2:
                        ++this.m_ColdBonus;
                        break;
                    case 3:
                        ++this.m_PoisonBonus;
                        break;
                    case 4:
                        ++this.m_EnergyBonus;
                        break;
                }
            }

            this.InvalidateProperties();
        }

        public CraftAttributeInfo GetResourceAttrs()
        {
            CraftResourceInfo info = CraftResources.GetInfo(this.m_Resource);

            if (info == null)
                return CraftAttributeInfo.Blank;

            return info.AttributeInfo;
        }

        public int GetProtOffset()
        {
            switch ( this.m_Protection )
            {
                case ArmorProtectionLevel.Guarding:
                    return 1;
                case ArmorProtectionLevel.Hardening:
                    return 2;
                case ArmorProtectionLevel.Fortification:
                    return 3;
                case ArmorProtectionLevel.Invulnerability:
                    return 4;
            }

            return 0;
        }

        public void UnscaleDurability()
        {
            int scale = 100 + this.GetDurabilityBonus();

            this.m_HitPoints = ((this.m_HitPoints * 100) + (scale - 1)) / scale;
            this.m_MaxHitPoints = ((this.m_MaxHitPoints * 100) + (scale - 1)) / scale;
            this.InvalidateProperties();
        }

        public void ScaleDurability()
        {
            int scale = 100 + this.GetDurabilityBonus();

            this.m_HitPoints = ((this.m_HitPoints * scale) + 99) / 100;
            this.m_MaxHitPoints = ((this.m_MaxHitPoints * scale) + 99) / 100;
            this.InvalidateProperties();
        }

        public int GetDurabilityBonus()
        {
            int bonus = 0;

            if (this.m_Quality == ArmorQuality.Exceptional)
                bonus += 20;

            switch ( this.m_Durability )
            {
                case ArmorDurabilityLevel.Durable:
                    bonus += 20;
                    break;
                case ArmorDurabilityLevel.Substantial:
                    bonus += 50;
                    break;
                case ArmorDurabilityLevel.Massive:
                    bonus += 70;
                    break;
                case ArmorDurabilityLevel.Fortified:
                    bonus += 100;
                    break;
                case ArmorDurabilityLevel.Indestructible:
                    bonus += 120;
                    break;
            }

            if (Core.AOS)
            {
                bonus += this.m_AosArmorAttributes.DurabilityBonus;

                CraftResourceInfo resInfo = CraftResources.GetInfo(this.m_Resource);
                CraftAttributeInfo attrInfo = null;

                if (resInfo != null)
                    attrInfo = resInfo.AttributeInfo;

                if (attrInfo != null)
                    bonus += attrInfo.ArmorDurability;
            }

            return bonus;
        }

        public bool Scissor(Mobile from, Scissors scissors)
        {
            if (!this.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack.
                return false;
            }

            if (Ethics.Ethic.IsImbued(this))
            {
                from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
                return false;
            }

            CraftSystem system = DefTailoring.CraftSystem;

            CraftItem item = system.CraftItems.SearchFor(this.GetType());

            if (item != null && item.Resources.Count == 1 && item.Resources.GetAt(0).Amount >= 2)
            {
                try
                {
                    Item res = (Item)Activator.CreateInstance(CraftResources.GetInfo(this.m_Resource).ResourceTypes[0]);

                    this.ScissorHelper(from, res, m_PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);
                    return true;
                }
                catch
                {
                }
            }

            from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
            return false;
        }

        private static double[] m_ArmorScalars = { 0.07, 0.07, 0.14, 0.15, 0.22, 0.35 };

        public static double[] ArmorScalars
        {
            get
            {
                return m_ArmorScalars;
            }
            set
            {
                m_ArmorScalars = value;
            }
        }

        public static void ValidateMobile(Mobile m)
        {
            for (int i = m.Items.Count - 1; i >= 0; --i)
            {
                if (i >= m.Items.Count)
                    continue;

                Item item = m.Items[i];

                if (item is BaseArmor)
                {
                    BaseArmor armor = (BaseArmor)item;

                    if (armor.RequiredRace != null && m.Race != armor.RequiredRace)
                    {
                        if (armor.RequiredRace == Race.Elf)
                            m.SendLocalizedMessage(1072203); // Only Elves may use this.
                        else
                            m.SendMessage("Only {0} may use this.", armor.RequiredRace.PluralName);

                        m.AddToBackpack(armor);
                    }
                    else if (!armor.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
                    {
                        if (armor.AllowFemaleWearer)
                            m.SendLocalizedMessage(1010388); // Only females can wear this.
                        else
                            m.SendMessage("You may not wear this.");

                        m.AddToBackpack(armor);
                    }
                    else if (!armor.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
                    {
                        if (armor.AllowMaleWearer)
                            m.SendLocalizedMessage(1063343); // Only males can wear this.
                        else
                            m.SendMessage("You may not wear this.");

                        m.AddToBackpack(armor);
                    }
                }
            }
        }

        public int GetLowerStatReq()
        {
            if (!Core.AOS)
                return 0;

            int v = this.m_AosArmorAttributes.LowerStatReq;

            CraftResourceInfo info = CraftResources.GetInfo(this.m_Resource);

            if (info != null)
            {
                CraftAttributeInfo attrInfo = info.AttributeInfo;

                if (attrInfo != null)
                    v += attrInfo.ArmorLowerRequirements;
            }

            if (v > 100)
                v = 100;

            return v;
        }

        public override void OnAdded(object parent)
        {
            if (parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                if (Core.AOS)
                    this.m_AosSkillBonuses.AddTo(from);

                from.Delta(MobileDelta.Armor); // Tell them armor rating has changed
            }
        }

        public virtual double ScaleArmorByDurability(double armor)
        {
            int scale = 100;

            if (this.m_MaxHitPoints > 0 && this.m_HitPoints < this.m_MaxHitPoints)
                scale = 50 + ((50 * this.m_HitPoints) / this.m_MaxHitPoints);

            return (armor * scale) / 100;
        }

        protected void Invalidate()
        {
            if (this.Parent is Mobile)
                ((Mobile)this.Parent).Delta(MobileDelta.Armor); // Tell them armor rating has changed
        }

        public BaseArmor(Serial serial) : base(serial)
        {
        }

        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        [Flags]
        private enum SaveFlag
        {
            None = 0x00000000,
            Attributes = 0x00000001,
            ArmorAttributes = 0x00000002,
            PhysicalBonus = 0x00000004,
            FireBonus = 0x00000008,
            ColdBonus = 0x00000010,
            PoisonBonus = 0x00000020,
            EnergyBonus = 0x00000040,
            Identified = 0x00000080,
            MaxHitPoints = 0x00000100,
            HitPoints = 0x00000200,
            Crafter = 0x00000400,
            Quality = 0x00000800,
            Durability = 0x00001000,
            Protection = 0x00002000,
            Resource = 0x00004000,
            BaseArmor = 0x00008000,
            StrBonus = 0x00010000,
            DexBonus = 0x00020000,
            IntBonus = 0x00040000,
            StrReq = 0x00080000,
            DexReq = 0x00100000,
            IntReq = 0x00200000,
            MedAllowance = 0x00400000,
            SkillBonuses = 0x00800000,
            PlayerConstructed = 0x01000000
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)7); // version

            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.Attributes, !this.m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.ArmorAttributes, !this.m_AosArmorAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PhysicalBonus, this.m_PhysicalBonus != 0);
            SetSaveFlag(ref flags, SaveFlag.FireBonus, this.m_FireBonus != 0);
            SetSaveFlag(ref flags, SaveFlag.ColdBonus, this.m_ColdBonus != 0);
            SetSaveFlag(ref flags, SaveFlag.PoisonBonus, this.m_PoisonBonus != 0);
            SetSaveFlag(ref flags, SaveFlag.EnergyBonus, this.m_EnergyBonus != 0);
            SetSaveFlag(ref flags, SaveFlag.Identified, this.m_Identified != false);
            SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, this.m_MaxHitPoints != 0);
            SetSaveFlag(ref flags, SaveFlag.HitPoints, this.m_HitPoints != 0);
            SetSaveFlag(ref flags, SaveFlag.Crafter, this.m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Quality, this.m_Quality != ArmorQuality.Regular);
            SetSaveFlag(ref flags, SaveFlag.Durability, this.m_Durability != ArmorDurabilityLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Protection, this.m_Protection != ArmorProtectionLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Resource, this.m_Resource != this.DefaultResource);
            SetSaveFlag(ref flags, SaveFlag.BaseArmor, this.m_ArmorBase != -1);
            SetSaveFlag(ref flags, SaveFlag.StrBonus, this.m_StrBonus != -1);
            SetSaveFlag(ref flags, SaveFlag.DexBonus, this.m_DexBonus != -1);
            SetSaveFlag(ref flags, SaveFlag.IntBonus, this.m_IntBonus != -1);
            SetSaveFlag(ref flags, SaveFlag.StrReq, this.m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, this.m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, this.m_IntReq != -1);
            SetSaveFlag(ref flags, SaveFlag.MedAllowance, this.m_Meditate != (AMA)(-1));
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !this.m_AosSkillBonuses.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, this.m_PlayerConstructed != false);

            writer.WriteEncodedInt((int)flags);

            if (GetSaveFlag(flags, SaveFlag.Attributes))
                this.m_AosAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.ArmorAttributes))
                this.m_AosArmorAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.PhysicalBonus))
                writer.WriteEncodedInt((int)this.m_PhysicalBonus);

            if (GetSaveFlag(flags, SaveFlag.FireBonus))
                writer.WriteEncodedInt((int)this.m_FireBonus);

            if (GetSaveFlag(flags, SaveFlag.ColdBonus))
                writer.WriteEncodedInt((int)this.m_ColdBonus);

            if (GetSaveFlag(flags, SaveFlag.PoisonBonus))
                writer.WriteEncodedInt((int)this.m_PoisonBonus);

            if (GetSaveFlag(flags, SaveFlag.EnergyBonus))
                writer.WriteEncodedInt((int)this.m_EnergyBonus);

            if (GetSaveFlag(flags, SaveFlag.MaxHitPoints))
                writer.WriteEncodedInt((int)this.m_MaxHitPoints);

            if (GetSaveFlag(flags, SaveFlag.HitPoints))
                writer.WriteEncodedInt((int)this.m_HitPoints);

            if (GetSaveFlag(flags, SaveFlag.Crafter))
                writer.Write((Mobile)this.m_Crafter);

            if (GetSaveFlag(flags, SaveFlag.Quality))
                writer.WriteEncodedInt((int)this.m_Quality);

            if (GetSaveFlag(flags, SaveFlag.Durability))
                writer.WriteEncodedInt((int)this.m_Durability);

            if (GetSaveFlag(flags, SaveFlag.Protection))
                writer.WriteEncodedInt((int)this.m_Protection);

            if (GetSaveFlag(flags, SaveFlag.Resource))
                writer.WriteEncodedInt((int)this.m_Resource);

            if (GetSaveFlag(flags, SaveFlag.BaseArmor))
                writer.WriteEncodedInt((int)this.m_ArmorBase);

            if (GetSaveFlag(flags, SaveFlag.StrBonus))
                writer.WriteEncodedInt((int)this.m_StrBonus);

            if (GetSaveFlag(flags, SaveFlag.DexBonus))
                writer.WriteEncodedInt((int)this.m_DexBonus);

            if (GetSaveFlag(flags, SaveFlag.IntBonus))
                writer.WriteEncodedInt((int)this.m_IntBonus);

            if (GetSaveFlag(flags, SaveFlag.StrReq))
                writer.WriteEncodedInt((int)this.m_StrReq);

            if (GetSaveFlag(flags, SaveFlag.DexReq))
                writer.WriteEncodedInt((int)this.m_DexReq);

            if (GetSaveFlag(flags, SaveFlag.IntReq))
                writer.WriteEncodedInt((int)this.m_IntReq);

            if (GetSaveFlag(flags, SaveFlag.MedAllowance))
                writer.WriteEncodedInt((int)this.m_Meditate);

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                this.m_AosSkillBonuses.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 7:
                case 6:
                case 5:
                    {
                        SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.Attributes))
                            this.m_AosAttributes = new AosAttributes(this, reader);
                        else
                            this.m_AosAttributes = new AosAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.ArmorAttributes))
                            this.m_AosArmorAttributes = new AosArmorAttributes(this, reader);
                        else
                            this.m_AosArmorAttributes = new AosArmorAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.PhysicalBonus))
                            this.m_PhysicalBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.FireBonus))
                            this.m_FireBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.ColdBonus))
                            this.m_ColdBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.PoisonBonus))
                            this.m_PoisonBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.EnergyBonus))
                            this.m_EnergyBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.Identified))
                            this.m_Identified = (version >= 7 || reader.ReadBool());

                        if (GetSaveFlag(flags, SaveFlag.MaxHitPoints))
                            this.m_MaxHitPoints = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.HitPoints))
                            this.m_HitPoints = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                            this.m_Crafter = reader.ReadMobile();

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                            this.m_Quality = (ArmorQuality)reader.ReadEncodedInt();
                        else
                            this.m_Quality = ArmorQuality.Regular;

                        if (version == 5 && this.m_Quality == ArmorQuality.Low)
                            this.m_Quality = ArmorQuality.Regular;

                        if (GetSaveFlag(flags, SaveFlag.Durability))
                        {
                            this.m_Durability = (ArmorDurabilityLevel)reader.ReadEncodedInt();

                            if (this.m_Durability > ArmorDurabilityLevel.Indestructible)
                                this.m_Durability = ArmorDurabilityLevel.Durable;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Protection))
                        {
                            this.m_Protection = (ArmorProtectionLevel)reader.ReadEncodedInt();

                            if (this.m_Protection > ArmorProtectionLevel.Invulnerability)
                                this.m_Protection = ArmorProtectionLevel.Defense;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                            this.m_Resource = (CraftResource)reader.ReadEncodedInt();
                        else
                            this.m_Resource = this.DefaultResource;

                        if (this.m_Resource == CraftResource.None)
                            this.m_Resource = this.DefaultResource;

                        if (GetSaveFlag(flags, SaveFlag.BaseArmor))
                            this.m_ArmorBase = reader.ReadEncodedInt();
                        else
                            this.m_ArmorBase = -1;

                        if (GetSaveFlag(flags, SaveFlag.StrBonus))
                            this.m_StrBonus = reader.ReadEncodedInt();
                        else
                            this.m_StrBonus = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexBonus))
                            this.m_DexBonus = reader.ReadEncodedInt();
                        else
                            this.m_DexBonus = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntBonus))
                            this.m_IntBonus = reader.ReadEncodedInt();
                        else
                            this.m_IntBonus = -1;

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                            this.m_StrReq = reader.ReadEncodedInt();
                        else
                            this.m_StrReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                            this.m_DexReq = reader.ReadEncodedInt();
                        else
                            this.m_DexReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                            this.m_IntReq = reader.ReadEncodedInt();
                        else
                            this.m_IntReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.MedAllowance))
                            this.m_Meditate = (AMA)reader.ReadEncodedInt();
                        else
                            this.m_Meditate = (AMA)(-1);

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                            this.m_AosSkillBonuses = new AosSkillBonuses(this, reader);

                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                            this.m_PlayerConstructed = true;

                        break;
                    }
                case 4:
                    {
                        this.m_AosAttributes = new AosAttributes(this, reader);
                        this.m_AosArmorAttributes = new AosArmorAttributes(this, reader);
                        goto case 3;
                    }
                case 3:
                    {
                        this.m_PhysicalBonus = reader.ReadInt();
                        this.m_FireBonus = reader.ReadInt();
                        this.m_ColdBonus = reader.ReadInt();
                        this.m_PoisonBonus = reader.ReadInt();
                        this.m_EnergyBonus = reader.ReadInt();
                        goto case 2;
                    }
                case 2:
                case 1:
                    {
                        this.m_Identified = reader.ReadBool();
                        goto case 0;
                    }
                case 0:
                    {
                        this.m_ArmorBase = reader.ReadInt();
                        this.m_MaxHitPoints = reader.ReadInt();
                        this.m_HitPoints = reader.ReadInt();
                        this.m_Crafter = reader.ReadMobile();
                        this.m_Quality = (ArmorQuality)reader.ReadInt();
                        this.m_Durability = (ArmorDurabilityLevel)reader.ReadInt();
                        this.m_Protection = (ArmorProtectionLevel)reader.ReadInt();

                        AMT mat = (AMT)reader.ReadInt();

                        if (this.m_ArmorBase == this.RevertArmorBase)
                            this.m_ArmorBase = -1;

                        /*m_BodyPos = (ArmorBodyType)*/reader.ReadInt();

                        if (version < 4)
                        {
                            this.m_AosAttributes = new AosAttributes(this);
                            this.m_AosArmorAttributes = new AosArmorAttributes(this);
                        }

                        if (version < 3 && this.m_Quality == ArmorQuality.Exceptional)
                            this.DistributeBonuses(6);

                        if (version >= 2)
                        {
                            this.m_Resource = (CraftResource)reader.ReadInt();
                        }
                        else
                        {
                            OreInfo info;

                            switch ( reader.ReadInt() )
                            {
                                default:
                                case 0:
                                    info = OreInfo.Iron;
                                    break;
                                case 1:
                                    info = OreInfo.DullCopper;
                                    break;
                                case 2:
                                    info = OreInfo.ShadowIron;
                                    break;
                                case 3:
                                    info = OreInfo.Copper;
                                    break;
                                case 4:
                                    info = OreInfo.Bronze;
                                    break;
                                case 5:
                                    info = OreInfo.Gold;
                                    break;
                                case 6:
                                    info = OreInfo.Agapite;
                                    break;
                                case 7:
                                    info = OreInfo.Verite;
                                    break;
                                case 8:
                                    info = OreInfo.Valorite;
                                    break;
                            }

                            this.m_Resource = CraftResources.GetFromOreInfo(info, mat);
                        }

                        this.m_StrBonus = reader.ReadInt();
                        this.m_DexBonus = reader.ReadInt();
                        this.m_IntBonus = reader.ReadInt();
                        this.m_StrReq = reader.ReadInt();
                        this.m_DexReq = reader.ReadInt();
                        this.m_IntReq = reader.ReadInt();

                        if (this.m_StrBonus == this.OldStrBonus)
                            this.m_StrBonus = -1;

                        if (this.m_DexBonus == this.OldDexBonus)
                            this.m_DexBonus = -1;

                        if (this.m_IntBonus == this.OldIntBonus)
                            this.m_IntBonus = -1;

                        if (this.m_StrReq == this.OldStrReq)
                            this.m_StrReq = -1;

                        if (this.m_DexReq == this.OldDexReq)
                            this.m_DexReq = -1;

                        if (this.m_IntReq == this.OldIntReq)
                            this.m_IntReq = -1;

                        this.m_Meditate = (AMA)reader.ReadInt();

                        if (this.m_Meditate == this.OldMedAllowance)
                            this.m_Meditate = (AMA)(-1);

                        if (this.m_Resource == CraftResource.None)
                        {
                            if (mat == ArmorMaterialType.Studded || mat == ArmorMaterialType.Leather)
                                this.m_Resource = CraftResource.RegularLeather;
                            else if (mat == ArmorMaterialType.Spined)
                                this.m_Resource = CraftResource.SpinedLeather;
                            else if (mat == ArmorMaterialType.Horned)
                                this.m_Resource = CraftResource.HornedLeather;
                            else if (mat == ArmorMaterialType.Barbed)
                                this.m_Resource = CraftResource.BarbedLeather;
                            else
                                this.m_Resource = CraftResource.Iron;
                        }

                        if (this.m_MaxHitPoints == 0 && this.m_HitPoints == 0)
                            this.m_HitPoints = this.m_MaxHitPoints = Utility.RandomMinMax(this.InitMinHits, this.InitMaxHits);

                        break;
                    }
            }

            if (this.m_AosSkillBonuses == null)
                this.m_AosSkillBonuses = new AosSkillBonuses(this);

            if (Core.AOS && this.Parent is Mobile)
                this.m_AosSkillBonuses.AddTo((Mobile)this.Parent);

            int strBonus = this.ComputeStatBonus(StatType.Str);
            int dexBonus = this.ComputeStatBonus(StatType.Dex);
            int intBonus = this.ComputeStatBonus(StatType.Int);

            if (this.Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)this.Parent;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            if (this.Parent is Mobile)
                ((Mobile)this.Parent).CheckStatTimers();

            if (version < 7)
                this.m_PlayerConstructed = true; // we don't know, so, assume it's crafted
        }

        public virtual CraftResource DefaultResource
        {
            get
            {
                return CraftResource.Iron;
            }
        }

        public BaseArmor(int itemID) : base(itemID)
        {
            this.m_Quality = ArmorQuality.Regular;
            this.m_Durability = ArmorDurabilityLevel.Regular;
            this.m_Crafter = null;

            this.m_Resource = this.DefaultResource;
            this.Hue = CraftResources.GetHue(this.m_Resource);

            this.m_HitPoints = this.m_MaxHitPoints = Utility.RandomMinMax(this.InitMinHits, this.InitMaxHits);

            this.Layer = (Layer)this.ItemData.Quality;

            this.m_AosAttributes = new AosAttributes(this);
            this.m_AosArmorAttributes = new AosArmorAttributes(this);
            this.m_AosSkillBonuses = new AosSkillBonuses(this);
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
                return false;

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public virtual Race RequiredRace
        {
            get
            {
                return null;
            }
        }

        public override bool CanEquip(Mobile from)
        {
            if (!Ethics.Ethic.CheckEquip(from, this))
                return false;

            if (from.AccessLevel < AccessLevel.GameMaster)
            {
                if (this.RequiredRace != null && from.Race != this.RequiredRace)
                {
                    if (this.RequiredRace == Race.Elf)
                        from.SendLocalizedMessage(1072203); // Only Elves may use this.
                    else
                        from.SendMessage("Only {0} may use this.", this.RequiredRace.PluralName);

                    return false;
                }
                else if (!this.AllowMaleWearer && !from.Female)
                {
                    if (this.AllowFemaleWearer)
                        from.SendLocalizedMessage(1010388); // Only females can wear this.
                    else
                        from.SendMessage("You may not wear this.");

                    return false;
                }
                else if (!this.AllowFemaleWearer && from.Female)
                {
                    if (this.AllowMaleWearer)
                        from.SendLocalizedMessage(1063343); // Only males can wear this.
                    else
                        from.SendMessage("You may not wear this.");

                    return false;
                }
                else
                {
                    int strBonus = this.ComputeStatBonus(StatType.Str), strReq = this.ComputeStatReq(StatType.Str);
                    int dexBonus = this.ComputeStatBonus(StatType.Dex), dexReq = this.ComputeStatReq(StatType.Dex);
                    int intBonus = this.ComputeStatBonus(StatType.Int), intReq = this.ComputeStatReq(StatType.Int);

                    if (from.Dex < dexReq || (from.Dex + dexBonus) < 1)
                    {
                        from.SendLocalizedMessage(502077); // You do not have enough dexterity to equip this item.
                        return false;
                    }
                    else if (from.Str < strReq || (from.Str + strBonus) < 1)
                    {
                        from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
                        return false;
                    }
                    else if (from.Int < intReq || (from.Int + intBonus) < 1)
                    {
                        from.SendMessage("You are not smart enough to equip that.");
                        return false;
                    }
                }
            }

            return base.CanEquip(from);
        }

        public override bool CheckPropertyConfliction(Mobile m)
        {
            if (base.CheckPropertyConfliction(m))
                return true;

            if (this.Layer == Layer.Pants)
                return (m.FindItemOnLayer(Layer.InnerLegs) != null);

            if (this.Layer == Layer.Shirt)
                return (m.FindItemOnLayer(Layer.InnerTorso) != null);

            return false;
        }

        public override bool OnEquip(Mobile from)
        {
            from.CheckStatTimers();

            int strBonus = this.ComputeStatBonus(StatType.Str);
            int dexBonus = this.ComputeStatBonus(StatType.Dex);
            int intBonus = this.ComputeStatBonus(StatType.Int);

            if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
            {
                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            return base.OnEquip(from);
        }

        public override void OnRemoved(object parent)
        {
            if (parent is Mobile)
            {
                Mobile m = (Mobile)parent;
                string modName = this.Serial.ToString();

                m.RemoveStatMod(modName + "Str");
                m.RemoveStatMod(modName + "Dex");
                m.RemoveStatMod(modName + "Int");

                if (Core.AOS)
                    this.m_AosSkillBonuses.Remove();

                ((Mobile)parent).Delta(MobileDelta.Armor); // Tell them armor rating has changed
                m.CheckStatTimers();
            }

            base.OnRemoved(parent);
        }

        public virtual int OnHit(BaseWeapon weapon, int damageTaken)
        {
            double HalfAr = this.ArmorRating / 2.0;
            int Absorbed = (int)(HalfAr + HalfAr * Utility.RandomDouble());

            damageTaken -= Absorbed;
            if (damageTaken < 0) 
                damageTaken = 0;

            if (Absorbed < 2)
                Absorbed = 2;

            if (25 > Utility.Random(100)) // 25% chance to lower durability
            {
                if (Core.AOS && this.m_AosArmorAttributes.SelfRepair > Utility.Random(10))
                {
                    this.HitPoints += 2;
                }
                else
                {
                    int wear;

                    if (weapon.Type == WeaponType.Bashing)
                        wear = Absorbed / 2;
                    else
                        wear = Utility.Random(2);

                    if (wear > 0 && this.m_MaxHitPoints > 0)
                    {
                        if (this.m_HitPoints >= wear)
                        {
                            this.HitPoints -= wear;
                            wear = 0;
                        }
                        else
                        {
                            wear -= this.HitPoints;
                            this.HitPoints = 0;
                        }

                        if (wear > 0)
                        {
                            if (this.m_MaxHitPoints > wear)
                            {
                                this.MaxHitPoints -= wear;

                                if (this.Parent is Mobile)
                                    ((Mobile)this.Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                            }
                            else
                            {
                                this.Delete();
                            }
                        }
                    }
                }
            }

            return damageTaken;
        }

        private string GetNameString()
        {
            string name = this.Name;

            if (name == null)
                name = String.Format("#{0}", this.LabelNumber);

            return name;
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                base.Hue = value;
                this.InvalidateProperties();
            }
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            int oreType;

            switch ( this.m_Resource )
            {
                case CraftResource.DullCopper:
                    oreType = 1053108;
                    break; // dull copper
                case CraftResource.ShadowIron:
                    oreType = 1053107;
                    break; // shadow iron
                case CraftResource.Copper:
                    oreType = 1053106;
                    break; // copper
                case CraftResource.Bronze:
                    oreType = 1053105;
                    break; // bronze
                case CraftResource.Gold:
                    oreType = 1053104;
                    break; // golden
                case CraftResource.Agapite:
                    oreType = 1053103;
                    break; // agapite
                case CraftResource.Verite:
                    oreType = 1053102;
                    break; // verite
                case CraftResource.Valorite:
                    oreType = 1053101;
                    break; // valorite
                case CraftResource.SpinedLeather:
                    oreType = 1061118;
                    break; // spined
                case CraftResource.HornedLeather:
                    oreType = 1061117;
                    break; // horned
                case CraftResource.BarbedLeather:
                    oreType = 1061116;
                    break; // barbed
                case CraftResource.RedScales:
                    oreType = 1060814;
                    break; // red
                case CraftResource.YellowScales:
                    oreType = 1060818;
                    break; // yellow
                case CraftResource.BlackScales:
                    oreType = 1060820;
                    break; // black
                case CraftResource.GreenScales:
                    oreType = 1060819;
                    break; // green
                case CraftResource.WhiteScales:
                    oreType = 1060821;
                    break; // white
                case CraftResource.BlueScales:
                    oreType = 1060815;
                    break; // blue
                default:
                    oreType = 0;
                    break;
            }

            if (this.m_Quality == ArmorQuality.Exceptional)
            {
                if (oreType != 0)
                    list.Add(1053100, "#{0}\t{1}", oreType, this.GetNameString()); // exceptional ~1_oretype~ ~2_armortype~
                else
                    list.Add(1050040, this.GetNameString()); // exceptional ~1_ITEMNAME~
            }
            else
            {
                if (oreType != 0)
                    list.Add(1053099, "#{0}\t{1}", oreType, this.GetNameString()); // ~1_oretype~ ~2_armortype~
                else if (this.Name == null)
                    list.Add(this.LabelNumber);
                else
                    list.Add(this.Name);
            }
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
                return true;

            return (this.m_AosAttributes.SpellChanneling != 0);
        }

        public virtual int GetLuckBonus()
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(this.m_Resource);

            if (resInfo == null)
                return 0;

            CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

            if (attrInfo == null)
                return 0;

            return attrInfo.ArmorLuck;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (this.m_Crafter != null)
                list.Add(1050043, this.m_Crafter.Name); // crafted by ~1_NAME~

            #region Factions
            if (this.m_FactionState != null)
                list.Add(1041350); // faction item
            #endregion

            if (this.RequiredRace == Race.Elf)
                list.Add(1075086); // Elves Only

            this.m_AosSkillBonuses.GetProperties(list);

            int prop;

            if ((prop = this.ArtifactRarity) > 0)
                list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~

            if ((prop = this.m_AosAttributes.WeaponDamage) != 0)
                list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

            if ((prop = this.m_AosAttributes.DefendChance) != 0)
                list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

            if ((prop = this.m_AosAttributes.BonusDex) != 0)
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

            if ((prop = this.m_AosAttributes.EnhancePotions) != 0)
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

            if ((prop = this.m_AosAttributes.CastRecovery) != 0)
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

            if ((prop = this.m_AosAttributes.CastSpeed) != 0)
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~

            if ((prop = this.m_AosAttributes.AttackChance) != 0)
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

            if ((prop = this.m_AosAttributes.BonusHits) != 0)
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

            if ((prop = this.m_AosAttributes.BonusInt) != 0)
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

            if ((prop = this.m_AosAttributes.LowerManaCost) != 0)
                list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

            if ((prop = this.m_AosAttributes.LowerRegCost) != 0)
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

            if ((prop = this.GetLowerStatReq()) != 0)
                list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

            if ((prop = (this.GetLuckBonus() + this.m_AosAttributes.Luck)) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

            if ((prop = this.m_AosArmorAttributes.MageArmor) != 0)
                list.Add(1060437); // mage armor

            if ((prop = this.m_AosAttributes.BonusMana) != 0)
                list.Add(1060439, prop.ToString()); // mana increase ~1_val~

            if ((prop = this.m_AosAttributes.RegenMana) != 0)
                list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

            if ((prop = this.m_AosAttributes.NightSight) != 0)
                list.Add(1060441); // night sight

            if ((prop = this.m_AosAttributes.ReflectPhysical) != 0)
                list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

            if ((prop = this.m_AosAttributes.RegenStam) != 0)
                list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

            if ((prop = this.m_AosAttributes.RegenHits) != 0)
                list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

            if ((prop = this.m_AosArmorAttributes.SelfRepair) != 0)
                list.Add(1060450, prop.ToString()); // self repair ~1_val~

            if ((prop = this.m_AosAttributes.SpellChanneling) != 0)
                list.Add(1060482); // spell channeling

            if ((prop = this.m_AosAttributes.SpellDamage) != 0)
                list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

            if ((prop = this.m_AosAttributes.BonusStam) != 0)
                list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

            if ((prop = this.m_AosAttributes.BonusStr) != 0)
                list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

            if ((prop = this.m_AosAttributes.WeaponSpeed) != 0)
                list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

            if (Core.ML && (prop = this.m_AosAttributes.IncreasedKarmaLoss) != 0)
                list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

            base.AddResistanceProperties(list);

            if ((prop = this.GetDurabilityBonus()) > 0)
                list.Add(1060410, prop.ToString()); // durability ~1_val~%

            if ((prop = this.ComputeStatReq(StatType.Str)) > 0)
                list.Add(1061170, prop.ToString()); // strength requirement ~1_val~

            if (this.m_HitPoints >= 0 && this.m_MaxHitPoints > 0)
                list.Add(1060639, "{0}\t{1}", this.m_HitPoints, this.m_MaxHitPoints); // durability ~1_val~ / ~2_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

            if (this.DisplayLootType)
            {
                if (this.LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (this.LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            #region Factions
            if (this.m_FactionState != null)
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            #endregion

            if (this.m_Quality == ArmorQuality.Exceptional)
                attrs.Add(new EquipInfoAttribute(1018305 - (int)this.m_Quality));

            if (this.m_Identified || from.AccessLevel >= AccessLevel.GameMaster)
            {
                if (this.m_Durability != ArmorDurabilityLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038000 + (int)this.m_Durability));

                if (this.m_Protection > ArmorProtectionLevel.Regular && this.m_Protection <= ArmorProtectionLevel.Invulnerability)
                    attrs.Add(new EquipInfoAttribute(1038005 + (int)this.m_Protection));
            }
            else if (this.m_Durability != ArmorDurabilityLevel.Regular || (this.m_Protection > ArmorProtectionLevel.Regular && this.m_Protection <= ArmorProtectionLevel.Invulnerability))
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified

            int number;

            if (this.Name == null)
            {
                number = this.LabelNumber;
            }
            else
            {
                this.LabelTo(from, this.Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && this.Crafter == null && this.Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, this.m_Crafter, false, attrs.ToArray());

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            this.Quality = (ArmorQuality)quality;

            if (makersMark)
                this.Crafter = from;

            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            this.Resource = CraftResources.GetFromType(resourceType);
            this.PlayerConstructed = true;

            CraftContext context = craftSystem.GetContext(from);

            if (context != null && context.DoNotColor)
                this.Hue = 0;

            if (this.Quality == ArmorQuality.Exceptional)
            {
                if (!(Core.ML && this is BaseShield))		// Guessed Core.ML removed exceptional resist bonuses from crafted shields
                    this.DistributeBonuses((tool is BaseRunicTool ? 6 : Core.SE ? 15 : 14)); // Not sure since when, but right now 15 points are added, not 14.

                if (Core.ML && !(this is BaseShield))
                {
                    int bonus = (int)(from.Skills.ArmsLore.Value / 20);

                    for (int i = 0; i < bonus; i++)
                    {
                        switch( Utility.Random(5) )
                        {
                            case 0:
                                this.m_PhysicalBonus++;
                                break;
                            case 1:
                                this.m_FireBonus++;
                                break;
                            case 2:
                                this.m_ColdBonus++;
                                break;
                            case 3:
                                this.m_EnergyBonus++;
                                break;
                            case 4:
                                this.m_PoisonBonus++;
                                break;
                        }
                    }

                    from.CheckSkill(SkillName.ArmsLore, 0, 100);
                }
            }

            if (Core.AOS && tool is BaseRunicTool)
                ((BaseRunicTool)tool).ApplyAttributesTo(this);

            return quality;
        }
        #endregion
    }
}