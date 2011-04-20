using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace PagingLinks
{
    public static class ExtensionMethods
    {
        public static MvcHtmlString PagingLinks(this HtmlHelper helper, int totalPages, string currentPageUrlParameter = "pagenumber")
        {
            RouteData routeData = helper.ViewContext.RouteData;

            string actionName = routeData.GetRequiredString("action");
            string controllerName = routeData.GetRequiredString("controller");

            RouteValueDictionary routeValues = new RouteValueDictionary(routeData.Values);

            int currentPage = 1;
            if (routeValues.ContainsKey(currentPageUrlParameter))
            {
                // Pagenumber is present in RouteValues
                Int32.TryParse(Convert.ToString(routeValues[currentPageUrlParameter]), out currentPage);
            }
            else if (!String.IsNullOrEmpty(helper.ViewContext.HttpContext.Request.QueryString[currentPageUrlParameter]))
            {
                // Pagenumber is present in QueryString
                Int32.TryParse(helper.ViewContext.HttpContext.Request.QueryString[currentPageUrlParameter], out currentPage);
            }

            // Add all values from the QueryString to the RouteValues so they are preserved in the pagelinks
            foreach (var key in helper.ViewContext.HttpContext.Request.QueryString.AllKeys)
            {
                if (!routeValues.ContainsKey(key)) routeValues.Add(key, helper.ViewContext.HttpContext.Request.QueryString[key]);
            }

            // Get the different items ("previous", "next", pagenumbers, "...") that need to be rendered
            var pageItems = GenerateLinkItems(currentPage, totalPages);

            StringBuilder pagerCode = new StringBuilder();
            pagerCode.Append(@"<div class=""pager"">");
            foreach (var pageItem in pageItems)
            {
                if (pageItem.IsLink)
                {
                    routeValues[currentPageUrlParameter] = pageItem.Pagenumber;
                    pagerCode.Append(helper.ActionLink(pageItem.Text, actionName, controllerName, routeValues, new Dictionary<string, object> { { "class", pageItem.CssClass } }));
                }
                else
                {
                    TagBuilder span = new TagBuilder("span");
                    span.AddCssClass(pageItem.CssClass);
                    span.SetInnerText(pageItem.Text);
                    pagerCode.Append(span.ToString());
                }
            }
            pagerCode.Append("</div>");

            return MvcHtmlString.Create(pagerCode.ToString());
        }

        private static List<int> GenerateNumbers(int currentPage, int totalPages)
        {
            var pages = new List<int>();

            if (currentPage <= 4) pages.AddRange(Enumerable.Range(1, 5));
            else pages.Add(1);

            if (currentPage > totalPages - 4) pages.AddRange(Enumerable.Range(totalPages - 4, 5));
            else pages.Add(totalPages);

            if (currentPage > 4 && currentPage <= totalPages - 4)
            {
                pages.AddRange(Enumerable.Range(currentPage - 2, 5));
            }

            return pages.Where(p => p >= 1 && p <= totalPages).Distinct().OrderBy(p => p).ToList();
        }

        private static List<PagelinkItem> GenerateLinkItems(int currentPage, int totalPages)
        {
            var pageLinks = new List<PagelinkItem>();

            var prev = new PagelinkItem { Text = "prev", CssClass = "previous" };
            if (currentPage != 1)
            {
                prev.Pagenumber = currentPage - 1;
            }
            pageLinks.Add(prev);

            int previousPageNumber = 0;
            foreach (var pageNumber in GenerateNumbers(currentPage, totalPages))
            {
                if (previousPageNumber + 1 < pageNumber || previousPageNumber - 1 > pageNumber)
                {
                    pageLinks.Add(new PagelinkItem { Text = "...", CssClass = "dots" });
                }
                previousPageNumber = pageNumber;

                var page = new PagelinkItem { Text = Convert.ToString(pageNumber), CssClass = "number currentPage" };
                if (pageNumber != currentPage)
                {
                    page.Pagenumber = pageNumber;
                    page.CssClass = "number";
                }
                pageLinks.Add(page);
            }

            var next = new PagelinkItem { Text = "next", CssClass = "next" };
            if (currentPage != totalPages)
            {
                next.Pagenumber = currentPage + 1;
            }
            pageLinks.Add(next);

            return pageLinks;
        }

        private class PagelinkItem
        {
            public string Text { get; set; }
            public int? Pagenumber { get; set; }
            public string CssClass { get; set; }

            public bool IsLink
            {
                get { return this.Pagenumber.HasValue; }
            }
        }
    }
}