using Server;

namespace CustomsFramework.Systems.VIPSystem
{
    public class BronzeVIPDeed : BaseVIPDeed
    {
        public override string DefaultName
        {
            get
            {
                return "A Bronze VIP Deed";
            }
        }

        [Constructable(AccessLevel.Developer)]
        public BronzeVIPDeed()
            : base()
        {
            Hue = 1055;
            Tier = VIPTier.Bronze;
            Bonuses.ResProtection.Enabled = true;
            Bonuses.ToolbarAccess.Enabled = true;
            Bonuses.BasicCommands.Enabled = true;
            Bonuses.GainIncrease.Enabled = true;
            Bonuses.FreeCorpseReturn.Enabled = true;
        }

        public BronzeVIPDeed(Serial serial)
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
