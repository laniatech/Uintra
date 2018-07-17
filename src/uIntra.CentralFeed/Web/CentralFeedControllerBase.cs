﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compent.Extensions;
using Uintra.CentralFeed.Navigation.Models;
using Uintra.Core;
using Uintra.Core.Activity;
using Uintra.Core.Attributes;
using Uintra.Core.Context;
using Uintra.Core.Extensions;
using Uintra.Core.Feed;
using Uintra.Core.TypeProviders;
using Uintra.Core.User;
using Uintra.Core.User.Permissions;
using Uintra.Subscribe;
using static Uintra.Core.Context.Extensions.ContextExtensions;

namespace Uintra.CentralFeed.Web
{
    public abstract class CentralFeedControllerBase : FeedControllerBase
    {
        private readonly ICentralFeedService _centralFeedService;
        private readonly ICentralFeedContentService _centralFeedContentService;
        private readonly IFeedTypeProvider _centralFeedTypeProvider;
        private readonly IActivitiesServiceFactory _activitiesServiceFactory;
        private readonly IFeedLinkService _feedLinkService;
        private readonly IStateService<FeedFiltersState> _feedFilterStateService;
        private readonly IPermissionsService _permissionsService;
        private readonly IFeedListBuilder _feedListAssembler;

        protected override string OverviewViewPath => "~/App_Plugins/CentralFeed/View/Overview.cshtml";
        protected override string DetailsViewPath => "~/App_Plugins/CentralFeed/View/Details.cshtml";
        protected override string CreateViewPath => "~/App_Plugins/CentralFeed/View/Create.cshtml";
        protected override string EditViewPath => "~/App_Plugins/CentralFeed/View/Edit.cshtml";
        protected override string ListViewPath => "~/App_Plugins/CentralFeed/View/List.cshtml";
        protected virtual string NavigationViewPath => "~/App_Plugins/CentralFeed/View/Navigation.cshtml";
        protected virtual string LatestActivitiesViewPath => "~/App_Plugins/LatestActivities/View/LatestActivities.cshtml";

        public override ContextType ControllerContextType { get; } = ContextType.CentralFeed;

        protected CentralFeedControllerBase(
            ICentralFeedService centralFeedService,
            ICentralFeedContentService centralFeedContentService,
            IActivitiesServiceFactory activitiesServiceFactory,
            ISubscribeService subscribeService,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IIntranetUserContentProvider intranetUserContentProvider,
            IFeedTypeProvider centralFeedTypeProvider,
            IFeedLinkService feedLinkService,
            IStateService<FeedFiltersState> feedFilterStateService,
            IPermissionsService permissionsService,
            IActivityTypeProvider activityTypeProvider,
            IContextTypeProvider contextTypeProvider,
            IFeedListBuilder feedListAssembler)
            : base(subscribeService, centralFeedService, intranetUserService, feedFilterStateService, centralFeedTypeProvider, contextTypeProvider)
        {
            _centralFeedService = centralFeedService;
            _centralFeedContentService = centralFeedContentService;
            _centralFeedTypeProvider = centralFeedTypeProvider;
            _feedLinkService = feedLinkService;
            _feedFilterStateService = feedFilterStateService;
            _permissionsService = permissionsService;
            _feedListAssembler = feedListAssembler;
            _activitiesServiceFactory = activitiesServiceFactory;
        }
        
