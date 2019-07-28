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
        public async Task ShouldReplicate()
        {
         
            var replicaStateHandler = new StateHolderEventHandler();
            var replicaCleanerHandler = new CleanerEventHandler();

            using (var replica = new FxPricingEngine(replicaStateHandler, replicaCleanerHandler))
            {
                var ioHandler = new IOPersistanceEventHandler();
                var stateHandler = new StateHolderEventHandler();
                var cleanerHandler = new CleanerEventHandler();
                var replicatorHandler = new ReplicatorEventHandler(replica);

                using (var engine = new FxPricingEngine(ioHandler, replicatorHandler, stateHandler, cleanerHandler))
                {
                    using (var publisher = new FxPricePublisher(engine))
                    {
                        publisher.PublishOne();

                        await Task.Delay(100);

                        Assert.AreEqual(replicaStateHandler.State.Count, stateHandler.State.Count);

                    }
                }
            }
        }

        [Test]
        public async Task ShouldTestTranslator()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            using (var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler))
            {
                var translator = new MarketDataCsvTranslator();

                var signal = "EUR/USD;1.11;1.22;636999249863636252;Harmony";

                engine.Disruptor.PublishEvent(translator, signal);

                await Task.Delay(100);

                Assert.AreEqual(1, stateHandler.State.Count);
                Assert.AreEqual(1, ioHandler.WriteCount);

            }

        }

        [Test]
        public async Task ShouldTestBatchEventPublish()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            using (var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler))
            {
                using (var publisher = new FxPricePublisher(engine))
                {
                  

                    publisher.PublishBatch(20);

                    await Task.Delay(100);

                    Assert.Greater(stateHandler.State.Count, 0);
                    Assert.AreEqual(20, stateHandler.State.SelectMany(s => s.Value.GetAll()).Count());
                    Assert.AreEqual(20, ioHandler.WriteCount);
                }
            }
        }

        [Test]
        public async Task ShouldTestOneEventPublish()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            using (var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler))
            {
                using (var publisher = new FxPricePublisher(engine))
                {

                    publisher.PublishOne();

                    await Task.Delay(100);

                    Assert.AreEqual(1, stateHandler.State.Count);
                    Assert.AreEqual(1, ioHandler.WriteCount);
                }
            }

        }

        [Test]
        public async Task ShouldPublishLooootsOfEvents()
        {
            var ioHandler = new IOPersistanceEventHandler();
            var stateHandler = new StateHolderEventHandler();
            var cleanerHandler = new CleanerEventHandler();

            using (var engine = new FxPricingEngine(ioHandler, stateHandler, cleanerHandler))
            {
                using (var publisher = new FxPricePublisher(engine))
                {

                    publisher.StartGenerateEvents();

                    await Task.Delay(2000);

                    var count = stateHandler.State.SelectMany(m => m.Value.GetAll()).Count();

                    Assert.Greater(count, publisher.Cache.Count * 0.90);
                    Assert.Greater(ioHandler.WriteCount, publisher.Cache.Count * 0.90);

                }
            }
        }

    }
}
