using System;
using System.Collections.Generic;

namespace Server.Engines.Craft
{
    public enum CraftMarkOption
    {
        MarkItem,
        DoNotMark,
        PromptForMark
    }

    public class CraftContext
    {
        private readonly List<CraftItem> m_Items;
        private int m_LastResourceIndex;
        private int m_LastResourceIndex2;
        private int m_LastGroupIndex;
        private bool m_DoNotColor;
        private CraftMarkOption m_MarkOption;

        public List<CraftItem> Items
        {
            get
            {
                return this.m_Items;
            }
        }
        public int LastResourceIndex
        {
            get
            {
                return this.m_LastResourceIndex;
            }
            set
            {
                this.m_LastResourceIndex = value;
            }
        }
        public int LastResourceIndex2
        {
            get
            {
                return this.m_LastResourceIndex2;
            }
            set
            {
                this.m_LastResourceIndex2 = value;
            }
        }
        public int LastGroupIndex
        {
            get
            {
                return this.m_LastGroupIndex;
            }
            set
            {
                this.m_LastGroupIndex = value;
            }
        }
        public bool DoNotColor
        {
            get
            {
                return this.m_DoNotColor;
            }
            set
            {
                this.m_DoNotColor = value;
            }
        }
        public CraftMarkOption MarkOption
        {
            get
            {
                return this.m_MarkOption;
            }
            set
            {
                this.m_MarkOption = value;
            }
        }

        public CraftContext()
        {
            this.m_Items = new List<CraftItem>();
            this.m_LastResourceIndex = -1;
            this.m_LastResourceIndex2 = -1;
            this.m_LastGroupIndex = -1;
        }

        public CraftItem LastMade
        {
            get
            {
                if (this.m_Items.Count > 0)
                    return this.m_Items[0];

                return null;
            }
        }

        public void OnMade(CraftItem item)
        {
            this.m_Items.Remove(item);

            if (this.m_Items.Count == 10)
                this.m_Items.RemoveAt(9);

            this.m_Items.Insert(0, item);
        }
    }
}