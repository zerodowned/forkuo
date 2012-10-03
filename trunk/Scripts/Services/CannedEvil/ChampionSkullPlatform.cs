using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.CannedEvil
{
    public class ChampionSkullPlatform : BaseAddon
    {
        private ChampionSkullBrazier m_Power, m_Enlightenment, m_Venom, m_Pain, m_Greed, m_Death;

        [Constructable]
        public ChampionSkullPlatform()
        {
            this.AddComponent(new AddonComponent(0x71A), -1, -1, -1);
            this.AddComponent(new AddonComponent(0x709), 0, -1, -1);
            this.AddComponent(new AddonComponent(0x709), 1, -1, -1);
            this.AddComponent(new AddonComponent(0x709), -1, 0, -1);
            this.AddComponent(new AddonComponent(0x709), 0, 0, -1);
            this.AddComponent(new AddonComponent(0x709), 1, 0, -1);
            this.AddComponent(new AddonComponent(0x709), -1, 1, -1);
            this.AddComponent(new AddonComponent(0x709), 0, 1, -1);
            this.AddComponent(new AddonComponent(0x71B), 1, 1, -1);

            this.AddComponent(new AddonComponent(0x50F), 0, -1, 4);
            this.AddComponent(m_Power = new ChampionSkullBrazier(this, ChampionSkullType.Power), 0, -1, 5);

            this.AddComponent(new AddonComponent(0x50F), 1, -1, 4);
            this.AddComponent(m_Enlightenment = new ChampionSkullBrazier(this, ChampionSkullType.Enlightenment), 1, -1, 5);

            this.AddComponent(new AddonComponent(0x50F), -1, 0, 4);
            this.AddComponent(m_Venom = new ChampionSkullBrazier(this, ChampionSkullType.Venom), -1, 0, 5);

            this.AddComponent(new AddonComponent(0x50F), 1, 0, 4);
            this.AddComponent(m_Pain = new ChampionSkullBrazier(this, ChampionSkullType.Pain), 1, 0, 5);

            this.AddComponent(new AddonComponent(0x50F), -1, 1, 4);
            this.AddComponent(m_Greed = new ChampionSkullBrazier(this, ChampionSkullType.Greed), -1, 1, 5);

            this.AddComponent(new AddonComponent(0x50F), 0, 1, 4);
            this.AddComponent(m_Death = new ChampionSkullBrazier(this, ChampionSkullType.Death), 0, 1, 5);

            AddonComponent comp = new LocalizedAddonComponent(0x20D2, 1049495);
            comp.Hue = 0x482;
            this.AddComponent(comp, 0, 0, 5);

            comp = new LocalizedAddonComponent(0x0BCF, 1049496);
            comp.Hue = 0x482;
            this.AddComponent(comp, 0, 2, -7);

            comp = new LocalizedAddonComponent(0x0BD0, 1049497);
            comp.Hue = 0x482;
            this.AddComponent(comp, 2, 0, -7);
        }

        public void Validate()
        {
            if (this.Validate(m_Power) && this.Validate(m_Enlightenment) && this.Validate(m_Venom) && this.Validate(m_Pain) && this.Validate(m_Greed) && this.Validate(m_Death))
            {
                Mobile harrower = Harrower.Spawn(new Point3D(this.X, this.Y, this.Z + 6), this.Map);

                if (harrower == null)
                    return;

                this.Clear(m_Power);
                this.Clear(m_Enlightenment);
                this.Clear(m_Venom);
                this.Clear(m_Pain);
                this.Clear(m_Greed);
                this.Clear(m_Death);
            }
        }

        public void Clear(ChampionSkullBrazier brazier)
        {
            if (brazier != null)
            {
                Effects.SendBoltEffect(brazier);

                if (brazier.Skull != null)
                    brazier.Skull.Delete();
            }
        }

        public bool Validate(ChampionSkullBrazier brazier)
        {
            return (brazier != null && brazier.Skull != null && !brazier.Skull.Deleted);
        }

        public ChampionSkullPlatform(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(this.m_Power);
            writer.Write(this.m_Enlightenment);
            writer.Write(this.m_Venom);
            writer.Write(this.m_Pain);
            writer.Write(this.m_Greed);
            writer.Write(this.m_Death);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 0:
                    {
                        this.m_Power = reader.ReadItem() as ChampionSkullBrazier;
                        this.m_Enlightenment = reader.ReadItem() as ChampionSkullBrazier;
                        this.m_Venom = reader.ReadItem() as ChampionSkullBrazier;
                        this.m_Pain = reader.ReadItem() as ChampionSkullBrazier;
                        this.m_Greed = reader.ReadItem() as ChampionSkullBrazier;
                        this.m_Death = reader.ReadItem() as ChampionSkullBrazier;

                        break;
                    }
            }
        }
    }
}