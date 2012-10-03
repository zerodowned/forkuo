using System;
using Server;
using Server.Gumps;

namespace CustomsFramework
{
    public interface ICustomsEntry
    {
        CustomSerial Serial { get; }
        int TypeID { get; }
        long Position { get; }
        int Length { get; }
    }

    public interface ICustomsEntity : IComparable, IComparable<ICustomsEntity>
    {
        CustomSerial Serial { get; }
        string Name { get; }
        string Description { get; }
        string Version { get; }
        AccessLevel EditLevel { get; }
        Gump SettingsGump { get; }

        void Delete();

        void Prep();
    }
}