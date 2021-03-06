﻿using System;
using Uintra.Core.Extensions;
using Uintra.Core.User;

namespace Uintra.Core.Links
{
    public class ProfileLinkProvider : IProfileLinkProvider
    {
        private readonly IIntranetUserContentProvider _intranetUserContentProvider;

        public ProfileLinkProvider(IIntranetUserContentProvider intranetUserContentProvider)
        {
            _intranetUserContentProvider = intranetUserContentProvider;
        }

        public string GetProfileLink(IIntranetUser user) => 
            GetProfileLink(user.Id);

        public string GetProfileLink(Guid userId) => 
            _intranetUserContentProvider.GetProfilePage().Url.AddIdParameter(userId);
    }
}