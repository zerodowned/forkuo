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
                return this._Compiled;
            }
        }

        public virtual Mobile User
        {
            get
            {
                return this._User;
            }
            set
            {
                this._User = value;
            }
        }

        public virtual bool EnableMacroProtection
        {
            get
            {
                return this._EnableMacroProtection;
            }
            set
            {
                this._EnableMacroProtection = value;
            }
        }

        public virtual bool BlockSpeech
        {
            get
            {
                return this._BlockSpeech;
            }
            set
            {
                this._BlockSpeech = value;
            }
        }

        public virtual bool IsOpen
        {
            get
            {
                return this._IsOpen;
            }
        }

        new public virtual List<GumpEntry> Entries
        {
            get
            {
                return this._Entries;
            }
        }
        #endregion
        #region Entry ID's
        // Buttons
        private List<int> _UsedButtonIDs;
        private int _NewButtonID = 1;

        protected int NewButtonID()
        {
            int id = this._NewButtonID;

            if (!this._UsedButtonIDs.Contains(id))
            {
                this._UsedButtonIDs.Add(id);
                this._NewButtonID++;
                return id;
            }
            else
            {
                this._NewButtonID++;
                return this.NewButtonID();
            }
        }

        protected int RandomButtonID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!this._UsedButtonIDs.Contains(id))
            {
                this._UsedButtonIDs.Add(id);
                return id;
            }
            else
                return this.RandomButtonID();
        }

        // Text Entries
        private List<int> _UsedTextEntryIDs;
        private int _NewTextEntryID = 0;

        protected int NewTextEntryID()
        {
            int id = this._NewTextEntryID;

            if (!this._UsedTextEntryIDs.Contains(id))
            {
                this._UsedTextEntryIDs.Add(id);
                this._NewTextEntryID++;
                return id;
            }
            else
            {
                this._NewTextEntryID++;
                return this.NewTextEntryID();
            }
        }

        protected int RandomTextEntryID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!this._UsedTextEntryIDs.Contains(id))
            {
                this._UsedTextEntryIDs.Add(id);
                return id;
            }
            else
                return this.RandomTextEntryID();
        }

        // Switch Entries
        private List<int> _UsedSwitchIDs;
        private int _NewSwitchID = 0;

        protected int NewSwitchID()
        {
            int id = this._NewSwitchID;

            if (!this._UsedSwitchIDs.Contains(id))
            {
                this._UsedSwitchIDs.Add(id);
                this._NewSwitchID++;
                return id;
            }
            else
            {
                this._NewSwitchID++;
                return this.NewSwitchID();
            }
        }

        protected int RandomSwitchID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!this._UsedSwitchIDs.Contains(id))
            {
                this._UsedSwitchIDs.Add(id);
                return id;
            }
            else
                return this.RandomTextEntryID();
        }

        // Radio Entries
        private List<int> _UsedRadioIDs;
        private int _NewRadioID = 0;

        protected int NewRadioID()
        {
            int id = this._NewRadioID;

            if (!this._UsedRadioIDs.Contains(id))
            {
                this._UsedRadioIDs.Add(id);
                this._NewRadioID++;
                return id;
            }
            else
            {
                this._NewRadioID++;
                return this.NewSwitchID();
            }
        }

        protected int RandomRadioID()
        {
            int id = Utility.RandomMinMax(1, 65535);

            if (!this._UsedRadioIDs.Contains(id))
            {
                this._UsedRadioIDs.Add(id);
                return id;
            }
            else
                return this.RandomRadioID();
        }

        #endregion
        #region Constructors
        public GumpPlus(int x, int y) : base(x, y)
        {
            this._UsedButtonIDs = new List<int>();
            this._UsedTextEntryIDs = new List<int>();
            this._UsedSwitchIDs = new List<int>();
            this._UsedRadioIDs = new List<int>();

            this._Entries = new List<GumpEntry>();
        }

        public GumpPlus(Mobile from, int x, int y) : this(x, y)
        {
            this._User = from;
        }

        #endregion

        public virtual GumpPlus Refresh()
        {
            return this.Refresh(true);
        }

        public virtual GumpPlus Refresh(bool openIfClosed)
        {
            return this.Refresh(openIfClosed, false);
        }

        public virtual GumpPlus Refresh(bool openIfClosed, bool recompile)
        {
            GumpPlus gump = this;
            Type type = this.GetType();

            try
            {
                if (!this._IsOpen && openIfClosed)
                {
                    if (recompile)
                        return this.Send();

                    this._IsOpen = this._User.SendGump(this, false);
                    return this;
                }

                if (this._IsOpen)
                    this._User.CloseGump(type);

                if (recompile)
                    return this.Send();

                this._IsOpen = this._User.SendGump(this, false);
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
                this.Entries.Clear();
                this.Compile();

                this._Compiled = true;
                this._User.CloseGump(this.GetType());
                this._IsOpen = this._User.SendGump(this, false);
            }
            catch (Exception error)
            {
                Console.WriteLine("GumpPlus '{0}' could not be sent.", this.GetType().FullName);
                Console.WriteLine("Message: {0}", error.Message);
                Console.WriteLine("Stack Trace: {0}", error.StackTrace);
            }

            return this;
        }

        public virtual GumpPlus Close()
        {
            return this.Close(false);
        }

        public virtual GumpPlus Close(bool all)
        {
            if (this._IsOpen)
                this._User.CloseGump(this.GetType());

            if (this._Parent != null)
            {
                if (all)
                {
                    if (this._Parent is GumpPlus)
                        ((GumpPlus)this._Parent).Close(all);
                    else
                        this._User.CloseGump(this._Parent.GetType());
                }
                else
                {
                    if (this._Parent is GumpPlus)
                        ((GumpPlus)this._Parent).Refresh();
                    else
                        this._User.SendGump(this._Parent);
                }
            }

            return this;
        }

        protected virtual void OnSpeech(SpeechEventArgs e)
        {
            e.Blocked = (this._IsOpen && this._BlockSpeech);
        }

        #region Linked Gumps
        private Gump _Parent;
        private List<GumpPlus> _Children = new List<GumpPlus>();

        public virtual Gump Parent
        {
            get
            {
                return this._Parent;
            }
            set
            {
                if (this._Parent == value)
                    return;

                if (this._Parent != null && this._Parent is GumpPlus)
                    ((GumpPlus)this._Parent).RemoveChild(this);

                this._Parent = value;

                if (this._Parent != null && this._Parent is GumpPlus)
                    ((GumpPlus)this._Parent).AddChild(this);
            }
        }

        public virtual List<GumpPlus> Children
        {
            get
            {
                return this._Children;
            }
            set
            {
                this._Children = value;
            }
        }

        public virtual bool AddChild(GumpPlus child)
        {
            if (child == null)
                return false;

            if (!this._Children.Contains(child))
            {
                if (child.Parent != this)
                    child.Parent = this;

                this._Children.Add(child);
                return true;
            }
            else
                return false;
        }

        public virtual bool RemoveChild(GumpPlus child)
        {
            if (child == null)
                return false;

            if (this._Children.Contains(child))
            {
                child.Parent = null;
                this._Children.Remove(child);
                return true;
            }
            else
                return false;
        }

        public virtual bool HasChild(GumpPlus child)
        {
            return this.HasChild(child, false);
        }

        public virtual bool HasChild(GumpPlus child, bool distantRelative)
        {
            if (this._Parent == null || child == null)
                return false;

            if (this._Children.Contains(child))
                return true;

            if (distantRelative)
            {
                foreach (GumpPlus grandChild in this._Children)
                {
                    if (grandChild.HasChild(child, distantRelative))
                        return true;
                }
            }

            return false;
        }

        public virtual bool IsChildOf(GumpPlus parent)
        {
            return this.IsChildOf(parent, false);
        }

        public virtual bool IsChildOf(GumpPlus parent, bool distantRelative)
        {
            if (this._Parent == null || parent == null)
                return false;

            if (this._Parent == parent)
                return true;

            if (distantRelative)
            {
                if (this._Parent is GumpPlus)
                    return ((GumpPlus)this._Parent).IsChildOf(parent, distantRelative);
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
                return this._DefaultButtonHandler;
            }
            set
            {
                this._DefaultButtonHandler = value;
            }
        }

        new public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            this.Add(new ButtonPlus(x, y, normalID, pressedID, buttonID, String.Format("Button:{0}", buttonID), DefaultButtonHandler));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, string name, ButtonResponse callback)
        {
            this.Add(new ButtonPlus(x, y, normalID, pressedID, (_EnableMacroProtection ? this.RandomButtonID() : this.NewButtonID()), name, callback));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, string name, ButtonParamResponse callback, object param)
        {
            this.Add(new ButtonPlus(x, y, normalID, pressedID, (_EnableMacroProtection ? this.RandomButtonID() : this.NewButtonID()), name, callback, param));
        }

        // Overrides
        public void AddButton(int x, int y, int normalID, int pressedID, ButtonResponse callback)
        {
            this.AddButton(x, y, normalID, pressedID, "", callback);
        }

        public void AddButton(int x, int y, int normalID, int pressedID, ButtonParamResponse callback, object param)
        {
            this.AddButton(x, y, normalID, pressedID, "", callback, param);
        }

        public void AddButton(int x, int y, int buttonID, string name, ButtonResponse callback)
        {
            this.AddButton(x, y, buttonID, buttonID, name, callback);
        }

        public void AddButton(int x, int y, int buttonID, string name, ButtonParamResponse callback, object param)
        {
            this.AddButton(x, y, buttonID, buttonID, name, callback, param);
        }

        public void AddButton(int x, int y, int buttonID, ButtonResponse callback)
        {
            this.AddButton(x, y, buttonID, buttonID, "", callback);
        }

        public void AddButton(int x, int y, int buttonID, ButtonParamResponse callback, object param)
        {
            this.AddButton(x, y, buttonID, buttonID, "", callback, param);
        }

        #endregion
        #region Text Entries
        private TextInputResponse _DefaultTextInputResponse = delegate(string text) { };

        public TextInputResponse DefaultTextInputResponse
        {
            get
            {
                return this._DefaultTextInputResponse;
            }
            set
            {
                this._DefaultTextInputResponse = value;
            }
        }

        new public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            this.Add(new TextEntryPlus(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), entryID, initialText, DefaultTextInputResponse));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputResponse callback)
        {
            this.Add(new TextEntryPlus(x, y, width, height, hue, name, entryID, initialText, callback));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, TextInputParamResponse callback, object param)
        {
            this.Add(new TextEntryPlus(x, y, width, height, hue, name, entryID, initialText, callback, param));
        }

        // Overrides
        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, TextInputResponse callback)
        {
            this.AddTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, TextInputParamResponse callback, object param)
        {
            this.AddTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, callback, param);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, TextInputResponse callback)
        {
            this.AddTextEntry(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, TextInputParamResponse callback, object param)
        {
            this.AddTextEntry(x, y, width, height, hue, String.Format("TextEntry:{0}", entryID), initialText, callback, param);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string initialText, TextInputResponse callback)
        {
            this.AddTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, callback);
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, string initialText, TextInputParamResponse callback, object param)
        {
            this.AddTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, callback, param);
        }

        #endregion
        #region Limited Text Entries
        private LimitedTextInputResponse _DefaultLimitedTextInputResponse = delegate(string text) { };

        public LimitedTextInputResponse DefaultLimitedTextInputResponse
        {
            get
            {
                return this._DefaultLimitedTextInputResponse;
            }
            set
            {
                this._DefaultLimitedTextInputResponse = value;
            }
        }

        new public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
        {
            this.Add(new TextEntryLimitedPlus(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, DefaultLimitedTextInputResponse));
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputResponse callback)
        {
            this.Add(new TextEntryLimitedPlus(x, y, width, height, hue, name, entryID, initialText, size, callback));
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            this.Add(new TextEntryLimitedPlus(x, y, width, height, hue, name, entryID, initialText, size, callback, param));
        }

        // Overrides
        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, string initialText, int size, LimitedTextInputResponse callback)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string name, string intialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, name, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), intialText, size, callback, param);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size, LimitedTextInputResponse callback)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, String.Format("LimitedTextEntry:{0}", entryID), entryID, initialText, size, callback, param);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string initialText, int size, LimitedTextInputResponse callback)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, size, callback);
        }

        public void AddLimitedTextEntry(int x, int y, int width, int height, int hue, string initialText, int size, LimitedTextInputParamResponse callback, object param)
        {
            this.AddLimitedTextEntry(x, y, width, height, hue, (_EnableMacroProtection ? this.NewTextEntryID() : this.RandomTextEntryID()), initialText, size, callback, param);
        }

        #endregion
        #region Check Entries
        private CheckResponse _DefaultCheckResponse = delegate(bool switched) { };

        public CheckResponse DefaultCheckResponse
        {
            get
            {
                return this._DefaultCheckResponse;
            }
            set
            {
                this._DefaultCheckResponse = value;
            }
        }

        new public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            this.Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), 0, DefaultCheckResponse));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, int group, CheckResponse callback)
        {
            this.Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, name, group, callback));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, int group, CheckParamResponse callback, object param)
        {
            this.Add(new CheckPlus(x, y, inactiveID, activeID, initialState, switchID, name, group, callback, param));
        }

        // Overrides
        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, CheckResponse callback)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewSwitchID() : this.RandomSwitchID()), name, group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, CheckParamResponse callback, object param)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewSwitchID() : this.RandomSwitchID()), name, group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, CheckResponse callback)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, CheckParamResponse callback, object param)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, switchID, String.Format("Check:{0}", switchID), group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int group, CheckResponse callback)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewSwitchID() : this.RandomSwitchID()), group, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int group, CheckParamResponse callback, object param)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewSwitchID() : this.RandomSwitchID()), group, callback, param);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, CheckResponse callback)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, 0, callback);
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, CheckParamResponse callback, object param)
        {
            this.AddCheck(x, y, inactiveID, activeID, initialState, 0, callback, param);
        }

        #endregion
        #region Radio Entries
        private RadioResponse _DefaultRadioResponse = delegate(bool switched) { };

        public RadioResponse DefaultRadioResponse
        {
            get
            {
                return this._DefaultRadioResponse;
            }
            set
            {
                this._DefaultRadioResponse = value;
            }
        }

        new public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            this.Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, 0, String.Format("Radio:{0}", switchID), DefaultRadioResponse));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, string name, RadioResponse callback)
        {
            this.Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, group, name, callback));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, string name, RadioParamResponse callback, object param)
        {
            this.Add(new RadioPlus(x, y, inactiveID, activeID, initialState, switchID, group, name, callback, param));
        }

        // Overrides
        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, RadioResponse callback)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, switchID, group, String.Format("Radio:{0}", switchID), callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, int group, RadioParamResponse callback, object param)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, switchID, group, String.Format("Radio:{0}", switchID), callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, RadioResponse callback)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, switchID, 0, name, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID, string name, RadioParamResponse callback, object param)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, switchID, 0, name, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, RadioResponse callback)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewRadioID() : this.RandomRadioID()), group, name, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, string name, int group, RadioParamResponse callback, object param)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewRadioID() : this.RandomRadioID()), group, name, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int group, RadioResponse callback)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewRadioID() : this.RandomRadioID()), group, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int group, RadioParamResponse callback, object param)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, (_EnableMacroProtection ? this.NewRadioID() : this.RandomRadioID()), group, callback, param);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, RadioResponse callback)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, 0, callback);
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, RadioParamResponse callback, object param)
        {
            this.AddRadio(x, y, inactiveID, activeID, initialState, 0, callback, param);
        }

        #endregion

        new public void Add(GumpEntry entry)
        {
            if (entry.Parent != this)
            {
                entry.Parent = this;
            }
            else if (!this._Entries.Contains(entry))
            {
                this._Entries.Add(entry);
            }
        }

        new public void Remove(GumpEntry entry)
        {
            this._Entries.Remove(entry);
            entry.Parent = null;
        }

        new public void SendTo(NetState state)
        {
            if (this.User == null)
                this.User = state.Mobile;

            state.AddGump((Gump)this);
            state.Send(this.Compile(state));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            foreach (CheckPlus entry in this.GetEntries<CheckPlus>())
                entry.Invoke(info.IsSwitched(entry.SwitchID));

            foreach (RadioPlus entry in this.GetEntries<RadioPlus>())
                entry.Invoke(info.IsSwitched(entry.SwitchID));

            foreach (TextEntryPlus entry in this.GetEntries<TextEntryPlus>())
            {
                TextRelay relay = info.GetTextEntry(entry.EntryID);

                if (relay != null)
                    entry.Invoke(relay.Text);
                else
                    entry.Invoke(String.Empty);
            }

            foreach (TextEntryLimitedPlus entry in this.GetEntries<TextEntryLimitedPlus>())
            {
                TextRelay relay = info.GetTextEntry(entry.EntryID);

                if (relay != null)
                    entry.Invoke(relay.Text);
                else
                    entry.Invoke(String.Empty);
            }

            int buttonID = info.ButtonID;

            if (buttonID == 0)
                this.Close();
            else
            {
                foreach (ButtonPlus entry in this.GetEntries<ButtonPlus>())
                {
                    if (entry.ButtonID == buttonID)
                        entry.Invoke();
                }
            }

            base.OnResponse(sender, info);
        }

        public override void OnServerClose(NetState owner)
        {
            this._IsOpen = false;
            base.OnServerClose(owner);
        }

        public virtual T[] GetEntries<T>() where T : GumpEntry
        {
            List<T> entries = new List<T>(this.Entries.Count);

            foreach (GumpEntry entry in this.Entries)
            {
                if (entry is T)
                {
                    entries.Add((T)entry);
                }
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
            return this.Serial;
        }
    }
}