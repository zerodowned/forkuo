using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using CustomsFramework;
using Server.Guilds;
using Server.Network;

namespace Server
{
    public static class World
    {
        private static Dictionary<Serial, Mobile> m_Mobiles;
        private static Dictionary<Serial, Item> m_Items;
        private static Dictionary<CustomSerial, BaseCore> _Cores;
        private static Dictionary<CustomSerial, BaseModule> _Modules;
        private static Dictionary<CustomSerial, BaseService> _Services;

        private static bool m_Loading;
        private static bool m_Loaded;

        private static bool m_Saving;
        private static readonly ManualResetEvent m_DiskWriteHandle = new ManualResetEvent(true);

        private static Queue<IEntity> _addQueue, _deleteQueue;
        private static Queue<ICustomsEntity> _CustomsAddQueue, _CustomsDeleteQueue;

        public static bool Saving
        {
            get
            {
                return m_Saving;
            }
        }
        public static bool Loaded
        {
            get
            {
                return m_Loaded;
            }
        }
        public static bool Loading
        {
            get
            {
                return m_Loading;
            }
        }

        public readonly static string MobileIndexPath = Path.Combine("Saves/Mobiles/", "Mobiles.idx");
        public readonly static string MobileTypesPath = Path.Combine("Saves/Mobiles/", "Mobiles.tdb");
        public readonly static string MobileDataPath = Path.Combine("Saves/Mobiles/", "Mobiles.bin");

        public readonly static string ItemIndexPath = Path.Combine("Saves/Items/", "Items.idx");
        public readonly static string ItemTypesPath = Path.Combine("Saves/Items/", "Items.tdb");
        public readonly static string ItemDataPath = Path.Combine("Saves/Items/", "Items.bin");

        public readonly static string GuildIndexPath = Path.Combine("Saves/Guilds/", "Guilds.idx");
        public readonly static string GuildDataPath = Path.Combine("Saves/Guilds/", "Guilds.bin");

        public readonly static string CoreIndexPath = Path.Combine("Saves/Cores/", "Cores.idx");
        public readonly static string CoreTypesPath = Path.Combine("Saves/Cores/", "Cores.tdb");
        public readonly static string CoresDataPath = Path.Combine("Saves/Cores/", "Cores.bin");

        public readonly static string ModuleIndexPath = Path.Combine("Saves/Modules/", "Modules.idx");
        public readonly static string ModuleTypesPath = Path.Combine("Saves/Modules/", "Modules.tdb");
        public readonly static string ModulesDataPath = Path.Combine("Saves/Modules/", "Modules.bin");

        public readonly static string ServiceIndexPath = Path.Combine("Saves/Services/", "Services.idx");
        public readonly static string ServiceTypesPath = Path.Combine("Saves/Services/", "Services.tdb");
        public readonly static string ServicesDataPath = Path.Combine("Saves/Services/", "Services.bin");

        public static void NotifyDiskWriteComplete()
        {
            if (m_DiskWriteHandle.Set())
            {
                Console.WriteLine("Closing Save Files. ");
            }
        }

        public static void WaitForWriteCompletion()
        {
            m_DiskWriteHandle.WaitOne();
        }

        public static Dictionary<Serial, Mobile> Mobiles
        {
            get
            {
                return m_Mobiles;
            }
        }

        public static Dictionary<Serial, Item> Items
        {
            get
            {
                return m_Items;
            }
        }

        public static Dictionary<CustomSerial, BaseCore> Cores
        {
            get
            {
                return _Cores;
            }
        }

        public static Dictionary<CustomSerial, BaseModule> Modules
        {
            get
            {
                return _Modules;
            }
        }

        public static Dictionary<CustomSerial, BaseService> Services
        {
            get
            {
                return _Services;
            }
        }

        public static bool OnDelete(IEntity entity)
        {
            if (m_Saving || m_Loading)
            {
                if (m_Saving)
                {
                    AppendSafetyLog("delete", entity);
                }

                _deleteQueue.Enqueue(entity);

                return false;
            }

            return true;
        }

        public static bool OnDelete(ICustomsEntity entity)
        {
            if (m_Saving || m_Loading)
            {
                if (m_Saving)
                    AppendSafetyLog("delete", entity);

                _CustomsDeleteQueue.Enqueue(entity);

                return false;
            }

            return true;
        }

        public static void Broadcast(int hue, bool ascii, string text)
        {
            Packet p;

            if (ascii)
                p = new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text);
            else
                p = new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text);

            List<NetState> list = NetState.Instances;

