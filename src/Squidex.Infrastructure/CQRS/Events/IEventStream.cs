﻿// ==========================================================================
//  IEventStream.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventStream : IDisposable
    {
        void Connect(string queueName, Action<EventData> received);
    }
}
