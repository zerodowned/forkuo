using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Farmer : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos
        {
            get
            {
                return this.m_SBInfos;
            }
        }

        [Constructable]
        public Farmer() : base("the farmer")
        {
            this.SetSkill(SkillName.Lumberjacking, 36.0, 68.0);
            this.SetSkill(SkillName.TasteID, 36.0, 68.0);
            this.SetSkill(SkillName.Cooking, 36.0, 68.0);
        }

        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBFarmer());
        }

        public override VendorShoeType ShoeType
        {
            get
            {
                return VendorShoeType.ThighBoots;
            }
        }

        public override int GetShoeHue()
        {
            return 0;
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            this.AddItem(new Server.Items.WideBrimHat(Utility.RandomNeutralHue()));
        }

        public Farmer(Serial serial) : base(serial)
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