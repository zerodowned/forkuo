/***************************************************************************
*                               GumpButton.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: GumpButton.cs 4 2006-06-15 04:28:39Z mark $
*
***************************************************************************/

/***************************************************************************
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 2 of the License, or
*   (at your option) any later version.
*
***************************************************************************/

using System;
using Server.Network;

namespace Server.Gumps
{
    public enum GumpButtonType
    {
        Page = 0,
        Reply = 1
    }

    public class GumpButton : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private int m_ButtonID;
        private GumpButtonType m_Type;
        private int m_Param;

        public GumpButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_ID1 = normalID;
            this.m_ID2 = pressedID;
            this.m_ButtonID = buttonID;
            this.m_Type = type;
            this.m_Param = param;
        }

        public int X
        {
            get
            {
                return this.m_X;
            }
            set
            {
                this.Delta(ref m_X, value);
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
                this.Delta(ref m_Y, value);
            }
        }

        public int NormalID
        {
            get
            {
                return this.m_ID1;
            }
            set
            {
                this.Delta(ref m_ID1, value);
            }
        }

        public int PressedID
        {
            get
            {
                return this.m_ID2;
            }
            set
            {
                this.Delta(ref m_ID2, value);
            }
        }

        public int ButtonID
        {
            get
            {
                return this.m_ButtonID;
            }
            set
            {
                this.Delta(ref m_ButtonID, value);
            }
        }

        public GumpButtonType Type
        {
            get
            {
                return this.m_Type;
            }
            set
            {
                if (this.m_Type != value)
                {
                    this.m_Type = value;

                    Gump parent = this.Parent;

                    if (parent != null)
                    {
                        parent.Invalidate();
                    }
                }
            }
        }

        public int Param
        {
            get
            {
                return this.m_Param;
            }
            set
            {
                this.Delta(ref m_Param, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ button {0} {1} {2} {3} {4} {5} {6} }}", this.m_X, this.m_Y, this.m_ID1, this.m_ID2, (int)this.m_Type, this.m_Param, this.m_ButtonID);
        }

        private static readonly byte[] m_LayoutName = Gump.StringToBuffer("button");

        public override void AppendTo(IGumpWriter disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(this.m_X);
            disp.AppendLayout(this.m_Y);
            disp.AppendLayout(this.m_ID1);
            disp.AppendLayout(this.m_ID2);
            disp.AppendLayout((int)this.m_Type);
            disp.AppendLayout(this.m_Param);
            disp.AppendLayout(this.m_ButtonID);
        }
    }
}