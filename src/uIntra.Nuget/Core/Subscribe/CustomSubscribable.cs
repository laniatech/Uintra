﻿using System;
using System.Collections.Generic;
using uIntra.Core.TypeProviders;
using uIntra.Subscribe;

namespace uIntra.Core.Subscribe
{
    public class CustomSubscribable : ISubscribable
    {
        public Guid Id { get; set; }

        public IEnumerable<global::uIntra.Subscribe.Subscribe> Subscribers { get; set; }

        public IIntranetType Type { get; set; }
    }
}