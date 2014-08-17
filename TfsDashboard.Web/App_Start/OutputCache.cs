using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TfsDashboard.Web.App_Start
{
    public class WebApiOutputCacheAttribute : ActionFilterAttribute
    {
        int _duration;
        string _key;

        public WebApiOutputCacheAttribute(int duration, string key)
        {
            if (duration <= 0)
                throw new InvalidOperationException("Invalid Duration");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Invalid Key");
            _key = key;
            _duration = duration;
        }

        private static ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        private Action<HttpActionExecutedContext> Callback { set; get; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionExecutedContext");
            }

            string cachedValue = Cache.Get(_key) as string;
            if (cachedValue != null)
            {
                actionContext.Response = actionContext.Request.CreateResponse();
                actionContext.Response.Content = new StringContent(cachedValue);
                return;
            }
            Callback = (actionExecutedContext) =>
            {
                var output = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                Cache.Add(_key, output, DateTimeOffset.UtcNow.AddSeconds(_duration));
                Cache.Add(_key + "+ContentType", actionExecutedContext.Response.Content.Headers.ContentType.MediaType, DateTimeOffset.UtcNow.AddSeconds(_duration));
            };
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException("actionExecutedContext");
            }
            Callback(actionExecutedContext);
        }
    }
}