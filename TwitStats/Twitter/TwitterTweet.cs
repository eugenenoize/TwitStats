using System;
using System.Collections.Generic;
using System.Text;

namespace TwitStats.Twitter
{
    public class TwitterTweet
    {
        public string UserId { get; set; }
        public string TweetId { get; set; }
        public string AuthorNick { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }
    }
}
