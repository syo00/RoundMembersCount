using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundMembersCount
{
    internal static class MainLogic
    {
        private static string path = @"D:\rounds.txt";

        public static void OnStarted()
        {
            File.Delete(path);
        }

        public static void Write(string str)
        {
            Console.Write(str);

            File.AppendAllText(path, str);
        }

        public static void WriteLine(string str)
        {
            Console.WriteLine(str);

            File.AppendAllLines(path, new[] {str});
        }

        public static void OnFinished()
        {

        }

        //  一人増えるごとにどれか一つのリーグを一人だけ増やす = true の場合、初期値がおかしいとその後の値も軒並みおかしくなる
        // membersRangeStart、winnersCount の値を調整したり、 manualPredicate を厳しくするなりしよう
        public static void WriteResults2(int leaguesCount, int winnersCount, int firstMembersCount,
            int lastMembersCount, Func<LeaguesInfo, HasPreviousValue, bool> manualPredicate,
            bool 一人増えるごとにどれか一つのリーグを一人だけ増やす = true)
        {

            LeaguesInfo prev = null;
            HasPreviousValue hasPreviousValue = HasPreviousValue.False;

            foreach (var membersCount in Enumerable.Range(firstMembersCount, lastMembersCount - firstMembersCount + 1))
            {
                {
                    Write("|");
                    Write(membersCount.ToString());

                    Write("|");
                    Write(winnersCount.ToString());

                    Write("|");
                    Write(leaguesCount.ToString());

                    var writing =
                        CreateAllLeagueInfoWithGosaCombinations(leaguesCount, membersCount, winnersCount)
                            .Where(leaguesInfo => manualPredicate(leaguesInfo, hasPreviousValue))
                            .Where(leaguesInfo =>
                            {
                                if (!一人増えるごとにどれか一つのリーグを一人だけ増やす)
                                {
                                    return true;
                                }

                                if (prev == null)
                                {
                                    return true;
                                }

                                var result =
                                    prev.Leagues
                                        .Zip(leaguesInfo.Leagues,
                                            (l1, l2) => Math.Abs(l1.MembersCount - l2.MembersCount))
                                        .Sum()
                                    == 1;
                                return result;
                            })
                            .OrderBy(a => Math.Abs(a.Gosa))
                            .FirstOrDefault();

                    if (writing != null)
                    {
                        prev = writing;
                        hasPreviousValue = HasPreviousValue.True;
                        Write(writing);
                    }
                    else
                    {
                        prev = null;
                        hasPreviousValue = HasPreviousValue.False;
                        WriteMissing();
                    }
                }
            }

        }

        private static IEnumerable<LeaguesInfo> CreateAllLeagueInfoWithGosaCombinations(int leaguesCount,
            int membersCount, int winnersCount)
        {
            return
                CreateAllLeagueInfoCombinations(leaguesCount, membersCount, winnersCount)
                    .Select(infos => new LeaguesInfo {Leagues = infos, Gosa = 誤差算出(infos)})
                    .Where(info => !double.IsNaN(info.Gosa));
        }

        private static IEnumerable<IReadOnlyList<LeagueInfo>> CreateAllLeagueInfoCombinations(int leaguesCount,
            int membersCount, int winnersCount)
        {
            var leagueMembersCountCandidates = GetCombinationsWhereSumsAreSame(leaguesCount, membersCount);
            var leagueWinnersCountCandidates = GetCombinationsWhereSumsAreSame(leaguesCount, winnersCount).ToArray();

            var result = new List<LeagueInfo[][]>();
            foreach (var leagueMembers in leagueMembersCountCandidates)
            {
                var resultElement = leagueWinnersCountCandidates
                    .Select(
                        w =>
                            leagueMembers.Zip(w,
                                (total, winner) => new LeagueInfo {MembersCount = total, WinnersCount = winner})
                                .ToArray())
                    .ToArray();
                result.Add(resultElement);
            }

            return result
                .SelectMany(x => x)
                .Where(infos => infos.All(info => info.MembersCount >= 1 && info.WinnersCount != 0 && info.WinRate < 1));
        }


        private static void Write(LeaguesInfo leaguesInfo)
        {
            foreach (var league in leaguesInfo.Leagues)
            {
                Write("|");

                Write(league.WinnersCount.ToString());
                Write("/");
                Write(league.MembersCount.ToString());
                Write(" (");
                Write((league.WinRate*100).ToString("0.00") + "%)");
            }

            Write("|");

            foreach (var league in leaguesInfo.Leagues.Take(leaguesInfo.Leagues.Count - 1))
            {
                Write((league.WinRate/leaguesInfo.Leagues.Last().WinRate).ToString("0.00"));
                Write(":");
            }

            Write("1");

            Write("|");

            Write(leaguesInfo.Gosa.ToString("0.0000"));

            WriteLine("|");
        }

        private static void WriteMissing()
        {
            Write("|");

            Write("有効な組み合わせが見つかりませんでした。");

            WriteLine("|");
        }


        // 要素が elementsCount 個あるintのコレクション（要素は全て0以上）の中で、要素の和が sum になるコレクションをすべて返す。
        // 例えば elementsCount = 3, sum = 2 とすると
        // [2,0,0], [0,2,0], [0,0,2], [1,1,0], [1,0,1], [0,1,1] を返す。ただし実際は順不同
        private static IEnumerable<IReadOnlyList<int>> GetCombinationsWhereSumsAreSame(int elementsCount, int sum)
        {
            IEnumerable<IEnumerable<int>> source = new List<IEnumerable<int>> {new[] {-1}}; //-1はダミー

            foreach (var _ in Enumerable.Range(0, elementsCount))
            {
                source = ConcatForEachEnumerable(source, Enumerable.Range(0, sum + 1).ToArray());
            }

            return source
                .Select(elem =>
                {
                    var result = elem.ToList();
                    result.RemoveAt(0);
                    return result;
                })
                .Where(elem => elem.Sum() == sum);
        }

        // 例えば
        // source = [ [1, 2, 3], [4, 5]] 
        // concat = [10, 11]
        // とすると、
        // [ [1, 2, 3, 10], [1, 2, 3, 11], [4, 5, 10], [4, 5, 11] ]
        // のように返す。ただし実際は順不同
        private static IEnumerable<IEnumerable<int>> ConcatForEachEnumerable(IEnumerable<IEnumerable<int>> source,
            IReadOnlyCollection<int> concat)
        {
            foreach (var elem in source)
            {
                foreach (var concatElem in concat)
                {
                    yield return elem.Concat(new[] {concatElem});
                }
            }
        }


        private static double 誤差算出(IReadOnlyList<LeagueInfo> info)
        {
            int 二倍補正 = 1;
            var list = new List<double>();
            var firstLeagueWinRate = info[0].WinRate;
            foreach (var i in info)
            {
                list.Add(1 - (i.WinRate*二倍補正/firstLeagueWinRate));
                二倍補正 *= 2;
            }
            return GetRms(list);
        }

        private static double GetRms(IEnumerable<double> gosa)
        {
            var notRooted =
                gosa
                    .Select(i => i*i)
                    .Average();

            return Math.Pow(notRooted, 0.5);
        }
    }

    // 面倒なのでsetterもpublic。書き換えてはいけない（戒め）
    internal class LeagueInfo
    {
        public int MembersCount { get; set; }

        public double WinRate
        {
            get { return ((double)WinnersCount) / MembersCount; }
        }

        public int WinnersCount { get; set; }
    }

    // 面倒なのでsetterもpublic。書き換えてはいけない（戒め）
    internal class LeaguesInfo
    {
        public IReadOnlyList<LeagueInfo> Leagues { get; set; }
        public double Gosa { get; set; }
    }

    internal enum HasPreviousValue
    {
        False,
        True,
    }

    internal static class A
    {
        public static IEnumerable<Tuple<T, T>> PairWise<T>(this IEnumerable<T> source)
        {
            T prev = default(T);
            foreach (var elem in source)
            {
                yield return Tuple.Create(prev, elem);
                prev = elem;
            }
        }
    }

}
