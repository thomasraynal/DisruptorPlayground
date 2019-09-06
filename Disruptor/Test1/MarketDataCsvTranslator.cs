using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Disruptor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class MarketDataCsvTranslator : IEventTranslatorOneArg<FxPricingEvent, String>
    {
        class MarketData
        {
            [Index(0)]
            public string CcyPair { get; set; }
            [Index(1)]
            public double Bid { get; set; }
            [Index(2)]
            public double Ask { get; set; }
            [Index(3)]
            public long Timestamp { get; set; }
            [Index(4)]
            public string Marketplace { get; set; }
        }

        public void TranslateTo(FxPricingEvent @event, long sequence, String arg0)
        {
            using(var reader = new StringReader(arg0))
            using (var csvReader = new CsvReader(reader))
            {
                csvReader.Configuration.CultureInfo = CultureInfo.InvariantCulture;
                csvReader.Configuration.HasHeaderRecord = false;

                var md = csvReader.GetRecords<MarketData>().First();

                @event.CcyPair = md.CcyPair;
                @event.Ask = md.Ask;
                @event.Bid = md.Bid;
                @event.Timestamp = md.Timestamp;
                @event.Marketplace = md.Marketplace;
            }

        }
    }
}
