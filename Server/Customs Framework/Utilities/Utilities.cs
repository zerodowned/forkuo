using System;
using System.IO;
using Server;
using Server.Items;
using SevenZip;

namespace CustomsFramework
{
    public enum SaveStrategyTypes
    {
        StandardSaveStrategy,
        DualSaveStrategy,
        DynamicSaveStrategy,
        ParallelSaveStrategy
    }

    public enum OldClientResponse
    {
        Ignore,
        Warn,
        Annoy,
        LenientKick,
        Kick
    }

    public partial class Utilities
    {
        public static void WriteVersion(GenericWriter writer, int version)
        {
            writer.Write(version);
        }

        public static void CheckFileStructure(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static bool IsPlayer(Mobile from)
        {
            if (from.AccessLevel <= AccessLevel.VIP)
                return true;
            else
                return false;
        }

        public static bool IsStaff(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.Counselor)
                return true;
            else
                return false;
        }

        public static bool IsDigit(string text)
        {
            bool isDigit = false;

            try
            {
                Convert.ToInt32(text);
                isDigit = true;
            }
            catch
            {
                isDigit = false;
            }

            return isDigit;
        }

        public void PlaceItemIn(Container container, int x, int y, Item item)
        {
            container.AddItem(item);
            item.Location = new Point3D(x, y, 0);
        }

        public Item BlessItem(Item item)
        {
            item.LootType = LootType.Blessed;

            return item;
        }

        public Item MakeNewbie(Item item)
        {
            if (!Core.AOS)
                item.LootType = LootType.Newbied;

            return item;
        }

        public void Compress7z(string copyPath, string outPath, CompressionLevel compressionLevel)
        {
            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.CustomParameters.Add("mt", "on");
            compressor.CompressionLevel = compressionLevel;
            compressor.ScanOnlyWritable = true;

            compressor.CompressDirectory(copyPath, outPath + ".7z");
        }

        public void Compress7z(string copyPath, Stream outStream, CompressionLevel compressionLevel)
        {
            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.CustomParameters.Add("mt", "on");
            compressor.CompressionLevel = compressionLevel;
            compressor.ScanOnlyWritable = true;

            compressor.CompressDirectory(copyPath, outStream);
        }

        public void Compress7z(string copyPath, string outPath)
        {
            Compress7z(copyPath, outPath, CompressionLevel.Normal);
        }

        public void Compress7z(string copyPath, Stream outStream)
        {
            Compress7z(copyPath, outStream, CompressionLevel.Normal);
        }

        public static SaveStrategy GetSaveStrategy(SaveStrategyTypes saveStrategyTypes)
        {
            if (saveStrategyTypes == SaveStrategyTypes.StandardSaveStrategy)
                return new StandardSaveStrategy();
            else if (saveStrategyTypes == SaveStrategyTypes.DualSaveStrategy)
                return new DualSaveStrategy();
            else if (saveStrategyTypes == SaveStrategyTypes.DynamicSaveStrategy)
                return new DynamicSaveStrategy();
            else if (saveStrategyTypes == SaveStrategyTypes.ParallelSaveStrategy)
                return new ParallelSaveStrategy(Core.ProcessorCount);
            else
                return new StandardSaveStrategy();
        }

        public static SaveStrategyTypes GetSaveType(SaveStrategy saveStrategy)
        {
            if (saveStrategy is StandardSaveStrategy)
                return SaveStrategyTypes.StandardSaveStrategy;
            else if (saveStrategy is DualSaveStrategy)
                return SaveStrategyTypes.StandardSaveStrategy;
            else if (saveStrategy is DynamicSaveStrategy)
                return SaveStrategyTypes.DynamicSaveStrategy;
            else if (saveStrategy is ParallelSaveStrategy)
                return SaveStrategyTypes.ParallelSaveStrategy;
            else
                return SaveStrategyTypes.StandardSaveStrategy;
        }
    }
}
