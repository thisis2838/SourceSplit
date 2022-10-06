using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    class TryMany
    {
        private Func<bool> _false;
        private List<Action> _actions;
        private Action _failAction = () => { };

        public TryMany(Func<bool> @false, params Action[] actions)
        {
            _false = @false;
            _actions = new List<Action>();
            actions.ToList().ForEach(x => _actions.Add(x));
        }

        public TryMany(Action failAction, Func<bool> @false, params Action[] actions)
        {
            _failAction = failAction;
            _false = @false;
            _actions = new List<Action>();
            actions.ToList().ForEach(x => _actions.Add(x));
        }

        public bool Begin()
        {
            for (int i = 0; i < _actions.Count() && _false(); i++)
                _actions[i]();

            if (_false())
            {
                _failAction();
                return false;
            }

            return true;
        }
    }
}
