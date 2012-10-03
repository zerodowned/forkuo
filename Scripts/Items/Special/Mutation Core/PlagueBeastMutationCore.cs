using System;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class PlagueBeastMutationCore : Item, IScissorable
    {
        private bool m_Cut;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Cut
        {
            get
            {
                return this.m_Cut;
            }
            set
            {
                this.m_Cut = value;
            }
        }

        [Constructable]
        public PlagueBeastMutationCore() : base(0x1CF0)
        {
            this.m_Cut = true;

            this.Name = "a plague beast mutation core";
            this.Weight = 1.0;
            this.Hue = 0x480;
        }

        public virtual bool Scissor(Mobile from, Scissors scissors)
        {
            if (!this.m_Cut)
            {
                PlagueBeastLord owner = this.RootParent as PlagueBeastLord;

                this.m_Cut = true;
                this.Movable = true;

                from.AddToBackpack(this);
                from.LocalOverheadMessage(MessageType.Regular, 0x34, 1071906); // * You remove the plague mutation core from the plague beast, causing it to dissolve into a pile of goo *				

                if (owner != null)
                    Timer.DelayCall<PlagueBeastLord>(TimeSpan.FromSeconds(1), new TimerStateCallback<PlagueBeastLord>(KillParent), owner);

                return true;
            }

            return false;
        }

        private void KillParent(PlagueBeastLord parent)
        {
            parent.Unfreeze();
            parent.Kill();
        }

        public PlagueBeastMutationCore(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((bool)this.m_Cut);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            this.m_Cut = reader.ReadBool();
        }
    }
}