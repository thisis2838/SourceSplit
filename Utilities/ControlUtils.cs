using LiveSplit.SourceSplit.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                if (IsInvocationRequired(ctrl)) ctrl.Invoke(a);
                else a();
            }
            catch (Exception ex)
            {
                throw ErrorDialog.Throw($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static T InvokeIfRequired<T>(this Control ctrl, Func<T> get)
        {
            try
            {
                if (IsInvocationRequired(ctrl)) return (T)ctrl.Invoke(get);
                else return (T)get.Invoke();
            }
            catch (Exception ex)
            {
                throw ErrorDialog.Throw($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static IAsyncResult BeginInvokeIfRequired(this Control ctrl, Action a)
        {
            try
            {
                if (IsInvocationRequired(ctrl)) return ctrl.BeginInvoke(a);
                else
                {
                    a();
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ErrorDialog.Throw($"Unhandled exception while invoking action on control {ctrl.Name}.", ex);
            }
        }

        public static void InvokeWithTimeout(this Control ctrl, int msTimeout, Action a)
        {
            var res = BeginInvokeIfRequired(ctrl, a);
            if (res is null) return;

            if (!res.AsyncWaitHandle.WaitOne(msTimeout))
            {
                throw ErrorDialog.Throw($"Timed out after {msTimeout}ms invoking action on control {ctrl.Name}.");
            }
        }
    }
}
