﻿using System.ComponentModel;
using System.Web.Mvc;
using uIntra.Core.Links;

namespace uIntra.Core.ModelBinders
{
    class LinksBinder : ICustomModelBinder
    {
        public const string OverviewFormKey = "links.Overview";
        public const string DetailsFormKey = "links.Details";
        public const string EditFormKey = "links.Edit";
        public const string DetailsNoIdFormKey = "links.DetailsNoId";
        public const string CreatorFormKey = "links.Creator";
        public const string CreateFormKey = "links.Create"; 
         
        public object BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext,
            PropertyDescriptor propertyDescriptor)
        {
            var overviewLink = GetValue(bindingContext.ValueProvider, OverviewFormKey);
            var createLink = GetValue(bindingContext.ValueProvider, CreateFormKey);
            var detailsLink = GetValue(bindingContext.ValueProvider, DetailsFormKey);
            var editLink = GetValue(bindingContext.ValueProvider, EditFormKey);
            var creatorLink = GetValue(bindingContext.ValueProvider, CreatorFormKey);
            var detailsNoIdLink = GetValue(bindingContext.ValueProvider, DetailsNoIdFormKey);

            var result = new ActivityLinks()
            {
                Overview = overviewLink,
                Create = createLink,
                Details = detailsLink,
                Edit = editLink,
                Creator = creatorLink,
                DetailsNoId = detailsNoIdLink
            };

            return result;
        }

        private string GetValue(IValueProvider provider, string key) => provider.GetValue(key)?.AttemptedValue;

    }
}