using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalytics
{
    public class TwitterTweet
    {
        public string id;

        public TwitterUser user;

        public string created_at;

        public string text;

        //public List<Entity> entities;

        public long retweet_count;

        public long favorite_count;

        public bool possibly_sensitive;

        public string lang;
    }
}
