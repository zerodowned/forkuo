using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Alchemist : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos
        {
            get
            {
                return this.m_SBInfos;
            }
        }

        public override NpcGuild NpcGuild
        {
            get
            {
                return NpcGuild.MagesGuild;
            }
        }

        [Constructable]
        public Alchemist() : base("the alchemist")
        {
            this.SetSkill(SkillName.Alchemy, 85.0, 100.0);
            this.SetSkill(SkillName.TasteID, 65.0, 88.0);
        }

        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBAlchemist());
        }

        public override VendorShoeType ShoeType
        {
            get
            {
                return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;
            }
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            this.AddItem(new Server.Items.Robe(Utility.RandomPinkHue()));
        }

        public Alchemist(Serial serial) : base(serial)
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