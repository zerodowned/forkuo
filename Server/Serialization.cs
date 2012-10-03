/***************************************************************************
*                             Serialization.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: Serialization.cs 844 2012-03-07 13:47:33Z mark $
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using CustomsFramework;
using Server.Guilds;

namespace Server
{
    public abstract class GenericReader
    {
        protected GenericReader()
        {
        }

        public abstract string ReadString();

        public abstract DateTime ReadDateTime();

        public abstract DateTimeOffset ReadDateTimeOffset();

        public abstract TimeSpan ReadTimeSpan();

        public abstract DateTime ReadDeltaTime();

        public abstract decimal ReadDecimal();

        public abstract long ReadLong();

        public abstract ulong ReadULong();

        public abstract int ReadInt();

        public abstract uint ReadUInt();

        public abstract short ReadShort();

        public abstract ushort ReadUShort();

        public abstract double ReadDouble();

        public abstract float ReadFloat();

        public abstract char ReadChar();

        public abstract byte ReadByte();

        public abstract sbyte ReadSByte();

        public abstract bool ReadBool();

        public abstract int ReadEncodedInt();

        public abstract IPAddress ReadIPAddress();

        public abstract Point3D ReadPoint3D();

        public abstract Point2D ReadPoint2D();

        public abstract Rectangle2D ReadRect2D();

        public abstract Rectangle3D ReadRect3D();

        public abstract Map ReadMap();

        public abstract Item ReadItem();

        public abstract Mobile ReadMobile();

        public abstract BaseGuild ReadGuild();

        public abstract BaseCore ReadCore();

        public abstract BaseModule ReadModule();

        public abstract BaseService ReadService();

        public abstract T ReadItem<T>() where T : Item;

        public abstract T ReadMobile<T>() where T : Mobile;

        public abstract T ReadGuild<T>() where T : BaseGuild;

        public abstract T ReadCore<T>() where T : BaseCore;

        public abstract T ReadModule<T>() where T : BaseModule;

        public abstract T ReadService<T>() where T : BaseService;

        public abstract ArrayList ReadItemList();

        public abstract ArrayList ReadMobileList();

        public abstract ArrayList ReadGuildList();

        public abstract ArrayList ReadCoreList();

        public abstract ArrayList ReadModuleList();

        public abstract ArrayList ReadServiceList();

        public abstract List<Item> ReadStrongItemList();

        public abstract List<T> ReadStrongItemList<T>() where T : Item;

        public abstract List<Mobile> ReadStrongMobileList();

        public abstract List<T> ReadStrongMobileList<T>() where T : Mobile;

        public abstract List<BaseGuild> ReadStrongGuildList();

        public abstract List<T> ReadStrongGuildList<T>() where T : BaseGuild;

        public abstract List<BaseCore> ReadStrongCoreList();

        public abstract List<T> ReadStrongCoreList<T>() where T : BaseCore;

        public abstract List<BaseModule> ReadStrongModuleList();

        public abstract List<T> ReadStrongModuleList<T>() where T : BaseModule;

        public abstract List<BaseService> ReadStrongServiceList();

        public abstract List<T> ReadStrongServiceList<T>() where T : BaseService;

        public abstract HashSet<Item> ReadItemSet();

        public abstract HashSet<T> ReadItemSet<T>() where T : Item;

        public abstract HashSet<Mobile> ReadMobileSet();

        public abstract HashSet<T> ReadMobileSet<T>() where T : Mobile;

        public abstract HashSet<BaseGuild> ReadGuildSet();

        public abstract HashSet<T> ReadGuildSet<T>() where T : BaseGuild;

        public abstract HashSet<BaseCore> ReadCoreSet();

        public abstract HashSet<T> ReadCoreSet<T>() where T : BaseCore;

        public abstract HashSet<BaseModule> ReadModuleSet();

        public abstract HashSet<T> ReadModuleSet<T>() where T : BaseModule;

        public abstract HashSet<BaseService> ReadServiceSet();

        public abstract HashSet<T> ReadServiceSet<T>() where T : BaseService;

        public abstract Race ReadRace();

        public abstract bool End();
    }

    public abstract class GenericWriter
    {
        protected GenericWriter()
        {
        }

        public abstract void Close();

        public abstract long Position { get; }

        public abstract void Write(string value);

        public abstract void Write(DateTime value);

        public abstract void Write(DateTimeOffset value);

        public abstract void Write(TimeSpan value);

        public abstract void Write(decimal value);

        public abstract void Write(long value);

        public abstract void Write(ulong value);

        public abstract void Write(int value);

        public abstract void Write(uint value);

        public abstract void Write(short value);

        public abstract void Write(ushort value);

        public abstract void Write(double value);

        public abstract void Write(float value);

        public abstract void Write(char value);

        public abstract void Write(byte value);

        public abstract void Write(sbyte value);

        public abstract void Write(bool value);

        public abstract void WriteEncodedInt(int value);

        public abstract void Write(IPAddress value);

        public abstract void WriteDeltaTime(DateTime value);

        public abstract void Write(Point3D value);

        public abstract void Write(Point2D value);

        public abstract void Write(Rectangle2D value);

        public abstract void Write(Rectangle3D value);

        public abstract void Write(Map value);

        public abstract void Write(Item value);

        public abstract void Write(Mobile value);

        public abstract void Write(BaseGuild value);

        public abstract void Write(BaseCore value);

        public abstract void Write(BaseModule value);

        public abstract void Write(BaseService value);

        public abstract void WriteItem<T>(T value) where T : Item;

        public abstract void WriteMobile<T>(T value) where T : Mobile;

        public abstract void WriteGuild<T>(T value) where T : BaseGuild;

        public abstract void WriteCore<T>(T value) where T : BaseCore;

        public abstract void WriteModule<T>(T value) where T : BaseModule;

        public abstract void WriteService<T>(T value) where T : BaseService;

        public abstract void Write(Race value);

        public abstract void WriteItemList(ArrayList list);

        public abstract void WriteItemList(ArrayList list, bool tidy);

        public abstract void WriteMobileList(ArrayList list);

        public abstract void WriteMobileList(ArrayList list, bool tidy);

        public abstract void WriteGuildList(ArrayList list);

        public abstract void WriteGuildList(ArrayList list, bool tidy);

        public abstract void WriteCoreList(ArrayList list);

        public abstract void WriteCoreList(ArrayList list, bool tidy);

        public abstract void WriteModuleList(ArrayList list);

        public abstract void WriteModuleList(ArrayList list, bool tidy);

        public abstract void WriteServiceList(ArrayList list);

        public abstract void Write(List<Item> list);

        public abstract void Write(List<Item> list, bool tidy);

        public abstract void WriteItemList<T>(List<T> list) where T : Item;

        public abstract void WriteItemList<T>(List<T> list, bool tidy) where T : Item;

        public abstract void Write(HashSet<Item> list);

        public abstract void Write(HashSet<Item> list, bool tidy);

        public abstract void WriteItemSet<T>(HashSet<T> set) where T : Item;

        public abstract void WriteItemSet<T>(HashSet<T> set, bool tidy) where T : Item;

        public abstract void Write(List<Mobile> list);

        public abstract void Write(List<Mobile> list, bool tidy);

        public abstract void WriteMobileList<T>(List<T> list) where T : Mobile;

        public abstract void WriteMobileList<T>(List<T> list, bool tidy) where T : Mobile;

        public abstract void Write(HashSet<Mobile> list);

        public abstract void Write(HashSet<Mobile> list, bool tidy);

        public abstract void WriteMobileSet<T>(HashSet<T> set) where T : Mobile;

        public abstract void WriteMobileSet<T>(HashSet<T> set, bool tidy) where T : Mobile;

        public abstract void Write(List<BaseGuild> list);

        public abstract void Write(List<BaseGuild> list, bool tidy);

        public abstract void WriteGuildList<T>(List<T> list) where T : BaseGuild;

        public abstract void WriteGuildList<T>(List<T> list, bool tidy) where T : BaseGuild;

        public abstract void Write(HashSet<BaseGuild> list);

        public abstract void Write(HashSet<BaseGuild> list, bool tidy);

        public abstract void WriteGuildSet<T>(HashSet<T> set) where T : BaseGuild;

        public abstract void WriteGuildSet<T>(HashSet<T> set, bool tidy) where T : BaseGuild;

        public abstract void Write(List<BaseCore> list);

        public abstract void Write(List<BaseCore> list, bool tidy);

        public abstract void WriteCoreList<T>(List<T> list) where T : BaseCore;

        public abstract void WriteCoreList<T>(List<T> list, bool tidy) where T : BaseCore;

        public abstract void Write(HashSet<BaseCore> set);

        public abstract void Write(HashSet<BaseCore> set, bool tidy);

        public abstract void WriteCoreSet<T>(HashSet<T> set) where T : BaseCore;

        public abstract void WriteCoreSet<T>(HashSet<T> set, bool tidy) where T : BaseCore;

        public abstract void Write(List<BaseModule> list);

        public abstract void Write(List<BaseModule> list, bool tidy);

        public abstract void WriteModuleList<T>(List<T> list) where T : BaseModule;

        public abstract void WriteModuleList<T>(List<T> list, bool tidy) where T : BaseModule;

        public abstract void Write(HashSet<BaseModule> set);

        public abstract void Write(HashSet<BaseModule> set, bool tidy);

        public abstract void WriteModuleSet<T>(HashSet<T> set) where T : BaseModule;

        public abstract void WriteModuleSet<T>(HashSet<T> set, bool tidy) where T : BaseModule;

        public abstract void Write(List<BaseService> list);

        public abstract void WriteServiceList<T>(List<T> list) where T : BaseService;

        public abstract void Write(HashSet<BaseService> set);

        public abstract void WriteServiceSet<T>(HashSet<T> set) where T : BaseService;
        //Stupid compiler won't notice there 'where' to differentiate the generic methods.
    }

    public class BinaryFileWriter : GenericWriter
    {
        private readonly bool PrefixStrings;
        private readonly Stream m_File;

        protected virtual int BufferSize
        {
            get
            {
                return 64 * 1024;
            }
        }

        private readonly byte[] m_Buffer;

        private int m_Index;

        private readonly Encoding m_Encoding;

        public BinaryFileWriter(Stream strm, bool prefixStr)
        {
            this.PrefixStrings = prefixStr;
            this.m_Encoding = Utility.UTF8;
            this.m_Buffer = new byte[this.BufferSize];
            this.m_File = strm;
        }

        public BinaryFileWriter(string filename, bool prefixStr)
        {
            this.PrefixStrings = prefixStr;
            this.m_Buffer = new byte[this.BufferSize];
            this.m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            this.m_Encoding = Utility.UTF8WithEncoding;
        }

        public void Flush()
        {
            if (this.m_Index > 0)
            {
                this.m_Position += this.m_Index;

                this.m_File.Write(this.m_Buffer, 0, this.m_Index);
                this.m_Index = 0;
            }
        }

        private long m_Position;

        public override long Position
        {
            get
            {
                return this.m_Position + this.m_Index;
            }
        }

        public Stream UnderlyingStream
        {
            get
            {
                if (this.m_Index > 0)
                    this.Flush();

                return this.m_File;
            }
        }

        public override void Close()
        {
            if (this.m_Index > 0)
                this.Flush();

            this.m_File.Close();
        }

        public override void WriteEncodedInt(int value)
        {
            uint v = (uint)value;

            while (v >= 0x80)
            {
                if ((this.m_Index + 1) > this.m_Buffer.Length)
                    this.Flush();

                this.m_Buffer[this.m_Index++] = (byte)(v | 0x80);
                v >>= 7;
            }

            if ((this.m_Index + 1) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index++] = (byte)v;
        }

        private byte[] m_CharacterBuffer;
        private int m_MaxBufferChars;
        private const int LargeByteBufferSize = 256;

        internal void InternalWriteString(string value)
        {
            int length = this.m_Encoding.GetByteCount(value);

            this.WriteEncodedInt(length);

            if (this.m_CharacterBuffer == null)
            {
                this.m_CharacterBuffer = new byte[LargeByteBufferSize];
                this.m_MaxBufferChars = LargeByteBufferSize / this.m_Encoding.GetMaxByteCount(1);
            }

            if (length > LargeByteBufferSize)
            {
                int current = 0;
                int charsLeft = value.Length;

                while (charsLeft > 0)
                {
                    int charCount = (charsLeft > this.m_MaxBufferChars) ? this.m_MaxBufferChars : charsLeft;
                    int byteLength = this.m_Encoding.GetBytes(value, current, charCount, this.m_CharacterBuffer, 0);

                    if ((this.m_Index + byteLength) > this.m_Buffer.Length)
                        this.Flush();

                    Buffer.BlockCopy(this.m_CharacterBuffer, 0, this.m_Buffer, this.m_Index, byteLength);
                    this.m_Index += byteLength;

                    current += charCount;
                    charsLeft -= charCount;
                }
            }
            else
            {
                int byteLength = this.m_Encoding.GetBytes(value, 0, value.Length, this.m_CharacterBuffer, 0);

                if ((this.m_Index + byteLength) > this.m_Buffer.Length)
                    this.Flush();

                Buffer.BlockCopy(this.m_CharacterBuffer, 0, this.m_Buffer, this.m_Index, byteLength);
                this.m_Index += byteLength;
            }
        }

        public override void Write(string value)
        {
            if (this.PrefixStrings)
            {
                if (value == null)
                {
                    if ((this.m_Index + 1) > this.m_Buffer.Length)
                        this.Flush();

                    this.m_Buffer[this.m_Index++] = 0;
                }
                else
                {
                    if ((this.m_Index + 1) > this.m_Buffer.Length)
                        this.Flush();

                    this.m_Buffer[this.m_Index++] = 1;

                    this.InternalWriteString(value);
                }
            }
            else
            {
                this.InternalWriteString(value);
            }
        }

        public override void Write(DateTime value)
        {
            this.Write(value.Ticks);
        }

        public override void Write(DateTimeOffset value)
        {
            this.Write(value.Ticks);
            this.Write(value.Offset.Ticks);
        }

        public override void WriteDeltaTime(DateTime value)
        {
            long ticks = value.Ticks;
            long now = DateTime.Now.Ticks;

            TimeSpan d;

            try
            {
                d = new TimeSpan(ticks - now);
            }
            catch
            {
                d = TimeSpan.MaxValue;
            }

            this.Write(d);
        }

        public override void Write(IPAddress value)
        {
            this.Write(Utility.GetLongAddressValue(value));
        }

        public override void Write(TimeSpan value)
        {
            this.Write(value.Ticks);
        }

        public override void Write(decimal value)
        {
            int[] bits = Decimal.GetBits(value);

            for (int i = 0; i < bits.Length; ++i)
                this.Write(bits[i]);
        }

        public override void Write(long value)
        {
            if ((this.m_Index + 8) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Buffer[this.m_Index + 2] = (byte)(value >> 16);
            this.m_Buffer[this.m_Index + 3] = (byte)(value >> 24);
            this.m_Buffer[this.m_Index + 4] = (byte)(value >> 32);
            this.m_Buffer[this.m_Index + 5] = (byte)(value >> 40);
            this.m_Buffer[this.m_Index + 6] = (byte)(value >> 48);
            this.m_Buffer[this.m_Index + 7] = (byte)(value >> 56);
            this.m_Index += 8;
        }

        public override void Write(ulong value)
        {
            if ((this.m_Index + 8) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Buffer[this.m_Index + 2] = (byte)(value >> 16);
            this.m_Buffer[this.m_Index + 3] = (byte)(value >> 24);
            this.m_Buffer[this.m_Index + 4] = (byte)(value >> 32);
            this.m_Buffer[this.m_Index + 5] = (byte)(value >> 40);
            this.m_Buffer[this.m_Index + 6] = (byte)(value >> 48);
            this.m_Buffer[this.m_Index + 7] = (byte)(value >> 56);
            this.m_Index += 8;
        }

        public override void Write(int value)
        {
            if ((this.m_Index + 4) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Buffer[this.m_Index + 2] = (byte)(value >> 16);
            this.m_Buffer[this.m_Index + 3] = (byte)(value >> 24);
            this.m_Index += 4;
        }

        public override void Write(uint value)
        {
            if ((this.m_Index + 4) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Buffer[this.m_Index + 2] = (byte)(value >> 16);
            this.m_Buffer[this.m_Index + 3] = (byte)(value >> 24);
            this.m_Index += 4;
        }

        public override void Write(short value)
        {
            if ((this.m_Index + 2) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Index += 2;
        }

        public override void Write(ushort value)
        {
            if ((this.m_Index + 2) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index] = (byte)value;
            this.m_Buffer[this.m_Index + 1] = (byte)(value >> 8);
            this.m_Index += 2;
        }

        public unsafe override void Write(double value)
        {
            if ((this.m_Index + 8) > this.m_Buffer.Length)
                this.Flush();

            fixed (byte* pBuffer = this.m_Buffer)
                *((double*)(pBuffer + this.m_Index)) = value;

            this.m_Index += 8;
        }

        public unsafe override void Write(float value)
        {
            if ((this.m_Index + 4) > this.m_Buffer.Length)
                this.Flush();

            fixed (byte* pBuffer = this.m_Buffer)
                *((float*)(pBuffer + this.m_Index)) = value;

            this.m_Index += 4;
        }

        private readonly char[] m_SingleCharBuffer = new char[1];

        public override void Write(char value)
        {
            if ((this.m_Index + 8) > this.m_Buffer.Length)
                this.Flush();

            this.m_SingleCharBuffer[0] = value;

            int byteCount = this.m_Encoding.GetBytes(this.m_SingleCharBuffer, 0, 1, this.m_Buffer, this.m_Index);
            this.m_Index += byteCount;
        }

        public override void Write(byte value)
        {
            if ((this.m_Index + 1) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index++] = value;
        }

        public override void Write(sbyte value)
        {
            if ((this.m_Index + 1) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index++] = (byte)value;
        }

        public override void Write(bool value)
        {
            if ((this.m_Index + 1) > this.m_Buffer.Length)
                this.Flush();

            this.m_Buffer[this.m_Index++] = (byte)(value ? 1 : 0);
        }

        public override void Write(Point3D value)
        {
            this.Write(value.m_X);
            this.Write(value.m_Y);
            this.Write(value.m_Z);
        }

        public override void Write(Point2D value)
        {
            this.Write(value.m_X);
            this.Write(value.m_Y);
        }

        public override void Write(Rectangle2D value)
        {
            this.Write(value.Start);
            this.Write(value.End);
        }

        public override void Write(Rectangle3D value)
        {
            this.Write(value.Start);
            this.Write(value.End);
        }

        public override void Write(Map value)
        {
            if (value != null)
                this.Write((byte)value.MapIndex);
            else
                this.Write((byte)0xFF);
        }

        public override void Write(Race value)
        {
            if (value != null)
                this.Write((byte)value.RaceIndex);
            else
                this.Write((byte)0xFF);
        }

        public override void Write(Item value)
        {
            if (value == null || value.Deleted)
                this.Write(Serial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(Mobile value)
        {
            if (value == null || value.Deleted)
                this.Write(Serial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseGuild value)
        {
            if (value == null)
                this.Write(0);
            else
                this.Write(value.Id);
        }

        public override void Write(BaseCore value)
        {
            if (value == null || value.Deleted)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseModule value)
        {
            if (value == null || value.Deleted)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseService value)
        {
            if (value == null)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void WriteItem<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteMobile<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteGuild<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteCore<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteModule<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteService<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteMobileList(ArrayList list)
        {
            this.WriteMobileList(list, false);
        }

        public override void WriteMobileList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((Mobile)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((Mobile)list[i]);
        }

        public override void WriteItemList(ArrayList list)
        {
            this.WriteItemList(list, false);
        }

        public override void WriteItemList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((Item)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((Item)list[i]);
        }

        public override void WriteGuildList(ArrayList list)
        {
            this.WriteGuildList(list, false);
        }

        public override void WriteGuildList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseGuild)list[i]).Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseGuild)list[i]);
        }

        public override void WriteCoreList(ArrayList list)
        {
            this.WriteCoreList(list, false);
        }

        public override void WriteCoreList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseCore)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseCore)list[i]);
        }

        public override void WriteModuleList(ArrayList list)
        {
            this.WriteModuleList(list, false);
        }

        public override void WriteModuleList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseModule)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseModule)list[i]);
        }

        public override void WriteServiceList(ArrayList list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseService)list[i]);
        }

        public override void Write(List<Item> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<Item> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteItemList<T>(List<T> list)
        {
            this.WriteItemList<T>(list, false);
        }

        public override void WriteItemList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<Item> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<Item> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(item => item.Deleted);
            }

            this.Write(set.Count);

            foreach (Item item in set)
            {
                this.Write(item);
            }
        }

        public override void WriteItemSet<T>(HashSet<T> set)
        {
            this.WriteItemSet(set, false);
        }

        public override void WriteItemSet<T>(HashSet<T> set, bool tidy) 
        {
            if (tidy)
            {
                set.RemoveWhere(item => item.Deleted);
            }

            this.Write(set.Count);

            foreach (Item item in set)
            {
                this.Write(item);
            }
        }

        public override void Write(List<Mobile> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<Mobile> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteMobileList<T>(List<T> list)
        {
            this.WriteMobileList<T>(list, false);
        }

        public override void WriteMobileList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<Mobile> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<Mobile> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(mobile => mobile.Deleted);
            }

            this.Write(set.Count);

            foreach (Mobile mob in set)
            {
                this.Write(mob);
            }
        }

        public override void WriteMobileSet<T>(HashSet<T> set)
        {
            this.WriteMobileSet(set, false);
        }

        public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(mob => mob.Deleted);
            }

            this.Write(set.Count);

            foreach (Mobile mob in set)
            {
                this.Write(mob);
            }
        }

        public override void Write(List<BaseGuild> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseGuild> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteGuildList<T>(List<T> list)
        {
            this.WriteGuildList<T>(list, false);
        }

        public override void WriteGuildList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseGuild> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseGuild> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(guild => guild.Disbanded);
            }

            this.Write(set.Count);

            foreach (BaseGuild guild in set)
            {
                this.Write(guild);
            }
        }

        public override void WriteGuildSet<T>(HashSet<T> set)
        {
            this.WriteGuildSet(set, false);
        }

        public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(guild => guild.Disbanded);
            }

            this.Write(set.Count);

            foreach (BaseGuild guild in set)
            {
                this.Write(guild);
            }
        }

        public override void Write(List<BaseCore> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseCore> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteCoreList<T>(List<T> list)
        {
            this.WriteCoreList<T>(list, false);
        }

        public override void WriteCoreList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseCore> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseCore> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(core => core.Deleted);

            this.Write(set.Count);

            foreach (BaseCore core in set)
                this.Write(core);
        }

        public override void WriteCoreSet<T>(HashSet<T> set)
        {
            this.WriteCoreSet(set, false);
        }

        public override void WriteCoreSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(core => core.Deleted);

            this.Write(set.Count);

            foreach (BaseCore core in set)
                this.Write(core);
        }

        public override void Write(List<BaseModule> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseModule> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteModuleList<T>(List<T> list)
        {
            this.WriteModuleList<T>(list, false);
        }

        public override void WriteModuleList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseModule> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseModule> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(module => module.Deleted);

            this.Write(set.Count);

            foreach (BaseModule module in set)
                this.Write(module);
        }

        public override void WriteModuleSet<T>(HashSet<T> set)
        {
            this.WriteModuleSet(set, false);
        }

        public override void WriteModuleSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(module => module.Deleted);

            this.Write(set.Count);

            foreach (BaseModule module in set)
                this.Write(module);
        }

        public override void Write(List<BaseService> list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteServiceList<T>(List<T> list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseService> set)
        {
            this.Write(set.Count);

            foreach (BaseService service in set)
                this.Write(service);
        }

        public override void WriteServiceSet<T>(HashSet<T> set)
        {
            this.Write(set.Count);

            foreach (BaseService service in set)
                this.Write(service);
        }
    }

    public sealed class BinaryFileReader : GenericReader
    {
        private readonly BinaryReader m_File;

        public BinaryFileReader(BinaryReader br)
        {
            this.m_File = br;
        }

        public void Close()
        {
            this.m_File.Close();
        }

        public long Position
        {
            get
            {
                return this.m_File.BaseStream.Position;
            }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return this.m_File.BaseStream.Seek(offset, origin);
        }

        public override string ReadString()
        {
            if (this.ReadByte() != 0)
                return this.m_File.ReadString();
            else
                return null;
        }

        public override DateTime ReadDeltaTime()
        {
            long ticks = this.m_File.ReadInt64();
            long now = DateTime.Now.Ticks;

            if (ticks > 0 && (ticks + now) < 0)
                return DateTime.MaxValue;
            else if (ticks < 0 && (ticks + now) < 0)
                return DateTime.MinValue;

            try
            {
                return new DateTime(now + ticks);
            }
            catch
            {
                if (ticks > 0)
                    return DateTime.MaxValue;
                else
                    return DateTime.MinValue;
            }
        }

        public override IPAddress ReadIPAddress()
        {
            return new IPAddress(this.m_File.ReadInt64());
        }

        public override int ReadEncodedInt()
        {
            int v = 0, shift = 0;
            byte b;

            do
            {
                b = this.m_File.ReadByte();
                v |= (b & 0x7F) << shift;
                shift += 7;
            }
            while (b >= 0x80);

            return v;
        }

        public override DateTime ReadDateTime()
        {
            return new DateTime(this.m_File.ReadInt64());
        }

        public override DateTimeOffset ReadDateTimeOffset()
        {
            long ticks = this.m_File.ReadInt64();
            TimeSpan offset = new TimeSpan(this.m_File.ReadInt64());

            return new DateTimeOffset(ticks, offset);
        }

        public override TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(this.m_File.ReadInt64());
        }

        public override decimal ReadDecimal()
        {
            return this.m_File.ReadDecimal();
        }

        public override long ReadLong()
        {
            return this.m_File.ReadInt64();
        }

        public override ulong ReadULong()
        {
            return this.m_File.ReadUInt64();
        }

        public override int ReadInt()
        {
            return this.m_File.ReadInt32();
        }

        public override uint ReadUInt()
        {
            return this.m_File.ReadUInt32();
        }

        public override short ReadShort()
        {
            return this.m_File.ReadInt16();
        }

        public override ushort ReadUShort()
        {
            return this.m_File.ReadUInt16();
        }

        public override double ReadDouble()
        {
            return this.m_File.ReadDouble();
        }

        public override float ReadFloat()
        {
            return this.m_File.ReadSingle();
        }

        public override char ReadChar()
        {
            return this.m_File.ReadChar();
        }

        public override byte ReadByte()
        {
            return this.m_File.ReadByte();
        }

        public override sbyte ReadSByte()
        {
            return this.m_File.ReadSByte();
        }

        public override bool ReadBool()
        {
            return this.m_File.ReadBoolean();
        }

        public override Point3D ReadPoint3D()
        {
            return new Point3D(this.ReadInt(), this.ReadInt(), this.ReadInt());
        }

        public override Point2D ReadPoint2D()
        {
            return new Point2D(this.ReadInt(), this.ReadInt());
        }

        public override Rectangle2D ReadRect2D()
        {
            return new Rectangle2D(this.ReadPoint2D(), this.ReadPoint2D());
        }

        public override Rectangle3D ReadRect3D()
        {
            return new Rectangle3D(this.ReadPoint3D(), this.ReadPoint3D());
        }

        public override Map ReadMap()
        {
            return Map.Maps[this.ReadByte()];
        }

        public override Item ReadItem()
        {
            return World.FindItem(this.ReadInt());
        }

        public override Mobile ReadMobile()
        {
            return World.FindMobile(this.ReadInt());
        }

        public override BaseGuild ReadGuild()
        {
            return BaseGuild.Find(this.ReadInt());
        }

        public override BaseCore ReadCore()
        {
            return World.GetCore(this.ReadInt());
        }

        public override BaseModule ReadModule()
        {
            return World.GetModule(this.ReadInt());
        }

        public override BaseService ReadService()
        {
            return World.GetService(this.ReadInt());
        }

        public override T ReadItem<T>()
        {
            return this.ReadItem() as T;
        }

        public override T ReadMobile<T>()
        {
            return this.ReadMobile() as T;
        }

        public override T ReadGuild<T>()
        {
            return this.ReadGuild() as T;
        }

        public override T ReadCore<T>()
        {
            return this.ReadCore() as T;
        }

        public override T ReadModule<T>()
        {
            return this.ReadModule() as T;
        }

        public override T ReadService<T>()
        {
            return this.ReadService() as T;
        }

        public override ArrayList ReadItemList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    Item item = this.ReadItem();

                    if (item != null)
                    {
                        list.Add(item);
                    }
                }

                return list;
            }
            else
            {
                return new ArrayList();
            }
        }

        public override ArrayList ReadMobileList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    Mobile m = this.ReadMobile();

                    if (m != null)
                    {
                        list.Add(m);
                    }
                }

                return list;
            }
            else
            {
                return new ArrayList();
            }
        }

        public override ArrayList ReadGuildList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    BaseGuild g = this.ReadGuild();

                    if (g != null)
                    {
                        list.Add(g);
                    }
                }

                return list;
            }
            else
            {
                return new ArrayList();
            }
        }

        public override ArrayList ReadCoreList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    BaseCore core = this.ReadCore();

                    if (core != null)
                        list.Add(core);
                }

                return list;
            }
            else
                return new ArrayList();
        }

        public override ArrayList ReadModuleList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    BaseModule module = this.ReadModule();

                    if (module != null)
                        list.Add(module);
                }

                return list;
            }
            else
                return new ArrayList();
        }

        public override ArrayList ReadServiceList()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                ArrayList list = new ArrayList(count);

                for (int i = 0; i < count; ++i)
                {
                    BaseService service = this.ReadService();

                    if (service != null)
                        list.Add(service);
                }

                return list;
            }
            else
                return new ArrayList();
        }

        public override List<Item> ReadStrongItemList()
        {
            return this.ReadStrongItemList<Item>();
        }

        public override List<T> ReadStrongItemList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T item = this.ReadItem() as T;

                    if (item != null)
                    {
                        list.Add(item);
                    }
                }

                return list;
            }
            else
            {
                return new List<T>();
            }
        }

        public override HashSet<Item> ReadItemSet()
        {
            return this.ReadItemSet<Item>();
        }

        public override HashSet<T> ReadItemSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T item = this.ReadItem() as T;

                    if (item != null)
                    {
                        set.Add(item);
                    }
                }

                return set;
            }
            else
            {
                return new HashSet<T>();
            }
        }

        public override List<Mobile> ReadStrongMobileList()
        {
            return this.ReadStrongMobileList<Mobile>();
        }

        public override List<T> ReadStrongMobileList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T m = this.ReadMobile() as T;

                    if (m != null)
                    {
                        list.Add(m);
                    }
                }

                return list;
            }
            else
            {
                return new List<T>();
            }
        }

        public override HashSet<Mobile> ReadMobileSet()
        {
            return this.ReadMobileSet<Mobile>();
        }

        public override HashSet<T> ReadMobileSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T item = this.ReadMobile() as T;

                    if (item != null)
                    {
                        set.Add(item);
                    }
                }

                return set;
            }
            else
            {
                return new HashSet<T>();
            }
        }

        public override List<BaseGuild> ReadStrongGuildList()
        {
            return this.ReadStrongGuildList<BaseGuild>();
        }

        public override List<T> ReadStrongGuildList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T g = this.ReadGuild() as T;

                    if (g != null)
                    {
                        list.Add(g);
                    }
                }

                return list;
            }
            else
            {
                return new List<T>();
            }
        }

        public override HashSet<BaseGuild> ReadGuildSet()
        {
            return this.ReadGuildSet<BaseGuild>();
        }

        public override HashSet<T> ReadGuildSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T item = this.ReadGuild() as T;

                    if (item != null)
                    {
                        set.Add(item);
                    }
                }

                return set;
            }
            else
            {
                return new HashSet<T>();
            }
        }

        public override List<BaseCore> ReadStrongCoreList()
        {
            return this.ReadStrongCoreList<BaseCore>();
        }

        public override List<T> ReadStrongCoreList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T core = this.ReadCore() as T;

                    if (core != null)
                        list.Add(core);
                }

                return list;
            }
            else
                return new List<T>();
        }

        public override HashSet<BaseCore> ReadCoreSet()
        {
            return this.ReadCoreSet<BaseCore>();
        }

        public override HashSet<T> ReadCoreSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T core = this.ReadCore() as T;

                    if (core != null)
                        set.Add(core);
                }

                return set;
            }
            else
                return new HashSet<T>();
        }

        public override List<BaseModule> ReadStrongModuleList()
        {
            return this.ReadStrongModuleList<BaseModule>();
        }

        public override List<T> ReadStrongModuleList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T module = this.ReadModule() as T;

                    if (module != null)
                        list.Add(module);
                }

                return list;
            }
            else
                return new List<T>();
        }

        public override HashSet<BaseModule> ReadModuleSet()
        {
            return this.ReadModuleSet<BaseModule>();
        }

        public override HashSet<T> ReadModuleSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T module = this.ReadModule() as T;

                    if (module != null)
                        set.Add(module);
                }

                return set;
            }
            else
                return new HashSet<T>();
        }

        public override List<BaseService> ReadStrongServiceList()
        {
            return this.ReadStrongServiceList<BaseService>();
        }

        public override List<T> ReadStrongServiceList<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                List<T> list = new List<T>(count);

                for (int i = 0; i < count; ++i)
                {
                    T service = this.ReadService() as T;

                    if (service != null)
                        list.Add(service);
                }

                return list;
            }
            else
                return new List<T>();
        }

        public override HashSet<BaseService> ReadServiceSet()
        {
            return this.ReadServiceSet<BaseService>();
        }

        public override HashSet<T> ReadServiceSet<T>()
        {
            int count = this.ReadInt();

            if (count > 0)
            {
                HashSet<T> set = new HashSet<T>();

                for (int i = 0; i < count; ++i)
                {
                    T service = this.ReadService() as T;

                    if (service != null)
                        set.Add(service);
                }
                
                return set;
            }
            else
                return new HashSet<T>();
        }

        public override Race ReadRace()
        {
            return Race.Races[this.ReadByte()];
        }

        public override bool End()
        {
            return this.m_File.PeekChar() == -1;
        }
    }

    public sealed class AsyncWriter : GenericWriter
    {
        private static int m_ThreadCount = 0;
        public static int ThreadCount
        {
            get
            {
                return m_ThreadCount;
            }
        }

        private readonly int BufferSize;

        private long m_LastPos, m_CurPos;
        private bool m_Closed;
        private readonly bool PrefixStrings;

        private MemoryStream m_Mem;
        private BinaryWriter m_Bin;
        private readonly FileStream m_File;

        private readonly Queue m_WriteQueue;
        private Thread m_WorkerThread;

        public AsyncWriter(string filename, bool prefix) : this(filename, 1048576, prefix)//1 mb buffer
        {
        }

        public AsyncWriter(string filename, int buffSize, bool prefix)
        {
            this.PrefixStrings = prefix;
            this.m_Closed = false;
            this.m_WriteQueue = Queue.Synchronized(new Queue());
            this.BufferSize = buffSize;

            this.m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            this.m_Mem = new MemoryStream(this.BufferSize + 1024);
            this.m_Bin = new BinaryWriter(this.m_Mem, Utility.UTF8WithEncoding);
        }

        private void Enqueue(MemoryStream mem)
        {
            this.m_WriteQueue.Enqueue(mem);

            if (this.m_WorkerThread == null || !this.m_WorkerThread.IsAlive)
            {
                this.m_WorkerThread = new Thread(new ThreadStart(new WorkerThread(this).Worker));
                this.m_WorkerThread.Priority = ThreadPriority.BelowNormal;
                this.m_WorkerThread.Start();
            }
        }

        private class WorkerThread
        {
            private readonly AsyncWriter m_Owner;

            public WorkerThread(AsyncWriter owner)
            {
                this.m_Owner = owner;
            }

            public void Worker()
            {
                AsyncWriter.m_ThreadCount++;
                while (this.m_Owner.m_WriteQueue.Count > 0)
                {
                    MemoryStream mem = (MemoryStream)this.m_Owner.m_WriteQueue.Dequeue();

                    if (mem != null && mem.Length > 0)
                        mem.WriteTo(this.m_Owner.m_File);
                }

                if (this.m_Owner.m_Closed)
                    this.m_Owner.m_File.Close();

                AsyncWriter.m_ThreadCount--;

                if (AsyncWriter.m_ThreadCount <= 0)
                    World.NotifyDiskWriteComplete();
            }
        }

        private void OnWrite()
        {
            long curlen = this.m_Mem.Length;
            this.m_CurPos += curlen - this.m_LastPos;
            this.m_LastPos = curlen;
            if (curlen >= this.BufferSize)
            {
                this.Enqueue(m_Mem);
                this.m_Mem = new MemoryStream(this.BufferSize + 1024);
                this.m_Bin = new BinaryWriter(this.m_Mem, Utility.UTF8WithEncoding);
                this.m_LastPos = 0;
            }
        }

        public MemoryStream MemStream
        {
            get
            {
                return this.m_Mem;
            }
            set
            {
                if (this.m_Mem.Length > 0)
                    this.Enqueue(m_Mem);

                this.m_Mem = value;
                this.m_Bin = new BinaryWriter(this.m_Mem, Utility.UTF8WithEncoding);
                this.m_LastPos = 0;
                this.m_CurPos = this.m_Mem.Length;
                this.m_Mem.Seek(0, SeekOrigin.End);
            }
        }

        public override void Close()
        {
            this.Enqueue(m_Mem);
            this.m_Closed = true;
        }

        public override long Position
        {
            get
            {
                return this.m_CurPos;
            }
        }

        public override void Write(IPAddress value)
        {
            this.m_Bin.Write(Utility.GetLongAddressValue(value));
            this.OnWrite();
        }

        public override void Write(string value)
        {
            if (this.PrefixStrings)
            {
                if (value == null)
                {
                    this.m_Bin.Write((byte)0);
                }
                else
                {
                    this.m_Bin.Write((byte)1);
                    this.m_Bin.Write(value);
                }
            }
            else
            {
                this.m_Bin.Write(value);
            }
            this.OnWrite();
        }

        public override void WriteDeltaTime(DateTime value)
        {
            long ticks = value.Ticks;
            long now = DateTime.Now.Ticks;

            TimeSpan d;

            try
            {
                d = new TimeSpan(ticks - now);
            }
            catch
            {
                d = TimeSpan.MaxValue;
            }

            this.Write(d);
        }

        public override void Write(DateTime value)
        {
            this.m_Bin.Write(value.Ticks);
            this.OnWrite();
        }

        public override void Write(DateTimeOffset value)
        {
            this.m_Bin.Write(value.Ticks);
            this.m_Bin.Write(value.Offset.Ticks);
            this.OnWrite();
        }

        public override void Write(TimeSpan value)
        {
            this.m_Bin.Write(value.Ticks);
            this.OnWrite();
        }

        public override void Write(decimal value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(long value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(ulong value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void WriteEncodedInt(int value)
        {
            uint v = (uint)value;

            while (v >= 0x80)
            {
                this.m_Bin.Write((byte)(v | 0x80));
                v >>= 7;
            }

            this.m_Bin.Write((byte)v);
            this.OnWrite();
        }

        public override void Write(int value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(uint value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(short value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(ushort value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(double value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(float value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(char value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(byte value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(sbyte value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(bool value)
        {
            this.m_Bin.Write(value);
            this.OnWrite();
        }

        public override void Write(Point3D value)
        {
            this.Write(value.m_X);
            this.Write(value.m_Y);
            this.Write(value.m_Z);
        }

        public override void Write(Point2D value)
        {
            this.Write(value.m_X);
            this.Write(value.m_Y);
        }

        public override void Write(Rectangle2D value)
        {
            this.Write(value.Start);
            this.Write(value.End);
        }

        public override void Write(Rectangle3D value)
        {
            this.Write(value.Start);
            this.Write(value.End);
        }

        public override void Write(Map value)
        {
            if (value != null)
                this.Write((byte)value.MapIndex);
            else
                this.Write((byte)0xFF);
        }

        public override void Write(Race value)
        {
            if (value != null)
                this.Write((byte)value.RaceIndex);
            else
                this.Write((byte)0xFF);
        }

        public override void Write(Item value)
        {
            if (value == null || value.Deleted)
                this.Write(Serial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(Mobile value)
        {
            if (value == null || value.Deleted)
                this.Write(Serial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseGuild value)
        {
            if (value == null)
                this.Write(0);
            else
                this.Write(value.Id);
        }

        public override void Write(BaseCore value)
        {
            if (value == null || value.Deleted)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseModule value)
        {
            if (value == null || value.Deleted)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void Write(BaseService value)
        {
            if (value == null)
                this.Write(CustomSerial.MinusOne);
            else
                this.Write(value.Serial);
        }

        public override void WriteItem<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteMobile<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteGuild<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteCore<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteModule<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteService<T>(T value)
        {
            this.Write(value);
        }

        public override void WriteMobileList(ArrayList list)
        {
            this.WriteMobileList(list, false);
        }

        public override void WriteMobileList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((Mobile)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((Mobile)list[i]);
        }

        public override void WriteItemList(ArrayList list)
        {
            this.WriteItemList(list, false);
        }

        public override void WriteItemList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((Item)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((Item)list[i]);
        }

        public override void WriteGuildList(ArrayList list)
        {
            this.WriteGuildList(list, false);
        }

        public override void WriteGuildList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseGuild)list[i]).Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseGuild)list[i]);
        }

        public override void WriteCoreList(ArrayList list)
        {
            this.WriteCoreList(list, false);
        }

        public override void WriteCoreList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseCore)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseCore)list[i]);
        }

        public override void WriteModuleList(ArrayList list)
        {
            this.WriteModuleList(list, false);
        }

        public override void WriteModuleList(ArrayList list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (((BaseModule)list[i]).Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseModule)list[i]);
        }

        public override void WriteServiceList(ArrayList list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write((BaseService)list[i]);
        }

        public override void Write(List<Item> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<Item> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteItemList<T>(List<T> list)
        {
            this.WriteItemList<T>(list, false);
        }

        public override void WriteItemList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<Item> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<Item> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(item => item.Deleted);
            }

            this.Write(set.Count);

            foreach (Item item in set)
            {
                this.Write(item);
            }
        }

        public override void WriteItemSet<T>(HashSet<T> set)
        {
            this.WriteItemSet(set, false);
        }

        public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(item => item.Deleted);
            }

            this.Write(set.Count);

            foreach (Item item in set)
            {
                this.Write(item);
            }
        }

        public override void Write(List<Mobile> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<Mobile> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteMobileList<T>(List<T> list)
        {
            this.WriteMobileList<T>(list, false);
        }

        public override void WriteMobileList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<Mobile> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<Mobile> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(mobile => mobile.Deleted);
            }

            this.Write(set.Count);

            foreach (Mobile mob in set)
            {
                this.Write(mob);
            }
        }

        public override void WriteMobileSet<T>(HashSet<T> set)
        {
            this.WriteMobileSet(set, false);
        }

        public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(mob => mob.Deleted);
            }

            this.Write(set.Count);

            foreach (Mobile mob in set)
            {
                this.Write(mob);
            }
        }

        public override void Write(List<BaseGuild> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseGuild> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteGuildList<T>(List<T> list)
        {
            this.WriteGuildList<T>(list, false);
        }

        public override void WriteGuildList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Disbanded)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseGuild> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseGuild> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(guild => guild.Disbanded);
            }

            this.Write(set.Count);

            foreach (BaseGuild guild in set)
            {
                this.Write(guild);
            }
        }

        public override void WriteGuildSet<T>(HashSet<T> set)
        {
            this.WriteGuildSet(set, false);
        }

        public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
            {
                set.RemoveWhere(guild => guild.Disbanded);
            }

            this.Write(set.Count);

            foreach (BaseGuild guild in set)
            {
                this.Write(guild);
            }
        }

        public override void Write(List<BaseCore> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseCore> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteCoreList<T>(List<T> list)
        {
            this.WriteCoreList<T>(list, false);
        }

        public override void WriteCoreList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseCore> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseCore> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(core => core.Deleted);

            this.Write(set.Count);

            foreach (BaseCore core in set)
                this.Write(core);
        }

        public override void WriteCoreSet<T>(HashSet<T> set)
        {
            this.WriteCoreSet(set, false);
        }

        public override void WriteCoreSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(core => core.Deleted);

            this.Write(set.Count);

            foreach (BaseCore core in set)
                this.Write(core);
        }

        public override void Write(List<BaseModule> list)
        {
            this.Write(list, false);
        }

        public override void Write(List<BaseModule> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteModuleList<T>(List<T> list)
        {
            this.WriteModuleList<T>(list, false);
        }

        public override void WriteModuleList<T>(List<T> list, bool tidy)
        {
            if (tidy)
            {
                for (int i = 0; i < list.Count;)
                {
                    if (list[i].Deleted)
                        list.RemoveAt(i);
                    else
                        ++i;
                }
            }

            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseModule> set)
        {
            this.Write(set, false);
        }

        public override void Write(HashSet<BaseModule> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(module => module.Deleted);

            this.Write(set.Count);

            foreach (BaseModule module in set)
                this.Write(module);
        }

        public override void WriteModuleSet<T>(HashSet<T> set)
        {
            this.WriteModuleSet(set, false);
        }

        public override void WriteModuleSet<T>(HashSet<T> set, bool tidy)
        {
            if (tidy)
                set.RemoveWhere(module => module.Deleted);

            this.Write(set.Count);

            foreach (BaseModule module in set)
                this.Write(module);
        }

        public override void Write(List<BaseService> list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void WriteServiceList<T>(List<T> list)
        {
            this.Write(list.Count);

            for (int i = 0; i < list.Count; ++i)
                this.Write(list[i]);
        }

        public override void Write(HashSet<BaseService> set)
        {
            this.Write(set.Count);

            foreach (BaseService service in set)
                this.Write(service);
        }

        public override void WriteServiceSet<T>(HashSet<T> set)
        {
            this.Write(set.Count);

            foreach (BaseService service in set)
                this.Write(service);
        }
    }

    public interface ISerializable
    {
        int TypeReference { get; }
        int SerialIdentity { get; }
        void Serialize(GenericWriter writer);
    }
}