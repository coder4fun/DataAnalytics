using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Configuration;

namespace DataAnalytics
{
    public class Twitter
    {
        public const string OauthVersion = "1.0";
        public const string OauthSignatureMethod = "HMAC-SHA1";

        public Twitter()
        {
            this.ConsumerKey = WebConfigurationManager.AppSettings["ConsumerKey"];
            this.ConsumerKeySecret = WebConfigurationManager.AppSettings["ConsumerKeySecret"];
            this.AccessToken = WebConfigurationManager.AppSettings["AccessToken"];
            this.AccessTokenSecret = WebConfigurationManager.AppSettings["AccessTokenSecret"];
        }

        public Twitter(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerKeySecret = consumerKeySecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        public string ConsumerKey { set; get; }
        public string ConsumerKeySecret { set; get; }
        public string AccessToken { set; get; }
        public string AccessTokenSecret { set; get; }

        public string GetMentions(int count)
        {
            string resourceUrl =
                string.Format("http://api.twitter.com/1/statuses/mentions.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("include_entities", "true");

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            return response;
        }

        public string GetTweets(string query, int count, string sinceId)
        {
            string resourceUrl =
                string.Format("https://api.twitter.com/1.1/search/tweets.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("q", query);
            requestParameters.Add("include_entities", "false");
            requestParameters.Add("since_id", sinceId);

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            dynamic res = JsonConvert.DeserializeObject(response);
            UInt64 max_id = 0;
            foreach (JToken d in ((Newtonsoft.Json.Linq.JContainer)((JToken)res))["statuses"])
            {
                d["timestamp"] = DateTimeOffset.UtcNow;
                UInt64 id = UInt64.Parse(d["id_str"].ToString());

                max_id = id > max_id ? id : max_id;
                //var request = HttpWebRequest.Create(String.Concat("http://192.168.1.124:9200/twitter/users/", id));

                //var postData = d.ToString();
                //var data = Encoding.ASCII.GetBytes(postData);

                //request.Method = "POST";
                //request.ContentType = "application/json";
                //request.ContentLength = data.Length;

                //using (var stream = request.GetRequestStream())
                //{
                //    stream.Write(data, 0, data.Length);
                //}

                //var elasticresponse = (HttpWebResponse)request.GetResponse();
                //Console.WriteLine(d.ToString());
                try
                {
                    Database.SaveTwitterTweet(JsonConvert.DeserializeObject<TwitterTweet>(d.ToString()));
                }
                catch (Exception)
                {
                    throw;
                }

            }
            return max_id.ToString();
        }

        public string GetFollowers(string screenName, int count, string cursor)
        {
            string resourceUrl =
                string.Format("https://api.twitter.com/1.1/followers/list.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("screen_name", screenName);
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("cursor", cursor);

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            dynamic res = JsonConvert.DeserializeObject(response);
            foreach (JToken d in ((Newtonsoft.Json.Linq.JContainer)((JToken)res))["users"])
            {
                d["timestamp"] = DateTimeOffset.UtcNow;
                string id = d["id_str"].ToString();

                //var request = HttpWebRequest.Create(String.Concat("http://192.168.1.124:9200/twitter/users/", id));

                //var postData = d.ToString();
                //var data = Encoding.ASCII.GetBytes(postData);

                //request.Method = "POST";
                //request.ContentType = "application/json";
                //request.ContentLength = data.Length;

                //using (var stream = request.GetRequestStream())
                //{
                //    stream.Write(data, 0, data.Length);
                //}

                //var elasticresponse = (HttpWebResponse)request.GetResponse();
                //Console.WriteLine(d.ToString());
                try
                {
                    Database.SaveTwitterUser(JsonConvert.DeserializeObject<TwitterUser>(d.ToString()));
                }
                catch (Exception)
                {}

            }
            return ((Newtonsoft.Json.Linq.JContainer)((JToken)res))["next_cursor"].ToString();
        }

        public TwitterUserFormatted GetUser(string screenName)
        {
            TwitterUserFormatted u = Database.GetTwitteruser(screenName);
            if (u.Id == null)
            {
                GetUserFromAPI(screenName);
                return Database.GetTwitteruser(screenName);
            }
            else
            {
                return u;
            }
        }

        private void GetUserFromAPI(string screenName)
        {
            string resourceUrl =
                string.Format("https://api.twitter.com/1.1/users/show.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("screen_name", screenName);

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            dynamic res = JsonConvert.DeserializeObject(response);
            JToken d = ((Newtonsoft.Json.Linq.JContainer)((JToken)res));

            d["timestamp"] = DateTimeOffset.UtcNow;
            string id = d["id_str"].ToString();

            try
            {
                TwitterUser u = JsonConvert.DeserializeObject<TwitterUser>(d.ToString());
                Database.SaveTwitterUser(u);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetFollowers(int count)
        {
            string resourceUrl =
                string.Format("https://api.twitter.com/1.1/followers/list.json");

            var requestParameters = new SortedDictionary<string, string>();
            //requestParameters.Add("screen_name", screenName);
            requestParameters.Add("count", count.ToString());

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            return response;
        }

        public string PostStatusUpdate(string status, double latitude, double longitude)
        {
            const string resourceUrl = "http://api.twitter.com/1/statuses/update.json";

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("status", status);
            requestParameters.Add("lat", latitude.ToString());
            requestParameters.Add("long", longitude.ToString());

            return GetResponse(resourceUrl, Method.POST, requestParameters);
        }

        private string GetResponse(string resourceUrl, Method method, SortedDictionary<string, string> requestParameters)
        {
            ServicePointManager.Expect100Continue = false;
            WebRequest request = null;
            string resultString = string.Empty;

            if (method == Method.POST)
            {
                var postBody = requestParameters.ToWebString();

                request = (HttpWebRequest)WebRequest.Create(resourceUrl);
                request.Method = method.ToString();
                request.ContentType = "application/x-www-form-urlencoded";

                using (var stream = request.GetRequestStream())
                {
                    byte[] content = Encoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }
            }
            else if (method == Method.GET)
            {
                request = (HttpWebRequest)WebRequest.Create(resourceUrl + "?"
                    + requestParameters.ToWebString());
                request.Method = method.ToString();
            }
            else
            {
                //other verbs can be addressed here...
            }

            if (request != null)
            {
                var authHeader = CreateHeader(resourceUrl, method, requestParameters);
                request.Headers.Add("Authorization", authHeader);
                var response = request.GetResponse();

                using (var sd = new StreamReader(response.GetResponseStream()))
                {
                    resultString = sd.ReadToEnd();
                    response.Close();
                }
            }

            return resultString;
        }

        private string CreateOauthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private string CreateHeader(string resourceUrl, Method method,
                                    SortedDictionary<string,string> requestParameters)
        {
            var oauthNonce = CreateOauthNonce();
            // Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauthTimestamp = CreateOAuthTimestamp();
            var oauthSignature = CreateOauthSignature
            (resourceUrl, method, oauthNonce, oauthTimestamp, requestParameters);

            //The oAuth signature is then used to generate the Authentication header. 
            string headerFormat = String.Concat("OAuth oauth_nonce=\"{0}\", oauth_signature_method =\"{1}\", " ,
                                         "oauth_timestamp=\"{2}\", oauth_consumer_key =\"{3}\", " ,
                                         "oauth_token=\"{4}\", oauth_signature =\"{5}\", " ,
                                         "oauth_version=\"{6}\"");

            var authHeader = string.Format(headerFormat,
                                           Uri.EscapeDataString(oauthNonce),
                                           Uri.EscapeDataString(OauthSignatureMethod),
                                           Uri.EscapeDataString(oauthTimestamp),
                                           Uri.EscapeDataString(ConsumerKey),
                                           Uri.EscapeDataString(AccessToken),
                                           Uri.EscapeDataString(oauthSignature),
                                           Uri.EscapeDataString(OauthVersion)
                );

            return authHeader;
        }

        private string CreateOauthSignature
        (string resourceUrl, Method method, string oauthNonce, string oauthTimestamp,
                                            SortedDictionary<string, string> requestParameters)
        {
            //firstly we need to add the standard oauth parameters to the sorted list
            requestParameters.Add("oauth_consumer_key", ConsumerKey);
            requestParameters.Add("oauth_nonce", oauthNonce);
            requestParameters.Add("oauth_signature_method", OauthSignatureMethod);
            requestParameters.Add("oauth_timestamp", oauthTimestamp);
            requestParameters.Add("oauth_token", AccessToken);
            requestParameters.Add("oauth_version", OauthVersion);

            var sigBaseString = requestParameters.ToWebString();

            var signatureBaseString = string.Concat
            (method.ToString(), "&", Uri.EscapeDataString(resourceUrl), "&",
                                Uri.EscapeDataString(sigBaseString.ToString()));

            //Using this base string, we then encrypt the data using a composite of the 
            //secret keys and the HMAC-SHA1 algorithm.
            var compositeKey = string.Concat(Uri.EscapeDataString(ConsumerKeySecret), "&",
                                             Uri.EscapeDataString(AccessTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));
            }

            return oauthSignature;
        }

        private static string CreateOAuthTimestamp()
        {

            var nowUtc = DateTime.UtcNow;
            var timeSpan = nowUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            return timestamp;
        }
    }
}