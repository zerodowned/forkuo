using System;

namespace Server
{
    public class UsageAttribute : Attribute
    {
        private readonly string m_Usage;

        public string Usage
        {
            get
            {
                return this.m_Usage;
            }
        }

        public UsageAttribute(string usage)
        {
            this.m_Usage = usage;
        }
    }

    public class DescriptionAttribute : Attribute
    {
        private readonly string m_Description;

        public string Description
        {
            get
            {
                return this.m_Description;
            }
        }

        public DescriptionAttribute(string description)
        {
            this.m_Description = description;
        }
    }

    public class AliasesAttribute : Attribute
    {
        private readonly string[] m_Aliases;

        public string[] Aliases
        {
            get
            {
                return this.m_Aliases;
            }
        }

        public AliasesAttribute(params string[] aliases)
        {
            this.m_Aliases = aliases;
        }
    }
}