using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;

namespace CustomsFramework.GumpPlus
{
    #region Entry Response Delegates
    public delegate void ButtonResponse();
    public delegate void ButtonParamResponse(object obj);
    public delegate void TextInputResponse(string text);
    public delegate void TextInputParamResponse(string text, object obj);
    public delegate void LimitedTextInputResponse(string text);
    public delegate void LimitedTextInputParamResponse(string text, object obj);
    public delegate void RadioResponse(bool switched);
    public delegate void RadioParamResponse(bool switched, object obj);
    public delegate void CheckResponse(bool selected);
    public delegate void CheckParamResponse(bool selected, object obj);
    #endregion
    public abstract partial class GumpPlus : Gump
    {
        #region Variables

        private bool _Compiled = false;

        private Mobile _User;

        private bool _EnableMacroProtection;
        private bool _BlockSpeech;
        private bool _IsOpen;

        private List<GumpEntry> _Entries;

        public bool Compiled
        {
            get
            {
                return _Compiled;
            }
        }

        public virtual Mobile User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }

        public virtual bool EnableMacroProtection
        {
            get
            {
                return _EnableMacroProtection;
            }
            set
            {
                _EnableMacroProtection = value;
            }
        }

        public virtual bool BlockSpeech
        {
            get
            {
                return _BlockSpeech;
            }
            set
            {
                _BlockSpeech = value;
            }
        }

        public virtual bool IsOpen
        {
            get
            {
                return _IsOpen;
            }
        }

        new public virtual List<GumpEntry> Entries
        {
            get
            {
                return _Entries;
            }
        }
        #endregion
        #region Entry ID's
        // Buttons
        private List<int> _UsedButtonIDs;
        private int _NewButtonID = 1;

        protected int NewButtonID()
        {
            int id = _NewButtonID;

            if (!_UsedButtonIDs.Contains(id))
            {
                _UsedButtonIDs.Add(id);
                _NewButtonID++;
                return id;
            }
            else
            {
                _NewButtonID++;
                return NewButtonID();
            }
        }

