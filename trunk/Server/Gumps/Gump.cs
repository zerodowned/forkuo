using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;

namespace Server.Gumps
{
    public class Gump
    {
        private readonly List<GumpEntry> m_Entries;
        private readonly List<string> m_Strings;

        internal int m_TextEntries, m_Switches;

        private static int m_NextSerial = 1;

        private int m_Serial;
        private readonly int m_TypeID;
        private int m_X, m_Y;

        private bool m_Dragable = true;
        private bool m_Closable = true;
        private bool m_Resizable = true;
        private bool m_Disposable = true;

        public static int GetTypeID(Type type)
        {
            return type.FullName.GetHashCode();
        }

        public Gump(int x, int y)
        {
            do
            {
                this.m_Serial = m_NextSerial++;
            }
            while (this.m_Serial == 0); // standard client apparently doesn't send a gump response packet if serial == 0

            this.m_X = x;
            this.m_Y = y;

            this.m_TypeID = GetTypeID(this.GetType());

            this.m_Entries = new List<GumpEntry>();
            this.m_Strings = new List<string>();
        }

        public void Invalidate()
        {
            //if ( m_Strings.Count > 0 )
            //	m_Strings.Clear();
        }

        public int TypeID
        {
            get
            {
                return this.m_TypeID;
            }
        }

        public List<GumpEntry> Entries
        {
            get
            {
                return this.m_Entries;
            }
        }

        public int Serial
        {
            get
            {
                return this.m_Serial;
            }
            set
            {
                if (this.m_Serial != value)
                {
                    this.m_Serial = value;
                    this.Invalidate();
                }
            }
        }

        public int X
        {
            get
            {
                return this.m_X;
            }
            set
            {
                if (this.m_X != value)
                {
                    this.m_X = value;
                    this.Invalidate();
                }
            }
        }

        public int Y
        {
            get
            {
                return this.m_Y;
            }
            set
            {
                if (this.m_Y != value)
                {
                    this.m_Y = value;
                    this.Invalidate();
                }
            }
        }

        public bool Disposable
        {
            get
            {
                return this.m_Disposable;
            }
            set
            {
                if (this.m_Disposable != value)
                {
                    this.m_Disposable = value;
                    this.Invalidate();
                }
            }
        }

        public bool Resizable
        {
            get
            {
                return this.m_Resizable;
            }
            set
            {
                if (this.m_Resizable != value)
                {
                    this.m_Resizable = value;
                    this.Invalidate();
                }
            }
        }

        public bool Dragable
        {
            get
            {
                return this.m_Dragable;
            }
            set
            {
                if (this.m_Dragable != value)
                {
                    this.m_Dragable = value;
                    this.Invalidate();
                }
            }
        }

        public bool Closable
        {
            get
            {
                return this.m_Closable;
            }
            set
            {
                if (this.m_Closable != value)
                {
                    this.m_Closable = value;
                    this.Invalidate();
                }
            }
        }

        public void AddPage(int page)
        {
            this.Add(new GumpPage(page));
        }

        public void AddAlphaRegion(int x, int y, int width, int height)
        {
            this.Add(new GumpAlphaRegion(x, y, width, height));
        }

        public void AddBackground(int x, int y, int width, int height, int gumpID)
        {
            this.Add(new GumpBackground(x, y, width, height, gumpID));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            this.Add(new GumpButton(x, y, normalID, pressedID, buttonID, type, param));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            this.Add(new GumpCheck(x, y, inactiveID, activeID, initialState, switchID));
        }

        public void AddGroup(int group)
        {
            this.Add(new GumpGroup(group));
        }

        public void AddTooltip(int number)
        {
            this.Add(new GumpTooltip(number));
        }

        public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
        {
            this.Add(new GumpHtml(x, y, width, height, text, background, scrollbar));
        }

        public void AddHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
        {
            this.Add(new GumpHtmlLocalized(x, y, width, height, number, background, scrollbar));
        }

        public void AddHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
        {
            this.Add(new GumpHtmlLocalized(x, y, width, height, number, color, background, scrollbar));
        }

