﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace uIntra.Core.Extentions
{
    public static class StringExtentions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !str.IsNullOrEmpty();
        }

        public static IEnumerable<int> ToIntCollection(this string str)
        {
            return str.IsNullOrEmpty() ? Enumerable.Empty<int>() : str.Split(',').Where(s => s.IsNotNullOrEmpty()).Select(int.Parse);
        }

        public static string GetMedia(this string str, int count)
        {
            if (str.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var fileIds = str.Split(',').ToList();

            return fileIds.Count <= count ? str : fileIds.Take(count).JoinWithComma();
        }

        public static bool Contains(this IEnumerable<string> source, string toCheck, StringComparison comp)
        {
            return source.Any(s => s.IndexOf(toCheck, comp) >= 0);
        }

        public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator = ",")
        {
            return string.Join(separator, enumerable);
        }

        public static string JoinWithComma(this IEnumerable<string> list)
        {
            return list.JoinWithSeparator(", ");
        }

        public static string JoinWithSeparator(this IEnumerable<string> list, string separator)
        {
            return list == null ? "" : string.Join(separator, list);
        }
     
        public static string StripHtml(this string input)
            {
            if (input.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static string AddIdParameter(this string url, object paramValue)
        {
            return AddParameter(url, "id", paramValue);
        }

        public static string AddParameter(this string url, string paramName, object paramValue)
        {
            var queryString = string.Empty;
            if (url.Contains("?"))
            {
                var urlSplit = url.Split('?');
                url = urlSplit[0];
                queryString = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
            }

            var queryCollection = HttpUtility.ParseQueryString(queryString);
            queryCollection.Add(paramName, paramValue.ToString());
            return $"{url.TrimEnd('/')}?{queryCollection}";
        }
        public static string RemoveHtmlTags(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static int? ToNullableInt(this string str)
        {
            int result;
            if (Int32.TryParse(str, out result))
                return result;
            return null;
        }
    }
}