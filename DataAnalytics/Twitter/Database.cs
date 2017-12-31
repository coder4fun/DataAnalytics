using System;
using Dapper;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Web.Configuration;

namespace DataAnalytics
{
    public static class Database
    {
        static string sqlConnection = WebConfigurationManager.AppSettings["mysqlConnectionstring"];
        public static bool SaveTwitterUser(TwitterUser user)
        {
            try
            {
                using (var conn = new MySqlConnection(sqlConnection))
                {
                    conn.Open();
                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = @"INSERT INTO `DataAnalytics`.`Twitter_User`
                                    (	`Id`,
	                                    `Name`,
	                                    `screen_name`,
	                                    `location`,
	                                    `Description`,
	                                    `FollowersCount`,
	                                    `FriendsCount`,
	                                    `ListedCount`,
	                                    `FavouritesCount`,
	                                    `TweetCount`,
	                                    `CreatedDate`
                                    )
                                    VALUES
                                    (	@Id,
	                                    @Name,
	                                    @screen_name,
	                                    @location,
	                                    @Description,
	                                    @FollowersCount,
	                                    @FriendsCount,
	                                    @ListedCount,
	                                    @FavouritesCount,
	                                    @TweetCount,
	                                    @CreatedDate
                                    )
                                    ON DUPLICATE KEY UPDATE Id = VALUES(Id), FollowersCount = VALUES(FollowersCount)";
                    comm.Parameters.AddWithValue("@Id", user.id);
                    comm.Parameters.AddWithValue("@Name", user.name);
                    comm.Parameters.AddWithValue("@screen_name", user.screen_name);
                    comm.Parameters.AddWithValue("@location", user.location);
                    comm.Parameters.AddWithValue("@Description", user.description);
                    comm.Parameters.AddWithValue("@FollowersCount", user.followers_count);
                    comm.Parameters.AddWithValue("@FriendsCount", user.friends_count);
                    comm.Parameters.AddWithValue("@ListedCount", user.listed_count);
                    comm.Parameters.AddWithValue("@FavouritesCount", user.favourites_count);
                    comm.Parameters.AddWithValue("@TweetCount", user.statuses_count);
                    comm.Parameters.AddWithValue("@CreatedDate", user.created_at);
                    comm.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool SaveTwitterTweet(TwitterTweet tweet)
        {
            try
            {
                using (var conn = new MySqlConnection(sqlConnection))
                {
                    conn.Open();
                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = @"INSERT INTO `DataAnalytics`.`Twitter_Tweet`
                                        (	`Id`,
	                                        `tweet`,
	                                        `CreatedDate`,
	                                        `UserId`,
	                                        `UserName`,
	                                        `UserScreenName`,
	                                        `RetweetCount`,
	                                        `FavouriteCount`,
	                                        `PossiblySensitive`,
	                                        `lang`
                                        )
                                        VALUES
                                        (	@Id,
	                                        @tweet,
	                                        @CreatedDate,
	                                        @UserId,
	                                        @UserName,
	                                        @UserScreenName,
	                                        @RetweetCount,
	                                        @FavouriteCount,
	                                        @PossiblySensitive,
	                                        @lang
                                        )
                                        ON DUPLICATE KEY UPDATE Id = VALUES(Id), RetweetCount = VALUES(RetweetCount)";
                    comm.Parameters.AddWithValue("@Id", tweet.id);
                    comm.Parameters.AddWithValue("@tweet", tweet.text);
                    comm.Parameters.AddWithValue("@CreatedDate", tweet.created_at);
                    comm.Parameters.AddWithValue("@UserId", tweet.user.id);
                    comm.Parameters.AddWithValue("@UserName", tweet.user.name);
                    comm.Parameters.AddWithValue("@UserScreenName", tweet.user.screen_name);
                    comm.Parameters.AddWithValue("@RetweetCount", tweet.retweet_count);
                    comm.Parameters.AddWithValue("@FavouriteCount", tweet.favorite_count);
                    comm.Parameters.AddWithValue("@PossiblySensitive", tweet.possibly_sensitive);
                    comm.Parameters.AddWithValue("@lang", tweet.lang);
                    comm.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        internal static TwitterUserFormatted GetTwitteruser(string screenName)
        {
            try
            {
                using (var conn = new MySqlConnection(sqlConnection))
                {
                    conn.Open();
                    string sql = @"SELECT	`Id`,
		                                        `Name`,
		                                        `screen_name`,
		                                        `location`,
		                                        `Description`,
		                                        `FollowersCount`,
		                                        `FriendsCount`,
		                                        `ListedCount`,
		                                        `FavouritesCount`,
		                                        `TweetCount`,
		                                        `CreatedDate`
                                        FROM `DataAnalytics`.`Twitter_User`
                                        WHERE `screen_name` = @screenname";
                    //comm.Parameters.AddWithValue("@screen_name", screenName);
                    TwitterUserFormatted u = conn.Query<TwitterUserFormatted>(sql, new { screenName }).ToList().FirstOrDefault();
                    conn.Close();

                    return u;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}