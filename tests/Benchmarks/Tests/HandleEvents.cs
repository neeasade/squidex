﻿// ==========================================================================
//  HandleEvents.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Benchmarks.Tests.TestData;
using Benchmarks.Utils;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;

// ReSharper disable InvertIf

namespace Benchmarks.Tests
{
    public sealed class HandleEvents : IBenchmark
    {
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry().Map(typeof(MyEvent));
        private readonly EventDataFormatter formatter;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private const int NumEvents = 5000;
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IEventStore eventStore;
        private IEventNotifier eventNotifier;
        private IEventConsumerInfoRepository eventConsumerInfos;
        private EventReceiver eventReceiver;
        private MyEventConsumer eventConsumer;

        public string Id
        {
            get { return "handleEvents"; }
        }

        public string Name
        {
            get { return "Handle Events"; }
        }

        public HandleEvents()
        {
            serializerSettings.Converters.Add(new PropertiesBagConverter());

            formatter = new EventDataFormatter(typeNameRegistry, serializerSettings);
        }

        public void Initialize()
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }

        public void RunInitialize()
        {
            mongoDatabase = mongoClient.GetDatabase(Guid.NewGuid().ToString());

            var log = new SemanticLog(new ILogChannel[0], new ILogAppender[0], () => new JsonLogWriter(Formatting.Indented, true));

            eventConsumerInfos = new MongoEventConsumerInfoRepository(mongoDatabase);
            eventConsumer = new MyEventConsumer(NumEvents);
            eventNotifier = new DefaultEventNotifier(new InMemoryPubSub());

            eventStore = new MongoEventStore(mongoDatabase, eventNotifier);
            eventStore.Warmup();

            eventReceiver = new EventReceiver(formatter, eventStore, eventConsumerInfos, log);
            eventReceiver.Subscribe(eventConsumer);
        }

        public long Run()
        {
            var streamName = Guid.NewGuid().ToString();

            for (var eventId = 0; eventId < NumEvents; eventId++)
            {
                var eventData = formatter.ToEventData(new Envelope<IEvent>(new MyEvent { EventNumber = eventId + 1 }), Guid.NewGuid());

                eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventId - 1, new [] { eventData }).Wait();
            }

            eventConsumer.WaitAndVerify();

            return NumEvents;
        }

        public void RunCleanup()
        {
            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);

            eventReceiver.Dispose();
        }

        public void Cleanup()
        {
        }
    }
}
