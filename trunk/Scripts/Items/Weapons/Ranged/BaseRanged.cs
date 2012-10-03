using System;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public abstract class BaseRanged : BaseMeleeWeapon
    {
        public abstract int EffectID { get; }
        public abstract Type AmmoType { get; }
        public abstract Item Ammo { get; }

        public override int DefHitSound
        {
            get
            {
                return 0x234;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x238;
            }
        }

        public override SkillName DefSkill
        {
            get
            {
                return SkillName.Archery;
            }
        }
        public override WeaponType DefType
        {
            get
            {
                return WeaponType.Ranged;
            }
        }
        public override WeaponAnimation DefAnimation
        {
            get
            {
                return WeaponAnimation.ShootXBow;
            }
        }

        public override SkillName AccuracySkill
        {
            get
            {
                return SkillName.Archery;
            }
        }

        private Timer m_RecoveryTimer; // so we don't start too many timers
        private bool m_Balanced;
        private int m_Velocity;
		
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Balanced
        {
            get
            {
                return this.m_Balanced;
            }
            set
            {
                this.m_Balanced = value;
                this.InvalidateProperties();
            }
        }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public int Velocity
        {
            get
            {
                return this.m_Velocity;
            }
            set
            {
                this.m_Velocity = value;
                this.InvalidateProperties();
            }
        }

        public BaseRanged(int itemID) : base(itemID)
        {
        }

        public BaseRanged(Serial serial) : base(serial)
        {
        }

        public override TimeSpan OnSwing(Mobile attacker, Mobile defender)
        {
            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            // Make sure we've been standing still for .25/.5/1 second depending on Era
            if (DateTime.Now > (attacker.LastMoveTime + TimeSpan.FromSeconds(Core.SE ? 0.25 : (Core.AOS ? 0.5 : 1.0))) || (Core.AOS && WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
            {
                bool canSwing = true;

                if (Core.AOS)
                {
                    canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                    if (canSwing)
                    {
                        Spell sp = attacker.Spell as Spell;

                        canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                    }
                }

                #region Dueling
                if (attacker is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)attacker;

                    if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
                        canSwing = false;
                }
                #endregion

                if (canSwing && attacker.HarmfulCheck(defender))
                {
                    attacker.DisruptiveAction();
                    attacker.Send(new Swing(0, attacker, defender));

                    if (this.OnFired(attacker, defender))
                    {
                        if (this.CheckHit(attacker, defender))
                            this.OnHit(attacker, defender);
                        else
                            this.OnMiss(attacker, defender);
                    }
                }

                attacker.RevealingAction();

                return this.GetDelay(attacker);
            }
            else
            {
                attacker.RevealingAction();

                return TimeSpan.FromSeconds(0.25);
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            if (attacker.Player && !defender.Player && (defender.Body.IsAnimal || defender.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
                defender.AddToBackpack(this.Ammo);

            if (Core.ML && this.m_Velocity > 0)
            {
                int bonus = (int)attacker.GetDistanceToSqrt(defender);

                if (bonus > 0 && this.m_Velocity > Utility.Random(100))
                {
                    AOS.Damage(defender, attacker, bonus * 3, 100, 0, 0, 0, 0);

                    if (attacker.Player)
                        attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!

                    if (defender.Player)
                        defender.SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
                }
            }

            base.OnHit(attacker, defender, damageBonus);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            if (attacker.Player && 0.4 >= Utility.RandomDouble())
            {
                if (Core.SE)
                {
                    PlayerMobile p = attacker as PlayerMobile;

                    if (p != null)
                    {
                        Type ammo = this.AmmoType;

                        if (p.RecoverableAmmo.ContainsKey(ammo))
                            p.RecoverableAmmo[ammo]++;
                        else
                            p.RecoverableAmmo.Add(ammo, 1);

                        if (!p.Warmode)
                        {
                            if (this.m_RecoveryTimer == null)
                                this.m_RecoveryTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(p.RecoverAmmo));

                            if (!this.m_RecoveryTimer.Running)
                                this.m_RecoveryTimer.Start();
                        }
                    }
                }
                else
                {
                    this.Ammo.MoveToWorld(new Point3D(defender.X + Utility.RandomMinMax(-1, 1), defender.Y + Utility.RandomMinMax(-1, 1), defender.Z), defender.Map);
                }
            }

            base.OnMiss(attacker, defender);
        }

        public virtual bool OnFired(Mobile attacker, Mobile defender)
        {
            if (attacker.Player)
            {
                BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
                Container pack = attacker.Backpack;

                if (quiver == null || Utility.Random(100) >= quiver.LowerAmmoCost)
                {
                    // consume ammo
                    if (quiver != null && quiver.ConsumeTotal(this.AmmoType, 1))
                        quiver.InvalidateWeight();
                    else if (pack == null || !pack.ConsumeTotal(this.AmmoType, 1))
                        return false;
                }
                else if (quiver.FindItemByType(this.AmmoType) == null && (pack == null || pack.FindItemByType(this.AmmoType) == null))
                {
                    // lower ammo cost should not work when we have no ammo at all
                    return false;
                }
            }

            attacker.MovingEffect(defender, this.EffectID, 18, 1, false, false);

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version

            writer.Write((bool)this.m_Balanced);
            writer.Write((int)this.m_Velocity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 3:
                    {
                        this.m_Balanced = reader.ReadBool();
                        this.m_Velocity = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                case 1:
                    {
                        break;
                    }
                case 0:
                    {
                        /*m_EffectID =*/ reader.ReadInt();
                        break;
                    }
            }

            if (version < 2)
            {
                this.WeaponAttributes.MageWeapon = 0;
                this.WeaponAttributes.UseBestSkill = 0;
            }
        }
    }
}