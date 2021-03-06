﻿using System.Collections.Generic;
using System.Linq;

namespace Uintra.Search
{
    public class SearchResult<T>
    {
        public IEnumerable<T> Documents { get; set; } = Enumerable.Empty<T>();

        public IEnumerable<BaseFacet> TypeFacets { get; set; } = Enumerable.Empty<BaseFacet>();

        public long TotalHits { get; set; }
    }
}