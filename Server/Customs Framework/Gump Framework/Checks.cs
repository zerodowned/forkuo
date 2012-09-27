using Server.Gumps;

namespace CustomsFramework.GumpPlus
{
    public class CheckPlus : GumpCheck
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

        public CheckPlus(int x, int y, int inactiveID, int activeID, bool initialState,
            int switchID, string name, int group, CheckResponse callback)
            : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            _Name = name;
            _Group = group;
            _Callback = callback;
            _Param = null;
        }

        public CheckPlus(int x, int y, int inactiveID, int activeID, bool initialState,
            int switchID, string name, int group, CheckParamResponse callback, object param)
            : base(x, y, inactiveID, activeID, initialState, switchID)
        {
            _Name = name;
            _Group = group;
            _Callback = callback;
            _Param = param;
        }

        public void Invoke(bool switched)
        {
            if (_Callback is CheckResponse)
                ((CheckResponse)_Callback)(switched);
            else if (_Callback is CheckParamResponse)
                ((CheckParamResponse)_Callback)(switched, _Param);
        }
    }
}
