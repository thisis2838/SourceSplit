using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Anniversary : GameSupport
    {
        public HL2Anniversary()
        {
            AdditionalGameSupport.Add(new HL2());
            AdditionalGameSupport.Add(new HL2Ep1());
            AdditionalGameSupport.Add(new HL2Ep2());
            AdditionalGameSupport.Add(new LostCoast());
        }
    }
}
