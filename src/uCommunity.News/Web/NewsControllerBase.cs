﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using uCommunity.Core;
using uCommunity.Core.Activity;
using uCommunity.Core.Activity.Models;
using uCommunity.Core.Controls.LightboxGallery;
using uCommunity.Core.Extentions;
using uCommunity.Core.Media;
using uCommunity.Core.User;
using uCommunity.Core.User.Permissions.Web;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace uCommunity.News.Web
{
    [ActivityController(IntranetActivityTypeEnum.News)]
    public abstract class NewsControllerBase : SurfaceController
    {
        protected virtual string ItemViewPath { get; } = "~/App_Plugins/News/List/ItemView.cshtml";
        protected virtual string DetailsViewPath { get; } = "~/App_Plugins/News/Details/DetailsView.cshtml";
        protected virtual string CreateViewPath { get; } = "~/App_Plugins/News/Create/CreateView.cshtml";
        protected virtual string EditViewPath { get; } = "~/App_Plugins/News/Edit/EditView.cshtml";
        protected virtual int ShortDescriptionLength { get; } = 500;
        protected virtual int DisplayedImagesCount { get; } = 3;

        private readonly INewsService<NewsBase> _newsService;
        private readonly IMediaHelper _mediaHelper;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;

        protected NewsControllerBase(
            IIntranetUserService<IIntranetUser> intranetUserService,
            INewsService<NewsBase> newsService,
            IMediaHelper mediaHelper)
        {
            _intranetUserService = intranetUserService;
            _newsService = newsService;
            _mediaHelper = mediaHelper;
        }
     
        public virtual ActionResult Details(Guid id)
        {
            FillLinks();
            var news = _newsService.Get(id);
            if (news.IsHidden)
            {
                HttpContext.Response.Redirect(ViewData.GetActivityOverviewPageUrl(IntranetActivityTypeEnum.News));
            }

            var model = news.Map<NewsViewModel>();
            model.HeaderInfo = news.Map<IntranetActivityDetailsHeaderViewModel>();
            model.HeaderInfo.Dates = new List<string> { news.PublishDate.ToDateTimeFormat() };
            model.CanEdit = _newsService.CanEdit(news);

            return PartialView(DetailsViewPath, model);
        }

        [RestrictedAction(IntranetActivityActionEnum.Create)]
        public virtual ActionResult Create()
        {
            FillLinks();
            var model = new NewsCreateModel { PublishDate = DateTime.Now.Date };
            FillCreateEditData(model);
            return PartialView(CreateViewPath, model);
        }

        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Create)]
        public virtual ActionResult Create(NewsCreateModel createModel)
        {
            FillLinks();
            if (!ModelState.IsValid)
            {
                FillCreateEditData(createModel);
                return PartialView(CreateViewPath, createModel);
            }

            var activityId = CreateNews(createModel);
            return Redirect(ViewData.GetActivityDetailsPageUrl(IntranetActivityTypeEnum.News, activityId));
        }

        [RestrictedAction(IntranetActivityActionEnum.Edit)]
        public virtual ActionResult Edit(Guid id)
        {
            FillLinks();

            var news = _newsService.Get(id);
            if (news.IsHidden)
            {
                HttpContext.Response.Redirect(ViewData.GetActivityOverviewPageUrl(IntranetActivityTypeEnum.News));
            }

            if (!_newsService.CanEdit(news))
            {
                HttpContext.Response.Redirect(ViewData.GetActivityDetailsPageUrl(IntranetActivityTypeEnum.News, id));
            }

            var model = news.Map<NewsEditModel>();
            FillCreateEditData(model);
            return PartialView(EditViewPath, model);
        }

        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Edit)]
        public virtual ActionResult Edit(NewsEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                FillCreateEditData(editModel);
                return PartialView(EditViewPath, editModel);
            }

            UpdateNews(editModel);

            return Redirect(ViewData.GetActivityDetailsPageUrl(IntranetActivityTypeEnum.News, editModel.Id));
        }

        protected virtual void FillCreateEditData(IContentWithMediaCreateEditModel model)
        {
            var mediaSettings = _newsService.GetMediaSettings();
            ViewData["AllowedMediaExtentions"] = mediaSettings.AllowedMediaExtentions;
            ViewData.SetDateTimeFormats();
            model.MediaRootId = mediaSettings.MediaRootId;
        }

        protected virtual NewsItemViewModel GetItemViewModel(NewsBase news)
        {
            var model = news.Map<NewsItemViewModel>();
            model.ShortDescription = news.Description.Truncate(ShortDescriptionLength);
            model.MediaIds = news.MediaIds;
            model.HeaderInfo = news.Map<IntranetActivityItemHeaderViewModel>();
            model.HeaderInfo.DetailsPageUrl = ViewData.GetActivityDetailsPageUrl(IntranetActivityTypeEnum.News, news.Id);
            model.Expired = _newsService.IsExpired(news);

            model.LightboxGalleryPreviewInfo = new LightboxGalleryPreviewModel
            {
                MediaIds = news.MediaIds,
                Url = ViewData.GetActivityDetailsPageUrl(IntranetActivityTypeEnum.Events, news.Id),
                DisplayedImagesCount = DisplayedImagesCount
            };
            return model;
        }

        protected virtual Guid CreateNews(NewsCreateModel createModel)
        {
            var news = createModel.Map<NewsBase>();
            news.MediaIds = news.MediaIds.Concat(_mediaHelper.CreateMedia(createModel));
            news.CreatorId = _intranetUserService.GetCurrentUserId();
            if (createModel.IsPinned && createModel.PinDays > 0)
            {
                news.EndPinDate = DateTime.Now.AddDays(createModel.PinDays);
            }

            return _newsService.Create(news);
        }

        protected virtual void UpdateNews(NewsEditModel editModel)
        {
            var activity = editModel.Map<NewsBase>();
            activity.MediaIds = activity.MediaIds.Concat(_mediaHelper.CreateMedia(editModel));
            activity.CreatorId = _intranetUserService.GetCurrentUserId();

            if (editModel.IsPinned && editModel.PinDays > 0 && activity.PinDays != editModel.PinDays)
            {
                activity.EndPinDate = DateTime.Now.AddDays(editModel.PinDays);
            }

            _newsService.Save(activity);
        }

        protected virtual void FillLinks()
        {
            var overviewPageUrl = _newsService.GetOverviewPage(CurrentPage).Url;
            var createPageUrl = _newsService.GetCreatePage(CurrentPage).Url;
            var detailsPageUrl = _newsService.GetDetailsPage(CurrentPage).Url;
            var editPageUrl = _newsService.GetEditPage(CurrentPage).Url;

            ViewData.SetActivityOverviewPageUrl(IntranetActivityTypeEnum.News, overviewPageUrl);
            ViewData.SetActivityDetailsPageUrl(IntranetActivityTypeEnum.News, detailsPageUrl);
            ViewData.SetActivityCreatePageUrl(IntranetActivityTypeEnum.News, createPageUrl);
            ViewData.SetActivityEditPageUrl(IntranetActivityTypeEnum.News, editPageUrl);
        }
    }
}