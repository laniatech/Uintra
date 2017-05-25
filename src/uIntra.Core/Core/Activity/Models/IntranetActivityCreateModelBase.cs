﻿using System.ComponentModel.DataAnnotations;

namespace uIntra.Core.Activity.Models
{
    public class IntranetActivityCreateModelBase
    {
        [Required]
        public virtual string Title { get; set; }

        public bool IsPinned { get; set; }

        public int PinDays { get; set; }        
    }
}