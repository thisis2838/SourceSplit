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
    public static class ThreadUtils
    {
        public static void InvokeIfRequired(this Control ctrl, Action a)
        {
            try
            {
                if (ctrl.InvokeRequired) ctrl.Invoke(a);
                else a();
            }
            catch (Exception e)
            {
                new ErrorDialog($"Thread invocation has gone wrong...\n\n{e}");
                throw e;
            }
        }

        public static T InvokeIfRequired<T>(this Control ctrl, Func<T> get)
        {
            try
            {
                if (ctrl.InvokeRequired) return (T)ctrl.Invoke(get);
                else return get();
            }
            catch (Exception e)
            {
                new ErrorDialog($"Thread invocation has gone wrong...\n\n{e}");
                throw e;
            }
        }
    }
}
