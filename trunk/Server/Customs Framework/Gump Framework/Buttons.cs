using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class ButtonPlus : GumpButton
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

        public ButtonPlus(int x, int y, int normalID, int pressedID, int buttonID, string name, ButtonResponse callback)
            : base(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0)
        {
            _Name = name;
            _Callback = callback;
            _Param = null;
        }

        public ButtonPlus(int x, int y, int normalID, int pressedID, int buttonID, string name, ButtonParamResponse callback, object param)
            : base(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0)
        {
            _Name = name;
            _Callback = callback;
            _Param = param;
        }

        public void Invoke()
        {
            if (_Callback is ButtonResponse)
                ((ButtonResponse)_Callback)();
            else if (_Callback is ButtonParamResponse)
                ((ButtonParamResponse)_Callback)(_Param);
        }
    }
}
