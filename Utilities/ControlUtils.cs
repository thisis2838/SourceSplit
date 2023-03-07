using LiveSplit.SourceSplit.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class ControlUtils
    {
        private static bool IsInvocationRequired(Control ctrl)
        {
            try
            {
                return ctrl.InvokeRequired;
            }
            catch (InvalidOperationException)
            {
                // TODO: is this safe?
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InvokeIfRequired(this Control ctrl, Action a)
        {
            try
            {
                /*
                if (!ctrl.IsHandleCreated)
                {
                    object locker = new object();
                    void callback(object sender, EventArgs e)
                    {
                        a.Invoke();
                        (sender as Control).HandleCreated -= callback;
                        Monitor.Pulse(locker);
                    }
                    ctrl.HandleCreated += callback;
                    Monitor.Wait(locker);
                }
                else*/
                {
                    if (IsInvocationRequired(ctrl)) ctrl.Invoke(a);
                    else a();
                }
            }
            catch (Exception ex)
            {
                throw ErrorDialog.Exception($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static T InvokeIfRequired<T>(this Control ctrl, Func<T> get)
        {
            try
            {
                /*
                if (!ctrl.IsHandleCreated)
                {
                    T val = default; 
                    object locker = new object();
                    void callback(object sender, EventArgs e)
                    {
                        val = get.Invoke();
                        (sender as Control).HandleCreated -= callback;
                        Monitor.Pulse(locker);
                    }
                    ctrl.HandleCreated += callback;
                    Monitor.Wait(locker);
                    return val;
                }
                else*/
                {
                    if (IsInvocationRequired(ctrl)) return (T)ctrl.Invoke(get);
                    else return (T)get.Invoke();
                }

            }
            catch (Exception ex)
            {
                throw ErrorDialog.Exception($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static IAsyncResult BeginInvokeIfRequired(this Control ctrl, Action a)
        {
            try
            {
                /*
                if (!ctrl.IsHandleCreated)
                {
                    void callback(object sender, EventArgs e)
                    {
                        a.Invoke();
                        (sender as Control).HandleCreated -= callback;
                    }
                    ctrl.HandleCreated += callback;
                    return null;
                }
                else*/
                {
                    if (IsInvocationRequired(ctrl)) return ctrl.BeginInvoke(a);
                    else
                    {
                        a();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorDialog.Exception($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static void InvokeWithTimeout(this Control ctrl, int msTimeout, Action a)
        {
            var res = BeginInvokeIfRequired(ctrl, a);
            if (res is null) return;

            if (!res.AsyncWaitHandle.WaitOne(msTimeout))
            {
                throw ErrorDialog.Exception($"Timed out after {msTimeout}ms invoking action on control {ctrl.Name}.");
            }
        }

        public static void AttemptInvoke(this Control ctrl, Action a, int msTimeout, int tries)
        {
            for (int i = tries; i > 0; i--)
            {
                var res = BeginInvokeIfRequired(ctrl, a);
                if (res is null) return;

                if (!res.AsyncWaitHandle.WaitOne(msTimeout))
                {
                    Debug.WriteLine($"Failed to invoke action on control {ctrl.Name} after {msTimeout}ms, {i - 1} tries left.");
                    continue;
                }
                return;
            }

            throw ErrorDialog.Exception($"Failed to invoke action on control {ctrl.Name} after {tries} tries and with {msTimeout}ms timeout.");
        }

        public static bool IsValid(this Control ctrl)
        {
            return ctrl != null && !ctrl.IsDisposed && ctrl.IsHandleCreated;
        }
    }
}
