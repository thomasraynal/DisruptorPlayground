using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    [TestFixture]
    public class TestAdvanced1
    {
        [Test]
        public void ShouldTestOneEventPublish()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler);

            engine.Start();

            var publisher = new FxPricePublisher(engine, true);

            publisher.Start();

            publisher.PublishOne();

            
        

        }

    }
}
