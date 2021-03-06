﻿using System.Linq;
using Uintra.Core.Helpers;
using Uintra.Core.Links;
using Uintra.Core.Localization;
using Uintra.Core.User;
using Uintra.Notification;
using Uintra.Notification.Base;
using Uintra.Notification.Configuration;
using static Uintra.Notification.Constants.TokensConstants;

namespace Compent.Uintra.Core.Notification
{
    public class PopupNotificationModelMapper : INotificationModelMapper<PopupNotifierTemplate, PopupNotificationMessage>
    {
        private const string ProfileLinkTitle = "PopupNotification.ProfileLink.Title";

        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly IIntranetLocalizationService _localizationService;
        private readonly IIntranetUserContentProvider _intranetUserContentProvider;

        public PopupNotificationModelMapper(
            IIntranetUserService<IIntranetUser> intranetUserService,
            IIntranetLocalizationService localizationService,
            IIntranetUserContentProvider intranetUserContentProvider)
        {
            _intranetUserService = intranetUserService;
            _localizationService = localizationService;
            _intranetUserContentProvider = intranetUserContentProvider;
        }

        public PopupNotificationMessage Map(INotifierDataValue notifierData, PopupNotifierTemplate template, IIntranetUser receiver)
        {
            var message = new PopupNotificationMessage
            {
                ReceiverId = receiver.Id,
                NotificationType = NotificationTypeEnum.Welcome
            };

            (string, string)[] tokens =
            {
                (FullName, _intranetUserService.Get(receiver.Id).DisplayedName),
                (ProfileLink, HtmlHelper.CreateLink(_localizationService.Translate(ProfileLinkTitle), _intranetUserContentProvider.GetEditPage().Url))
            };
            message.Message = ReplaceTokens(template.Message, tokens);
            return message;
        }

        private string ReplaceTokens(string source, params (string token, string value)[] replacePairs) =>
            replacePairs
                .Aggregate(source, (acc, pair) => acc.Replace(pair.token, pair.value));
    }
}