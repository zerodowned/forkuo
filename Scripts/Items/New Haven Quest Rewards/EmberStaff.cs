using System;

namespace Server.Items
{
    public class EmberStaff : QuarterStaff
    {
        public override int LabelNumber
        {
            get
            {
                return 1077582;
            }
        }// Ember Staff

        [Constructable]
        public EmberStaff()
        {
            this.LootType = LootType.Blessed;

            this.WeaponAttributes.HitFireball = 15;
            this.WeaponAttributes.MageWeapon = 10;
            this.Attributes.SpellChanneling = 1;
            this.Attributes.CastSpeed = -1;
            this.WeaponAttributes.LowerStatReq = 50;
        }

        public EmberStaff(Serial serial) : base(serial)
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