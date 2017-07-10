﻿using System;
using System.Web.Mvc;
using Localization.Umbraco.Attributes;
using uIntra.Comments;
using uIntra.Comments.Web;
using uIntra.Core.Activity;
using uIntra.Core.Comments;
using uIntra.Core.Extentions;
using uIntra.Core.User;
using uIntra.Notification;
using uIntra.Notification.Configuration;
using uIntra.Users;

namespace uIntra.Controllers
{
    [ThreadCulture]
    public class CommentsController : CommentsControllerBase
    {
        protected override string OverviewViewPath { get; } = "~/Views/Comments/CommentsOverView.cshtml";

        private readonly ICommentableService _customCommentableService;
        private readonly IActivitiesServiceFactory _activitiesServiceFactory;
        private readonly ICommentsService _commentsService;
        private readonly IIntranetUserService<IntranetUser> _intranetUserService;
        private readonly INotificationTypeProvider _notificationTypeProvider;

        public CommentsController(
            ICommentsService commentsService,
            IIntranetUserService<IntranetUser> intranetUserService,
            IActivitiesServiceFactory activitiesServiceFactory,
            ICommentableService customCommentableService, IIntranetUserContentHelper intranetUserContentHelper, INotificationTypeProvider notificationTypeProvider)
            : base(commentsService, intranetUserService, activitiesServiceFactory, intranetUserContentHelper)
        {
            _customCommentableService = customCommentableService;
            _notificationTypeProvider = notificationTypeProvider;
            _activitiesServiceFactory = activitiesServiceFactory;
            _commentsService = commentsService;
            _intranetUserService = intranetUserService;
        }

        protected override void OnCommentCreated(Comment comment)
        {
            var service = _activitiesServiceFactory.GetService<INotifyableService>(comment.ActivityId);
            if (service != null)
            {
                var notificationId = comment.ParentId.HasValue
                    ? NotificationTypeEnum.CommentReplied.ToInt()
                    : NotificationTypeEnum.CommentAdded.ToInt();

                var notificationType = _notificationTypeProvider.Get(notificationId);
                service.Notify(comment.ParentId ?? comment.Id, notificationType);
            }
        }

        protected override void OnCommentEdited(Comment comment)
        {
            var service = _activitiesServiceFactory.GetService<INotifyableService>(comment.ActivityId);
            if (service != null)
            {
                var notificationType = _notificationTypeProvider.Get(NotificationTypeEnum.CommentEdited.ToInt());
                service.Notify(comment.Id, notificationType);
            }
        }

        [HttpPost]
        public override PartialViewResult Add(CommentCreateModel model)
        {
            FillProfileLink();
            if (!ModelState.IsValid)
            {
                return OverView(model.ActivityId);
            }

            if (model.ActivityId == CommentsTestConstants.ActivityId)
            {
                _customCommentableService.CreateComment(_intranetUserService.GetCurrentUser().Id, model.ActivityId, model.Text, model.ParentId);
                return OverView(model.ActivityId);
            }

            var service = _activitiesServiceFactory.GetService<ICommentableService>(model.ActivityId);
            var comment = service.CreateComment(_intranetUserService.GetCurrentUser().Id, model.ActivityId, model.Text, model.ParentId);
            OnCommentCreated(comment);

            return OverView(model.ActivityId);
        }

        [HttpPut]
        public override PartialViewResult Edit(CommentEditModel model)
        {
            FillProfileLink();
            var comment = _commentsService.Get(model.Id);

            if (!ModelState.IsValid || !_commentsService.CanEdit(comment, _intranetUserService.GetCurrentUser().Id))
            {
                return OverView(model.Id);
            }

            if (comment.ActivityId == CommentsTestConstants.ActivityId)
            {
                _customCommentableService.UpdateComment(model.Id, model.Text);
                return OverView(comment.ActivityId);
            }

            var service = _activitiesServiceFactory.GetService<ICommentableService>(comment.ActivityId);
            service.UpdateComment(model.Id, model.Text);
            OnCommentEdited(comment);
            return OverView(comment.ActivityId);
        }

        [HttpDelete]
        public override PartialViewResult Delete(Guid id)
        {
            FillProfileLink();
            var comment = _commentsService.Get(id);
            var currentUserId = _intranetUserService.GetCurrentUser().Id;

            if (!_commentsService.CanDelete(comment, currentUserId))
            {
                return OverView(comment.ActivityId);
            }

            if (comment.ActivityId == CommentsTestConstants.ActivityId)
            {
                _customCommentableService.DeleteComment(id);
                return OverView(comment.ActivityId);
            }

            var service = _activitiesServiceFactory.GetService<ICommentableService>(comment.ActivityId);
            service.DeleteComment(id);

            return OverView(comment.ActivityId);
        }
    }
}