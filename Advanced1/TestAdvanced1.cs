using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    [TestFixture]
    public class TestAdvanced1
    {

        [Test]
        public async Task ShouldTestBatchEventPublish()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler);

            engine.Start();

            var publisher = new FxPricePublisher(engine, true);

            publisher.Start();

            publisher.PublishBatch(20);

            await Task.Delay(100);

            Assert.Greater(stateHandler.State.Count, 0);
            Assert.AreEqual(20, stateHandler.State.SelectMany(s => s.Value.GetAll()).Count());
            Assert.AreEqual(20, ioHandler.WriteCount);
        }

        [Test]
        public async Task ShouldTestOneEventPublish()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler);

            engine.Start();

            var publisher = new FxPricePublisher(engine, true);

            publisher.Start();

            publisher.PublishOne();

            await Task.Delay(100);

            Assert.AreEqual(1, stateHandler.State.Count);
            Assert.AreEqual(1, ioHandler.WriteCount);

        }

    }
}
