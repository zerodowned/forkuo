/***************************************************************************
*                          ParallelSaveStrategy.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: ParallelSaveStrategy.cs 641 2010-12-20 03:34:25Z asayre $
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
using System.Threading;
using CustomsFramework;
using Server.Guilds;

namespace Server
{
    public sealed class ParallelSaveStrategy : SaveStrategy
    {
        public override string Name
        {
            get
            {
                return "Parallel";
            }
        }

        private readonly int processorCount;

        public ParallelSaveStrategy(int processorCount)
        {
            this.processorCount = processorCount;

            this._decayQueue = new Queue<Item>();
        }

        private int GetThreadCount()
        {
            return this.processorCount - 1;
        }

        private SaveMetrics metrics;

        private SequentialFileWriter itemData, itemIndex;
        private SequentialFileWriter mobileData, mobileIndex;
        private SequentialFileWriter guildData, guildIndex;
        private SequentialFileWriter coreData, coreIndex;
        private SequentialFileWriter moduleData, moduleIndex;
        private SequentialFileWriter serviceData, serviceIndex;

        private readonly Queue<Item> _decayQueue;

        private Consumer[] consumers;
        private int cycle;

        private bool finished;

        public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
        {
            this.metrics = metrics;

            this.OpenFiles();

            this.consumers = new Consumer[this.GetThreadCount()];

            for (int i = 0; i < this.consumers.Length; ++i)
            {
                this.consumers[i] = new Consumer(this, 256);
            }

            IEnumerable<ISerializable> collection = new Producer();

            foreach (ISerializable value in collection)
            {
                while (!this.Enqueue(value))
                {
                    if (!this.Commit())
                    {
                        Thread.Sleep(0);
                    }
                }
            }

            this.finished = true;

            this.SaveTypeDatabases();

            WaitHandle.WaitAll(
                Array.ConvertAll<Consumer, WaitHandle>(
                    this.consumers,
                    delegate(Consumer input)
                    {
                        return input.completionEvent;
                    }));

            this.Commit();

            this.CloseFiles();
        }

        public override void ProcessDecay()
        {
            while (this._decayQueue.Count > 0)
            {
                Item item = this._decayQueue.Dequeue();

                if (item.OnDecay())
                {
                    item.Delete();
                }
            }
        }

        private void SaveTypeDatabases()
        {
            this.SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
            this.SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);
            this.SaveTypeDatabase(World.CoreTypesPath, World._CoreTypes);
            this.SaveTypeDatabase(World.ModuleTypesPath, World._ModuleTypes);
            this.SaveTypeDatabase(World.ServiceTypesPath, World._ServiceTypes);
        }

        private void SaveTypeDatabase(string path, List<Type> types)
        {
            BinaryFileWriter bfw = new BinaryFileWriter(path, false);

            bfw.Write(types.Count);

            foreach (Type type in types)
            {
                bfw.Write(type.FullName);
            }

            bfw.Flush();

            bfw.Close();
        }

        private void OpenFiles()
        {
            this.itemData = new SequentialFileWriter(World.ItemDataPath, this.metrics);
            this.itemIndex = new SequentialFileWriter(World.ItemIndexPath, this.metrics);

            this.mobileData = new SequentialFileWriter(World.MobileDataPath, this.metrics);
            this.mobileIndex = new SequentialFileWriter(World.MobileIndexPath, this.metrics);

            this.guildData = new SequentialFileWriter(World.GuildDataPath, this.metrics);
            this.guildIndex = new SequentialFileWriter(World.GuildIndexPath, this.metrics);

            this.coreData = new SequentialFileWriter(World.CoresDataPath, this.metrics);
            this.coreIndex = new SequentialFileWriter(World.CoreIndexPath, this.metrics);

            this.moduleData = new SequentialFileWriter(World.ModulesDataPath, this.metrics);
            this.moduleIndex = new SequentialFileWriter(World.ModuleIndexPath, this.metrics);

            this.serviceData = new SequentialFileWriter(World.ServicesDataPath, this.metrics);
            this.serviceIndex = new SequentialFileWriter(World.ServiceIndexPath, this.metrics);

            this.WriteCount(itemIndex, World.Items.Count);
            this.WriteCount(mobileIndex, World.Mobiles.Count);
            this.WriteCount(guildIndex, BaseGuild.List.Count);
            this.WriteCount(coreIndex, World.Cores.Count);
            this.WriteCount(moduleIndex, World.Modules.Count);
            this.WriteCount(serviceIndex, World.Services.Count);
        }

        private void WriteCount(SequentialFileWriter indexFile, int count)
        {
            byte[] buffer = new byte[4];

            buffer[0] = (byte)(count);
            buffer[1] = (byte)(count >> 8);
            buffer[2] = (byte)(count >> 16);
            buffer[3] = (byte)(count >> 24);

            indexFile.Write(buffer, 0, buffer.Length);
        }

        private void CloseFiles()
        {
            this.itemData.Close();
            this.itemIndex.Close();

            this.mobileData.Close();
            this.mobileIndex.Close();

            this.guildData.Close();
            this.guildIndex.Close();

            this.coreData.Close();
            this.coreIndex.Close();

            this.moduleData.Close();
            this.moduleIndex.Close();

            this.serviceData.Close();
            this.serviceIndex.Close();

            World.NotifyDiskWriteComplete();
        }

        private void OnSerialized(ConsumableEntry entry)
        {
            ISerializable value = entry.value;
            BinaryMemoryWriter writer = entry.writer;

            Item item = value as Item;

            if (item != null)
                this.Save(item, writer);
            else
            {
                Mobile mob = value as Mobile;

                if (mob != null)
                    this.Save(mob, writer);
                else
                {
                    BaseGuild guild = value as BaseGuild;

                    if (guild != null)
                        this.Save(guild, writer);
                    else
                    {
                        BaseCore core = value as BaseCore;

                        if (core != null)
                            this.Save(core, writer);
                        else
                        {
                            BaseModule module = value as BaseModule;

                            if (module != null)
                                this.Save(module, writer);
                            else
                            {
                                BaseService service = value as BaseService;

                                if (service != null)
                                    this.Save(service, writer);
                            }
                        }
                    }
                }
            }
        }

        private void Save(Item item, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.itemData, this.itemIndex, item.m_TypeRef, item.Serial);

            if (this.metrics != null)
            {
                this.metrics.OnItemSaved(length);
            }

            if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.Now > (item.LastMoved + item.DecayTime))
            {
                this._decayQueue.Enqueue(item);
            }
        }

        private void Save(Mobile mob, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.mobileData, this.mobileIndex, mob.m_TypeRef, mob.Serial);

            if (this.metrics != null)
            {
                this.metrics.OnMobileSaved(length);
            }
        }

        private void Save(BaseGuild guild, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.guildData, this.guildIndex, 0, guild.Id);

            if (this.metrics != null)
            {
                this.metrics.OnGuildSaved(length);
            }
        }

        private void Save(BaseCore core, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.coreData, this.coreIndex, core._TypeID, core.Serial);

            if (this.metrics != null)
                this.metrics.OnCoreSaved(length);
        }

        private void Save(BaseModule module, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.moduleData, this.moduleIndex, module._TypeID, module.Serial);

            if (this.metrics != null)
                this.metrics.OnModuleSaved(length);
        }

        private void Save(BaseService service, BinaryMemoryWriter writer)
        {
            int length = writer.CommitTo(this.serviceData, this.serviceIndex, service._TypeID, service.Serial);

            if (this.metrics != null)
                this.metrics.OnServiceSaved(length);
        }

        private bool Enqueue(ISerializable value)
        {
            for (int i = 0; i < this.consumers.Length; ++i)
            {
                Consumer consumer = this.consumers[this.cycle++ % this.consumers.Length];

                if ((consumer.tail - consumer.head) < consumer.buffer.Length)
                {
                    consumer.buffer[consumer.tail % consumer.buffer.Length].value = value;
                    consumer.tail++;

                    return true;
                }
            }

            return false;
        }

        private bool Commit()
        {
            bool committed = false;

            for (int i = 0; i < this.consumers.Length; ++i)
            {
                Consumer consumer = this.consumers[i];

                while (consumer.head < consumer.done)
                {
                    this.OnSerialized(consumer.buffer[consumer.head % consumer.buffer.Length]);
                    consumer.head++;

                    committed = true;
                }
            }

            return committed;
        }

        private sealed class Producer : IEnumerable<ISerializable>
        {
            private readonly IEnumerable<Item> items;
            private readonly IEnumerable<Mobile> mobiles;
            private readonly IEnumerable<BaseGuild> guilds;
            private readonly IEnumerable<BaseCore> cores;
            private readonly IEnumerable<BaseModule> modules;
            private readonly IEnumerable<BaseService> services;

            public Producer()
            {
                this.items = World.Items.Values;
                this.mobiles = World.Mobiles.Values;
                this.guilds = BaseGuild.List.Values;
                this.cores = World.Cores.Values;
                this.modules = World.Modules.Values;
                this.services = World.Services.Values;
            }

            public IEnumerator<ISerializable> GetEnumerator()
            {
                foreach (Item item in this.items)
                    yield return item;

                foreach (Mobile mob in this.mobiles)
                    yield return mob;

                foreach (BaseGuild guild in this.guilds)
                    yield return guild;

                foreach (BaseCore core in this.cores)
                    yield return core;

                foreach (BaseModule module in this.modules)
                    yield return module;

                foreach (BaseService service in this.services)
                    yield return service;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private struct ConsumableEntry
        {
            public ISerializable value;
            public BinaryMemoryWriter writer;
        }

        private sealed class Consumer
        {
            private readonly ParallelSaveStrategy owner;

            public readonly ManualResetEvent completionEvent;

            public readonly ConsumableEntry[] buffer;
            public int head, done, tail;

            private readonly Thread thread;

            public Consumer(ParallelSaveStrategy owner, int bufferSize)
            {
                this.owner = owner;

                this.buffer = new ConsumableEntry[bufferSize];

                for (int i = 0; i < this.buffer.Length; ++i)
                {
                    this.buffer[i].writer = new BinaryMemoryWriter();
                }

                this.completionEvent = new ManualResetEvent(false);

                this.thread = new Thread(Processor);

                this.thread.Name = "Parallel Serialization Thread";

                this.thread.Start();
            }

            private void Processor()
            {
                try
                {
                    while (!this.owner.finished)
                    {
                        this.Process();
                        Thread.Sleep(0);
                    }

                    this.Process();

                    this.completionEvent.Set();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            private void Process()
            {
                ConsumableEntry entry;

                while (this.done < this.tail)
                {
                    entry = this.buffer[this.done % this.buffer.Length];

                    entry.value.Serialize(entry.writer);

                    ++this.done;
                }
            }
        }
    }
}