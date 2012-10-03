using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a Virulent corpse")]
    public class Virulent : DreadSpider
    {
        [Constructable]
        public Virulent()
        {
            this.IsParagon = true;

            this.Name = "Virulent";
            this.Hue = 0x8FD;

            this.SetStr(207, 252);
            this.SetDex(156, 194);
            this.SetInt(346, 398);

            this.SetHits(616, 740);
            this.SetStam(156, 194);
            this.SetMana(346, 398);

            this.SetDamage(15, 22);

            this.SetDamageType(ResistanceType.Physical, 20);
            this.SetDamageType(ResistanceType.Poison, 80);

            this.SetResistance(ResistanceType.Physical, 60, 68);
            this.SetResistance(ResistanceType.Fire, 40, 49);
            this.SetResistance(ResistanceType.Cold, 41, 50);
            this.SetResistance(ResistanceType.Poison, 100);
            this.SetResistance(ResistanceType.Energy, 40, 49);

            this.SetSkill(SkillName.Wrestling, 92.8, 111.7);
            this.SetSkill(SkillName.Tactics, 91.6, 107.4);
            this.SetSkill(SkillName.MagicResist, 78.1, 93.3);
            this.SetSkill(SkillName.Poisoning, 120.0);
            this.SetSkill(SkillName.Magery, 104.2, 119.8);
            this.SetSkill(SkillName.EvalInt, 102.8, 117.8);

            this.Fame = 21000;
            this.Karma = -21000;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.UltraRich, 3);
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.MortalStrike;
        }

        /*
        // TODO: uncomment once added
        public override void OnDeath( Container c )
        {
        base.OnDeath( c );

        if ( Utility.RandomDouble() < 0.025 )
        {
        switch ( Utility.Random( 2 ) )
        {
        case 0: c.DropItem( new HunterLegs() ); break;
        case 1: c.DropItem( new MalekisHonor() ); break;
        }
        }

        if ( Utility.RandomDouble() < 0.1 )
        c.DropItem( new ParrotItem() );
        }
        */

        public override bool GivesMLMinorArtifact
        {
            get
            {
                return true;
            }
        }

        public Virulent(Serial serial) : base(serial)
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