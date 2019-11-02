using Umbraco;
using Umbraco.Web;
using Umbraco.Core.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Vokseverk {
	
	public class PictureSource {
		public string Media { get; set; }
		public string Crop { get; set; }
		public int Width { get; set; }
	}
	
	public class MediaHelper {
		private readonly static UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
		
		/// <summary>
		/// Render a <c>picture</c> tag with specified source elements
		/// </summary>
		/// <param name="mediaItem">The media item to render</param>
		/// <param name="sources">A <c>List</c> of <seealso cref="PictureSource" /> definitions to use as <c>source</c> elements</param>
		/// <param name="cssClass">An optional CSS classname to put on the <c>picture</c> tag</param>
		/// <example>
		/// <code>
		/// @{
		///   var sources = new List<PictureSource>();
		///   
		///   sources.Add(new PictureSource { Media = "max375", Crop = "Portrait", Width = 400 });
		///   sources.Add(new PictureSource { Media = "min376", Crop = "Landscape", Width = 800 });
		///   sources.Add(new PictureSource { Media = "min1200", Crop = "Landscape", Width = 1600 });
		///   // Specify `""` or `null` for the default to load in the `<img>` tag
		///   sources.Add(new PictureSource { Media = "", Crop = "Landscape", Width = 600 });
		/// }
		/// @MediaHelper.RenderPicture(Model.PageImage, sources)
		/// </code>
		/// </example>
		public static HtmlString RenderPicture(IPublishedContent mediaItem, List<PictureSource> sources, string cssClass = "") {
			var html = string.Format("<picture class=\"{0}\">", cssClass);
			html = html.Replace(" class=\"\"", "");
			string mediaAttr = "";
			
			try {
				foreach (var source in sources) {
					var mediaURL1x = mediaItem.GetCropUrl(cropAlias: source.Crop, width: source.Width, quality: 70);
					var mediaURL2x = mediaItem.GetCropUrl(cropAlias: source.Crop, width: source.Width * 2, quality: 40);

					if (string.IsNullOrEmpty(source.Media)) {
						// Add required `<img>` tag
						html += GetOutputTag(mediaURL1x, mediaItem.Name);
					} else {
						// Add `<source>` tag
						mediaAttr = source.Media.Replace("min", "min-width: ");
						mediaAttr = mediaAttr.Replace("max", "max-width: ");
					
						mediaAttr = string.Format("({0}px)", mediaAttr);
						html += GetSourceTag(mediaURL1x, mediaURL2x, mediaAttr);
					}
				}
			} catch (Exception ex) {
				html += "<p style=\"color:red;font-weight:bold;\">Error: " + ex.Message + "</p>";
			}
			
			html += "</picture>";
			
			return new HtmlString(html);
		}
		
		
		/// <summary>
		/// Render an <c>img</c> tag with <c>srcset</c> and <c>src</c> attributes for a media item,
		/// using the specified crop and output width.
		/// </summary>
		public static HtmlString RenderMedia(object mediaId, string crop, int width) {
			string imageTag = "";
			
			try {
				var media = umbracoHelper.TypedMedia(mediaId);
				if (media != null) {
					var crop1x = media.GetCropUrl(cropAlias: crop, width: width, quality: 70);
					var crop2x = media.GetCropUrl(cropAlias: crop, width: width * 2, quality: 40);
					
					imageTag = GetOutputTag(crop1x, crop2x, media.Name);
				}
			} catch (Exception ex) {
				imageTag = GetOutputTag("/media/blank.png", string.Format("Did not find the media item. ({0})", ex.Message));
			}
			
			return new HtmlString(imageTag);
		}

		/// <summary>
		/// Overload to render an <c>img</c> tag with <c>srcset</c> and <c>src</c> attributes for a media item,
		/// using the specified output width (as the 1x width).
		/// </summary>
		public static HtmlString RenderMedia(object mediaId, int width) {
			string imageTag = "";
			
			try {
				var media = umbracoHelper.TypedMedia(mediaId);
				if (media != null) {
					// Need to use `Url` instead of `GetCropUrl()`
					// to not get a crop. Then build manually...
					var url = media.Url;
					var combiner = url.Contains("?") ? "&" : "?";
					var size1x = string.Format("{0}{1}width={2}&quality=70", url, combiner, width);
					var size2x = string.Format("{0}{1}width={2}&quality=40", url, combiner, width * 2);
					
					var extension = media.GetPropertyValue<string>("umbracoExtension");
					imageTag = extension == "gif"
						? GetOutputTag(url, media.Name)
						: GetOutputTag(size1x, size2x, media.Name);
				}
			} catch (Exception ex) {
				imageTag = GetOutputTag("/media/blank.png", string.Format("Could not find media item. ({0})", ex.Message));
			}
			
			return new HtmlString(imageTag);
		}
		
		// Overloads for rendering media when the image is already an IPublishedContent
		public static HtmlString RenderMedia(IPublishedContent image, string crop, int width) {
			return RenderMedia(image.Id, crop, width);
		}

		public static HtmlString RenderMedia(IPublishedContent image, int width) {
			return RenderMedia(image.Id, width);
		}
		
		/// <summary>
		/// Get the URL for a placeholder image of the specified size
		/// </summary>
		/// <param name="size">A string of the form <c>800x600</c> to define the width and height of the placeholder</param>
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
		
		#region Private
		
		private static string GetOutputTag(string image, string altText) {
			return string.Format("<img src=\"{0}\" alt=\"{1}\" />", image, altText);
		}
		
		private static string GetOutputTag(string size1x, string size2x, string altText) {
			return string.Format("<img srcset=\"{0} 2x\" src=\"{1}\" alt=\"{2}\" />", size2x, size1x, altText);
		}
		
		private static string GetSourceTag(string size1x, string size2x, string media) {
			return string.Format("<source media=\"{2}\" srcset=\"{0} 2x,{1}\" />", size2x, size1x, media);
		}
		
		#endregion
	}
}
