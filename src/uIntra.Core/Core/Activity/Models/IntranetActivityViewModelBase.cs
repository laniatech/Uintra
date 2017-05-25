﻿using System;

namespace uIntra.Core.Activity.Models
{
    public abstract class IntranetActivityViewModelBase
    {
        public Guid Id { get; set; }
        public bool CanEdit { get; set; }
        public bool IsPinned { get; set; }
        public IntranetActivityDetailsHeaderViewModel HeaderInfo { get; set; }
    }
}