﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using uIntra.CentralFeed;
using uIntra.CentralFeed.Web;
using uIntra.Core.Activity;
using uIntra.Core.Extentions;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;
using uIntra.Subscribe;

namespace uIntra.Groups.Web
{
    public abstract class GroupFeedControllerBase : FeedControllerBase
    {
        private readonly ICentralFeedContentHelper _centralFeedContentHelper;
        private readonly IGroupFeedService _groupFeedService;
        private readonly IActivitiesServiceFactory _activitiesServiceFactory;
        private readonly IFeedTypeProvider _centralFeedTypeProvider;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly IGroupContentHelper _groupContentHelper;
        private readonly IGroupService _groupService;

        // TODO : remove redundancies in pathes
        protected override string OverviewViewPath => "~/App_Plugins/Groups/Feed/GroupFeedOverviewView.cshtml";
        protected override string DetailsViewPath => "~/App_Plugins/Groups/Feed/GroupFeedDetailsView.cshtml";
        protected override string CreateViewPath => "~/App_Plugins/Groups/Feed/GroupFeedCreateView.cshtml";
        protected override string EditViewPath => "~/App_Plugins/Groups/Feed/GroupFeedDetailsView.cshtml";

        protected override string ListViewPath => "~/App_Plugins/Groups/Feed/GroupFeedList.cshtml";
        protected override string NavigationViewPath => "-";
        protected override string LatestActivitiesViewPath => "-";

        public GroupFeedControllerBase(
            ICentralFeedContentHelper centralFeedContentHelper,
            ISubscribeService subscribeService,
            IGroupFeedService groupFeedService,
            IActivitiesServiceFactory activitiesServiceFactory,
            IIntranetUserContentHelper intranetUserContentHelper,
            IFeedTypeProvider centralFeedTypeProvider,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IGroupContentHelper groupContentHelper,
            IGroupService groupService) 
            : base(centralFeedContentHelper,
                  subscribeService,
                  groupFeedService,
                  activitiesServiceFactory,
                  intranetUserContentHelper,
                  centralFeedTypeProvider,
                  intranetUserService)
        {
            _centralFeedContentHelper = centralFeedContentHelper;
            _groupFeedService = groupFeedService;
            _activitiesServiceFactory = activitiesServiceFactory;
            _centralFeedTypeProvider = centralFeedTypeProvider;
            _intranetUserService = intranetUserService;
            _groupContentHelper = groupContentHelper;
            _groupService = groupService;
        }

        [NonAction]
        public override ActionResult Overview()
        {
            return base.Overview();
        }

        public ActionResult Overview(Guid groupId)
        {
            var model = GetOverviewModel(groupId);
            return PartialView(OverviewViewPath, model);
        }


        public ActionResult List(GroupFeedListModel model)
        {
            var centralFeedType = _centralFeedTypeProvider.Get(model.TypeId);
            var items = GetGroupFeedItems(centralFeedType, model.GroupId).ToList();

            if (IsEmptyFilters(model.FilterState, _centralFeedContentHelper.CentralFeedCookieExists()))
            {
                model.FilterState = GetFilterStateModel();
            }

            var tabSettings = _groupFeedService.GetSettings(centralFeedType);

            var filteredItems = ApplyFilters(items, model.FilterState, tabSettings).ToList();

            var currentVersion = _groupFeedService.GetFeedVersion(filteredItems);

            if (model.Version.HasValue && currentVersion == model.Version.Value)
            {
                return null;
            }

            var centralFeedModel = GetFeedListViewModel(model, filteredItems, centralFeedType);
            var filterState = MapToFilterState(centralFeedModel.FilterState);
            _centralFeedContentHelper.SaveFiltersState(filterState);

            return PartialView(ListViewPath, centralFeedModel);
        }

        protected virtual IEnumerable<IFeedItem> GetGroupFeedItems(IIntranetType type, Guid groupId)
        {
            if (type.Id == CentralFeedTypeEnum.All.ToInt())
            {
                var items = _groupFeedService.GetFeed(groupId).OrderByDescending(item => item.PublishDate);
                return items;
            }

            return _groupFeedService.GetFeed(type, groupId);
        }

