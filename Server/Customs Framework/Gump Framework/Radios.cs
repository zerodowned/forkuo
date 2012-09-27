using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class RadioPlus : GumpRadio
    {
        private string _Name;
        private int _Group;
        private object _Callback;
        private object _Param;

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public int Group
        {
            get
            {
                return _Group;
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

        public RadioPlus(int x, int y, int inactiveID, int activeID, bool initialState, int switchID,
            int groupID, string name, RadioResponse callback)
            : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            _Group = groupID;
            _Name = name;
            _Callback = callback;
            _Param = null;
        }

        public RadioPlus(int x, int y, int inactiveID, int activeID, bool initialState, int switchID,
            int groupID, string name, RadioParamResponse callback, object param)
            : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            _Group = groupID;
            _Name = name;
            _Callback = callback;
            _Param = param;
        }

        public void Invoke(bool switched)
        {
            if (_Callback is RadioResponse)
                ((RadioResponse)_Callback)(switched);
            else if (_Callback is RadioParamResponse)
                ((RadioParamResponse)_Callback)(switched, _Param);
        }
    }
}
