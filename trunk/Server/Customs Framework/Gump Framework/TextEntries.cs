using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class TextEntryPlus : GumpTextEntry
    {
        private string _Name;
        private object _Callback;
        private object _Param;

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public object Param
        {
            get
            {
                return _Param;
            }
            set
            {
                _Param = value;
            }
        }

        public TextEntryPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputResponse callback)
            : base(x, y, width, height, hue, entryID, initialText)
        {
            _Name = name;
            _Callback = callback;
            _Param = null;
        }

        public TextEntryPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputParamResponse callback, object param)
            : base(x, y, width, height, hue, entryID, initialText)
        {
            _Name = name;
            _Callback = callback;
            _Param = param;
        }

        public void Invoke(string input)
        {
            if (_Callback is TextInputResponse)
                ((TextInputResponse)_Callback)(input);
            else if (_Callback is TextInputParamResponse)
                ((TextInputParamResponse)_Callback)(input, _Param);
        }
    }

    public class TextEntryLimitedPlus : GumpTextEntryLimited
    {
        private string _Name;
        private object _Callback;
        private object _Param;

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public TextEntryLimitedPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputResponse callback)
            : base(x, y, width, height, hue, entryID, initialText, size)
        {
            _Name = name;
            _Callback = callback;
            _Param = null;
        }

        public TextEntryLimitedPlus(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param)
            : base(x, y, width, height, hue, entryID, initialText, size)
        {
            _Name = name;
            _Callback = callback;
            _Param = param;
        }

        public void Invoke(string input)
        {
            if (_Callback is LimitedTextInputResponse)
                ((LimitedTextInputResponse)_Callback)(input);
            else if (_Callback is LimitedTextInputParamResponse)
                ((LimitedTextInputParamResponse)_Callback)(input, _Param);
        }
    }
}
