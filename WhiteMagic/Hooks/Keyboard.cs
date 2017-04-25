﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Hooks;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class KeyInfo
    {
        private KBDLLHOOKSTRUCT Raw;

        public KeyInfo(KBDLLHOOKSTRUCT raw)
        {
            Raw = raw;
        }

        public Keys VirtualKey { get { return (Keys)Raw.vkCode; } }
        public ScanCodeShort ScanCode { get { return (ScanCodeShort)Raw.scanCode; } }
        
        public bool IsExtended { get { return Flags.HasFlag(KBDLLHOOKSTRUCT.LLFlags.LLKHF_EXTENDED); } }

        private KBDLLHOOKSTRUCT.LLFlags Flags { get { return (KBDLLHOOKSTRUCT.LLFlags)Raw.flags; } }
    }

    public delegate bool KeyboardMessageHandler(WM mEvent, KeyInfo info);

    public class Keyboard : HookBase<KeyboardMessageHandler>
    {
        public Keyboard() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        public bool LAltPressed { get; private set; }
        public bool RAltPressed { get; private set; }
        public bool LControlPressed { get; private set; }
        public bool RControlPressed { get; private set; }
        public bool LShiftPressed { get; private set; }
        public bool RShiftPressed { get; private set; }

        public bool AltPressed { get { return LAltPressed || RAltPressed; } }
        public bool ControlPressed { get { return LControlPressed || RControlPressed; } }
        public bool ShiftPressed { get { return LShiftPressed || RShiftPressed; } }

        private Dictionary<Keys, bool> SpecialKeyStates = new Dictionary<Keys, bool>();
        void StoreSpecialKeyState(WM Event, KeyInfo info)
        {
            var toggle = Event == WM.KEYDOWN || Event == WM.SYSKEYDOWN;
            switch (info.VirtualKey)
            {
                case Keys.LMenu: LAltPressed = toggle; break;
                case Keys.RMenu: RAltPressed = toggle; break;
                case Keys.LControlKey: LControlPressed = toggle; break;
                case Keys.RControlKey: RControlPressed = toggle; break;
                case Keys.LShiftKey: LShiftPressed = toggle; break;
                case Keys.RShiftKey: RShiftPressed = toggle; break;
                default: break;
            }
        }

        public override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var ev = (WM)wParam;

            try
            {
                var str = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var keyInfo = new KeyInfo(str);

                StoreSpecialKeyState(ev, keyInfo);

                foreach (var Handler in Handlers)
                    if (!Handler(ev, keyInfo))
                        return false;
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
