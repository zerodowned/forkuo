using Server;

namespace CustomsFramework.Systems.VIPSystem
{
    public class SilverVIPDeed : BaseVIPDeed
    {
        public override string DefaultName
        {
            get
            {
                return "A Silver VIP Deed";
            }
        }

        [Constructable(AccessLevel.Developer)]
        public SilverVIPDeed()
            : base()
        {
            Hue = 2407;
            Tier = VIPTier.Silver;
            Bonuses.FullLRC.Enabled = true;
            Bonuses.BankIncrease.Enabled = true;
            Bonuses.LifeStoneNoUses.Enabled = true;
            Bonuses.LootGoldFromGround.Enabled = true;
            Bonuses.DoubleResources.Enabled = true;
        }

        public SilverVIPDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            Utilities.WriteVersion(writer, 0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {

                        break;
                    }
            }
        }
    }
}
