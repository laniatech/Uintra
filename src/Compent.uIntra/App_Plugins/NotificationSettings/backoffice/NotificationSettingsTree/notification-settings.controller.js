﻿(function (angular) {
    'use strict';

    var controller = function ($rootScope, $scope, $location, appState, notificationsService, notificationSettingsService, notificationSettingsConfig, navigationService) {

        var self = this;
        self.settings = {};
        self.selectedNotifierSettings = {};

        const notifierType = {
            email: 1,
            ui: 2
        };

        let selectedNotifierType;


        self.isEmailTabSelected = function () {
            return selectedNotifierType === notifierType.email;
        }

        self.isUiTabSelected = function () {
            return selectedNotifierType === notifierType.ui;
        }

        self.selectEmailTab = function () {
            selectedNotifierType = notifierType.email;
            self.selectedNotifierSettings = self.settings.emailNotifierSetting;
        }

        self.selectUiTab = function () {
            selectedNotifierType = notifierType.ui;
            self.selectedNotifierSettings = self.settings.uiNotifierSetting;
        }

        self.save = function () {
            saveSettings(self.settings);
        }

        self.splitOnUpperCaseCharacters = function (text) {
            if (!text || text.length === 0) return '';
            return text.split(/(?=[A-Z])/).join(' ');
        }

        function initalize() {
            initLocationChangeStartEvent();
            initCurrentNodeHighlighting();

            var params = getCurrentUrlParams();
            notificationSettingsService.getSettings(params.activityType, params.notificationType).then(function (result) {
                self.settings = result.data;
                self.selectEmailTab();

                initEmailSubjectControlConfig();
                initEmailBodyControlConfig();
                initUiMessageControlConfig();

            }, showGetErrorMessage);
        }

        function getUrlParams(url) {
            var params = {};
            (url + '?').split('?')[1].split('&').forEach(function (pair) {
                pair = (pair + '=').split('=').map(decodeURIComponent);
                if (pair[0].length) {
                    params[pair[0]] = pair[1];
                }
            });
            return params;
        };

        function getCurrentUrlParams() {
            var params = $location.search();

            if (angular.equals(params, {})) {
                params = getUrlParams($location.path());
            }
            return params;
        }

        function initCurrentNodeHighlighting() {
            var queryString = getCurrentUrlParams();
            var parentId = queryString.activityType;
            var currentNodeId = queryString.id;
            navigationService.syncTree({ tree: 'NotificationSettingsTree', path: ["-1", parentId, currentNodeId], forceReload: false });
        }

        function saveSettings(settings) {
            if (self.isEmailTabSelected()) {
                notificationSettingsService.seveEmailSettings(settings.emailNotifierSetting).then(showSaveSuccessMessage, showSaveErrorMessage);
            }
            else if (self.isUiTabSelected()) {
                notificationSettingsService.seveUiSettings(settings.uiNotifierSetting).then(showSaveSuccessMessage, showSaveErrorMessage);
            }
        }

        function showGetErrorMessage() {
            notificationsService.error("Error", "Notification settings were not loaded, because some error has occurred");
        }

        function showSaveSuccessMessage() {
            notificationsService.success("Success", "Notification settings were updated successfully");
        }

        function showSaveErrorMessage() {
            notificationsService.error("Error", "Notification settings were not updated, because some error has occurred");
        }

        function initEmailSubjectControlConfig() {
            self.emailSubjectControlConfig = new TextControlModel(ControlMode.view);
            self.emailSubjectControlConfig.value = self.settings.emailNotifierSetting.template.subject;

            self.emailSubjectControlConfig.isRequired = true;
            self.emailSubjectControlConfig.requiredValidationMessage = 'E-mail subject is required';
            self.emailSubjectControlConfig.maxLength = 400;
            self.emailSubjectControlConfig.maxLengthValidationMessage = 'E-mail subject max length is 400 symbols';

            self.emailSubjectControlConfig.onSave = function (emailSubject) {
                self.settings.emailNotifierSetting.template.subject = emailSubject;
                self.save();
            };

            self.emailSubjectControlConfig.triggerRefresh();
        }

        function initEmailBodyControlConfig() {
            self.emailBodyControlConfig = new RichTextEditorModel(ControlMode.view);
            self.emailBodyControlConfig.tinyMceOptions = notificationSettingsConfig.emailMessageTinyMceOptions;
            self.emailBodyControlConfig.value = self.settings.emailNotifierSetting.template.body;

            self.emailBodyControlConfig.isRequired = true;
            self.emailBodyControlConfig.requiredValidationMessage = 'E-mail content is required';
            self.emailBodyControlConfig.maxLength = 4000;
            self.emailBodyControlConfig.maxLengthValidationMessage = 'E-mail content max length is 4000 symbols';

            self.emailBodyControlConfig.onSave = function (emailBody) {
                self.settings.emailNotifierSetting.template.body = emailBody;
                self.save();
            };

            self.emailBodyControlConfig.triggerRefresh();
        }

        function initUiMessageControlConfig() {
            self.uiMessageControlConfig = new RichTextEditorModel(ControlMode.view);
            self.uiMessageControlConfig.value = self.settings.uiNotifierSetting.template.message;

            self.uiMessageControlConfig.tinyMceOptions = notificationSettingsConfig.uiMessageTinyMceOptions;

            self.uiMessageControlConfig.isRequired = true;
            self.uiMessageControlConfig.requiredValidationMessage = 'In-App message is required';
            self.uiMessageControlConfig.maxLength = 200;
            self.uiMessageControlConfig.maxLengthValidationMessage = 'In-App message max length is 200 symbols';

            self.uiMessageControlConfig.onSave = function (uiMessage) {
                self.settings.uiNotifierSetting.template.message = uiMessage;
                self.save();
            };

            self.uiMessageControlConfig.triggerRefresh();
        }

        function initLocationChangeStartEvent() {
            $rootScope.$on('$locationChangeStart', function () {
                var settingsForm = $scope.settingsForm;
                if (!settingsForm) return;
                settingsForm.$setPristine();
                var activeEditControls = angular.element(document.querySelectorAll('form[name="' + settingsForm.$name + '"] .js-active-edit-control'));
                if (activeEditControls.length > 0) {
                    settingsForm.$setDirty(); // for showing umbraco confirmation popup
                }
            });
        }

        initalize();
    }

    controller.$inject = ['$rootScope', '$scope', '$location', 'appState', 'notificationsService', 'notificationSettingsService', 'notificationSettingsConfig', 'navigationService'];

    angular.module('umbraco').controller('notificationSettingController', controller);
})(angular);