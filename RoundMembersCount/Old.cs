using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundMembersCount
{
    internal static class Old
    {
        private static string path = @"D:\rounds.txt";

        private static void Write(string str)
        {
            Console.Write(str);

            File.AppendAllText(path, str);
        }

        private static void WriteLine(string str)
        {
            Console.WriteLine(str);

            File.AppendAllLines(path, new[] {str});
        }

        public static void WriteResults(int winnersCount, int rangeStart, int rangeCount, int マイナーリーグとメジャーリーグの差の下限)
        {

            MyClass prev = null;

            foreach (var membersCount in Enumerable.Range(rangeStart, rangeCount))
            {
                {
                    Write("|");
                    Write(membersCount.ToString());

                    Write("|");
                    Write(winnersCount.ToString());

                    var writing =
                        GetResults(membersCount, winnersCount, マイナーリーグとメジャーリーグの差の下限)
                            .Where(m =>
                            {
                                if (prev == null)
                                {
                                    return true;
                                }

                                var result =
                                    prev.MajorLeagueMembersCount == m.MajorLeagueMembersCount
                                    || prev.MinorLeagueMembersCount == m.MinorLeagueMembersCount;

                                prev = m;
                                return result;
                            })
                            .OrderBy(a => Math.Abs(a.Gosa))
                            .First();
                    Write(writing);
                }
            }

        }

        private static IEnumerable<MyClass> GetResults(int membersCount, int winners, int マイナーリーグとメジャーリーグの差の下限)
        {
            var result =
                Enumerable.Range(1, membersCount)
                    .Select(i => new {MinorLeagueMembersCount = i, MajorLeagueMembersCount = membersCount - i})
                    .Where(a => a.MajorLeagueMembersCount - a.MinorLeagueMembersCount >= マイナーリーグとメジャーリーグの差の下限)
                    .SelectMany(a =>
                    {
                        return 勝者数の組み合わせ(winners)
                            .Select(tuple => new
                            {
                                a.MinorLeagueMembersCount,
                                a.MajorLeagueMembersCount,
                                MinorLeagueWinnersCount = tuple.Item1,
                                MajorLeagueWinnersCount = tuple.Item2,
                                MinorLeagueWinRate = ((double) tuple.Item1)/a.MinorLeagueMembersCount,
                                MajorLeagueWinRate = ((double) tuple.Item2)/a.MajorLeagueMembersCount
                            });
                    })
                    .Where(a => { return a.MajorLeagueWinRate < 1; })
                    .Where(a => a.MinorLeagueWinRate < 1)
                    .Select(a =>
                        new MyClass
                        {
                            MinorLeagueMembersCount = a.MinorLeagueMembersCount,
                            MajorLeagueMembersCount = a.MajorLeagueMembersCount,
                            MajorLeagueWinRate = a.MajorLeagueWinRate,
                            MinorLeagueWinRate = a.MinorLeagueWinRate,
                            MajorLeagueWinnersCount = a.MajorLeagueWinnersCount,
                            MinorLeagueWinnersCount = a.MinorLeagueWinnersCount,
                            Gosa = 誤差算出(a.MinorLeagueWinRate, a.MajorLeagueWinRate),
                            GosaRate = (a.MajorLeagueWinRate/a.MinorLeagueWinRate).ToString("0.00") + " : 1",
                        });
            //.OrderBy(a => Math.Abs(a.Gosa))
            //.Take(takeCount);

            return result;
        }


        private static double 誤差算出(double minorLeagueWinRate, double majorLeagueWinRate)
        {
            return 1 - (majorLeagueWinRate/minorLeagueWinRate/2);

        }


        private static IEnumerable<Tuple<int, int>> 勝者数の組み合わせ(int source)
        {

            return
                Enumerable.Range(1, source - 1)
                    .Select(i => Tuple.Create(i, source - i));


        }

        private class MyClass
        {
            public int MinorLeagueMembersCount { get; set; }
            public int MajorLeagueMembersCount { get; set; }
            public double MajorLeagueWinRate { get; set; }
            public double MinorLeagueWinRate { get; set; }
            public int MajorLeagueWinnersCount { get; set; }
            public int MinorLeagueWinnersCount { get; set; }
            public double Gosa { get; set; }
            public string GosaRate { get; set; }
        }


        private static void Write(MyClass r)
        {
            Write("|");

            //Write("|メジャーリーグ: ");
            Write(r.MajorLeagueWinnersCount.ToString());
            Write("/");
            Write(r.MajorLeagueMembersCount.ToString());
            Write(" (");
            Write((r.MajorLeagueWinRate*100).ToString("0.00") + "%)");

            Write("|");

            //Write("マイナーリーグ: ");
            Write(r.MinorLeagueWinnersCount.ToString());
            Write("/");
            Write(r.MinorLeagueMembersCount.ToString());
            Write(" (");
            Write((r.MinorLeagueWinRate*100).ToString("0.00") + "%)");

            Write("|");

            Write(r.GosaRate);

            Write("|");

            //Write("誤差: ");
            Write((r.Gosa*100).ToString("0.00") + "%");

            WriteLine("|");

            //WriteLine("------");
        }
    }
}
