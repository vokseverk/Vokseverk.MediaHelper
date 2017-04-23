# Media Helper for Umbraco Views

In the good old days of XSLT, my views looked like this when I needed to render an image:

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

Of course, I could also pass in some parameters to customize the output but the main thing to note is that in my view file, I just put the equivalent of "Render the media here".

This was powered by an included XSLT file that had a bunch of templates for handling various scenarios; e.g. it didn't matter whether the `poster` property was an upload field or a DAMP picker - or the new Image Cropper in Umbraco 7. I could handle it so my view was just a single instruction for rendering the media.

These days, with Razor, my views are not that pretty - I'm working on it, but the example above would look something like this:

```razor
<figure>
	@{
		var mediaNode = Umbraco.TypedMedia(Model.Content.GetPropertyValue<int>("poster"));
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

```razor
<figure>
	@MediaHelper.RenderMedia(Model.Content.GetPropertyValue<int>("poster"), 600)
</figure>
```

or even better, when/if it's possible:

```razor
<figure>
	@MediaHelper.RenderMedia(Model.Content.Poster, 600)
</figure>
```