        public void AddHtmlLocalized(int x, int y, int width, int height, int number, string args, int color, bool background, bool scrollbar)
        {
            this.Add(new GumpHtmlLocalized(x, y, width, height, number, args, color, background, scrollbar));
        }

        public void AddImage(int x, int y, int gumpID)
        {
            this.Add(new GumpImage(x, y, gumpID));
        }

        public void AddImage(int x, int y, int gumpID, int hue)
        {
            this.Add(new GumpImage(x, y, gumpID, hue));
        }

        public void AddImageTiled(int x, int y, int width, int height, int gumpID)
        {
            this.Add(new GumpImageTiled(x, y, width, height, gumpID));
        }

        public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height)
        {
            this.Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height));
        }

        public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
        {
            this.Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height, localizedTooltip));
        }

        public void AddItem(int x, int y, int itemID)
        {
            this.Add(new GumpItem(x, y, itemID));
        }

        public void AddItem(int x, int y, int itemID, int hue)
        {
            this.Add(new GumpItem(x, y, itemID, hue));
        }

        public void AddLabel(int x, int y, int hue, string text)
        {
            this.Add(new GumpLabel(x, y, hue, text));
        }

        public void AddLabelCropped(int x, int y, int width, int height, int hue, string text)
        {
            this.Add(new GumpLabelCropped(x, y, width, height, hue, text));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            this.Add(new GumpRadio(x, y, inactiveID, activeID, initialState, switchID));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            this.Add(new GumpTextEntry(x, y, width, height, hue, entryID, initialText));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
        {
            this.Add(new GumpTextEntryLimited(x, y, width, height, hue, entryID, initialText, size));
        }

        public void Add(GumpEntry g)
        {
            if (g.Parent != this)
            {
                g.Parent = this;
            }
            else if (!this.m_Entries.Contains(g))
            {
                this.Invalidate();
                this.m_Entries.Add(g);
            }
        }

        public void Remove(GumpEntry g)
        {
            this.Invalidate();
            this.m_Entries.Remove(g);
            g.Parent = null;
        }

        public int Intern(string value)
        {
            int indexOf = this.m_Strings.IndexOf(value);

            if (indexOf >= 0)
            {
                return indexOf;
            }
            else
            {
                this.Invalidate();
                this.m_Strings.Add(value);
                return this.m_Strings.Count - 1;
            }
        }

        public void SendTo(NetState state)
        {
            state.AddGump(this);
            state.Send(this.Compile(state));
        }

        public static byte[] StringToBuffer(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static readonly byte[] m_BeginLayout = StringToBuffer("{ ");
        private static readonly byte[] m_EndLayout = StringToBuffer(" }");

        private static readonly byte[] m_NoMove = StringToBuffer("{ nomove }");
        private static readonly byte[] m_NoClose = StringToBuffer("{ noclose }");
        private static readonly byte[] m_NoDispose = StringToBuffer("{ nodispose }");
        private static readonly byte[] m_NoResize = StringToBuffer("{ noresize }");

        protected Packet Compile()
        {
            return this.Compile(null);
        }

        protected Packet Compile(NetState ns)
        {
            IGumpWriter disp;

            if (ns != null && ns.Unpack)
                disp = new DisplayGumpPacked(this);
            else
                disp = new DisplayGumpFast(this);

            if (!this.m_Dragable)
                disp.AppendLayout(m_NoMove);

            if (!this.m_Closable)
                disp.AppendLayout(m_NoClose);

            if (!this.m_Disposable)
                disp.AppendLayout(m_NoDispose);

            if (!this.m_Resizable)
                disp.AppendLayout(m_NoResize);

            int count = this.m_Entries.Count;
            GumpEntry e;

            for (int i = 0; i < count; ++i)
            {
                e = this.m_Entries[i];

                disp.AppendLayout(m_BeginLayout);
                e.AppendTo(disp);
                disp.AppendLayout(m_EndLayout);
            }

            disp.WriteStrings(this.m_Strings);

            disp.Flush();

            this.m_TextEntries = disp.TextEntries;
            this.m_Switches = disp.Switches;

            return disp as Packet;
        }

        public virtual void OnResponse(NetState sender, RelayInfo info)
        {
        }

        public virtual void OnServerClose(NetState owner)
        {
        }
    }
}