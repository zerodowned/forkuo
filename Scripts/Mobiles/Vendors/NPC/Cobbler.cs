using System; 
using System.Collections.Generic;
 
namespace Server.Mobiles 
{ 
    public class Cobbler : BaseVendor 
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
        public Cobbler() : base("the cobbler")
        { 
            this.SetSkill(SkillName.Tailoring, 60.0, 83.0);
        }

        public override void InitSBInfo() 
        { 
            this.m_SBInfos.Add(new SBCobbler()); 
        }

        public override VendorShoeType ShoeType
        {
            get
            {
                return Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;
            }
        }

        public Cobbler(Serial serial) : base(serial)
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