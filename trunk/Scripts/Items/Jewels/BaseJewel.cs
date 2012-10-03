using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public enum GemType
    {
        None,
        StarSapphire,
        Emerald,
        Sapphire,
        Ruby,
        Citrine,
        Amethyst,
        Tourmaline,
        Amber,
        Diamond
    }

    public abstract class BaseJewel : Item, ICraftable
    {
        private int m_MaxHitPoints;
        private int m_HitPoints;

        private AosAttributes m_AosAttributes;
        private AosElementAttributes m_AosResistances;
        private AosSkillBonuses m_AosSkillBonuses;
        private CraftResource m_Resource;
        private GemType m_GemType;

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

        [CommandProperty(AccessLevel.Player)]
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
        public AosElementAttributes Resistances
        {
            get
            {
                return this.m_AosResistances;
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

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return this.m_Resource;
            }
            set
            {
                this.m_Resource = value;
                this.Hue = CraftResources.GetHue(this.m_Resource);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public GemType GemType
        {
            get
            {
                return this.m_GemType;
            }
            set
            {
                this.m_GemType = value;
                this.InvalidateProperties();
            }
        }

        public override int PhysicalResistance
        {
            get
            {
                return this.m_AosResistances.Physical;
            }
        }
        public override int FireResistance
        {
            get
            {
                return this.m_AosResistances.Fire;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return this.m_AosResistances.Cold;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return this.m_AosResistances.Poison;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return this.m_AosResistances.Energy;
            }
        }
        public virtual int BaseGemTypeNumber
        {
            get
            {
                return 0;
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

        public override int LabelNumber
        {
            get
            {
                if (this.m_GemType == GemType.None)
                    return base.LabelNumber;

                return this.BaseGemTypeNumber + (int)this.m_GemType - 1;
            }
        }

        public override void OnAfterDuped(Item newItem)
        {
            BaseJewel jewel = newItem as BaseJewel;

            if (jewel == null)
                return;

            jewel.m_AosAttributes = new AosAttributes(newItem, this.m_AosAttributes);
            jewel.m_AosResistances = new AosElementAttributes(newItem, this.m_AosResistances);
            jewel.m_AosSkillBonuses = new AosSkillBonuses(newItem, this.m_AosSkillBonuses);
        }

        public virtual int ArtifactRarity
        {
            get
            {
                return 0;
            }
        }

        public BaseJewel(int itemID, Layer layer) : base(itemID)
        {
            this.m_AosAttributes = new AosAttributes(this);
            this.m_AosResistances = new AosElementAttributes(this);
            this.m_AosSkillBonuses = new AosSkillBonuses(this);
            this.m_Resource = CraftResource.Iron;
            this.m_GemType = GemType.None;

            this.Layer = layer;

            this.m_HitPoints = this.m_MaxHitPoints = Utility.RandomMinMax(this.InitMinHits, this.InitMaxHits);
        }

        public override void OnAdded(object parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                this.m_AosSkillBonuses.AddTo(from);

                int strBonus = this.m_AosAttributes.BonusStr;
                int dexBonus = this.m_AosAttributes.BonusDex;
                int intBonus = this.m_AosAttributes.BonusInt;

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

                from.CheckStatTimers();
            }
        }

        public override void OnRemoved(object parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                this.m_AosSkillBonuses.Remove();

                string modName = this.Serial.ToString();

                from.RemoveStatMod(modName + "Str");
                from.RemoveStatMod(modName + "Dex");
                from.RemoveStatMod(modName + "Int");

                from.CheckStatTimers();
            }
        }

        public BaseJewel(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

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

            if ((prop = this.m_AosAttributes.Luck) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

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

            if (this.m_HitPoints >= 0 && this.m_MaxHitPoints > 0)
                list.Add(1060639, "{0}\t{1}", this.m_HitPoints, this.m_MaxHitPoints); // durability ~1_val~ / ~2_val~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version

            writer.WriteEncodedInt((int)this.m_MaxHitPoints);
            writer.WriteEncodedInt((int)this.m_HitPoints);

            writer.WriteEncodedInt((int)this.m_Resource);
            writer.WriteEncodedInt((int)this.m_GemType);

            this.m_AosAttributes.Serialize(writer);
            this.m_AosResistances.Serialize(writer);
            this.m_AosSkillBonuses.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 3:
                    {
                        this.m_MaxHitPoints = reader.ReadEncodedInt();
                        this.m_HitPoints = reader.ReadEncodedInt();

                        goto case 2;
                    }
                case 2:
                    {
                        this.m_Resource = (CraftResource)reader.ReadEncodedInt();
                        this.m_GemType = (GemType)reader.ReadEncodedInt();

                        goto case 1;
                    }
                case 1:
                    {
                        this.m_AosAttributes = new AosAttributes(this, reader);
                        this.m_AosResistances = new AosElementAttributes(this, reader);
                        this.m_AosSkillBonuses = new AosSkillBonuses(this, reader);

                        if (Core.AOS && this.Parent is Mobile)
                            this.m_AosSkillBonuses.AddTo((Mobile)this.Parent);

                        int strBonus = this.m_AosAttributes.BonusStr;
                        int dexBonus = this.m_AosAttributes.BonusDex;
                        int intBonus = this.m_AosAttributes.BonusInt;

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

                        break;
                    }
                case 0:
                    {
                        this.m_AosAttributes = new AosAttributes(this);
                        this.m_AosResistances = new AosElementAttributes(this);
                        this.m_AosSkillBonuses = new AosSkillBonuses(this);

                        break;
                    }
            }

            if (version < 2)
            {
                this.m_Resource = CraftResource.Iron;
                this.m_GemType = GemType.None;
            }
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            this.Resource = CraftResources.GetFromType(resourceType);

            CraftContext context = craftSystem.GetContext(from);

            if (context != null && context.DoNotColor)
                this.Hue = 0;

            if (1 < craftItem.Resources.Count)
            {
                resourceType = craftItem.Resources.GetAt(1).ItemType;

                if (resourceType == typeof(StarSapphire))
                    this.GemType = GemType.StarSapphire;
                else if (resourceType == typeof(Emerald))
                    this.GemType = GemType.Emerald;
                else if (resourceType == typeof(Sapphire))
                    this.GemType = GemType.Sapphire;
                else if (resourceType == typeof(Ruby))
                    this.GemType = GemType.Ruby;
                else if (resourceType == typeof(Citrine))
                    this.GemType = GemType.Citrine;
                else if (resourceType == typeof(Amethyst))
                    this.GemType = GemType.Amethyst;
                else if (resourceType == typeof(Tourmaline))
                    this.GemType = GemType.Tourmaline;
                else if (resourceType == typeof(Amber))
                    this.GemType = GemType.Amber;
                else if (resourceType == typeof(Diamond))
                    this.GemType = GemType.Diamond;
            }

            return 1;
        }
        #endregion
    }
}