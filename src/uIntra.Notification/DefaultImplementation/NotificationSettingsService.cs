﻿using System;
using uIntra.Core.Extensions;
using uIntra.Core.Persistence;
using uIntra.Core.TypeProviders;
using uIntra.Notification.Configuration;
using uIntra.Notification.Core;
using uIntra.Notification.Core.Sql;

namespace uIntra.Notification
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly ISqlRepository<NotificationSetting> _repository;
        private readonly IBackofficeNotificationSettingsProvider _backofficeNotificationSettingsProvider;
        private readonly INotifierTypeProvider _notifierTypeProvider;

        public NotificationSettingsService(
            ISqlRepository<NotificationSetting> repository,
            INotifierTypeProvider notifierTypeProvider,
            IBackofficeNotificationSettingsProvider backofficeNotificationSettingsProvider)
        {
            _repository = repository;
            _notifierTypeProvider = notifierTypeProvider;
            _backofficeNotificationSettingsProvider = backofficeNotificationSettingsProvider;
        }

        public NotifierSettingsModel GetSettings(ActivityEventIdentity activityEventIdentity)
        {
            var emailNotifierSetting = Get<EmailNotifierTemplate>(activityEventIdentity.AddNotifierIdentity(GetIntranetType(NotifierTypeEnum.EmailNotifier)));
            var uiNotifierSetting = Get<UiNotifierTemplate>(activityEventIdentity.AddNotifierIdentity(GetIntranetType(NotifierTypeEnum.UiNotifier)));
            var notifierSettings = new NotifierSettingsModel
            {
                EmailNotifierSetting = emailNotifierSetting,
                UiNotifierSetting = uiNotifierSetting
            };

            return notifierSettings;
        }

        public NotifierSettingModel<T> Get<T>(ActivityEventNotifierIdentity activityEventNotifierIdentity) where T : INotifierTemplate
        {
            var defaultTemplate = _backofficeNotificationSettingsProvider.Get<T>(activityEventNotifierIdentity);
            var (setting, _) = FindOrCreateSetting<T>(activityEventNotifierIdentity);

            var mappedSetting = MappedNotifierSetting(setting, activityEventNotifierIdentity, defaultTemplate);

            return mappedSetting;
        }

        public void Save<T>(NotifierSettingModel<T> settingModel) where T : INotifierTemplate
        {
            var identity = new ActivityEventIdentity(settingModel.ActivityType, settingModel.NotificationType)
                .AddNotifierIdentity(settingModel.NotifierType);

            var (setting, isCreated) = FindOrCreateSetting<T>(identity);

            var updatedSetting = GetUpdatedSetting(setting, settingModel);

            if (isCreated)
            {
                _repository.Add(updatedSetting);
            }
            else
            {
                _repository.Update(updatedSetting);
            }
        }

        protected virtual NotificationSetting Find(ActivityEventNotifierIdentity activityEventNotifierIdentity) =>
            _repository.Find(s =>
                s.ActivityType == activityEventNotifierIdentity.Event.ActivityType.Id &&
                s.NotificationType == activityEventNotifierIdentity.Event.NotificationType.Id &&
                s.NotifierType == activityEventNotifierIdentity.NotifierType.Id);

        protected virtual (NotificationSetting setting, bool isCreated) FindOrCreateSetting<T>(ActivityEventNotifierIdentity identity)
            where T : INotifierTemplate
        {
            var defaults = _backofficeNotificationSettingsProvider.Get<T>(identity);
            var entry = Find(identity);
            var setting = entry ?? NewSetting(identity, defaults);
            return (setting, entry is null);
        }

        protected virtual NotificationSetting NewSetting<T>(ActivityEventNotifierIdentity identity,
            NotificationSettingDefaults<T> defaults) where T : INotifierTemplate
        {
            return new NotificationSetting
            {
                Id = Guid.NewGuid(),
                NotifierType = identity.NotifierType.Id,
                ActivityType = identity.Event.ActivityType.Id,
                NotificationType = identity.Event.NotificationType.Id,
                IsEnabled = true,
                JsonData = defaults.Template.ToJson()
            };
        }

        protected virtual NotifierSettingModel<T> MappedNotifierSetting<T>(
            NotificationSetting notificationSetting,
            ActivityEventNotifierIdentity identity,
            NotificationSettingDefaults<T> defaults)
            where T : INotifierTemplate
        {

            return new NotifierSettingModel<T>
            {
                ActivityType = identity.Event.ActivityType,
                NotificationType = identity.Event.NotificationType,
                NotifierType = identity.NotifierType,
                IsEnabled = notificationSetting.IsEnabled,
                NotificationInfo = defaults.Label,
                Template = notificationSetting.JsonData.Deserialize<T>()
            };
        }

        protected virtual NotificationSetting GetUpdatedSetting<T>(NotificationSetting setting, NotifierSettingModel<T> notifierSettingModel)
            where T : INotifierTemplate
        {
            setting.JsonData = notifierSettingModel.Template.ToJson();
            setting.IsEnabled = notifierSettingModel.IsEnabled;
            return setting;
        }

        protected virtual IIntranetType GetIntranetType(NotifierTypeEnum notifierType) => _notifierTypeProvider.Get((int)notifierType);
    }
}