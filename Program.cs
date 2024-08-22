using System.Diagnostics;
using System.Reflection;
public class Program
{
    private static object locker = new();
    private const ConsoleColor defColor = ConsoleColor.Blue;

    public static void Main(string[] args)
    {
        var needDrawing = false;

        if (args.Length > 0 && args[0] == "drawEnable")
        {
            needDrawing = true;
            Write("The input will NOT work (icat cannot work with it)", ConsoleColor.Red);
        }

        const double workMinute = 25;
        const double restMinute = 5;

        const double bigRestMinute = 15;
        const int intervalToBigPomo = 4;

        int curPomo = 0;
        double curSecond = workMinute * 60;
        bool isTimeToWork = true;

        if (!needDrawing)
        {
            //https://www.reddit.com/r/KittyTerminal/comments/1eyl9bn/kitty_cat_doesnt_support_multithreading/
            //Судя по докам icat не будет работать когда есть ввод((((
            Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(intercept: true).Key;

                    if (key == ConsoleKey.R)
                    {
                        Console.Clear();
                        Write(isTimeToWork ? "RESTART!" : "Rest SKIP", ConsoleColor.Red);

                        if (!isTimeToWork)
                            curPomo++;

                        isTimeToWork = true;
                        curSecond = workMinute * 60;
                    }

                }
            });
        }

        while (true)
        {
            Thread.Sleep(1000);
            Console.Clear();

            if (needDrawing)
                Draw();

            Write("To RESTART the pomo or rest SKIP, press 'R'\n\n");

            if (curPomo != 0)
            {
                Write("Completed pomodoro: ");
                Write($"{curPomo}\n", ConsoleColor.Yellow);
            }

            Write(isTimeToWork ? "Work: " : "Rest: ", ConsoleColor.DarkMagenta);
            Write($"{BeautifulTime(curSecond)}\n", ConsoleColor.Cyan);

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
    private static void Write(string str, ConsoleColor color = 0)
    {
        lock (locker)
        {
            Console.ForegroundColor = color is not 0 ? color : defColor;
            Console.Write(str);
        }
    }
    private static void Draw()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/sbin/sh",
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = "/home/timur/Desktop/tpomodoro/pomodoroImage",
        };

        using var process = Process.Start(psi);
        process?.WaitForExit();
    }
}