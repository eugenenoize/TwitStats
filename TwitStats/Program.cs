using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TwitStats.Twitter;
using System.Threading.Tasks;
using LinqToTwitter;

namespace TwitStats
{
    class Program
    {
        static void Main(string[] args)
        {
            bool repeate = false;

            Console.WriteLine("\nДобро пожаловать в Сбор частотной статистики");
            Console.WriteLine("Для работы с программой необходимо пройти авторизацию");
            Console.WriteLine();

            TwitterClient twitter = new TwitterClient("ogCNhN64pQydIMJHTaSh6Nin1", "8O6cKHRq1nrKfCIYCvwI6pknmow1MJCAWbHkPGCNBHBQ4N1iAF");

            try
            {
                Task TwitterAuth = TwitterAuthAsync(twitter);
                TwitterAuth.Wait();

                do
                {
                    try
                    {
                        Task TaskWork = TwitterWorkAsync(twitter);
                        TaskWork.Wait();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nОшибка: {0}", ex.Message);
                        Console.WriteLine("\nПовторить работу программы?(Y/N)");
                        char key = Console.ReadKey(true).KeyChar;
                        switch (char.ToUpper(key))
                        {
                            case 'Y':
                                {
                                    repeate = true;
                                    break;
                                }
                            default:
                                {
                                    repeate = false;
                                    Console.WriteLine("\nНажмите любую клавишу для выхода...");
                                    Console.ReadKey();
                                    break;
                                }
                        }
                    }
                }
                while (repeate);
            }
            catch(Exception ex)
            {
                Console.WriteLine("\nОшибка авторизации: {0}", ex.Message);
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        static async Task TwitterAuthAsync(TwitterClient twitter)
        {
            await twitter.DoAuthorizedAsync();
        }

        static async Task TwitterWorkAsync(TwitterClient twitter)
        {

            char key;

            do
            {
                ShowMenu();

                key = Console.ReadKey(true).KeyChar;

                switch (char.ToUpper(key))
                {
                    case '1':
                        {
                            await GetStatistic(twitter);
                            break;
                        }
                    case 'q':
                    case 'Q':
                        {
                            Console.WriteLine("Выход из программы");
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Неверно нажата клавиша");
                            break;
                        }
                }
            }
            while (char.ToUpper(key) != 'Q');

        }

        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Пожалуйста выберите категорию:");
            Console.WriteLine("Клавиша 1: Частотная статистика последних пяти твитов");
            Console.WriteLine("Клавиша Q: Выход из программы");
        }


        private static async Task GetStatistic(TwitterClient twitter)
        {
            Console.WriteLine("Введите имя необходимого пользователя:");

            string user_name = Console.ReadLine();

            List<TwitterTweet> twits = twitter.GetLastTweets(user_name);

            if (twits.Count == 0)
            {
                Console.WriteLine("Твиты не найдены");
            }
            else
            {
                Console.WriteLine("Последние твиты:");
                int count = 1;
                foreach (TwitterTweet twit in twits)
                {
                    Console.WriteLine("{0}: Твит - {1}, Пользователь - {2}", count, twit.TweetId, twit.AuthorNick);
                    Console.WriteLine("{0}", twit.Content);
                    Console.WriteLine();
                    count++;
                }

                List<TwitterStats> stats = twitter.GetStats(twits);

                Console.WriteLine("Частотная статистика твитов:");
                count = 1;
                Console.WriteLine("№   Буква   Количество   Частотность");
                foreach (TwitterStats stat in stats)
                {
                    Console.WriteLine("{0}     {1}     {2}     {3}%", count, stat.Label, stat.Count, stat.Frequency);
                    count++;
                }

                bool repeatestat = true;
                do
                {
                    Console.WriteLine("Отправить статистику себе? (y/n)");
                    string send = Console.ReadLine();
                    switch (send)
                    {
                        case "y":
                            {
                                repeatestat = false;
                                await twitter.SendStatisticAsync(user_name,stats);
                                break;
                            }
                        case "n":
                            {
                                repeatestat = false;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Вы ввели неверный символ. Повторите ввод.");
                                break;
                            }
                    }
                }
                while (repeatestat);

            }

        }
    }
}