            p.Acquire();

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Mobile != null)
                    list[i].Send(p);
            }

            p.Release();

            NetState.FlushAll();
        }

        public static void Broadcast(int hue, bool ascii, string format, params object[] args)
        {
            Broadcast(hue, ascii, String.Format(format, args));
        }

        private interface IEntityEntry
        {
            Serial Serial { get; }
            int TypeID { get; }
            long Position { get; }
            int Length { get; }
        }

        private sealed class GuildEntry : IEntityEntry
        {
            private readonly BaseGuild m_Guild;
            private readonly long m_Position;
            private readonly int m_Length;

            public BaseGuild Guild
            {
                get
                {
                    return this.m_Guild;
                }
            }

            public Serial Serial
            {
                get
                {
                    return this.m_Guild == null ? 0 : this.m_Guild.Id;
                }
            }

            public int TypeID
            {
                get
                {
                    return 0;
                }
            }

            public long Position
            {
                get
                {
                    return this.m_Position;
                }
            }

            public int Length
            {
                get
                {
                    return this.m_Length;
                }
            }

            public GuildEntry(BaseGuild g, long pos, int length)
            {
                this.m_Guild = g;
                this.m_Position = pos;
                this.m_Length = length;
            }
        }

        private sealed class ItemEntry : IEntityEntry
        {
            private readonly Item m_Item;
            private readonly int m_TypeID;
            private readonly string m_TypeName;
            private readonly long m_Position;
            private readonly int m_Length;

            public Item Item
            {
                get
                {
                    return this.m_Item;
                }
            }

            public Serial Serial
            {
                get
                {
                    return this.m_Item == null ? Serial.MinusOne : this.m_Item.Serial;
                }
            }

            public int TypeID
            {
                get
                {
                    return this.m_TypeID;
                }
            }

            public string TypeName
            {
                get
                {
                    return this.m_TypeName;
                }
            }

            public long Position
            {
                get
                {
                    return this.m_Position;
                }
            }

            public int Length
            {
                get
                {
                    return this.m_Length;
                }
            }

            public ItemEntry(Item item, int typeID, string typeName, long pos, int length)
            {
                this.m_Item = item;
                this.m_TypeID = typeID;
                this.m_TypeName = typeName;
                this.m_Position = pos;
                this.m_Length = length;
            }
        }

        private sealed class MobileEntry : IEntityEntry
        {
            private readonly Mobile m_Mobile;
            private readonly int m_TypeID;
            private readonly string m_TypeName;
            private readonly long m_Position;
            private readonly int m_Length;

            public Mobile Mobile
            {
                get
                {
                    return this.m_Mobile;
                }
            }

            public Serial Serial
            {
                get
                {
                    return this.m_Mobile == null ? Serial.MinusOne : this.m_Mobile.Serial;
                }
            }

            public int TypeID
            {
                get
                {
                    return this.m_TypeID;
                }
            }

            public string TypeName
            {
                get
                {
                    return this.m_TypeName;
                }
            }

            public long Position
            {
                get
                {
                    return this.m_Position;
                }
            }

            public int Length
            {
                get
                {
                    return this.m_Length;
                }
            }

            public MobileEntry(Mobile mobile, int typeID, string typeName, long pos, int length)
            {
                this.m_Mobile = mobile;
                this.m_TypeID = typeID;
                this.m_TypeName = typeName;
                this.m_Position = pos;
                this.m_Length = length;
            }
        }

        public sealed class CoreEntry : ICustomsEntry
        {
            private readonly BaseCore _Core;
            private readonly int _TypeID;
            private readonly string _TypeName;
            private readonly long _Position;
            private readonly int _Length;

            public BaseCore Core
            {
                get
                {
                    return this._Core;
                }
            }

            public CustomSerial Serial
            {
                get
                {
                    return this._Core == null ? CustomSerial.MinusOne : this._Core.Serial;
                }
            }

            public int TypeID
            {
                get
                {
                    return this._TypeID;
                }
            }

            public string TypeName
            {
                get
                {
                    return this._TypeName;
                }
            }

            public long Position
            {
                get
                {
                    return this._Position;
                }
            }

            public int Length
            {
                get
                {
                    return this._Length;
                }
            }

            public CoreEntry(BaseCore core, int typeID, string typeName, long pos, int length)
            {
                this._Core = core;
                this._TypeID = typeID;
                this._TypeName = typeName;
                this._Position = pos;
                this._Length = length;
            }
        }

        private sealed class ModuleEntry : ICustomsEntry
        {
            private readonly BaseModule _Module;
            private readonly int _TypeID;
            private readonly string _TypeName;
            private readonly long _Position;
            private readonly int _Length;

            public BaseModule Module
            {
                get
                {
                    return this._Module;
                }
            }

            public CustomSerial Serial
            {
                get
                {
                    return _Modules == null ? CustomSerial.MinusOne : this._Module.Serial;
                }
            }

            public int TypeID
            {
                get
                {
                    return this._TypeID;
                }
            }

            public string TypeName
            {
                get
                {
                    return this._TypeName;
                }
            }

            public long Position
            {
                get
                {
                    return this._Position;
                }
            }

            public int Length
            {
                get
                {
                    return this._Length;
                }
            }

            public ModuleEntry(BaseModule module, int typeID, string typeName, long pos, int length)
            {
                this._Module = module;
                this._TypeID = typeID;
                this._TypeName = typeName;
                this._Position = pos;
                this._Length = length;
            }
        }

        private sealed class ServiceEntry : ICustomsEntry
        {
            private readonly BaseService _Service;
            private readonly int _TypeID;
            private readonly string _TypeName;
            private readonly long _Position;
            private readonly int _Length;

            public BaseService Service
            {
                get
                {
                    return this._Service;
                }
            }

            public CustomSerial Serial
            {
                get
                {
                    return _Services == null ? CustomSerial.MinusOne : this._Service.Serial;
                }
            }

            public int TypeID
            {
                get
                {
                    return this._TypeID;
                }
            }

            public string TypeName
            {
                get
                {
                    return this._TypeName;
                }
            }

            public long Position
            {
                get
                {
                    return this._Position;
                }
            }

            public int Length
            {
                get
                {
                    return this._Length;
                }
            }

            public ServiceEntry(BaseService service, int typeID, string typeName, long pos, int length)
            {
                this._Service = service;
                this._TypeID = typeID;
                this._TypeName = typeName;
                this._Position = pos;
                this._Length = length;
            }
        }

        private static string m_LoadingType;

        public static string LoadingType
        {
            get
            {
                return m_LoadingType;
            }
        }

        private static readonly Type[] _CustomSerialTypeArray = new Type[1] { typeof(CustomSerial) };
        private static readonly Type[] m_SerialTypeArray = new Type[1] { typeof(Serial) };

        private static List<Tuple<ConstructorInfo, string>> ReadTypes(BinaryReader tdbReader)
        {
            int count = tdbReader.ReadInt32();

            List<Tuple<ConstructorInfo, string>> types = new List<Tuple<ConstructorInfo, string>>(count);

            for (int i = 0; i < count; ++i)
            {
                string typeName = tdbReader.ReadString();

                Type t = ScriptCompiler.FindTypeByFullName(typeName);

                if (t == null)
                {
                    Console.WriteLine("failed");

                    if (!Core.Service)
                    {
                        Console.WriteLine("Error: Type '{0}' was not found. Delete all of those types? (y/n)", typeName);

                        if (Console.ReadKey(true).Key == ConsoleKey.Y)
                        {
                            types.Add(null);
                            Console.Write("World: Loading...");
                            continue;
                        }

                        Console.WriteLine("Types will not be deleted. An exception will be thrown.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Type '{0}' was not found.", typeName);
                    }

                    throw new Exception(String.Format("Bad type '{0}'", typeName));
                }

                ConstructorInfo ctor = t.GetConstructor(m_SerialTypeArray);

                if (ctor != null)
                {
                    types.Add(new Tuple<ConstructorInfo, string>(ctor, typeName));
                }
                else
                {
                    throw new Exception(String.Format("Type '{0}' does not have a serialization constructor", t));
                }
            }

            return types;
        }

        public static void Load()
        {
            if (m_Loaded)
                return;

            m_Loaded = true;
            m_LoadingType = null;

            Console.Write("World: Loading...");

            Stopwatch watch = Stopwatch.StartNew();

            m_Loading = true;

            _addQueue = new Queue<IEntity>();
            _deleteQueue = new Queue<IEntity>();
            _CustomsAddQueue = new Queue<ICustomsEntity>();
            _CustomsDeleteQueue = new Queue<ICustomsEntity>();

            int mobileCount = 0, itemCount = 0, guildCount = 0, coreCount = 0, moduleCount = 0, serviceCount = 0;

            object[] ctorArgs = new object[1];

            List<ItemEntry> items = new List<ItemEntry>();
            List<MobileEntry> mobiles = new List<MobileEntry>();
            List<GuildEntry> guilds = new List<GuildEntry>();
            List<CoreEntry> cores = new List<CoreEntry>();
            List<ModuleEntry> modules = new List<ModuleEntry>();
            List<ServiceEntry> services = new List<ServiceEntry>();

            if (File.Exists(MobileIndexPath) && File.Exists(MobileTypesPath))
            {
                using (FileStream idx = new FileStream(MobileIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader idxReader = new BinaryReader(idx);

                    using (FileStream tdb = new FileStream(MobileTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader tdbReader = new BinaryReader(tdb);

                        List<Tuple<ConstructorInfo, string>> types = ReadTypes(tdbReader);

                        mobileCount = idxReader.ReadInt32();

                        m_Mobiles = new Dictionary<Serial, Mobile>(mobileCount);

                        for (int i = 0; i < mobileCount; ++i)
                        {
                            int typeID = idxReader.ReadInt32();
                            int serial = idxReader.ReadInt32();
                            long pos = idxReader.ReadInt64();
                            int length = idxReader.ReadInt32();

                            Tuple<ConstructorInfo, string> objs = types[typeID];

                            if (objs == null)
                                continue;

                            Mobile m = null;
                            ConstructorInfo ctor = objs.Item1;
                            string typeName = objs.Item2;

                            try
                            {
                                ctorArgs[0] = (Serial)serial;
                                m = (Mobile)(ctor.Invoke(ctorArgs));
                            }
                            catch
                            {
                            }

                            if (m != null)
                            {
                                mobiles.Add(new MobileEntry(m, typeID, typeName, pos, length));
                                AddMobile(m);
                            }
                        }

                        tdbReader.Close();
                    }

                    idxReader.Close();
                }
            }
            else
            {
                m_Mobiles = new Dictionary<Serial, Mobile>();
            }

            if (File.Exists(ItemIndexPath) && File.Exists(ItemTypesPath))
            {
                using (FileStream idx = new FileStream(ItemIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader idxReader = new BinaryReader(idx);

                    using (FileStream tdb = new FileStream(ItemTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader tdbReader = new BinaryReader(tdb);

                        List<Tuple<ConstructorInfo, string>> types = ReadTypes(tdbReader);

                        itemCount = idxReader.ReadInt32();

                        m_Items = new Dictionary<Serial, Item>(itemCount);

                        for (int i = 0; i < itemCount; ++i)
                        {
                            int typeID = idxReader.ReadInt32();
                            int serial = idxReader.ReadInt32();
                            long pos = idxReader.ReadInt64();
                            int length = idxReader.ReadInt32();

                            Tuple<ConstructorInfo, string> objs = types[typeID];

                            if (objs == null)
                                continue;

                            Item item = null;
                            ConstructorInfo ctor = objs.Item1;
                            string typeName = objs.Item2;

                            try
                            {
                                ctorArgs[0] = (Serial)serial;
                                item = (Item)(ctor.Invoke(ctorArgs));
                            }
                            catch
                            {
                            }

                            if (item != null)
                            {
                                items.Add(new ItemEntry(item, typeID, typeName, pos, length));
                                AddItem(item);
                            }
                        }

                        tdbReader.Close();
                    }

                    idxReader.Close();
                }
            }
            else
            {
                m_Items = new Dictionary<Serial, Item>();
            }

            if (File.Exists(GuildIndexPath))
            {
                using (FileStream idx = new FileStream(GuildIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader idxReader = new BinaryReader(idx);

                    guildCount = idxReader.ReadInt32();

                    CreateGuildEventArgs createEventArgs = new CreateGuildEventArgs(-1);
                    for (int i = 0; i < guildCount; ++i)
                    {
                        idxReader.ReadInt32();//no typeid for guilds
                        int id = idxReader.ReadInt32();
                        long pos = idxReader.ReadInt64();
                        int length = idxReader.ReadInt32();

                        createEventArgs.Id = id;
                        BaseGuild guild = EventSink.InvokeCreateGuild(createEventArgs);
                        if (guild != null)
                            guilds.Add(new GuildEntry(guild, pos, length));
                    }

                    idxReader.Close();
                }
            }

            if (File.Exists(CoreIndexPath) && File.Exists(CoreTypesPath))
            {
                using (FileStream indexStream = new FileStream(CoreIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader indexReader = new BinaryReader(indexStream);

                    using (FileStream typeStream = new FileStream(CoreTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader typeReader = new BinaryReader(typeStream);

                        List<Tuple<ConstructorInfo, string>> types = ReadTypes(typeReader);

                        coreCount = indexReader.ReadInt32();
                        _Cores = new Dictionary<CustomSerial, BaseCore>(coreCount);

                        for (int i = 0; i < coreCount; ++i)
                        {
                            int typeID = indexReader.ReadInt32();
                            int serial = indexReader.ReadInt32();
                            long pos = indexReader.ReadInt64();
                            int length = indexReader.ReadInt32();

                            Tuple<ConstructorInfo, string> objects = types[typeID];

                            if (objects == null)
                                continue;

                            BaseCore core = null;
                            ConstructorInfo ctor = objects.Item1;
                            string typeName = objects.Item2;

                            try
                            {
                                ctorArgs[0] = (CustomSerial)serial;
                                core = (BaseCore)(ctor.Invoke(ctorArgs));
                            }
                            catch
                            {
                                Console.WriteLine("Error loading {0}, Serial: {1}", typeName, serial);
                            }

                            if (core != null)
                            {
                                cores.Add(new CoreEntry(core, typeID, typeName, pos, length));
                                AddCore(core);
                            }
                        }

                        typeReader.Close();
                    }

                    indexReader.Close();
                }
            }
            else
                _Cores = new Dictionary<CustomSerial, BaseCore>();

            if (File.Exists(ModuleIndexPath) && File.Exists(ModuleTypesPath))
            {
                using (FileStream indexStream = new FileStream(ModuleIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader indexReader = new BinaryReader(indexStream);

                    using (FileStream typeStream = new FileStream(ModuleTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader typeReader = new BinaryReader(typeStream);

                        List<Tuple<ConstructorInfo, string>> types = ReadTypes(typeReader);

                        moduleCount = indexReader.ReadInt32();

                        for (int i = 0; i < moduleCount; ++i)
                        {
                            int typeID = indexReader.ReadInt32();
                            int serial = indexReader.ReadInt32();
                            long pos = indexReader.ReadInt64();
                            int length = indexReader.ReadInt32();

                            Tuple<ConstructorInfo, string> objects = types[typeID];

                            if (objects == null)
                                continue;

                            BaseModule module = null;
                            ConstructorInfo ctor = objects.Item1;
                            string typeName = objects.Item2;

                            try
                            {
                                ctorArgs[0] = (CustomSerial)serial;
                                module = (BaseModule)(ctor.Invoke(ctorArgs));
                            }
                            catch
                            {
                            }

                            if (module != null)
                            {
                                modules.Add(new ModuleEntry(module, typeID, typeName, pos, length));
                                AddModule(module);
                            }
                        }

                        typeReader.Close();
                    }

                    indexReader.Close();
                }
            }
            else
                _Modules = new Dictionary<CustomSerial, BaseModule>();

            if (File.Exists(ServiceIndexPath) && File.Exists(ServiceTypesPath))
            {
                using (FileStream indexStream = new FileStream(ServiceIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader indexReader = new BinaryReader(indexStream);

                    using (FileStream typeStream = new FileStream(ServiceTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader typeReader = new BinaryReader(typeStream);

                        List<Tuple<ConstructorInfo, string>> types = ReadTypes(typeReader);

                        serviceCount = indexReader.ReadInt32();

                        _Services = new Dictionary<CustomSerial, BaseService>(serviceCount);

                        for (int i = 0; i < serviceCount; ++i)
                        {
                            int typeID = indexReader.ReadInt32();
                            int serial = indexReader.ReadInt32();
                            long pos = indexReader.ReadInt64();
                            int length = indexReader.ReadInt32();

                            Tuple<ConstructorInfo, string> objects = types[typeID];

                            if (objects == null)
                                continue;

                            BaseService service = null;
                            ConstructorInfo ctor = objects.Item1;
                            string typeName = objects.Item2;

                            try
                            {
                                ctorArgs[0] = (CustomSerial)serial;
                                service = (BaseService)(ctor.Invoke(ctorArgs));
                            }
                            catch
                            {
                            }

                            if (service != null)
                            {
                                services.Add(new ServiceEntry(service, typeID, typeName, pos, length));
                                AddService(service);
                            }
                        }

                        typeReader.Close();
                    }

                    indexReader.Close();
                }
            }
            else
                _Services = new Dictionary<CustomSerial, BaseService>();

            bool failedMobiles = false, failedItems = false, failedGuilds = false, failedCores = false, failedModules = false, failedServices = false;
            Type failedType = null;
            Serial failedSerial = Serial.Zero;
            CustomSerial failedCustomSerial = CustomSerial.Zero;
            Exception failed = null;
            int failedTypeID = 0;

            if (File.Exists(MobileDataPath))
            {
                using (FileStream bin = new FileStream(MobileDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

                    for (int i = 0; i < mobiles.Count; ++i)
                    {
                        MobileEntry entry = mobiles[i];
                        Mobile m = entry.Mobile;

                        if (m != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                m_LoadingType = entry.TypeName;
                                m.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on {0} *****", m.GetType()));
                            }
                            catch (Exception e)
                            {
                                mobiles.RemoveAt(i);

                                failed = e;
                                failedMobiles = true;
                                failedType = m.GetType();
                                failedTypeID = entry.TypeID;
                                failedSerial = m.Serial;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            if (!failedMobiles && File.Exists(ItemDataPath))
            {
                using (FileStream bin = new FileStream(ItemDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

                    for (int i = 0; i < items.Count; ++i)
                    {
                        ItemEntry entry = items[i];
                        Item item = entry.Item;

                        if (item != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                m_LoadingType = entry.TypeName;
                                item.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on {0} *****", item.GetType()));
                            }
                            catch (Exception e)
                            {
                                items.RemoveAt(i);

                                failed = e;
                                failedItems = true;
                                failedType = item.GetType();
                                failedTypeID = entry.TypeID;
                                failedSerial = item.Serial;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            m_LoadingType = null;

            if (!failedMobiles && !failedItems && File.Exists(GuildDataPath))
            {
                using (FileStream bin = new FileStream(GuildDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

                    for (int i = 0; i < guilds.Count; ++i)
                    {
                        GuildEntry entry = guilds[i];
                        BaseGuild g = entry.Guild;

                        if (g != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                g.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on Guild {0} *****", g.Id));
                            }
                            catch (Exception e)
                            {
                                guilds.RemoveAt(i);

                                failed = e;
                                failedGuilds = true;
                                failedType = typeof(BaseGuild);
                                failedTypeID = g.Id;
                                failedSerial = g.Id;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            if (!failedMobiles && !failedItems && !failedGuilds && File.Exists(CoresDataPath))
            {
                using (FileStream stream = new FileStream(CoresDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));

                    for (int i = 0; i < cores.Count; ++i)
                    {
                        CoreEntry entry = cores[i];
                        BaseCore core = entry.Core;

                        if (core != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                m_LoadingType = entry.TypeName;
                                core.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on {0} *****", core.GetType()));
                            }
                            catch (Exception error)
                            {
                                cores.RemoveAt(i);

                                failed = error;
                                failedCores = true;
                                failedType = core.GetType();
                                failedTypeID = entry.TypeID;
                                failedCustomSerial = core.Serial;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            if (!failedMobiles && !failedItems && !failedGuilds && !failedCores && File.Exists(ModulesDataPath))
            {
                using (FileStream stream = new FileStream(ModulesDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));

                    for (int i = 0; i < modules.Count; ++i)
                    {
                        ModuleEntry entry = modules[i];
                        BaseModule module = entry.Module;

                        if (module != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                m_LoadingType = entry.TypeName;
                                module.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on {0} *****", module.GetType()));
                            }
                            catch (Exception error)
                            {
                                modules.RemoveAt(i);

                                failed = error;
                                failedModules = true;
                                failedType = module.GetType();
                                failedTypeID = entry.TypeID;
                                failedCustomSerial = module.Serial;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            if (!failedModules && !failedItems && !failedGuilds && !failedCores && !failedModules && File.Exists(ServicesDataPath))
            {
                using (FileStream stream = new FileStream(ServicesDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));

                    for (int i = 0; i < services.Count; ++i)
                    {
                        ServiceEntry entry = services[i];
                        BaseService service = entry.Service;

                        if (service != null)
                        {
                            reader.Seek(entry.Position, SeekOrigin.Begin);

                            try
                            {
                                m_LoadingType = entry.TypeName;
                                service.Deserialize(reader);

                                if (reader.Position != (entry.Position + entry.Length))
                                    throw new Exception(String.Format("***** Bad serialize on {0} *****", service.GetType()));
                            }
                            catch (Exception error)
                            {
                                services.RemoveAt(i);

                                failed = error;
                                failedServices = true;
                                failedType = services.GetType();
                                failedTypeID = entry.TypeID;
                                failedCustomSerial = service.Serial;

                                break;
                            }
                        }
                    }

                    reader.Close();
                }
            }

            if (failedItems || failedMobiles || failedGuilds || failedCores || failedModules || failedServices)
            {
                Console.WriteLine("An error was encountered while loading a saved object");

                Console.WriteLine(" - Type: {0}", failedType);

                if (failedSerial != Serial.Zero)
                    Console.WriteLine(" - Serial: {0}", failedSerial);
                else
                    Console.WriteLine(" - Serial: {0}", failedCustomSerial);

                if (!Core.Service)
                {
                    Console.WriteLine("Delete the object? (y/n)");

                    if (Console.ReadKey(true).Key == ConsoleKey.Y)
                    {
                        if (failedType != typeof(BaseGuild))
                        {
                            Console.WriteLine("Delete all objects of that type? (y/n)");

                            if (Console.ReadKey(true).Key == ConsoleKey.Y)
                            {
                                if (failedMobiles)
                                {
                                    for (int i = 0; i < mobiles.Count;)
                                    {
                                        if (mobiles[i].TypeID == failedTypeID)
                                            mobiles.RemoveAt(i);
                                        else
                                            ++i;
                                    }
                                }
                                else if (failedItems)
                                {
                                    for (int i = 0; i < items.Count;)
                                    {
                                        if (items[i].TypeID == failedTypeID)
                                            items.RemoveAt(i);
                                        else
                                            ++i;
                                    }
                                }
                                else if (failedCores)
                                {
                                    for (int i = 0; i < cores.Count;)
                                    {
                                        if (cores[i].TypeID == failedTypeID)
                                            cores.RemoveAt(i);
                                        else
                                            ++i;
                                    }
                                }
                                else if (failedModules)
                                {
                                    for (int i = 0; i < modules.Count;)
                                    {
                                        if (modules[i].TypeID == failedTypeID)
                                            modules.RemoveAt(i);
                                        else
                                            ++i;
                                    }
                                }
                                else if (failedServices)
                                {
                                    for (int i = 0; i < services.Count;)
                                    {
                                        if (services[i].TypeID == failedTypeID)
                                            services.RemoveAt(i);
                                        else
                                            ++i;
                                    }
                                }
                            }
                        }

                        SaveIndex<MobileEntry>(mobiles, MobileIndexPath);
                        SaveIndex<ItemEntry>(items, ItemIndexPath);
                        SaveIndex<GuildEntry>(guilds, GuildIndexPath);
                        SaveIndex<CoreEntry>(cores, CoreIndexPath);
                        SaveIndex<ModuleEntry>(modules, ModuleIndexPath);
                        SaveIndex<ServiceEntry>(services, ServiceIndexPath);
                    }

                    Console.WriteLine("After pressing return an exception will be thrown and the server will terminate.");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("An exception will be thrown and the server will terminate.");
                }

                throw new Exception(String.Format("Load failed (items={0}, mobiles={1}, guilds={2}, cores={3}, modules={4}, services={5} type={6}, serial={7})", failedItems, failedMobiles, failedGuilds, failedCores, failedModules, failedServices, failedType, (failedSerial != Serial.Zero ? failedSerial.ToString() : failedCustomSerial.ToString())), failed);
            }

            EventSink.InvokeWorldLoad();

            m_Loading = false;

            ProcessSafetyQueues();

            foreach (Item item in m_Items.Values)
            {
                if (item.Parent == null)
                    item.UpdateTotals();

                item.ClearProperties();
            }

            foreach (Mobile m in m_Mobiles.Values)
            {
                m.UpdateRegion(); // Is this really needed?
                m.UpdateTotals();

                m.ClearProperties();
            }

            foreach (BaseCore core in _Cores.Values)
                core.Prep();

            foreach (BaseModule module in _Modules.Values)
                module.Prep();

            foreach (BaseService service in _Services.Values)
                service.Prep();

            watch.Stop();

            Console.WriteLine("done ({1} items, {2} mobiles) ({0:F2} seconds)", watch.Elapsed.TotalSeconds, m_Items.Count, m_Mobiles.Count);
        }

        private static void ProcessSafetyQueues()
        {
            while (_addQueue.Count > 0)
            {
                IEntity entity = _addQueue.Dequeue();

                Item item = entity as Item;

                if (item != null)
                {
                    AddItem(item);
                }
                else
                {
                    Mobile mob = entity as Mobile;

                    if (mob != null)
                    {
                        AddMobile(mob);
                    }
                }
            }

            while (_deleteQueue.Count > 0)
            {
                IEntity entity = _deleteQueue.Dequeue();

                Item item = entity as Item;

                if (item != null)
                {
                    item.Delete();
                }
                else
                {
                    Mobile mob = entity as Mobile;

                    if (mob != null)
                    {
                        mob.Delete();
                    }
                }
            }

            while (_CustomsAddQueue.Count > 0)
            {
                ICustomsEntity entity = _CustomsAddQueue.Dequeue();

                BaseCore core = entity as BaseCore;

                if (core != null)
                    AddCore(core);
                else
                {
                    BaseModule module = entity as BaseModule;

                    if (module != null)
                        AddModule(module);
                    else
                    {
                        BaseService service = entity as BaseService;

                        if (service != null)
                            AddService(service);
                    }
                }
            }

            while (_CustomsDeleteQueue.Count > 0)
            {
                ICustomsEntity entity = _CustomsDeleteQueue.Dequeue();

                BaseCore core = entity as BaseCore;

                if (core != null)
                    core.Delete();
                else
                {
                    BaseModule module = entity as BaseModule;

                    if (module != null)
                        module.Delete();
                    else
                    {
                        BaseService service = entity as BaseService;

                        if (service != null)
                            service.Delete();
                    }
                }
            }
        }

        private static void AppendSafetyLog(string action, ICustomsEntity entity)
        {
            string message = String.Format("Warning: Attempted to {1} {2} during world save." +
                                           "{0}This action could cause inconsistent state." +
                                           "{0}It is strongly advised that the offending scripts be corrected.",
                Environment.NewLine,
                action, entity);

            AppendSafetyLog(message);
        }

        private static void AppendSafetyLog(string action, IEntity entity)
        {
            string message = String.Format("Warning: Attempted to {1} {2} during world save." +
                                           "{0}This action could cause inconsistent state." +
                                           "{0}It is strongly advised that the offending scripts be corrected.",
                Environment.NewLine,
                action, entity);

            AppendSafetyLog(message);
        }

        private static void AppendSafetyLog(string message)
        {
            Console.WriteLine(message);

            try
            {
                using (StreamWriter op = new StreamWriter("world-save-errors.log", true))
                {
                    op.WriteLine("{0}\t{1}", DateTime.Now, message);
                    op.WriteLine(new StackTrace(2).ToString());
                    op.WriteLine();
                }
            }
            catch
            {
            }
        }

        private static void SaveIndex<T>(List<T> list, string path) where T : IEntityEntry
        {
            if (!Directory.Exists("Saves/Mobiles/"))
                Directory.CreateDirectory("Saves/Mobiles/");

            if (!Directory.Exists("Saves/Items/"))
                Directory.CreateDirectory("Saves/Items/");

            if (!Directory.Exists("Saves/Guilds/"))
                Directory.CreateDirectory("Saves/Guilds/");

            using (FileStream idx = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryWriter idxWriter = new BinaryWriter(idx);

                idxWriter.Write(list.Count);

                for (int i = 0; i < list.Count; ++i)
                {
                    T e = list[i];

                    idxWriter.Write(e.TypeID);
                    idxWriter.Write(e.Serial);
                    idxWriter.Write(e.Position);
                    idxWriter.Write(e.Length);
                }

                idxWriter.Close();
            }
        }

        private static void SaveIndex<T>(List<T> list, string path) where T : ICustomsEntry
        {
            if (!Directory.Exists("Saves/Cores/"))
                Directory.CreateDirectory("Saves/Cores/");

            if (!Directory.Exists("Saves/Modules/"))
                Directory.CreateDirectory("Saves/Modules");

            if (!Directory.Exists("Saves/Services/"))
                Directory.CreateDirectory("Saves/Services");

            using (FileStream indexStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryWriter indexWriter = new BinaryWriter(indexStream);

                indexWriter.Write(list.Count);

                for (int i = 0; i < list.Count; ++i)
                {
                    T e = list[i];

                    indexWriter.Write(e.TypeID);
                    indexWriter.Write(e.Serial);
                    indexWriter.Write(e.Position);
                    indexWriter.Write(e.Length);
                }

                indexWriter.Close();
            }
        }

        internal static int m_Saves;

        public static void Save()
        {
            Save(true, false);
        }

        public static void Save(bool message, bool permitBackgroundWrite)
        {
            if (m_Saving)
                return;

            ++m_Saves;

            NetState.FlushAll();
            NetState.Pause();

            World.WaitForWriteCompletion();//Blocks Save until current disk flush is done.

            m_Saving = true;

            m_DiskWriteHandle.Reset();

            if (message)
                Broadcast(0x35, true, "The world is saving, please wait.");

            SaveStrategy strategy = SaveStrategy.Acquire();
            Console.WriteLine("Core: Using {0} save strategy", strategy.Name.ToLowerInvariant());

            Console.Write("World: Saving...");

            Stopwatch watch = Stopwatch.StartNew();

            if (!Directory.Exists("Saves/Mobiles/"))
                Directory.CreateDirectory("Saves/Mobiles/");
            if (!Directory.Exists("Saves/Items/"))
                Directory.CreateDirectory("Saves/Items/");
            if (!Directory.Exists("Saves/Guilds/"))
                Directory.CreateDirectory("Saves/Guilds/");
            if (!Directory.Exists("Saves/Cores/"))
                Directory.CreateDirectory("Saves/Cores/");
            if (!Directory.Exists("Saves/Modules/"))
                Directory.CreateDirectory("Saves/Modules/");
            if (!Directory.Exists("Saves/Services/"))
                Directory.CreateDirectory("Saves/Services/");

            /*using ( SaveMetrics metrics = new SaveMetrics() ) {*/
            strategy.Save(null, permitBackgroundWrite);
            /*}*/

            try
            {
                EventSink.InvokeWorldSave(new WorldSaveEventArgs(message));
            }
            catch (Exception e)
            {
                throw new Exception("World Save event threw an exception.  Save failed!", e);
            }

            watch.Stop();

            m_Saving = false;

            if (!permitBackgroundWrite)
                World.NotifyDiskWriteComplete();	//Sets the DiskWriteHandle.  If we allow background writes, we leave this upto the individual save strategies.

            ProcessSafetyQueues();

            strategy.ProcessDecay();

            Console.WriteLine("Save done in {0:F2} seconds.", watch.Elapsed.TotalSeconds);

            if (message)
                Broadcast(0x35, true, "World save complete. The entire process took {0:F1} seconds.", watch.Elapsed.TotalSeconds);

            NetState.Resume();
        }

        internal static List<Type> m_ItemTypes = new List<Type>();
        internal static List<Type> m_MobileTypes = new List<Type>();
        internal static List<Type> _CoreTypes = new List<Type>();
        internal static List<Type> _ModuleTypes = new List<Type>();
        internal static List<Type> _ServiceTypes = new List<Type>();

        public static IEntity FindEntity(Serial serial)
        {
            if (serial.IsItem)
                return FindItem(serial);
            else if (serial.IsMobile)
                return FindMobile(serial);

            return null;
        }

        public static ICustomsEntity FindCustomEntity(CustomSerial serial)
        {
            if (serial.IsCore)
                return GetCore(serial);
            else if (serial.IsModule)
                return GetModule(serial);
            else if (serial.IsService)
                return GetService(serial);

            return null;
        }

        public static Mobile FindMobile(Serial serial)
        {
            Mobile mob;

            m_Mobiles.TryGetValue(serial, out mob);

            return mob;
        }

        public static void AddMobile(Mobile m)
        {
            if (m_Saving)
            {
                AppendSafetyLog("add", m);
                _addQueue.Enqueue(m);
            }
            else
            {
                m_Mobiles[m.Serial] = m;
            }
        }

        public static Item FindItem(Serial serial)
        {
            Item item;

            m_Items.TryGetValue(serial, out item);

            return item;
        }

        public static void AddItem(Item item)
        {
            if (m_Saving)
            {
                AppendSafetyLog("add", item);
                _addQueue.Enqueue(item);
            }
            else
            {
                m_Items[item.Serial] = item;
            }
        }

        public static void RemoveMobile(Mobile m)
        {
            m_Mobiles.Remove(m.Serial);
        }

        public static void RemoveItem(Item item)
        {
            m_Items.Remove(item.Serial);
        }

        public static BaseCore GetCore(CustomSerial serial)
        {
            BaseCore core;

            _Cores.TryGetValue(serial, out core);

            return core;
        }

        public static void AddCore(BaseCore core)
        {
            if (m_Saving)
            {
                AppendSafetyLog("add", core);
                _CustomsAddQueue.Enqueue(core);
            }
            else
                _Cores[core.Serial] = core;
        }

        public static void RemoveCore(BaseCore core)
        {
            _Cores.Remove(core.Serial);
        }

        public static BaseModule GetModule(CustomSerial serial)
        {
            BaseModule module;

            _Modules.TryGetValue(serial, out module);

            return module;
        }

        public static void AddModule(BaseModule module)
        {
            if (m_Saving)
            {
                AppendSafetyLog("add", module);
                _CustomsAddQueue.Enqueue(module);
            }
            else
                _Modules[module.Serial] = module;
        }

        public static void RemoveModule(BaseModule module)
        {
            _Modules.Remove(module.Serial);
        }

        public static BaseService GetService(CustomSerial serial)
        {
            BaseService service;

            _Services.TryGetValue(serial, out service);

            return service;
        }

        public static void AddService(BaseService service)
        {
            if (m_Saving)
            {
                AppendSafetyLog("add", service);
                _CustomsAddQueue.Enqueue(service);
            }
            else
                _Services[service.Serial] = service;
        }

        public static void RemoveService(BaseService service)
        {
            _Services.Remove(service.Serial);
        }

        public static BaseCore GetCore(string name)
        {
            foreach (BaseCore core in _Cores.Values)
            {
                if (core.Name == name)
                    return core;
            }

            return null;
        }

        public static BaseModule GetModule(string name)
        {
            foreach (BaseModule module in _Modules.Values)
            {
                if (module.Name == name)
                    return module;
            }

            return null;
        }

        public static BaseService GetService(string name)
        {
            foreach (BaseService service in _Services.Values)
            {
                if (service.Name == name)
                    return service;
            }

            return null;
        }

        public static BaseCore GetCore(Type type)
        {
            foreach (BaseCore core in _Cores.Values)
            {
                if (core.GetType() == type)
                    return core;
            }

            return null;
        }

        public static List<BaseModule> GetModules(Type type)
        {
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                if (module.GetType() == type)
                    results.Add(module);
            }

            return results;
        }

        public static BaseService GetService(Type type)
        {
            foreach (BaseService service in _Services.Values)
            {
                if (service.GetType() == type)
                    return service;
            }

            return null;
        }

        public static List<BaseCore> GetCores(Type type)
        {
            List<BaseCore> results = new List<BaseCore>();

            foreach (BaseCore core in _Cores.Values)
            {
                if (core.GetType() == type)
                    results.Add(core);
            }

            return results;
        }

        public static List<BaseService> GetServices(Type type)
        {
            List<BaseService> results = new List<BaseService>();

            foreach (BaseService service in _Services.Values)
            {
                if (service.GetType() == type)
                    results.Add(service);
            }

            return results;
        }

        public static BaseModule GetModule(Mobile mobile, string name)
        {
            foreach (BaseModule module in _Modules.Values)
            {
                if (module.LinkedMobile == mobile && module.Name == name)
                    return module;
            }

            return null;
        }

        public static BaseModule GetModule(Mobile mobile, Type type)
        {
            foreach (BaseModule module in _Modules.Values)
            {
                if (module.LinkedMobile == mobile && module.GetType() == type)
                    return module;
            }

            return null;
        }

        public static List<BaseModule> GetModules(Mobile mobile)
        {
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                if (module.LinkedMobile == mobile)
                    results.Add(module);
            }

            return results;
        }

        public static List<BaseModule> GetModules(Mobile mobile, string name)
        {
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                if (module.LinkedMobile == mobile && module.Name == name)
                    results.Add(module);
            }

            return results;
        }

        public static List<BaseModule> GetModules(Item item)
        {
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                if (module.LinkedItem == item)
                    results.Add(module);
            }

            return results;
        }

        public static List<BaseCore> SearchCores(string find)
        {
            string[] keywords = find.ToLower().Split(' ');
            List<BaseCore> results = new List<BaseCore>();

            foreach (BaseCore core in _Cores.Values)
            {
                bool match = true;
                string name = core.Name.ToLower();

                for (int i = 0; i < keywords.Length; i++)
                {
                    if (name.IndexOf(keywords[i]) == -1)
                        match = false;
                }

                if (match)
                    results.Add(core);
            }

            return results;
        }

        public static List<BaseModule> SearchModules(string find)
        {
            string[] keywords = find.ToLower().Split(' ');
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                bool match = true;
                string name = module.Name.ToLower();

                for (int i = 0; i < keywords.Length; i++)
                {
                    if (name.IndexOf(keywords[i]) == -1)
                        match = false;
                }

                if (match)
                    results.Add(module);
            }

            return results;
        }

        public static List<BaseService> SearchServices(string find)
        {
            string[] keywords = find.ToLower().Split(' ');
            List<BaseService> results = new List<BaseService>();

            foreach (BaseService service in _Services.Values)
            {
                bool match = true;
                string name = service.Name.ToLower();

                for (int i = 0; i < keywords.Length; i++)
                {
                    if (name.IndexOf(keywords[i]) == -1)
                        match = false;
                }

                if (match)
                    results.Add(service);
            }

            return results;
        }

        public static List<BaseModule> SearchModules(Mobile mobile, string find)
        {
            string[] keywords = find.ToLower().Split(' ');
            List<BaseModule> results = new List<BaseModule>();

            foreach (BaseModule module in _Modules.Values)
            {
                bool match = true;
                string name = module.Name.ToLower();

                if (module.LinkedMobile == mobile)
                {
                    for (int i = 0; i < keywords.Length; i++)
                    {
                        if (name.IndexOf(keywords[i]) == -1)
                            match = false;
                    }

                    if (match)
                        results.Add(module);
                }
            }

            return results;
        }
    }
}