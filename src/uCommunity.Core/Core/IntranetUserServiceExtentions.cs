﻿using System;
using uCommunity.Core.User;

namespace uCommunity.Core
{
    public static class IntranetUserServiceExtentions
    {
        public static Guid GetCurrentUserId(this IIntranetUserService<IIntranetUser> intranetUserService)
        {
            var currentUser = intranetUserService.GetCurrentUser();
            return currentUser.Id;
        }

        public static int? GetCurrentUserUmbracoId(this IIntranetUserService<IIntranetUser> intranetUserService)
        {
            var currentUser = intranetUserService.GetCurrentUser();
            return currentUser.UmbracoId;
        }
    }
}