using Server.Gumps;

namespace CustomsFramework.GumpPlus.Gumplings
{
    /* This is meant help make gump design easier, later down the road.
    * All images aren't in place yet, the point of it is to make it easier
    * to add images to gumps. Eliminating the need to look up gump id numbers.
    * 
    * Aka...work in progress.
    */
    public class UpsideDownUO : GumpImage
    {
        public UpsideDownUO(int x, int y, int hue) : base(x, y, 0, hue)
        {
        }

        public UpsideDownUO(int x, int y) : base(x, y, 0, 0)
        {
        }
    }

    public class Scroll1 : GumpImage
    {
        public Scroll1(int x, int y, int hue) : base(x, y, 7, hue)
        {
        }

        public Scroll1(int x, int y) : base(x, y, 7, 0)
        {
        }
    }

    public class CorpseContainer : GumpImage
    {
        public CorpseContainer(int x, int y, int hue) : base(x, y, 9, hue)
        {
        }

        public CorpseContainer(int x, int y) : base(x, y, 9, 0)
        {
        }
    }

    public class OldPaperDollContainer1 : GumpImage
    {
        public OldPaperDollContainer1(int x, int y, int hue) : base(x, y, 10, hue)
        {
        }

        public OldPaperDollContainer1(int x, int y) : base(x, y, 10, 0)
        {
        }
    }

    public class OldPaperDollContainer2 : GumpImage
    {
        public OldPaperDollContainer2(int x, int y, int hue) : base(x, y, 11, hue)
        {
        }

        public OldPaperDollContainer2(int x, int y) : base(x, y, 11, 0)
        {
        }
    }

    public class SmallHumanMaleBody : GumpImage
    {
        public SmallHumanMaleBody(int x, int y, int hue) : base(x, y, 12, hue)
        {
        }

        public SmallHumanMaleBody(int x, int y) : base(x, y, 12, 0)
        {
        }
    }

    public class SmallHumanFemaleBody : GumpImage
    {
        public SmallHumanFemaleBody(int x, int y, int hue) : base(x, y, 13, hue)
        {
        }

        public SmallHumanFemaleBody(int x, int y) : base(x, y, 13, 0)
        {
        }
    }
}