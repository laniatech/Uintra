using System;
using System.Collections.Generic;
using uIntra.Core.Extensions;
using uIntra.Core.TypeProviders;
using Umbraco.Web;

namespace uIntra.Core.Activity
{
    public class ActivityPageHelper : IActivityPageHelper
    {
        public IIntranetType ActivityType { get; }

        private readonly IEnumerable<string> _activityXPath;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly IDocumentTypeAliasProvider _aliasProvider;

        public ActivityPageHelper(IIntranetType activityType, IEnumerable<string> baseXPath, UmbracoHelper umbracoHelper, IDocumentTypeAliasProvider documentTypeAliasProvider)
        {
            _umbracoHelper = umbracoHelper;
            _aliasProvider = documentTypeAliasProvider;
            ActivityType = activityType;

            _activityXPath = baseXPath.Append(_aliasProvider.GetOverviewPage(ActivityType));
        }

        public string GetOverviewPageUrl()
        {
            return GetPageUrl(_activityXPath);
        }

        public string GetDetailsPageUrl(Guid? activityId = null)
        {
            var xPath = _activityXPath.Append(_aliasProvider.GetDetailsPage(ActivityType));
            var detailsPageUrl = GetPageUrl(xPath);

            return activityId.HasValue ? detailsPageUrl.AddIdParameter(activityId) : detailsPageUrl;
        }

        public string GetCreatePageUrl()
        {
            var createPage = _aliasProvider.GetCreatePage(ActivityType);

            return createPage == null
                ? null
                : GetPageUrl(_activityXPath.Append(createPage));
        }

        public string GetEditPageUrl(Guid activityId)
        {
            var xPath = _activityXPath.Append(_aliasProvider.GetEditPage(ActivityType));
            return GetPageUrl(xPath).AddIdParameter(activityId);
        }

        private string GetPageUrl(IEnumerable<string> xPath)
        {
            return _umbracoHelper.TypedContentSingleAtXPath(XPathHelper.GetXpath(xPath))?.Url;
        }
    }
}