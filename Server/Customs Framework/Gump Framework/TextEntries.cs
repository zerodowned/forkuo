using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class TextEntryPlus : GumpTextEntry
    {
        private readonly string _Name;
        private readonly object _Callback;
        private object _Param;

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public object Param
        {
            get
            {
                return this._Param;
            }
            set
            {
                this._Param = value;
            }
        }

        public TextEntryPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputResponse callback) : base(x, y, width, height, hue, entryID, initialText)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = null;
        }

        public TextEntryPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputParamResponse callback, object param) : base(x, y, width, height, hue, entryID, initialText)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = param;
        }

        public void Invoke(string input)
        {
            if (this._Callback is TextInputResponse)
                ((TextInputResponse)this._Callback)(input);
            else if (this._Callback is TextInputParamResponse)
                ((TextInputParamResponse)this._Callback)(input, this._Param);
        }
    }

    public class TextEntryLimitedPlus : GumpTextEntryLimited
    {
        private readonly string _Name;
        private readonly object _Callback;
        private readonly object _Param;

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public TextEntryLimitedPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputResponse callback) : base(x, y, width, height, hue, entryID, initialText, size)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = null;
        }

        public TextEntryLimitedPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param) : base(x, y, width, height, hue, entryID, initialText, size)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = param;
        }

        public void Invoke(string input)
        {
            if (this._Callback is LimitedTextInputResponse)
                ((LimitedTextInputResponse)this._Callback)(input);
            else if (this._Callback is LimitedTextInputParamResponse)
                ((LimitedTextInputParamResponse)this._Callback)(input, this._Param);
        }
    }
}