using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class CheckPlus : GumpCheck
    {
        private readonly string _Name;
        private readonly int _Group;
        private readonly object _Callback;
        private object _Param;

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public int Group
        {
            get
            {
                return this._Group;
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

        public CheckPlus(int x, int y, int inactiveID, int activeID, bool initialState,
            int switchID, string name, int group, CheckResponse callback) : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            this._Name = name;
            this._Group = group;
            this._Callback = callback;
            this._Param = null;
        }

        public CheckPlus(int x, int y, int inactiveID, int activeID, bool initialState,
            int switchID, string name, int group, CheckParamResponse callback, object param) : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            this._Name = name;
            this._Group = group;
            this._Callback = callback;
            this._Param = param;
        }

        public void Invoke(bool switched)
        {
            if (this._Callback is CheckResponse)
                ((CheckResponse)this._Callback)(switched);
            else if (this._Callback is CheckParamResponse)
                ((CheckParamResponse)this._Callback)(switched, this._Param);
        }
    }
}