/***************************************************************************
*                                GumpCheck.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: GumpCheck.cs 4 2006-06-15 04:28:39Z mark $
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
    public class GumpCheck : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private bool m_InitialState;
        private int m_SwitchID;

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

        public int InactiveID
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

        public int ActiveID
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

        public bool InitialState
        {
            get
            {
                return this.m_InitialState;
            }
            set
            {
                this.Delta(ref m_InitialState, value);
            }
        }

        public int SwitchID
        {
            get
            {
                return this.m_SwitchID;
            }
            set
            {
                this.Delta(ref m_SwitchID, value);
            }
        }

        public GumpCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_ID1 = inactiveID;
            this.m_ID2 = activeID;
            this.m_InitialState = initialState;
            this.m_SwitchID = switchID;
        }

        public override string Compile()
        {
            return String.Format("{{ checkbox {0} {1} {2} {3} {4} {5} }}", this.m_X, this.m_Y, this.m_ID1, this.m_ID2, this.m_InitialState ? 1 : 0, this.m_SwitchID);
        }

        private static readonly byte[] m_LayoutName = Gump.StringToBuffer("checkbox");

        public override void AppendTo(IGumpWriter disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(this.m_X);
            disp.AppendLayout(this.m_Y);
            disp.AppendLayout(this.m_ID1);
            disp.AppendLayout(this.m_ID2);
            disp.AppendLayout(this.m_InitialState);
            disp.AppendLayout(this.m_SwitchID);

            disp.Switches++;
        }
    }
}