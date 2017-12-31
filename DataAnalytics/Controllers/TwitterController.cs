using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DataAnalytics.Controllers
{
    public class TwitterController : ApiController
    {
        public TwitterUserFormatted GetUser(string id)
        {
            return (new Twitter()).GetUser(id);
        }
    }
}
