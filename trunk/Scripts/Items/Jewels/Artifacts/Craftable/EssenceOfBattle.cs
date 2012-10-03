using System;

namespace Server.Items
{
    public class EssenceOfBattle : GoldRing
    {
        public override int LabelNumber
        {
            get
            {
                return 1072935;
            }
        }// Essence of Battle

        [Constructable]
        public EssenceOfBattle()
        {
            this.Hue = 0x550;
            this.Attributes.BonusDex = 7;
            this.Attributes.BonusStr = 7;
            this.Attributes.WeaponDamage = 30;
        }

        public EssenceOfBattle(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}