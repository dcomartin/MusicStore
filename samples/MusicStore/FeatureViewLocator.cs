using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace MusicStore
{
    public class FeaturesViewLocator : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["customviewlocation"] = nameof(FeaturesViewLocator);
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var viewLocationFormats = new[]
            {
                "~/Features/{1}/{0}.cshtml",
                "~/Features/Shared/{0}.cshtml"
            };
            return viewLocationFormats;
        }
    }
}
