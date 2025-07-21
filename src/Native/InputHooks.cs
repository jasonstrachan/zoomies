using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Zoomies.Native
{
    internal static class InputHooks
    {
        // Hook types
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        // Key codes
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12; // Alt key
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LMENU = 0xA4;
        public const int VK_RMENU = 0xA5;

        // Mouse messages
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_MOUSEHWHEEL = 0x020E;

        // Key messages
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        // Delegates
        public delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public int x;
            public int y;
            public int mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Helper methods
        public static int GetWheelDelta(IntPtr lParam)
        {
            var info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            return (short)(info.mouseData >> 16);
        }

        public static bool IsKeyPressed(int vKey)
        {
            return (GetKeyState(vKey) & 0x8000) != 0;
        }

        public static IntPtr GetCurrentModuleHandle()
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                return GetModuleHandle(module?.ModuleName ?? string.Empty);
            }
        }
    }
}