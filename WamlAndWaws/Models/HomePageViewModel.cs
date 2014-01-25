using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WamlAndWaws.Models
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            this.WebSites = new List<WebSiteRowItem>();
            this.WebSpaces = new List<WebSpaceListItem>();
        }

        public List<WebSiteRowItem> WebSites { get; set; }
        public List<WebSpaceListItem> WebSpaces { get; set; }
    }

    public class WebSpaceListItem
    {
        public string WebSpaceName { get; set; }
        public string GeoRegion { get; set; }
    }

    public class WebSiteRowItem
    {
        public WebSiteRowItem()
        {
            this.DomainNames = new List<string>();
        }

        public string WebSiteName { get; set; }
        public WebSpaceListItem WebSpace { get; set; }
        public List<string> DomainNames { get; set; }
    }
}