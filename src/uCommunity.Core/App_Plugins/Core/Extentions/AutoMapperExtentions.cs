﻿using AutoMapper;

namespace uCommunity.Core.Extentions
{
    public static class AutoMapperExtentions
    {
        public static T Map<T>(this object obj) where T: class
        {
            return Mapper.Map<T>(obj);
        }
    }
}