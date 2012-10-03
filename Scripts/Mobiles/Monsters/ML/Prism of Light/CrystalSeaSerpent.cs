using System;

namespace Server.Mobiles
{
    [CorpseName("a crystal sea serpent corpse")]
    public class CrystalSeaSerpent : SeaSerpent
    {
        [Constructable]
        public CrystalSeaSerpent()
        {
            this.Name = "a crystal sea serpent";
            this.Hue = 0x47E;

            this.SetStr(250, 450);
            this.SetDex(100, 150);
            this.SetInt(90, 190);

            this.SetHits(230, 330);

            this.SetDamage(10, 18);

            this.SetDamageType(ResistanceType.Physical, 10);
            this.SetDamageType(ResistanceType.Cold, 45);
            this.SetDamageType(ResistanceType.Energy, 45);

            this.SetResistance(ResistanceType.Physical, 50, 70);
            this.SetResistance(ResistanceType.Fire, 0);
            this.SetResistance(ResistanceType.Cold, 70, 90);
            this.SetResistance(ResistanceType.Poison, 20, 30);
            this.SetResistance(ResistanceType.Energy, 60, 80);
        }

        /*
        // TODO: uncomment once added
        public override void OnDeath( Container c )
        {
        base.OnDeath( c );

        if ( Utility.RandomDouble() < 0.05 )
        c.DropItem( new CrushedCrystals() );

        if ( Utility.RandomDouble() < 0.1 )
        c.DropItem( new IcyHeart() );

        if ( Utility.RandomDouble() < 0.1 )
        c.DropItem( new LuckyDagger() );
        }
        */

        public CrystalSeaSerpent(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}