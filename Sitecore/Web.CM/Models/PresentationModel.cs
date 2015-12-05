using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.CM.Models
{
    public class PresentationModel
    {
        public LayoutModel Layout { get; set; }
        public IEnumerable<RenderingModel> Renderings { get; set; }
    }

    public class LayoutModel
    {
        public string Path { get; set; }
    }

    public class RenderingModel
    {
        public string Placeholder { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Datasource { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public RenderingCachingModel Caching { get; set; }
    }

    public class RenderingCachingModel
    {
        public bool Cacheable { get; set; }
        public bool VaryByItem { get; set; }
        public bool VaryByParm { get; set; }
        public bool VaryByQueryString { get; set; }
    }
}