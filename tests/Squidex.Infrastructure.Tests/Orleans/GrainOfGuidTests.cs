﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class GrainOfGuidTests
    {
        private readonly IPersistence<MyGrain.GrainState> persistence = A.Fake<IPersistence<MyGrain.GrainState>>();
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyGrain sut;
        private HandleSnapshot<MyGrain.GrainState> read;

        public sealed class MyGrain : GrainOfGuid<MyGrain.GrainState>
        {
            public sealed class GrainState
            {
                public Guid Id { get; set; }
            }

            public GrainState PublicState
            {
                get { return State; }
            }

            public MyGrain(IStore<Guid> store)
                : base(store)
            {
            }

            public Task PublicWriteAsync()
            {
                return WriteStateAsync();
            }

            public Task PublicClearAsync()
            {
                return ClearStateAsync();
            }
        }

        public GrainOfGuidTests()
        {
            A.CallTo(() => persistence.ReadAsync(EtagVersion.Any))
                .Invokes(_ =>
                {
                    read(new MyGrain.GrainState { Id = id });
                });

            A.CallTo(() => store.WithSnapshots(typeof(MyGrain), id, A<HandleSnapshot<MyGrain.GrainState>>.Ignored))
                .Invokes(new Action<Type, Guid, HandleSnapshot<MyGrain.GrainState>>((type, id, callback) =>
                {
                    read = callback;
                }))
                .Returns(persistence);

            sut = new MyGrain(store);
        }

        [Fact]
        public async Task Should_read_on_activate()
        {
            await sut.ActivateAsync(id);

            Assert.Equal(id, sut.PublicState.Id);

            A.CallTo(() => persistence.ReadAsync(EtagVersion.Any))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_persistence_on_write()
        {
            await sut.ActivateAsync(id);
            await sut.PublicWriteAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(sut.PublicState))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_persistence_on_clear()
        {
            await sut.ActivateAsync(id);
            await sut.PublicClearAsync();

            A.CallTo(() => persistence.DeleteAsync())
                .MustHaveHappened();
        }
    }
}
