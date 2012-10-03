/***************************************************************************
*                               RelayInfo.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: RelayInfo.cs 4 2006-06-15 04:28:39Z mark $
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

namespace Server.Gumps
{
    public class TextRelay
    {
        private readonly int m_EntryID;
        private readonly string m_Text;

        public TextRelay(int entryID, string text)
        {
            this.m_EntryID = entryID;
            this.m_Text = text;
        }

        public int EntryID
        {
            get
            {
                return this.m_EntryID;
            }
        }

        public string Text
        {
            get
            {
                return this.m_Text;
            }
        }
    }

    public class RelayInfo
    {
        private readonly int m_ButtonID;
        private readonly int[] m_Switches;
        private readonly TextRelay[] m_TextEntries;

        public RelayInfo(int buttonID, int[] switches, TextRelay[] textEntries)
        {
            this.m_ButtonID = buttonID;
            this.m_Switches = switches;
            this.m_TextEntries = textEntries;
        }

        public int ButtonID
        {
            get
            {
                return this.m_ButtonID;
            }
        }

        public int[] Switches
        {
            get
            {
                return this.m_Switches;
            }
        }

        public TextRelay[] TextEntries
        {
            get
            {
                return this.m_TextEntries;
            }
        }

        public bool IsSwitched(int switchID)
        {
            for (int i = 0; i < this.m_Switches.Length; ++i)
            {
                if (this.m_Switches[i] == switchID)
                {
                    return true;
                }
            }

            return false;
        }

        public TextRelay GetTextEntry(int entryID)
        {
            for (int i = 0; i < this.m_TextEntries.Length; ++i)
            {
                if (this.m_TextEntries[i].EntryID == entryID)
                {
                    return this.m_TextEntries[i];
                }
            }

            return null;
        }
    }
}