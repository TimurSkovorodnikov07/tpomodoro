public class Program
{
    private static object locker = new();
    public static void Main(string[] args)
    {
        const ConsoleColor defColor = ConsoleColor.DarkBlue;
        const ConsoleColor color = ConsoleColor.Magenta;

        const double workMinute = 25;
        const double restMinute = 5;

        const double bigRestMinute = 15;
        const int intervalToBigPomo = 4;

        int curPomo = 0;
        double curSecond = workMinute * 60;
        bool isTimeToWork = true;

        new Task(() =>
        {
            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;

                if (key == ConsoleKey.R)
                {
                    Console.Clear();
                    Write(isTimeToWork ? "RESTART!" : "Rest SKIP", color, defColor);

                    if (!isTimeToWork)
                        curPomo++;

                    isTimeToWork = true;
                    curSecond = workMinute * 60;
                }

            }
        }).Start();

        while (true)
        {
            Thread.Sleep(1000);

            Console.Clear();
            Write("To RESTART the pomo or rest SKIP, press 'R'\n\n", color, defColor);

            if (curPomo != 0)
            {
                Write("Completed pomodoro: ", color, ConsoleColor.Gray);
                Write($"{curPomo}\n", color, ConsoleColor.Green);
            }

            Write(isTimeToWork ? "Work: " : "Rest: ", color, defColor);
            Write($"{BeautifulTime(curSecond)}\n", color, ConsoleColor.Cyan);

            curSecond--;


            if (curSecond <= 0)
            {
                isTimeToWork = !isTimeToWork;

                if (isTimeToWork)
                {
                    curSecond = workMinute * 60;
                }
                else
                {
                    curPomo++;
                    curSecond = ((curPomo != 0 && curPomo % intervalToBigPomo == 0) ? bigRestMinute : restMinute) * 60;
                }
            }
        }

    }

    private static string BeautifulTime(double second)
    {
        var minute = Math.Truncate(second / 60);//Truncate уберает дробную часть
        var sec = second - (minute * 60);
        return $"{minute}:{sec}";
    }
    private static void Write(string str, ConsoleColor color, ConsoleColor defColor)
    {
        lock (locker)
        {
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ForegroundColor = defColor;
        }
    }
}