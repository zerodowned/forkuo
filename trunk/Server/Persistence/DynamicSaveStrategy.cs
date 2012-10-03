/***************************************************************************
*                          DynamicSaveStrategy.cs
*                            -------------------
*   begin                : December 16, 2010
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: DynamicSaveStrategy.cs 844 2012-03-07 13:47:33Z mark $
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomsFramework;
using Server.Guilds;

namespace Server
{
    public sealed class DynamicSaveStrategy : SaveStrategy
    {
        public override string Name
        {
            get
            {
                return "Dynamic";
            }
        }

        private SaveMetrics _metrics;

        private SequentialFileWriter _itemData, _itemIndex;
        private SequentialFileWriter _mobileData, _mobileIndex;
        private SequentialFileWriter _guildData, _guildIndex;
        private SequentialFileWriter _coreData, _coreIndex;
        private SequentialFileWriter _moduleData, _moduleIndex;
        private SequentialFileWriter _serviceData, _serviceIndex;

        private readonly ConcurrentBag<Item> _decayBag;

        private readonly BlockingCollection<QueuedMemoryWriter> _itemThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _mobileThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _guildThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _coreThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _moduleThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _serviceThreadwriters;

        public DynamicSaveStrategy()
        {
            this._decayBag = new ConcurrentBag<Item>();
            this._itemThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            this._mobileThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            this._guildThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            this._coreThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            this._moduleThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            this._serviceThreadwriters = new BlockingCollection<QueuedMemoryWriter>();
        }

        public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
        {
            this._metrics = metrics;

            this.OpenFiles();

            Task[] saveTasks = new Task[3];

            saveTasks[0] = this.SaveItems();
            saveTasks[1] = this.SaveMobiles();
            saveTasks[2] = this.SaveGuilds();
            saveTasks[3] = this.SaveCores();
            saveTasks[4] = this.SaveModules();
            saveTasks[5] = this.SaveServices();

            this.SaveTypeDatabases();

            if (permitBackgroundWrite)
            {
                //This option makes it finish the writing to disk in the background, continuing even after Save() returns.
                Task.Factory.ContinueWhenAll(saveTasks, _ =>
                {
                    this.CloseFiles();

                    World.NotifyDiskWriteComplete();
                });
            }
            else
            {
                Task.WaitAll(saveTasks);	//Waits for the completion of all of the tasks(committing to disk)
                this.CloseFiles();
            }
        }

        private Task StartCommitTask(BlockingCollection<QueuedMemoryWriter> threadWriter, SequentialFileWriter data, SequentialFileWriter index)
        {
            Task commitTask = Task.Factory.StartNew(() =>
            {
                while (!(threadWriter.IsCompleted))
                {
                    QueuedMemoryWriter writer;

                    try
                    {
                        writer = threadWriter.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        //Per MSDN, it's fine if we're here, successful completion of adding can rarely put us into this state.
                        break;
                    }

                    writer.CommitTo(data, index);
                }
            });

            return commitTask;
        }

        private Task SaveItems()
        {
            //Start the blocking consumer; this runs in background.
            Task commitTask = this.StartCommitTask(_itemThreadWriters, _itemData, _itemIndex);

            IEnumerable<Item> items = World.Items.Values;

            //Start the producer.
            Parallel.ForEach(items, () => new QueuedMemoryWriter(),
                (Item item, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    item.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(item, size);

                    if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.Now > (item.LastMoved + item.DecayTime))
                    {
                        this._decayBag.Add(item);
                    }

                    if (this._metrics != null)
                    {
                        this._metrics.OnItemSaved(size);
                    }

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._itemThreadWriters.Add(writer);
                });

            this._itemThreadWriters.CompleteAdding();	//We only get here after the Parallel.ForEach completes.  Lets our task 

            return commitTask;
        }

        private Task SaveMobiles()
        {
            //Start the blocking consumer; this runs in background.
            Task commitTask = this.StartCommitTask(_mobileThreadWriters, _mobileData, _mobileIndex);

            IEnumerable<Mobile> mobiles = World.Mobiles.Values;

            //Start the producer.
            Parallel.ForEach(mobiles, () => new QueuedMemoryWriter(),
                (Mobile mobile, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    mobile.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(mobile, size);

                    if (this._metrics != null)
                    {
                        this._metrics.OnMobileSaved(size);
                    }

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._mobileThreadWriters.Add(writer);
                });

            this._mobileThreadWriters.CompleteAdding();	//We only get here after the Parallel.ForEach completes.  Lets our task tell the consumer that we're done

            return commitTask;
        }

        private Task SaveGuilds()
        {
            //Start the blocking consumer; this runs in background.
            Task commitTask = this.StartCommitTask(_guildThreadWriters, _guildData, _guildIndex);

            IEnumerable<BaseGuild> guilds = BaseGuild.List.Values;

            //Start the producer.
            Parallel.ForEach(guilds, () => new QueuedMemoryWriter(),
                (BaseGuild guild, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    guild.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(guild, size);

                    if (this._metrics != null)
                    {
                        this._metrics.OnGuildSaved(size);
                    }

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._guildThreadWriters.Add(writer);
                });

            this._guildThreadWriters.CompleteAdding();	//We only get here after the Parallel.ForEach completes.  Lets our task 

            return commitTask;
        }

        private Task SaveCores()
        {
            Task commitTask = this.StartCommitTask(_coreThreadWriters, _coreData, _coreIndex);

            IEnumerable<BaseCore> cores = World.Cores.Values;

            Parallel.ForEach(cores, () => new QueuedMemoryWriter(),
                (BaseCore core, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    core.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(core, size);

                    if (this._metrics != null)
                        this._metrics.OnCoreSaved(size);

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._coreThreadWriters.Add(writer);
                });

            this._coreThreadWriters.CompleteAdding();

            return commitTask;
        }

        private Task SaveModules()
        {
            Task commitTask = this.StartCommitTask(_moduleThreadWriters, _moduleData, _moduleIndex);

            IEnumerable<BaseModule> modules = World.Modules.Values;

            Parallel.ForEach(modules, () => new QueuedMemoryWriter(),
                (BaseModule module, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    module.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(module, size);

                    if (this._metrics != null)
                        this._metrics.OnModuleSaved(size);

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._moduleThreadWriters.Add(writer);
                });

            this._moduleThreadWriters.CompleteAdding();

            return commitTask;
        }

        private Task SaveServices()
        {
            Task commitTask = this.StartCommitTask(_serviceThreadwriters, _serviceData, _serviceIndex);

            IEnumerable<BaseService> services = World.Services.Values;

            Parallel.ForEach(services, () => new QueuedMemoryWriter(),
                (BaseService service, ParallelLoopState state, QueuedMemoryWriter writer) =>
                {
                    long startPosition = writer.Position;

                    service.Serialize(writer);

                    int size = (int)(writer.Position - startPosition);

                    writer.QueueForIndex(service, size);

                    if (this._metrics != null)
                        this._metrics.OnServiceSaved(size);

                    return writer;
                },
                (writer) =>
                {
                    writer.Flush();

                    this._serviceThreadwriters.Add(writer);
                });

            this._serviceThreadwriters.CompleteAdding();

            return commitTask;
        }

        public override void ProcessDecay()
        {
            Item item;

            while (this._decayBag.TryTake(out item))
            {
                if (item.OnDecay())
                {
                    item.Delete();
                }
            }
        }

        private void OpenFiles()
        {
            this._itemData = new SequentialFileWriter(World.ItemDataPath, this._metrics);
            this._itemIndex = new SequentialFileWriter(World.ItemIndexPath, this._metrics);

            this._mobileData = new SequentialFileWriter(World.MobileDataPath, this._metrics);
            this._mobileIndex = new SequentialFileWriter(World.MobileIndexPath, this._metrics);

            this._guildData = new SequentialFileWriter(World.GuildDataPath, this._metrics);
            this._guildIndex = new SequentialFileWriter(World.GuildIndexPath, this._metrics);

            this._coreData = new SequentialFileWriter(World.CoresDataPath, this._metrics);
            this._coreIndex = new SequentialFileWriter(World.CoreIndexPath, this._metrics);

            this._moduleData = new SequentialFileWriter(World.ModulesDataPath, this._metrics);
            this._moduleIndex = new SequentialFileWriter(World.ModuleIndexPath, this._metrics);

            this._serviceData = new SequentialFileWriter(World.ServicesDataPath, this._metrics);
            this._serviceIndex = new SequentialFileWriter(World.ServiceIndexPath, this._metrics);

            this.WriteCount(_itemIndex, World.Items.Count);
            this.WriteCount(_mobileIndex, World.Mobiles.Count);
            this.WriteCount(_guildIndex, BaseGuild.List.Count);
            this.WriteCount(_coreIndex, World.Cores.Count);
            this.WriteCount(_moduleIndex, World.Modules.Count);
            this.WriteCount(_serviceIndex, World.Services.Count);
        }

        private void CloseFiles()
        {
            this._itemData.Close();
            this._itemIndex.Close();

            this._mobileData.Close();
            this._mobileIndex.Close();

            this._guildData.Close();
            this._guildIndex.Close();

            this._coreData.Close();
            this._coreIndex.Close();

            this._moduleData.Close();
            this._moduleIndex.Close();

            this._serviceData.Close();
            this._serviceIndex.Close();
        }

        private void WriteCount(SequentialFileWriter indexFile, int count)
        {
            //Equiv to GenericWriter.Write( (int)count );
            byte[] buffer = new byte[4];

            buffer[0] = (byte)(count);
            buffer[1] = (byte)(count >> 8);
            buffer[2] = (byte)(count >> 16);
            buffer[3] = (byte)(count >> 24);

            indexFile.Write(buffer, 0, buffer.Length);
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
    }
}