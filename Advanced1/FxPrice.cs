using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class FxPrice
    {
        public FxPrice(string ccyPair, double bid, double ask)
        {
            CcyPair = ccyPair;
            Bid = bid;
            Ask = ask;
        }

        public string CcyPair { get; }
        public double Bid { get; }
        public double Ask { get; }
        public double Spread => Ask - Bid;
    }
}
