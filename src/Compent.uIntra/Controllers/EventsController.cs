﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compent.uIntra.Core.Events;
using uIntra.Core.Extentions;
using uIntra.Core.Grid;
using uIntra.Core.Links;
using uIntra.Core.Media;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;
using uIntra.Events;
using uIntra.Events.Web;
using uIntra.Groups;
using uIntra.Notification;
using uIntra.Notification.Configuration;
using uIntra.Search;

namespace Compent.uIntra.Controllers
{
    public class EventsController : EventsControllerBase
    {
        protected override string DetailsViewPath => "~/Views/Events/DetailsView.cshtml";
        protected override string CreateViewPath => "~/Views/Events/CreateView.cshtml";
        protected override string EditViewPath => "~/Views/Events/EditView.cshtml";
        protected override string ItemViewPath => "~/Views/Events/ItemView.cshtml";

        private readonly IEventsService<Event> _eventsService;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly IReminderService _reminderService;
        private readonly IDocumentIndexer _documentIndexer;
        private readonly INotificationTypeProvider _notificationTypeProvider;
        private readonly IGroupService _groupService;

        public EventsController(
            IEventsService<Event> eventsService,
            IMediaHelper mediaHelper,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IReminderService reminderService,
            IIntranetUserContentHelper intranetUserContentHelper,
            IGridHelper gridHelper,
            IActivityTypeProvider activityTypeProvider,
            IDocumentIndexer documentIndexer,
            INotificationTypeProvider notificationTypeProvider, IGroupService groupService)
            : base(eventsService, mediaHelper, intranetUserService, intranetUserContentHelper, gridHelper, activityTypeProvider)
        {
            _eventsService = eventsService;
            _intranetUserService = intranetUserService;
            _reminderService = reminderService;
            _documentIndexer = documentIndexer;
            _notificationTypeProvider = notificationTypeProvider;
            _groupService = groupService;
        }

        public ActionResult CentralFeedItem(Event item, ActivityLinks links)
        {
            var activity = item;
            var extendedModel = GetItemViewModel(activity, links).Map<EventExtendedItemModel>();
            var  userId =_intranetUserService.GetCurrentUser();
            extendedModel.LikesInfo = activity;
            extendedModel.IsSubscribed = activity.Subscribers.Any(s => s.UserId == userId.Id);
            return PartialView(ItemViewPath, extendedModel);
        }

        public ActionResult PreviewItem(Event item, ActivityLinks links)
        {
            EventPreviewViewModel viewModel = GetPreviewViewModel(item, links);
            return PartialView(PreviewItemViewPath, viewModel);
        }

        protected override void DeleteMedia(IEnumerable<int> mediaIds)
        {
            base.DeleteMedia(mediaIds);
            _documentIndexer.DeleteFromIndex(mediaIds);
        }

        protected override void OnEventCreated(Guid activityId, EventCreateModel model)
        {
            _reminderService.CreateIfNotExists(activityId, ReminderTypeEnum.OneDayBefore);

            var groupId = _groupService.GetGroupIdFromQuery(Request.QueryString.ToString());
            if (groupId.HasValue)
            {
                _groupService.AddGroupActivityRelation(groupId.Value, activityId);
                var @event = _eventsService.Get(activityId);               
                @event.GroupIds = @event.GroupIds.Concat(groupId.Value.ToEnumerableOfOne());
            }
        }

        protected override void OnEventEdited(EventBase @event, EventEditModel model)
        {
            if (!_eventsService.IsActual(@event))
            {
                return;
            }

            if (model.NotifyAllSubscribers)
            {
                var notificationType = _notificationTypeProvider.Get(NotificationTypeEnum.EventUpdated.ToInt());
                ((INotifyableService) _eventsService).Notify(@event.Id, notificationType);
            }

            _reminderService.CreateIfNotExists(@event.Id, ReminderTypeEnum.OneDayBefore);
        }

        protected override void OnEventHidden(Guid id, bool isNotificationNeeded)
        {
            if (isNotificationNeeded)
            {
                var notificationType = _notificationTypeProvider.Get(NotificationTypeEnum.EventHided.ToInt());
                ((INotifyableService)_eventsService).Notify(id, notificationType);
            }
        }
    }
}