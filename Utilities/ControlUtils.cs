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
        public static void InvokeIfRequired(this Control ctrl, Action a)
        {
            try
            {
                var required = false;
                try
                {
                    required = ctrl.InvokeRequired;
                }
                catch (InvalidOperationException)
                {
                    // TODO: is this safe?
                    required = true;
                }
                catch (Exception)
                {
                    throw;
                }

                if (required) ctrl.Invoke(a);
                else a();
            }
            catch (Exception ex)
            {
                new ErrorDialog($"Unhandled exception while invoking action on control.", true, ex);
            }
        }

        public static T InvokeIfRequired<T>(this Control ctrl, Func<T> get)
        {
            try
            {
                var required = false;
                try
                {
                    required = ctrl.InvokeRequired;
                }
                catch (InvalidOperationException)
                {
                    // TODO: is this safe?
                    required = true;
                }
                catch (Exception)
                {
                    throw;
                }

                if (required) return (T)ctrl.Invoke(get);
                else return get();
            }
            catch (Exception ex)
            {
                new ErrorDialog($"Unhandled exception while invoking action on control.", true, ex);
                throw;
            }
        }
    }
}