        protected virtual FeedListViewModel GetFeedListViewModel(GroupFeedListModel model, List<IFeedItem> filteredItems, IIntranetType centralFeedType)
        {
            var take = model.Page * ItemsPerPage;
            var pagedItemsList = Sort(filteredItems, centralFeedType).Take(take).ToList();

            var settings = _groupFeedService.GetAllSettings();
            var tabSettings = settings
                .Single(s => s.Type.Id == model.TypeId)
                .Map<FeedTabSettings>();

            return new FeedListViewModel
            {
                Version = _groupFeedService.GetFeedVersion(filteredItems),
                Feed = GetFeedItems(pagedItemsList, settings, model.GroupId),
                TabSettings = tabSettings,
                Type = centralFeedType,
                BlockScrolling = filteredItems.Count < take,
                FilterState = MapToFilterStateViewModel(model.FilterState)
            };
        }

        protected virtual IEnumerable<FeedItemViewModel> GetFeedItems(IEnumerable<IFeedItem> items, IEnumerable<FeedSettings> settings, Guid groupId)
        {
            var activityTypes = GetInvolvedTypes(items);

            var activityServices = activityTypes
                .Select(t => _activitiesServiceFactory.GetService<IIntranetActivityService>(t.Id))
                .ToDictionary(s => s.ActivityType.Id);

            var activitySettings = settings
                .ToDictionary(s => s.Type.Id);

            var result = items
                .Select(i => new FeedItemViewModel()
                {
                    Item = i,
                    Links = activityServices[i.Type.Id].GetGroupFeedLinks(i.Id, groupId),
                    ControllerName = activitySettings[i.Type.Id].Controller
                });

            return result;
        }

        public override ActionResult Create(int typeId)
        {
            var groupId = _groupService.GetGroupIdFromQuery(Request.QueryString.ToString());
            if (!groupId.HasValue)
                throw new NotImplementedException();
            var activityType = _centralFeedTypeProvider.Get(typeId);
            var viewModel = GetCreateViewModel(activityType, groupId.Value);
            return PartialView(CreateViewPath, viewModel);
        }


        protected virtual GroupFeedOverviewModel GetOverviewModel(Guid groupId)
        {
            var currentUser = _intranetUserService.GetCurrentUser();
            var tabType = _centralFeedContentHelper.GetTabType(CurrentPage);

            var centralFeedState = _centralFeedContentHelper.GetFiltersState<FeedFiltersState>();
            var tabs = _groupContentHelper.GetTabs(groupId, currentUser, CurrentPage).Map<IEnumerable<FeedTabViewModel>>();

            var model = new GroupFeedOverviewModel
            {
                Tabs = tabs,
                CurrentType = tabType,
                IsFiltersOpened = centralFeedState.IsFiltersOpened,
                GroupId = groupId
            };
            return model;
        }

        protected virtual CreateViewModel GetCreateViewModel(IIntranetType activityType, Guid groupId)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(activityType.Id);
            var links = service.GetGroupFeedCreateLinks(groupId);

            var settings = _groupFeedService.GetSettings(activityType);

            return new CreateViewModel()
            {
                Links = links,
                Settings = settings
            };
        }

        protected override EditViewModel GetEditViewModel(Guid id)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var links = service.GetCentralFeedLinks(id);

            var type = service.ActivityType;
            var settings = _groupFeedService.GetSettings(type);

            var viewModel = new EditViewModel()
            {
                Id = id,
                Links = links,
                Settings = settings
            };
            return viewModel;
        }

        protected override DetailsViewModel GetDetailsViewModel(Guid id)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var links = service.GetCentralFeedLinks(id);

            var type = service.ActivityType;
            var settings = _groupFeedService.GetSettings(type);

            var viewModel = new DetailsViewModel()
            {
                Id = id,
                Links = links,
                Settings = settings
            };
            return viewModel;
        }
    }
}
