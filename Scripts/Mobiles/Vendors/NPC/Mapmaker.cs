using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Mapmaker : BaseVendor
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
        public Mapmaker() : base("the mapmaker")
        {
            this.SetSkill(SkillName.Cartography, 90.0, 100.0);
        }

        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBMapmaker());
        }

        public Mapmaker(Serial serial) : base(serial)
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