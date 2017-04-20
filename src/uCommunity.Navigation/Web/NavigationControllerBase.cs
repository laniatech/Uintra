﻿using System.Web.Mvc;
using uCommunity.Core.Extentions;
using uCommunity.Navigation.Core;
using uCommunity.Navigation.DefaultImplementation;
using Umbraco.Web.Mvc;

namespace uCommunity.Navigation.Plugin
{
    public class NavigationControllerBase : SurfaceController
    {
        protected virtual string LeftNavigationViewPath { get; } = "~/App_Plugins/Navigation/LeftNavigation/View/Navigation.cshtml";
        protected virtual string SubNavigationViewPath { get; } = "~/App_Plugins/Navigation/SubNavigation/View/Navigation.cshtml";
        protected virtual string TopNavigationViewPath { get; } = "~/App_Plugins/Navigation/TopNavigation/View/Navigation.cshtml";
        protected virtual string MyLinksViewPath { get; } = "~/App_Plugins/Navigation/MyLinks/View/MyLinks.cshtml";

        protected readonly ILeftSideNavigationModelBuilder _leftSideNavigationModelBuilder;
        protected readonly ISubNavigationModelBuilder _subNavigationModelBuilder;
        protected readonly ITopNavigationModelBuilder _topNavigationModelBuilder;
        protected readonly IMyLinksModelBuilder _myLinksModelBuilder;

        protected NavigationControllerBase(
            ILeftSideNavigationModelBuilder leftSideNavigationModelBuilder, 
            ISubNavigationModelBuilder subNavigationModelBuilder, 
            ITopNavigationModelBuilder topNavigationModelBuilder,
            IMyLinksModelBuilder myLinksModelBuilder)
        {
            _leftSideNavigationModelBuilder = leftSideNavigationModelBuilder;
            _subNavigationModelBuilder = subNavigationModelBuilder;
            _topNavigationModelBuilder = topNavigationModelBuilder;
            _myLinksModelBuilder = myLinksModelBuilder;
        }

        public virtual ActionResult LeftNavigation()
        {
            var leftNavigation = _leftSideNavigationModelBuilder.GetMenu();
            var result = leftNavigation.Map<MenuViewModel>();

            return PartialView(LeftNavigationViewPath, result);
        }

        public virtual ActionResult SubNavigation()
        {
            var subNavigation = _subNavigationModelBuilder.GetMenu();
            var result = subNavigation.Map<SubNavigationMenuViewModel>();

            return PartialView(SubNavigationViewPath, result);
        }

        public virtual ActionResult TopNavigation()
        {
            var topNavigation = _topNavigationModelBuilder.Get();
            var result = topNavigation.Map<TopNavigationViewModel>();

            return PartialView(TopNavigationViewPath, result);
        }

        public virtual ActionResult MyLinks()
        {
            var myLinks = _myLinksModelBuilder.Get();
            var result = myLinks.Map<MyLinksViewModel>();

            return PartialView(MyLinksViewPath, result);
        }
    }
}
