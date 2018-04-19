using LinqToTwitter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace TwitStats.Twitter
{
    class TwitterClient
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        private TwitterContext twitterCtx;

        public TwitterClient(string ConsumerKey, string ConsumerSecret)
        {
            this.ConsumerKey = ConsumerKey;
            this.ConsumerSecret = ConsumerSecret;
        }

        public async Task DoAuthorizedAsync()
        {
            var auth = new PinAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = ConsumerKey,
                    ConsumerSecret = ConsumerSecret
                },
                GoToTwitterAuthorization = (page) => 
                {
                    Process.Start("cmd","/C start "+page);
                    },
                GetPin = () =>
                {
                    Console.WriteLine(
                        "\nПосле авторизации в Twitter " +
                        "скопируйте 7-значный PIN код.\n");
                    Console.Write("Введите PIN-код здесь: ");
                    return Console.ReadLine();
                }
            };

            await auth.AuthorizeAsync();
            twitterCtx = new TwitterContext(auth);
        }

        //Получаем последние 5 твитов
        public List<TwitterTweet> GetLastTweets(string user_name)
        {

            List<TwitterTweet> twits = twitterCtx.Status.Where(t => t.ScreenName == user_name).Where(t => t.Type == StatusType.User).Where(t => t.Count == 5).Select(t => new TwitterTweet()
            {
                TweetId = (t.StatusID).ToString(),
                UserId = t.User.UserIDResponse,
                Content = t.Text,
                AuthorName = t.User.Name,
                AuthorNick = t.ScreenName
            }).ToList();

            return twits;
        }

        //Получаем статистику
        public List<TwitterStats> GetStats(List<TwitterTweet> twits)
        {
            string letters = "";
            Regex reg = new Regex(@"\W|\d");

            foreach (TwitterTweet twit in twits)
            {

                letters += reg.Replace(twit.Content.ToLower(), "");
            }


            List<TwitterStats> stats = letters.GroupBy(l => l).OrderByDescending(g => g.Count()).Select(g => new TwitterStats()
            {
                Label = g.Key,
                Count = g.Count(),
                Frequency = Math.Round(((double)g.Count() / (double)letters.Length) * 100, 3)
            }).ToList();

            return stats;

        }

        public async Task<Status> SendStatisticAsync(string user_name,List<TwitterStats> stats)
        {
            string text = "@" + user_name + ", статистика для последних 5 твитов: ";

            JObject jstats = new JObject();
            
            foreach(TwitterStats stat in stats)
            {
                jstats.Add((stat.Label).ToString(), stat.Frequency);
            }

            text +=jstats.ToString(Formatting.None).Replace("\"","");

            //Ограничение твиттера на 280 символов
            if (text.Length > 280)
            {
                throw new Exception("Текст больше ограничения в 280 символов");
            }



            Status twit =  await twitterCtx.TweetAsync(text);

            if (twit == null)
            {
                throw new Exception("Ошибка создания твита");
            }

            return twit;

        }
    }
}

