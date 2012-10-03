using System;
using System.Collections.Generic;

namespace Server.Factions
{
    public class VendorList
    {
        private readonly VendorDefinition m_Definition;
        private readonly List<BaseFactionVendor> m_Vendors;

        public VendorDefinition Definition
        {
            get
            {
                return this.m_Definition;
            }
        }
        public List<BaseFactionVendor> Vendors
        {
            get
            {
                return this.m_Vendors;
            }
        }

        public BaseFactionVendor Construct(Town town, Faction faction)
        {
            try
            {
                return Activator.CreateInstance(this.m_Definition.Type, new object[] { town, faction }) as BaseFactionVendor;
            }
            catch
            {
                return null;
            }
        }

        public VendorList(VendorDefinition definition)
        {
            this.m_Definition = definition;
            this.m_Vendors = new List<BaseFactionVendor>();
        }
    }
}