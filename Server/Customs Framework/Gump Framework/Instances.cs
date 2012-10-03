using System.Collections.Generic;
using System.IO;
using Server;
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
            if (this._User == null || this._User.Deleted)
                return;

            if (_InternalInstances == null)
                _InternalInstances = new Dictionary<int, GumpPlus>();

            if (!_InternalInstances.ContainsKey(this.Serial))
                _InternalInstances.Add(this.Serial, this);
            else
                _InternalInstances[this.Serial] = this;

            if (_Instances == null)
                _Instances = new Dictionary<Mobile, List<GumpPlus>>();

            if (!_Instances.ContainsKey(this._User))
                _Instances.Add(this._User, new List<GumpPlus>());
            else if (_Instances[this._User] == null)
                _Instances[this._User] = new List<GumpPlus>();

            if (!_Instances[this._User].Contains(this))
                _Instances[this._User].Add(this);
        }

        protected virtual void UnregisterInstance()
        {
            if (_InternalInstances != null && _InternalInstances.ContainsKey(this.Serial))
                _InternalInstances.Remove(this.Serial);

            if (this.User == null)
                return;

            if (_Instances == null || !_Instances.ContainsKey(this.User) || _Instances[this.User] == null || !_Instances[this.User].Contains(this))
                return;

            _Instances[this.User].Remove(this);
        }
    }
}