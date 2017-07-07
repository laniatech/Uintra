//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder v3.0.7.99
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.ModelsBuilder;
using Umbraco.ModelsBuilder.Umbraco;

namespace Umbraco.Web.PublishedContentModels
{
	// Mixin content Type 1069 with alias "defaultGridComposition"
	/// <summary>Default Grid Composition</summary>
	public partial interface IDefaultGridComposition : IPublishedContent
	{
		/// <summary>Grid</summary>
		Newtonsoft.Json.Linq.JToken Grid { get; }
	}

	/// <summary>Default Grid Composition</summary>
	[PublishedContentModel("defaultGridComposition")]
	public partial class DefaultGridComposition : PublishedContentModel, IDefaultGridComposition
	{
#pragma warning disable 0109 // new is redundant
		public new const string ModelTypeAlias = "defaultGridComposition";
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
#pragma warning restore 0109

		public DefaultGridComposition(IPublishedContent content)
			: base(content)
		{ }

#pragma warning disable 0109 // new is redundant
		public new static PublishedContentType GetModelContentType()
		{
			return PublishedContentType.Get(ModelItemType, ModelTypeAlias);
		}
#pragma warning restore 0109

		public static PublishedPropertyType GetModelPropertyType<TValue>(Expression<Func<DefaultGridComposition, TValue>> selector)
		{
			return PublishedContentModelUtility.GetModelPropertyType(GetModelContentType(), selector);
		}

		///<summary>
		/// Grid
		///</summary>
		[ImplementPropertyType("grid")]
		public Newtonsoft.Json.Linq.JToken Grid
		{
			get { return GetGrid(this); }
		}

		/// <summary>Static getter for Grid</summary>
		public static Newtonsoft.Json.Linq.JToken GetGrid(IDefaultGridComposition that) { return that.GetPropertyValue<Newtonsoft.Json.Linq.JToken>("grid"); }
	}
}