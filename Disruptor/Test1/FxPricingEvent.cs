using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class FxPricingEvent
    {
        public string CcyPair;
        public string Marketplace;
        public long Timestamp;
        public double Bid;
        public double Ask;
    }
}
