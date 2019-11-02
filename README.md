# Media Helper for Umbraco Views

In the good old days of XSLT, my views for sites built with [Umbraco][UMB] looked something like this when I needed to render an image:

```xslt
<figure>
	<xsl:apply-templates select="$currentPage/poster" mode="media" />
</figure>
```

which would give me the `img` tag with both `srcset` and `src` attributes, e.g.:

```html
<figure>
	<img
		srcset="/media/1977/ep4-poster.jpg?width=1200 2x"
		src="/media/1977/ep4-poster.jpg?width=600"
		alt="Episode IV Poster"
	/>
</figure>
```

Of course, I could also pass in some parameters to customize the output but the main thing to note is that in my view file, I only had to put the equivalent of "Render the media here".

This was powered by an included XSLT file that had a bunch of templates for handling various scenarios; e.g. it didn't matter whether the `poster` property was an upload field or a DAMP picker - or the new Image Cropper in Umbraco 7. I could handle it so my view was just a single instruction for rendering the media.

These days, with Razor, my views are not that pretty - I'm working on it, but the example above would look something like this:

```csharp
<figure>
	@{
		var mediaNode = Umbraco.TypedMedia(Model.GetPropertyValue<int>("poster"));
	}
	<img
		srcset="@mediaNode.GetCropUrl(width: 1200) 2x"
		src="@mediaNode.GetCropUrl(width: 600)"
		alt="@mediaNode.Name"
	/>
</figure>
```

Note that this has no `null`-checking, does only cater for the `poster` property being a Media Picker and generally looks like a whole lot of work to "just" display an image.

So that's why I created this helper file — to enable me to boil the media rendering process down into a single line once again: 

```csharp
<figure>
	@MediaHelper.RenderMedia(Model.GetPropertyValue<int>("poster"), 600)
</figure>
```

or even better, when/if possible using [Models Builder][MB]:

```csharp
<figure>
	@MediaHelper.RenderMedia(Model.Poster, 600)
</figure>
```

## Using MediaHelper

Add the `Vokseverk.MediaHelper.cs` file to your project (or put it the `/App_Code` folder).

Then in your views, add a reference to the **Vokseverk** namespace:

```csharp
@using Vokseverk
```

Now you should be able to use the various media helpers.

*Note: All of the `RenderMedia` methods render a single `<img>` tag with `src` and `srcset` attributes — rendering 1x and 2x URLs for the media item. The `width` parameter specifies the 1x width (the 2x width is automatically calculated). The `RenderPicture` method renders a `<picture>` element with a number of `<source>` children with `srcset` attributes, along with the default <img> fallback element.*

### RenderMedia(mediaId, width)

Render an image, specifying its Id and a specific output width for the image.

### RenderMedia(mediaId, crop, width)

Render an image, specifying its Id with a specific crop and width.

### RenderMedia(image, width)

Render an image, specifying a specific output width for the image.

### RenderMedia(image, crop, width)

Render an image with a specific crop and width.

- - - 

### RenderPicture(media, sources)

Render a `<picture>` tag with a set of `<source>` children. The `sources` param is a List of sources, e.g.:

```csharp
@{
	var sources = new List<PictureSource>();
	
	sources.Add(new PictureSource { Media = "max375", Crop = "Portrait", Width = 400 });
	sources.Add(new PictureSource { Media = "min376", Crop = "Landscape", Width = 800 });
	sources.Add(new PictureSource { Media = "min1200", Crop = "Landscape", Width = 1400 });
	// Specify Media as `""` or `null` for the default to load in the `<img>` tag
	sources.Add(new PictureSource { Media = "", Crop = "Landscape", Width = 600 });
}

@MediaHelper.RenderPicture(Model.PageImage, sources)
```

Again, the `Width` parameter specifies the desired 1x width of the image.

The above would then output something like this (omitting various QueryString params for the crops):

```html
<picture>
	<source media="(max-width: 375px)" srcset="/media/1234/image.jpg?width=800 2x,/media/1234/image.jpg?width=400">
	<source media="(min-width: 376px)" srcset="/media/1234/image.jpg?width=1600 2x,/media/1234/image.jpg?width=800">
	<source media="(min-width: 1200px)" srcset="/media/1234/image.jpg?width=2800 2x,/media/1234/image.jpg?width=1400">
	<img src="/media/1234/image.jpg?width=600" alt="Image description">
</picture>
```

## Notes

The generated URLs have a burned-in quality setting applied — for 1x images it's `70` and for 2x it's `40`. These are values we've found works pretty great for most of the scenarios we cover, by not hurting the image quality while keeping the download sizes down. I'd like to have those values configurable but haven't found a great way yet — if you have a good suggestion, by all means [let me know.][ISSUE1]


[UMB]: https://umbraco.com/
[MB]: https://our.umbraco.com/documentation/Reference/Templating/Modelsbuilder/
[ISSUE1]: https://github.com/vokseverk/Vokseverk.MediaHelper/issues/1
