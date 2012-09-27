using System.IO;
using System.Collections.Generic; 
using Server;
using Server.Gumps;
using Server.Network;

namespace CustomsFramework.GumpPlus
{
    public abstract partial class GumpPlus
    {
        private static Dictionary<int, GumpPlus> _InternalInstances;
        private static Dictionary<Mobile, List<GumpPlus>> _Instances;

        public static Dictionary<Mobile, List<GumpPlus>> Instances
        {
            get
            {
                return _Instances;
            }
        }

        private static void Initialize()
        {
            _InternalInstances = new Dictionary<int, GumpPlus>();
            _Instances = new Dictionary<Mobile, List<GumpPlus>>();

            EventSink.Speech += delegate(SpeechEventArgs e)
            {
                Mobile user = e.Mobile;

                if (user == null || user.Deleted || user.NetState == null)
                    return;

                if (_Instances.ContainsKey(user))
                {
                    foreach (GumpPlus gump in _Instances[user])
                    {
                        if (gump == null)
                            continue;

                        gump.OnSpeech(e);

                        if (e.Blocked)
                            break;
                    }
                }
            };

            OutgoingPacketOverrides.Register(0xB0, true, OnEncode0xB0_0xDD);
            OutgoingPacketOverrides.Register(0xDD, true, OnEncode0xB0_0xDD);
        }

        private static void OnEncode0xB0_0xDD(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
        {
            if (state == null || reader == null || buffer == null || length < 0)
                return;

            int pos = reader.Seek(0, SeekOrigin.Current);
            reader.Seek(3, SeekOrigin.Begin);
            int serial = reader.ReadInt32();
            reader.Seek(pos, SeekOrigin.Begin);

            if (serial < 0 || !_InternalInstances.ContainsKey(serial))
                return;

            CompileCheckOnSend(serial);
        }

        private static void CompileCheckOnSend(int serial)
        {
            GumpPlus gump = _InternalInstances[serial];

            if (!gump.Compiled)
                gump.Refresh(true, true);
        }

        protected virtual void RegisterInstance()
        {
            if (_User == null || _User.Deleted)
                return;

            if (_InternalInstances == null)
                _InternalInstances = new Dictionary<int, GumpPlus>();

            if (!_InternalInstances.ContainsKey(Serial))
                _InternalInstances.Add(Serial, this);
            else
                _InternalInstances[Serial] = this;

            if (_Instances == null)
                _Instances = new Dictionary<Mobile, List<GumpPlus>>();

            if (!_Instances.ContainsKey(_User))
                _Instances.Add(_User, new List<GumpPlus>());
            else if (_Instances[_User] == null)
                _Instances[_User] = new List<GumpPlus>();

            if (!_Instances[_User].Contains(this))
                _Instances[_User].Add(this);
        }

        protected virtual void UnregisterInstance()
        {
            if (_InternalInstances != null && _InternalInstances.ContainsKey(Serial))
                _InternalInstances.Remove(Serial);

            if (User == null)
                return;

            if (_Instances == null || !_Instances.ContainsKey(User) || _Instances[User] == null || !_Instances[User].Contains(this))
                return;

            _Instances[User].Remove(this);
        }
    }
}
