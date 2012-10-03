using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Rancher : BaseVendor
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
        public Rancher() : base("the rancher")
        {
            this.SetSkill(SkillName.AnimalLore, 55.0, 78.0);
            this.SetSkill(SkillName.AnimalTaming, 55.0, 78.0);
            this.SetSkill(SkillName.Herding, 64.0, 100.0);
            this.SetSkill(SkillName.Veterinary, 60.0, 83.0);
        }

        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBRancher());
        }

        public Rancher(Serial serial) : base(serial)
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