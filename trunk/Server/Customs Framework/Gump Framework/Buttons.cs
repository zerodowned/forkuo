using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class ButtonPlus : GumpButton
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

        public ButtonPlus(int x, int y, int normalID, int pressedID, int buttonID, string name, ButtonResponse callback) : base(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = null;
        }

        public ButtonPlus(int x, int y, int normalID, int pressedID, int buttonID, string name, ButtonParamResponse callback, object param) : base(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0)
        {
            this._Name = name;
            this._Callback = callback;
            this._Param = param;
        }

        public void Invoke()
        {
            if (this._Callback is ButtonResponse)
                ((ButtonResponse)this._Callback)();
            else if (this._Callback is ButtonParamResponse)
                ((ButtonParamResponse)this._Callback)(this._Param);
        }
    }
}