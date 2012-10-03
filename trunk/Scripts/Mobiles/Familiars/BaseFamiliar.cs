using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Items;

namespace Server.Mobiles
{
    public abstract class BaseFamiliar : BaseCreature
    {
        public BaseFamiliar() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override bool Commandable
        {
            get
            {
                return false;
            }
        }

        private bool m_LastHidden;

        public override void OnThink()
        {
            base.OnThink();

            Mobile master = this.ControlMaster;

            if (master == null)
                return;

            if (master.Deleted)
            {
                this.DropPackContents();
                this.EndRelease(null);
                return;
            }

            if (this.m_LastHidden != master.Hidden)
                this.Hidden = this.m_LastHidden = master.Hidden;

            Mobile toAttack = null;

            if (!this.Hidden)
            {
                toAttack = master.Combatant;

                if (toAttack == this)
                    toAttack = master;
                else if (toAttack == null)
                    toAttack = this.Combatant;
            }

            if (this.Combatant != toAttack)
                this.Combatant = null;

            if (toAttack == null)
            {
                if (this.ControlTarget != master || this.ControlOrder != OrderType.Follow)
                {
                    this.ControlTarget = master;
                    this.ControlOrder = OrderType.Follow;
                }
            }
            else if (this.ControlTarget != toAttack || this.ControlOrder != OrderType.Attack)
            {
                this.ControlTarget = toAttack;
                this.ControlOrder = OrderType.Attack;
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive && this.Controlled && from == this.ControlMaster && from.InRange(this, 14))
                list.Add(new ReleaseEntry(from, this));
        }

        public virtual void BeginRelease(Mobile from)
        {
            if (!this.Deleted && this.Controlled && from == this.ControlMaster && from.CheckAlive())
                this.EndRelease(from);
        }

        public virtual void EndRelease(Mobile from)
        {
            if (from == null || (!this.Deleted && this.Controlled && from == this.ControlMaster && from.CheckAlive()))
            {
                Effects.SendLocationParticles(EffectItem.Create(this.Location, this.Map, EffectItem.DefaultDuration), 0x3728, 1, 13, 2100, 3, 5042, 0);
                this.PlaySound(0x201);
                this.Delete();
            }
        }

        public virtual void DropPackContents()
        {
            Map map = this.Map;
            Container pack = this.Backpack;

            if (map != null && map != Map.Internal && pack != null)
            {
                List<Item> list = new List<Item>(pack.Items);

                for (int i = 0; i < list.Count; ++i)
                    list[i].MoveToWorld(this.Location, map);
            }
        }

        public BaseFamiliar(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            ValidationQueue<BaseFamiliar>.Add(this);
        }

        public void Validate()
        {
            this.DropPackContents();
            this.Delete();
        }

        private class ReleaseEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseFamiliar m_Familiar;

            public ReleaseEntry(Mobile from, BaseFamiliar familiar) : base(6118, 14)
            {
                this.m_From = from;
                this.m_Familiar = familiar;
            }

            public override void OnClick()
            {
                if (!this.m_Familiar.Deleted && this.m_Familiar.Controlled && this.m_From == this.m_Familiar.ControlMaster && this.m_From.CheckAlive())
                    this.m_Familiar.BeginRelease(this.m_From);
            }
        }
    }
}