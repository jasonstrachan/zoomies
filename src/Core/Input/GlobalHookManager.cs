using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Zoomies.Native;

namespace Zoomies.Core.Input
{
    public class GlobalHookManager : IDisposable
    {
        private readonly ILogger<GlobalHookManager> _logger;
        private readonly Dispatcher _dispatcher;

        private IntPtr _keyboardHook = IntPtr.Zero;
        private IntPtr _mouseHook = IntPtr.Zero;
        private InputHooks.LowLevelProc? _keyboardProc;
        private InputHooks.LowLevelProc? _mouseProc;

        private bool _ctrlPressed;
        private bool _altPressed;
        private bool _disposed;

        public event EventHandler<int>? ZoomChanged;
        public event EventHandler? HotkeysReleased;

        public GlobalHookManager(ILogger<GlobalHookManager> logger, Dispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        public void Initialize()
        {
            if (_keyboardHook != IntPtr.Zero || _mouseHook != IntPtr.Zero)
            {
                _logger.LogWarning("Hooks already initialized");
                return;
            }

            try
            {
                // Keep references to prevent garbage collection
                _keyboardProc = KeyboardHookCallback;
                _mouseProc = MouseHookCallback;

                var moduleHandle = InputHooks.GetCurrentModuleHandle();

                _keyboardHook = InputHooks.SetWindowsHookEx(
                    InputHooks.WH_KEYBOARD_LL,
                    _keyboardProc,
                    moduleHandle,
                    0);

                _mouseHook = InputHooks.SetWindowsHookEx(
                    InputHooks.WH_MOUSE_LL,
                    _mouseProc,
                    moduleHandle,
                    0);

                if (_keyboardHook == IntPtr.Zero || _mouseHook == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to set Windows hooks");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize global hooks");
                Cleanup();
                throw;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    var vkCode = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
                    var isKeyDown = wParam == (IntPtr)InputHooks.WM_KEYDOWN ||
                                   wParam == (IntPtr)InputHooks.WM_SYSKEYDOWN;
                    var isKeyUp = wParam == (IntPtr)InputHooks.WM_KEYUP ||
                                 wParam == (IntPtr)InputHooks.WM_SYSKEYUP;

                    // Track Ctrl and Alt state
                    if (vkCode == InputHooks.VK_LCONTROL || vkCode == InputHooks.VK_RCONTROL)
                    {
                        bool wasPressed = _ctrlPressed;
                        _ctrlPressed = isKeyDown;
                        if (wasPressed && !_ctrlPressed)
                        {
                            CheckHotkeysReleased();
                        }
                    }
                    else if (vkCode == InputHooks.VK_LMENU || vkCode == InputHooks.VK_RMENU)
                    {
                        bool wasPressed = _altPressed;
                        _altPressed = isKeyDown;
                        if (wasPressed && !_altPressed)
                        {
                            CheckHotkeysReleased();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in keyboard hook callback");
                    // Don't rethrow - continue with normal hook processing
                }
            }

            try
            {
                return InputHooks.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling next hook");
                return IntPtr.Zero;
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)InputHooks.WM_MOUSEWHEEL)
            {
                try
                {
                    var wheelDelta = InputHooks.GetWheelDelta(lParam);

                    // Check if Ctrl+Alt is pressed
                    if (_ctrlPressed && _altPressed)
                    {
                        // Fire event on UI thread safely
                        try
                        {
                            _dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    ZoomChanged?.Invoke(this, wheelDelta);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error in zoom changed event handler");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error dispatching zoom event");
                        }

                        // Consume the event
                        return (IntPtr)1;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in mouse hook callback");
                }
            }

            try
            {
                return InputHooks.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling next mouse hook");
                return IntPtr.Zero;
            }
        }

        private void CheckHotkeysReleased()
        {
            try
            {
                if (!_ctrlPressed && !_altPressed)
                {
                    _dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            HotkeysReleased?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in hotkeys released event handler");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckHotkeysReleased");
            }
        }

        private void Cleanup()
        {
            if (_keyboardHook != IntPtr.Zero)
            {
                InputHooks.UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = IntPtr.Zero;
            }

            if (_mouseHook != IntPtr.Zero)
            {
                InputHooks.UnhookWindowsHookEx(_mouseHook);
                _mouseHook = IntPtr.Zero;
            }

            _keyboardProc = null;
            _mouseProc = null;
        }

        public void Dispose()
        {
            if (_disposed) return;

            Cleanup();
            _disposed = true;
        }
    }
}