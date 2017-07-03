﻿using System;
using uIntra.Core.TypeProviders;

namespace uIntra.CentralFeed
{
    public interface ICentralFeedItem
    {
        Guid Id { get; }

        IIntranetType Type { get; }

        DateTime PublishDate { get; }

        DateTime ModifyDate { get; }

        bool IsPinned { get; }

        bool IsPinActual { get; }
    }
}