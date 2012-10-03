/***************************************************************************
*                               GumpImageTileButton.cs
*                            -------------------
*   begin                : April 26, 2005
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: GumpImageTileButton.cs 4 2006-06-15 04:28:39Z mark $
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
    public class GumpImageTileButton : GumpEntry
    {
        //Note, on OSI, The tooltip supports ONLY clilocs as far as I can figure out, and the tooltip ONLY works after the buttonTileArt (as far as I can tell from testing)
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private int m_ButtonID;
        private GumpButtonType m_Type;
        private int m_Param;

        private int m_ItemID;
        private int m_Hue;
        private int m_Width;
        private int m_Height;

        private int m_LocalizedTooltip;

        public GumpImageTileButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height) : this(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height, -1)
        {
        }

        public GumpImageTileButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_ID1 = normalID;
            this.m_ID2 = pressedID;
            this.m_ButtonID = buttonID;
            this.m_Type = type;
            this.m_Param = param;

            this.m_ItemID = itemID;
            this.m_Hue = hue;
            this.m_Width = width;
            this.m_Height = height;

            this.m_LocalizedTooltip = localizedTooltip;
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

        public int ItemID
        {
            get
            {
                return this.m_ItemID;
            }
            set
            {
                this.Delta(ref m_ItemID, value);
            }
        }

        public int Hue
        {
            get
            {
                return this.m_Hue;
            }
            set
            {
                this.Delta(ref m_Hue, value);
            }
        }

        public int Width
        {
            get
            {
                return this.m_Width;
            }
            set
            {
                this.Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return this.m_Height;
            }
            set
            {
                this.Delta(ref m_Height, value);
            }
        }

        public int LocalizedTooltip
        {
            get
            {
                return this.m_LocalizedTooltip;
            }
            set
            {
                this.m_LocalizedTooltip = value;
            }
        }

        public override string Compile()
        {
            if (this.m_LocalizedTooltip > 0)
                return String.Format("{{ buttontileart {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} }}{{ tooltip {11} }}", this.m_X, this.m_Y, this.m_ID1, this.m_ID2, (int)this.m_Type, this.m_Param, this.m_ButtonID, this.m_ItemID, this.m_Hue, this.m_Width, this.m_Height, this.m_LocalizedTooltip);
            else
                return String.Format("{{ buttontileart {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} }}", this.m_X, this.m_Y, this.m_ID1, this.m_ID2, (int)this.m_Type, this.m_Param, this.m_ButtonID, this.m_ItemID, this.m_Hue, this.m_Width, this.m_Height);
        }

        private static readonly byte[] m_LayoutName = Gump.StringToBuffer("buttontileart");
        private static readonly byte[] m_LayoutTooltip = Gump.StringToBuffer(" }{ tooltip");

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

            disp.AppendLayout(this.m_ItemID);
            disp.AppendLayout(this.m_Hue);
            disp.AppendLayout(this.m_Width);
            disp.AppendLayout(this.m_Height);

            if (this.m_LocalizedTooltip > 0)
            {
                disp.AppendLayout(m_LayoutTooltip);
                disp.AppendLayout(this.m_LocalizedTooltip);
            }
        }
    }
}