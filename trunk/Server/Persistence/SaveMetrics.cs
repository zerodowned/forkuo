/***************************************************************************
*                              SaveMetrics.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: SaveMetrics.cs 632 2010-12-18 11:00:57Z asayre $
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
using System.Diagnostics;

namespace Server
{
    public sealed class SaveMetrics : IDisposable
    {
        private const string PerformanceCategoryName = "ForkUO 0.2";
        private const string PerformanceCategoryDesc = "Performance counters for ForkUO 0.2";

        private readonly PerformanceCounter numberOfWorldSaves;

        private readonly PerformanceCounter itemsPerSecond;
        private readonly PerformanceCounter mobilesPerSecond;
        private readonly PerformanceCounter coresPerSecond;
        private readonly PerformanceCounter modulesPerSecond;
        private readonly PerformanceCounter servicesPerSecond;

        private readonly PerformanceCounter serializedBytesPerSecond;
        private readonly PerformanceCounter writtenBytesPerSecond;

        public SaveMetrics()
        {
            if (!PerformanceCounterCategory.Exists(PerformanceCategoryName))
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                counters.Add(new CounterCreationData(
                    "Save - Count",
                    "Number of world saves.",
                    PerformanceCounterType.NumberOfItems32));

                counters.Add(new CounterCreationData(
                    "Save - Items/sec",
                    "Number of items saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Mobiles/sec",
                    "Number of mobiles saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Cores/sec",
                    "Number of cores saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Modules/sec",
                    "Number of modules saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Services/sec",
                    "Number of services saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Serialized bytes/sec",
                    "Amount of world-save bytes serialized per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Written bytes/sec",
                    "Amount of world-save bytes written to disk per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                #if !MONO
                PerformanceCounterCategory.Create(PerformanceCategoryName, PerformanceCategoryDesc, PerformanceCounterCategoryType.SingleInstance, counters);
                #endif
            }

            this.numberOfWorldSaves = new PerformanceCounter(PerformanceCategoryName, "Save - Count", false);

            this.itemsPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Items/sec", false);
            this.mobilesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Mobiles/sec", false);
            this.coresPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Cores/sec", false);
            this.modulesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Modules/sec", false);
            this.servicesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Services/sec", false);

            this.serializedBytesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Serialized bytes/sec", false);
            this.writtenBytesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Written bytes/sec", false);

            // increment number of world saves
            this.numberOfWorldSaves.Increment();
        }

        public void OnItemSaved(int numberOfBytes)
        {
            this.itemsPerSecond.Increment();

            this.serializedBytesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnMobileSaved(int numberOfBytes)
        {
            this.mobilesPerSecond.Increment();

            this.serializedBytesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnGuildSaved(int numberOfBytes)
        {
            this.serializedBytesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnCoreSaved(int numberOfBytes)
        {
            this.coresPerSecond.Increment();

            this.serializedBytesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnModuleSaved(int numberOfBytes)
        {
            this.modulesPerSecond.Increment();

            this.serializedBytesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnServiceSaved(int numberOfBytes)
        {
            this.servicesPerSecond.Increment();

            this.servicesPerSecond.IncrementBy(numberOfBytes);
        }

        public void OnFileWritten(int numberOfBytes)
        {
            this.writtenBytesPerSecond.IncrementBy(numberOfBytes);
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                this.numberOfWorldSaves.Dispose();

                this.itemsPerSecond.Dispose();
                this.mobilesPerSecond.Dispose();
                this.coresPerSecond.Dispose();
                this.modulesPerSecond.Dispose();
                this.servicesPerSecond.Dispose();

                this.serializedBytesPerSecond.Dispose();
                this.writtenBytesPerSecond.Dispose();
            }
        }
    }
}