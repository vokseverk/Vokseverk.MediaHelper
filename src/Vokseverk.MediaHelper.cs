using Umbraco;
using Umbraco.Web;
using Umbraco.Core.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Vokseverk {
	
	public class MediaHelper {
		private readonly static UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
		
		/// <summary>
		/// Render an img tag with srcset and src attributes for a media item,
		/// using the specified crop and output width.
		/// </summary>
		public static HtmlString RenderMedia(object mediaId, string crop, int width) {
			string imageTag = "";
			
			try {
				var media = umbracoHelper.TypedMedia(mediaId);
				if (media != null) {
					var crop1x = media.GetCropUrl(cropAlias: crop, width: width);
					var crop2x = media.GetCropUrl(cropAlias: crop, width: width * 2);
					
					imageTag = string.Format("<img srcset=\"{0} 2x\" src=\"{1}\" alt=\"\" />", crop2x, crop1x);
				}
			} catch {
				imageTag = @"<img src=""/media/blank.png"" alt=""Could not find media item"" />";
			}
			
			return new HtmlString(imageTag);
		}

		/// <summary>
		/// Overload to render an img tag with srcset and src attributes for a media item,
		/// using the specified output width.
		/// </summary>
		public static HtmlString RenderMedia(object mediaId, int width) {
			string imageTag = "";
			
			try {
				var media = umbracoHelper.TypedMedia(mediaId);
				if (media != null) {
					var size1x = media.GetCropUrl(width: width);
					var size2x = media.GetCropUrl(width: width * 2);
					
					imageTag = string.Format("<img srcset=\"{0} 2x\" src=\"{1}\" alt=\"\" />", size2x, size1x);
				}
			} catch {
				imageTag = @"<img src=""/media/blank.png"" alt=""Could not find media item"" />";
			}
			
			return new HtmlString(imageTag);
		}
		
	
		public static string GetPlaceholderUrl(string size) {
			return string.Format("//placehold.it/{0}", size);
		}
		
		public static string GetMediaUrl(object mediaId) {
			try {
				var media = umbracoHelper.TypedMedia(mediaId);
				if (media != null) {
					return media.Url;
				} else {
					return "(Media not found in GetMediaUrl)";
				}
			} catch {
				return "(Error in GetMediaUrl)";
			}
		}
	}
}
