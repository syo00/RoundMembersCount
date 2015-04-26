using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoundMembersCount
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainLogic.OnStarted();

            MainLogic.WriteResults2(2, 8, 12, 30, Getリーグとその1つ下のリーグの差の下限Predicate(2));

            MainLogic.WriteLine("");

            MainLogic.WriteResults2(2, 8, 12, 30, Getリーグとその1つ下のリーグの差の下限Predicate(2), false);

            MainLogic.WriteLine("");

            MainLogic.WriteResults2(3, 16, 20, 40, Getリーグとその1つ下のリーグの差の下限Predicate_3リーグ以上(1.2), false);

            MainLogic.OnFinished();

            Console.ReadLine();
        }



        private static Func<LeaguesInfo, HasPreviousValue, bool> Getリーグとその1つ下のリーグの差の下限Predicate(int リーグとその1つ下のリーグの差の下限)
        {
            return
                (leagues, hasPreviousValue) =>
                    leagues.Leagues.Select(l => l.MembersCount)
                        .PairWise()
                        .Skip(1)
                        .All(pair => pair.Item1 - pair.Item2 >= リーグとその1つ下のリーグの差の下限)
                    && leagues.Gosa < 0.2;
        }

        private static Func<LeaguesInfo, HasPreviousValue, bool> Getリーグとその1つ下のリーグの差の下限Predicate_3リーグ以上(double 比の下限)
        {
            return
                (leagues, hasPreviousValue) =>
                {
                    var 比はOK = leagues.Leagues.Select(l => l.MembersCount)
                        .PairWise()
                        .Skip(1)
                        .All(pair => ((double)pair.Item1) / pair.Item2 >= 比の下限);

                    if (hasPreviousValue == HasPreviousValue.False)
                    {
                        return 比はOK && leagues.Gosa < 0.2;
                    }

                    return 比はOK;
                };
        }
    }
}
