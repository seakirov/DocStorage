using System;
using System.Web.Mvc;
using DocStorage.Models;

namespace DocStorage.Helpers
{
    public static class PageHelper
    {
        public static MvcHtmlString CreateButtons(this HtmlHelper html, PageInfo pageInfo, Func<int, string> pageUrl)
        {
            TagBuilder ul = new TagBuilder("ul");
            ul.MergeAttribute("class", "pagination");

            for (int i = 1; i <= pageInfo.TotalPages; i++)
            {
                TagBuilder li = new TagBuilder("li");

                if (i == pageInfo.PageNumber)
                {
                    li.MergeAttribute("class", "active");
                }

                TagBuilder a = new TagBuilder("a");
                a.MergeAttribute("href", pageUrl(i));              
                a.SetInnerText(i.ToString());

                li.InnerHtml = a.ToString();
                
                ul.InnerHtml += li.ToString();
            }
            return new MvcHtmlString(ul.ToString());
        }
    }
}