using System; 
using System.Collections.Generic;
 
namespace Server.Mobiles 
{ 
    public class Herbalist : BaseVendor 
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
        public Herbalist() : base("the herbalist")
        { 
            this.SetSkill(SkillName.Alchemy, 80.0, 100.0);
            this.SetSkill(SkillName.Cooking, 80.0, 100.0);
            this.SetSkill(SkillName.TasteID, 80.0, 100.0);
        }

        public override void InitSBInfo() 
        { 
            this.m_SBInfos.Add(new SBHerbalist()); 
        }

        public override VendorShoeType ShoeType
        {
            get
            {
                return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;
            }
        }

        public Herbalist(Serial serial) : base(serial)
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