        protected int RandomButtonID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!_UsedButtonIDs.Contains(id))
            {
                _UsedButtonIDs.Add(id);
                return id;
            }
            else
                return RandomButtonID();
        }

        // Text Entries
        private List<int> _UsedTextEntryIDs;
        private int _NewTextEntryID = 0;

        protected int NewTextEntryID()
        {
            int id = _NewTextEntryID;

            if (!_UsedTextEntryIDs.Contains(id))
            {
                _UsedTextEntryIDs.Add(id);
                _NewTextEntryID++;
                return id;
            }
            else
            {
                _NewTextEntryID++;
                return NewTextEntryID();
            }
        }

        protected int RandomTextEntryID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!_UsedTextEntryIDs.Contains(id))
            {
                _UsedTextEntryIDs.Add(id);
                return id;
            }
            else
                return RandomTextEntryID();
        }

        // Switch Entries
        private List<int> _UsedSwitchIDs;
        private int _NewSwitchID = 0;

        protected int NewSwitchID()
        {
            int id = _NewSwitchID;

            if (!_UsedSwitchIDs.Contains(id))
            {
                _UsedSwitchIDs.Add(id);
                _NewSwitchID++;
                return id;
            }
            else
            {
                _NewSwitchID++;
                return NewSwitchID();
            }
        }

        protected int RandomSwitchID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!_UsedSwitchIDs.Contains(id))
            {
                _UsedSwitchIDs.Add(id);
                return id;
            }
            else
                return RandomTextEntryID();
        }

        // Radio Entries
        private List<int> _UsedRadioIDs;
        private int _NewRadioID = 0;

        protected int NewRadioID()
        {
            int id = _NewRadioID;

            if (!_UsedRadioIDs.Contains(id))
            {
                _UsedRadioIDs.Add(id);
                _NewRadioID++;
                return id;
            }
            else
            {
                _NewRadioID++;
                return NewSwitchID();
            }
        }

        protected int RandomRadioID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!_UsedRadioIDs.Contains(id))
            {
                _UsedRadioIDs.Add(id);
                return id;
            }
            else
                return RandomRadioID();
        }
        #endregion
        #region Constructors
        public GumpPlus(int x, int y)
            : base(x, y)
        {
            _UsedButtonIDs = new List<int>();
            _UsedTextEntryIDs = new List<int>();
            _UsedSwitchIDs = new List<int>();
            _UsedRadioIDs = new List<int>();

            _Entries = new List<GumpEntry>();
        }
        public GumpPlus(Mobile from, int x, int y)
            : this(x, y)
        {
            _User = from;
        }
        #endregion

        public virtual GumpPlus Refresh()
        {
            return Refresh(true);
        }

        public virtual GumpPlus Refresh(bool openIfClosed)
        {
            return Refresh(openIfClosed, false);
        }

        public virtual GumpPlus Refresh(bool openIfClosed, bool recompile)
        {
            GumpPlus gump = this;
            Type type = GetType();

            try
            {
                if (!_IsOpen && openIfClosed)
                {
                    if (recompile)
                        return Send();

                    _IsOpen = _User.SendGump(this, false);
                    return this;
                }

                if (_IsOpen)
                    _User.CloseGump(type);

                if (recompile)
                    return Send();

                _IsOpen = _User.SendGump(this, false);
            }
            catch (Exception error)
            {
                Console.WriteLine("GumpPlus '{0}' could not be refreshed.", type.FullName);
                Console.WriteLine("Message: {0}", error.Message);
                Console.WriteLine("Stack Trace: {0}", error.StackTrace);
            }

            return this;
        }

        public virtual GumpPlus Send()
        {
            try
            {
                Entries.Clear();
                Compile();

                _Compiled = true;
                _User.CloseGump(GetType());
                _IsOpen = _User.SendGump(this, false);
            }
            catch (Exception error)
            {
                Console.WriteLine("GumpPlus '{0}' could not be sent.", GetType().FullName);
                Console.WriteLine("Message: {0}", error.Message);
                Console.WriteLine("Stack Trace: {0}", error.StackTrace);
            }

            return this;
        }

        public virtual GumpPlus Close()
        {
            return Close(false);
        }

        public virtual GumpPlus Close(bool all)
        {
            if (_IsOpen)
                _User.CloseGump(GetType());

            if (_Parent != null)
            {
                if (all)
                {
                    if (_Parent is GumpPlus)
                        ((GumpPlus)_Parent).Close(all);
                    else
                        _User.CloseGump(_Parent.GetType());
                }
                else
                {
                    if (_Parent is GumpPlus)
                        ((GumpPlus)_Parent).Refresh();
                    else
                        _User.SendGump(_Parent);
                }
            }

            return this;
        }

        protected virtual void OnSpeech(SpeechEventArgs e)
        {
            e.Blocked = (_IsOpen && _BlockSpeech);
        }
        #region Linked Gumps
        private Gump _Parent;
        private List<GumpPlus> _Children = new List<GumpPlus>();

        public virtual Gump Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                if (_Parent == value)
                    return;

                if (_Parent != null && _Parent is GumpPlus)
                    ((GumpPlus)_Parent).RemoveChild(this);

                _Parent = value;

                if (_Parent != null && _Parent is GumpPlus)
                    ((GumpPlus)_Parent).AddChild(this);
            }
        }

        public virtual List<GumpPlus> Children
        {
            get
            {
                return _Children;
            }
            set
            {
                _Children = value;
            }
        }

        public virtual bool AddChild(GumpPlus child)
        {
            if (child == null)
                return false;

            if (!_Children.Contains(child))
            {
                if (child.Parent != this)
                    child.Parent = this;

                _Children.Add(child);
                return true;
            }
            else
                return false;
        }

        public virtual bool RemoveChild(GumpPlus child)
        {
            if (child == null)
                return false;

            if (_Children.Contains(child))
            {
                child.Parent = null;
                _Children.Remove(child);
                return true;
            }
            else
                return false;
        }

        public virtual bool HasChild(GumpPlus child)
        {
            return HasChild(child, false);
        }

        public virtual bool HasChild(GumpPlus child, bool distantRelative)
        {
            if (_Parent == null || child == null)
                return false;

            if (_Children.Contains(child))
                return true;

            if (distantRelative)
            {
                foreach (GumpPlus grandChild in _Children)
                {
                    if (grandChild.HasChild(child, distantRelative))
                        return true;
                }
            }

            return false;
        }

        public virtual bool IsChildOf(GumpPlus parent)
        {
            return IsChildOf(parent, false);
        }

        public virtual bool IsChildOf(GumpPlus parent, bool distantRelative)
        {
            if (_Parent == null || parent == null)
                return false;

            if (_Parent == parent)
                return true;

            if (distantRelative)
            {
                if (_Parent is GumpPlus)
                    return ((GumpPlus)_Parent).IsChildOf(parent, distantRelative);
            }

            return false;
        }
        #endregion
        #region Buttons
        private ButtonResponse _DefaultButtonHandler = delegate() { };

        public ButtonResponse DefaultButtonHandler
        {
            get
            {
                return _DefaultButtonHandler;
            }
            set
            {
                _DefaultButtonHandler = value;
            }
        }

        new public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            Add(new ButtonPlus(x, y, normalID, pressedID, buttonID, String.Format("Button:{0}", buttonID), DefaultButtonHandler));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, string name, ButtonResponse callback)
        {
            Add(new ButtonPlus(x, y, normalID, pressedID, (_EnableMacroProtection ? RandomButtonID() : NewButtonID()), name, callback));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, string name, ButtonParamResponse callback, object param)
        {
            Add(new ButtonPlus(x, y, normalID, pressedID, (_EnableMacroProtection ? RandomButtonID() : NewButtonID()), name, callback, param));
        }

        // Overrides
        public void AddButton(int x, int y, int normalID, int pressedID, ButtonResponse callback)
        {
            AddButton(x, y, normalID, pressedID, "", callback);
        }

        public void AddButton(int x, int y, int normalID, int pressedID, ButtonParamResponse callback, object param)
        {
            AddButton(x, y, normalID, pressedID, "", callback, param);
        }

        public void AddButton(int x, int y, int buttonID, string name, ButtonResponse callback)
        {
            AddButton(x, y, buttonID, buttonID, name, callback);
        }

        public void AddButton(int x, int y, int buttonID, string name, ButtonParamResponse callback, object param)
        {
            AddButton(x, y, buttonID, buttonID, name, callback, param);
        }

        public void AddButton(int x, int y, int buttonID, ButtonResponse callback)
        {
            AddButton(x, y, buttonID, buttonID, "", callback);
        }

        public void AddButton(int x, int y, int buttonID, ButtonParamResponse callback, object param)
        {
            AddButton(x, y, buttonID, buttonID, "", callback, param);
        }
        #endregion
        #region Text Entries
        private TextInputResponse _DefaultTextInputResponse = delegate(string text) { };

        public TextInputResponse DefaultTextInputResponse
        {
            get
            {
                return _DefaultTextInputResponse;
            }
            set
            {
                _DefaultTextInputResponse = value;
            }
        }

        new public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            Add(new TextEntryPlus(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), entryID, initialText, DefaultTextInputResponse));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputResponse callback)
        {
            Add(new TextEntryPlus(x, y, width, height, hue, name, entryID, initialText, callback));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputParamResponse callback, object param)
        {
            Add(new TextEntryPlus(x, y, width, height, hue, name, entryID, initialText, callback, param));
        }

        // Overrides
        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, TextInputResponse callback)
        {
            AddTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, TextInputParamResponse callback, object param)
        {
            AddTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, callback, param);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, TextInputResponse callback)
        {
            AddTextEntry(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, TextInputParamResponse callback, object param)
        {
            AddTextEntry(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), initialText, callback, param);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string initialText, TextInputResponse callback)
        {
            AddTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string initialText, TextInputParamResponse callback, object param)
        {
            AddTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, callback, param);
        }
        #endregion
        #region Limited Text Entries
        private LimitedTextInputResponse _DefaultLimitedTextInputResponse = delegate(string text) { };

        public LimitedTextInputResponse DefaultLimitedTextInputResponse
        {
            get
            {
                return _DefaultLimitedTextInputResponse;
            }
            set
            {
                _DefaultLimitedTextInputResponse = value;
            }
        }

        new public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
        {
            Add(new TextEntryLimitedPlus(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, DefaultLimitedTextInputResponse));
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputResponse callback)
        {
            Add(new TextEntryLimitedPlus(x, y, width, height, hue, name, entryID, initialText, size, callback));
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            Add(new TextEntryLimitedPlus(x, y, width, height, hue, name, entryID, initialText, size, callback, param));
        }

        // Overrides
        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, int size, LimitedTextInputResponse callback)
        {
            AddLimitedTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, string intialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            AddLimitedTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), intialText, size, callback, param);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size, LimitedTextInputResponse callback)
        {
            AddLimitedTextEntry(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            AddLimitedTextEntry(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, callback, param);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string initialText, int size, LimitedTextInputResponse callback)
        {
            AddLimitedTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            AddLimitedTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? NewTextEntryID() : RandomTextEntryID()), initialText, size, callback, param);
        }
        #endregion
        #region Check Entries
        private CheckResponse _DefaultCheckResponse = delegate(bool switched) { };

        public CheckResponse DefaultCheckResponse
        {
            get
            {
                return _DefaultCheckResponse;
            }
            set
            {
                _DefaultCheckResponse = value;
            }
        }

        new public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), 0, DefaultCheckResponse));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, int group, CheckResponse callback)
        {
            Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, name, group, callback));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, int group, CheckParamResponse callback, object param)
        {
            Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, name, group, callback, param));
        }

        // Overrides
        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, CheckResponse callback)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewSwitchID() : RandomSwitchID()), name, group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, CheckParamResponse callback, object param)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewSwitchID() : RandomSwitchID()), name, group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, CheckResponse callback)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, CheckParamResponse callback, object param)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int group, CheckResponse callback)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewSwitchID() : RandomSwitchID()), group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int group, CheckParamResponse callback, object param)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewSwitchID() : RandomSwitchID()), group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, CheckResponse callback)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, 0, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, CheckParamResponse callback, object param)
        {
            AddCheck(x, y, inactiveID, activeID, initialState, 0, callback, param);
        }
        #endregion
        #region Radio Entries
        private RadioResponse _DefaultRadioResponse = delegate(bool switched) { };

        public RadioResponse DefaultRadioResponse
        {
            get
            {
                return _DefaultRadioResponse;
            }
            set
            {
                _DefaultRadioResponse = value;
            }
        }

        new public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, 0, String.Format("Radio:{0}", switchID), DefaultRadioResponse));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, string name, RadioResponse callback)
        {
            Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, group, name, callback));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, string name, RadioParamResponse callback, object param)
        {
            Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, group, name, callback, param));
        }

        // Overrides
        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, RadioResponse callback)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, switchID, group, String.Format("Radio:{0}", switchID), callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, RadioParamResponse callback, object param)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, switchID, group, String.Format("Radio:{0}", switchID), callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, RadioResponse callback)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, switchID, 0, name, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, RadioParamResponse callback, object param)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, switchID, 0, name, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, RadioResponse callback)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewRadioID() : RandomRadioID()), group, name, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, RadioParamResponse callback, object param)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewRadioID() : RandomRadioID()), group, name, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int group, RadioResponse callback)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewRadioID() : RandomRadioID()), group, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int group, RadioParamResponse callback, object param)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? NewRadioID() : RandomRadioID()), group, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, RadioResponse callback)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, 0, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, RadioParamResponse callback, object param)
        {
            AddRadio(x, y, inactiveID, activeID, initialState, 0, callback, param);
        }
        #endregion

        new public void Add(GumpEntry entry)
        {
            if (entry.Parent != this)
            {
                entry.Parent = this;
            }
            else if (!_Entries.Contains(entry))
            {
                _Entries.Add(entry);
            }
        }

        new public void Remove(GumpEntry entry)
        {
            _Entries.Remove(entry);
            entry.Parent = null;
        }

        new public void SendTo(NetState state)
        {
            if (User == null)
                User = state.Mobile;

            state.AddGump((Gump)this);
            state.Send(Compile(state));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            foreach (CheckPlus entry in GetEntries<CheckPlus>())
                entry.Invoke(info.IsSwitched(entry.SwitchID));

            foreach (RadioPlus entry in GetEntries<RadioPlus>())
                entry.Invoke(info.IsSwitched(entry.SwitchID));

            foreach (TextEntryPlus entry in GetEntries<TextEntryPlus>())
            {
                TextRelay relay = info.GetTextEntry(entry.EntryID);

                if (relay != null)
                    entry.Invoke(relay.Text);
                else
                    entry.Invoke(String.Empty);
            }

            foreach (TextEntryLimitedPlus entry in GetEntries<TextEntryLimitedPlus>())
            {
                TextRelay relay = info.GetTextEntry(entry.EntryID);

                if (relay != null)
                    entry.Invoke(relay.Text);
                else
                    entry.Invoke(String.Empty);
            }

            int buttonID = info.ButtonID;

            if (buttonID == 0)
                Close();
            else
            {
                foreach (ButtonPlus entry in GetEntries<ButtonPlus>())
                {
                    if (entry.ButtonID == buttonID)
                        entry.Invoke();
                }
            }

            base.OnResponse(sender, info);
        }

        public override void OnServerClose(NetState owner)
        {
            _IsOpen = false;
            base.OnServerClose(owner);
        }

        public virtual T[] GetEntries<T>() where T : GumpEntry
        {
            List<T> entries = new List<T>(Entries.Count);

            foreach (GumpEntry entry in Entries)
            {
                if (entry is T)
                { entries.Add((T)entry); }
            }

            return entries.ToArray();
        }

        public override bool Equals(object obj)
        {
            GumpPlus temp = obj as GumpPlus;
            if (temp == null)
                return false;
            return this.Equals(temp);
        }

        public override int GetHashCode()
        {
            return Serial;
        }
    }
}