        [HttpGet]
        public virtual ActionResult Overview()
        {
            var model = GetOverviewModel();
            return PartialView(OverviewViewPath, model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var activityType = _centralFeedContentService.GetCreateActivityType(CurrentPage);
            var viewModel = GetCreateViewModel(activityType);
            return PartialView(CreateViewPath, viewModel);
        }

        [HttpGet]
        [NotFoundActivity]
        public virtual ActionResult Details(Guid id)
        {
            var viewModel = GetDetailsViewModel(id);
            return PartialView(DetailsViewPath, viewModel);
        }

        [HttpGet]
        public virtual ActionResult Edit(Guid id)
        {
            var viewModel = GetEditViewModel(id);
            return PartialView(EditViewPath, viewModel);
        }

        public virtual ActionResult List(FeedListModel model)
        {
            var filterState = _feedFilterStateService.Get();
            var centralFeedType = _centralFeedTypeProvider[filterState.SelectedActivityTypeId];

            var tabSettings = _centralFeedService.GetSettings(centralFeedType);
            
            var items = _centralFeedService.GetFeed(centralFeedType);
            var filteredItems = _feedListAssembler.BuildForFeed(items, filterState, tabSettings);

            var centralFeedModel = GetFeedListViewModel(model, filteredItems, centralFeedType);

            return PartialView(ListViewPath, centralFeedModel);
        }

        public virtual ActionResult LatestActivities(LatestActivitiesPanelModel panelModel)
        {
            var viewModel = GetLatestActivities(panelModel);
            return PartialView(LatestActivitiesViewPath, viewModel);
        }

        protected virtual IEnumerable<IFeedItem> GetCentralFeedItems(Enum type) => 
            _centralFeedService.GetFeed(type);

        protected virtual FeedListViewModel GetFeedListViewModel(FeedListModel model, IEnumerable<IFeedItem> items, Enum centralFeedType)
        {
            var itemsList = items.AsList();

            var settings = _centralFeedService
                .GetAllSettings()
                .AsList();

            var tabSettings = settings
                .Single(s => ExactScalar(s.Type, centralFeedType))
                .Map<FeedTabSettings>();

            var takeCount = model.Page * ItemsPerPage;

            var viewedItems = itemsList.Take(takeCount);
            var feedItems = GetFeedItems(viewedItems, settings);

            return new FeedListViewModel
            {
                Feed = feedItems,
                TabSettings = tabSettings,
                Type = centralFeedType,
                BlockScrolling = itemsList.Count < takeCount
            };
        }

        protected override ActivityFeedOptions GetActivityFeedOptions(Guid activityId)
        {
            return new ActivityFeedOptions
            {
                Links = _feedLinkService.GetLinks(activityId)
            };
        }

        protected virtual CentralFeedOverviewModel GetOverviewModel()
        {
            var tabType = _centralFeedContentService.GetFeedTabType(CurrentPage);
            var centralFeedState = _feedFilterStateService.Get();

            var activityTabs = GetActivityTabs().Map<List<ActivityFeedTabViewModel>>();

            var model = new CentralFeedOverviewModel
            {
                Tabs = activityTabs,
                TabsWithCreateUrl = GetTabsWithCreateUrl(activityTabs)
                    .Where(tab => _permissionsService.IsCurrentUserHasAccess(tab.Type, IntranetActivityActionEnum.Create)),
                CurrentType = tabType,
                IsFiltersOpened = centralFeedState.IsFiltersOpened
            };
            return model;
        }

        protected virtual IEnumerable<ActivityFeedTabModel> GetActivityTabs() =>
            _centralFeedContentService.GetTabs(CurrentPage);

        protected virtual LatestActivitiesViewModel GetLatestActivities(LatestActivitiesPanelModel panelModel)
        {
            var settings = _centralFeedService.GetAllSettings();
            var centralFeedType = _centralFeedTypeProvider[panelModel.ActivityTypeId];

            var latestActivities = GetLatestActivities(centralFeedType, panelModel.ActivityAmount);
            var feedItems = GetFeedItems(latestActivities.activities, settings);
            var tab = GetTabForActivityType(centralFeedType);

            return new LatestActivitiesViewModel
            {
                Title = panelModel.Title,
                Teaser = panelModel.Teaser,
                Feed = feedItems,
                Tab = tab,
                ShowSeeAllButton = latestActivities.activities.Count() < latestActivities.totalCount
            };
        }

        protected virtual (IEnumerable<IFeedItem> activities, int totalCount) GetLatestActivities(Enum activityType, int activityAmount)
        {
            var items = _centralFeedService.GetFeed(activityType).ToList();
            var filteredItems = FilterLatestActivities(items).Take(activityAmount);
            var sortedItems = _feedListAssembler.BuildForLatestActivity(filteredItems);

            return (sortedItems, items.Count);
        }

        private ActivityFeedTabViewModel GetTabForActivityType(Enum activitiesType)
        {
            var result = _centralFeedContentService
                .GetTabs(CurrentPage)
                .Single(el => Equals(el.Type, activitiesType))
                .Map<ActivityFeedTabViewModel>();
            return result;
        }

        private IEnumerable<IFeedItem> FilterLatestActivities(IEnumerable<IFeedItem> activities)
        {
            var settings = _centralFeedService.GetAllSettings().Where(s => !s.ExcludeFromLatestActivities).Select(s => s.Type);
            var items = activities.Join(settings, item => item.Type.ToInt(), type => type.ToInt(), (item, _) => item);

            return items;
        }
        
        protected virtual CreateViewModel GetCreateViewModel(Enum activityType)
        {
            var links = _feedLinkService.GetCreateLinks(activityType);
            var settings = _centralFeedService.GetSettings(activityType);

            return new CreateViewModel()
            {
                Links = links,
                Settings = settings
            };
        }

        protected virtual EditViewModel GetEditViewModel(Guid id)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var links = _feedLinkService.GetLinks(id);
            var settings = _centralFeedService.GetSettings(service.Type);

            var viewModel = new EditViewModel()
            {
                Id = id,
                Links = links,
                Settings = settings
            };
            return viewModel;
        }

        protected virtual DetailsViewModel GetDetailsViewModel(Guid id)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var options = GetActivityFeedOptions(id);
            var settings = _centralFeedService.GetSettings(service.Type);

            var viewModel = new DetailsViewModel
            {
                Id = id,
                Options = options,
                Settings = settings
            };
            return viewModel;
        }
    }
}