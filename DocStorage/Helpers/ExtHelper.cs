using System.Web.Mvc;

namespace DocStorage.Helpers
{
    public static class ExtHelper
    {
        public static MvcHtmlString TrimString(this MvcHtmlString value, int maxLength)
        {
            if (value.ToString().Length > maxLength)
            {
                return new MvcHtmlString(value.ToString().Substring(0, maxLength) + "...");
            }

            return value;
        }
    }
}