﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace Karambolo.AspNetCore.Bundling.ViewHelpers
{
    public static class Styles
    {
        public static string DefaultTagFormat { get; set; } = "<link href=\"{0}\" rel=\"stylesheet\"/>";

        public static Task<IHtmlContent> UrlAsync(string path)
        {
            return ViewHelper.UrlAsync(path, addVersion: null);
        }

        public static Task<IHtmlContent> UrlAsync(string path, bool addVersion)
        {
            return ViewHelper.UrlAsync(path, addVersion);
        }

        public static Task<IHtmlContent> RenderAsync(params string[] paths)
        {
            return ViewHelper.RenderFormatAsync(DefaultTagFormat, addVersion: null, paths);
        }

        public static Task<IHtmlContent> RenderFormatAsync(string tagFormat, params string[] paths)
        {
            return ViewHelper.RenderFormatAsync(tagFormat ?? DefaultTagFormat, addVersion: null, paths);
        }

        public static Task<IHtmlContent> RenderFormatAsync(string tagFormat, bool addVersion, params string[] paths)
        {
            return ViewHelper.RenderFormatAsync(tagFormat ?? DefaultTagFormat, addVersion, paths);
        }
    }
}
