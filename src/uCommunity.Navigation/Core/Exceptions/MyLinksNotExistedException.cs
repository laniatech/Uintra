﻿using System;
using uCommunity.Navigation.Core.Models;

namespace uCommunity.Navigation.Core.Exceptions
{
    public class MyLinksNotExistedException : ApplicationException
    {
        public MyLinksNotExistedException(MyLinkDTO model)
            : base($"Can not delete myLink with content {model.ContentId} for {model.UserId} and querString {model.QueryString}, becase it's not existed")
        {

        }
    }
}