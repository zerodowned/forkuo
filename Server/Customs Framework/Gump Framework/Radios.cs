using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class RadioPlus : GumpRadio
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

        public RadioPlus(int x, int y, int inactiveID, int activeID, bool initialState, int switchID,
            int groupID, string name, RadioResponse callback) : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            this._Group = groupID;
            this._Name = name;
            this._Callback = callback;
            this._Param = null;
        }

        public RadioPlus(int x, int y, int inactiveID, int activeID, bool initialState, int switchID,
            int groupID, string name, RadioParamResponse callback, object param) : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            this._Group = groupID;
            this._Name = name;
            this._Callback = callback;
            this._Param = param;
        }

        public void Invoke(bool switched)
        {
            if (this._Callback is RadioResponse)
                ((RadioResponse)this._Callback)(switched);
            else if (this._Callback is RadioParamResponse)
                ((RadioParamResponse)this._Callback)(switched, this._Param);
        }
    }
}