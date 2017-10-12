﻿using System.Collections.Generic;

namespace uIntra.Navigation
{
    public class MenuItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public bool IsHomePage { get; set; }
        public bool IsClickable { get; set; }
        public bool IsHeading { get; set; }

        public List<MenuItemModel> Children { get; set; } = new List<MenuItemModel>();
    }
}
