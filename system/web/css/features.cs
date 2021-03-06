﻿using Aura.Data;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Text;

public class FeaturesCssController : Controller
{
	private string cache;
	private DateTime cacheTime;

	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		response.ContentType = "text/css";
		response.CacheControl = "public, max-age: 3600";

		if (cache == null || cacheTime < AuraData.FeaturesDb.LastEntryRead)
		{
			var sb = new StringBuilder();
			foreach (var entry in AuraData.FeaturesDb.Entries)
			{
				var feature = entry.Value.Name;
				var enabled = entry.Value.Enabled;

				sb.AppendLine(string.Format("*[data-feature='{0}']  {{ display: {1}; }}", feature, enabled ? "inline" : "none"));
				sb.AppendLine(string.Format("*[data-feature='!{0}'] {{ display: {1}; }}", feature, !enabled ? "inline" : "none"));
				sb.AppendLine(string.Format("div[data-feature='{0}']  {{ display: {1}; }}", feature, enabled ? "block" : "none"));
				sb.AppendLine(string.Format("div[data-feature='!{0}'] {{ display: {1}; }}", feature, !enabled ? "block" : "none"));
			}

			cache = sb.ToString();
			cacheTime = DateTime.Now;
		}

		response.Send(cache);
	}
}
