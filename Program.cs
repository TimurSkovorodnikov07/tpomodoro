using System.Diagnostics;

namespace tpomodoro;

public static class Program
{
    private static readonly object Locker = new();
    private const ConsoleColor DefaultColor = ConsoleColor.Gray;

    public static void Main(string[] args)
    {
        bool needDrawing = args.Length > 0 && args[0] == "drawEnable";

        const int workMinutes = 25;
        const int restMinutes = 5;
        const int bigRestMinutes = 15;

        const int intervalUntilBigPomo = 4;

        int curPomo = 0;
        int curSeconds = workMinutes * 60;
        bool isItTimeToWork = true;
        bool needToStop = false;

        if (!needDrawing)
        {
            //https://www.reddit.com/r/KittyTerminal/comments/1eyl9bn/kitty_cat_doesnt_support_multithreading/
            //Судя по докам icat не будет работать когда есть ввод((((
            Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(intercept: true).Key;

                    if (key == ConsoleKey.Enter || key == ConsoleKey.Spacebar)
                    {
                        needToStop = !needToStop;
                        Thread.Sleep(1000);
                    }
                    else if (key == ConsoleKey.R && isItTimeToWork)
                    {
                        Console.Clear();
                        Write("RESTART!", ConsoleColor.Red);

                        StartWorkTime(out curSeconds, workSeconds: workMinutes * 60, curPomo, isItRestart: true);
                        isItTimeToWork = true;
                    }
                    else if (key == ConsoleKey.S)
                    {
                        Console.Clear();
                        Write(isItTimeToWork ? "Pomo SKIP" : "Rest SKIP", ConsoleColor.DarkCyan);

                        if (isItTimeToWork)
                        {
                            StartRestTime(out curSeconds,
                                restSeconds: GetRestMinutes(curPomo, intervalUntilBigPomo, restMinutes, bigRestMinutes),
                                curPomo);
                        }
                        else
                        {
                            StartWorkTime(out curSeconds, workSeconds: workMinutes * 60, curPomo: curPomo);
                            curPomo++;
                        }

                        isItTimeToWork = !isItTimeToWork;
                    }
                }
            });
        }

        while (true)
        {
            Thread.Sleep(1000);
            Console.Clear();

            if (needToStop)
            {
                ShowCurrentPomo(curPomo);
                //ShowCurrentTime();
                Write(isItTimeToWork ? "\ue003 Work: " : "\uee91 Rest: ");
                Write($"STOPPED", ConsoleColor.DarkRed);

                continue;
            }

            if (needDrawing)
                Draw(isItTimeToWork);

            ShowCurrentPomo(curPomo);
            ShowCurrentTime(curSeconds, isItTimeToWork);

            curSeconds--;

            if (curSeconds <= 0)
            {
                isItTimeToWork = !isItTimeToWork;
                if (isItTimeToWork)
                {
                    StartWorkTime(out curSeconds, workMinutes * 60, curPomo);
                }
                else
                {
                    StartRestTime(out curSeconds,
                        GetRestMinutes(curPomo, intervalUntilBigPomo, restMinutes, bigRestMinutes), curPomo);

                    curPomo++;
                }
            }
        }
    }

    private static int GetRestMinutes(int curPomo, int intervalUntilBigPomo, int restMinutes, int bigRestMinutes)
    {
        return ((curPomo != 0 && curPomo % intervalUntilBigPomo == 0)
            ? bigRestMinutes
            : restMinutes) * 60;
    }

    private static void ShowCurrentTime(int curSeconds, bool isItTimeToWork)
    {
        Write(isItTimeToWork ? "\ue003 Work: " : "\uee91 Rest: ");
        Write($"{GetBeautifulTime(curSeconds)}", ConsoleColor.Blue);
    }

    private static void ShowCurrentPomo(int curPomo)
    {
        if (curPomo != 0)
        {
            Write("Comp. pomo: ");
            Write($"{curPomo}\n", ConsoleColor.Yellow);
        }
    }

    private static string GetBeautifulTime(double seconds)
    {
        var minutes = Math.Truncate(seconds / 60); //Truncate уберает дробную часть
        var remainSeconds = seconds - (minutes * 60);

        var visibleMinutes = minutes <= 9 ? $"0{minutes}" : $"{minutes}";
        var visibleSeconds = remainSeconds <= 9 ? $"0{remainSeconds}" : $"{remainSeconds}";
        return $"{visibleMinutes}:{visibleSeconds}";
    }

    private static void Write(string str, ConsoleColor color = 0)
    {
        lock (Locker)
        {
            Console.ForegroundColor = color is not 0 ? color : DefaultColor;
            Console.Write(str);
        }
    }

    private static void Draw(bool isWorkTime)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/sbin/sh",
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = "/home/timur/Desktop/tpomodoro/pomodoroImage" + (isWorkTime ? "" : " r"),
        };
        ProcessStartAndWait(psi);
    }

    private static void SendMessage(string text)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/sbin/sh",
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"-c \"DISPLAY=:0 notify-send '{text}'\"",
        };
        ProcessStartAndWait(psi);
    }

    private static void ProcessStartAndWait(ProcessStartInfo psi)
    {
        using var process = Process.Start(psi);
        process?.WaitForExit();
    }

    private static void StartWorkTime(out int curSecond, int workSeconds, int curPomo, bool isItRestart = false)
    {
        curSecond = workSeconds;

        if (isItRestart)
            SendMessage($"\ue003 Restart! Current pomo: {curPomo}");
        else 
            SendMessage($"\ue003 The rest is over! Current pomo: {curPomo}");
    }

    private static void StartRestTime(out int curSecond, int restSeconds, int finishedPomo)
    {
        curSecond = restSeconds;
        SendMessage($"\ue003 Finished the {finishedPomo}th pomodoro, time of rest!");
    }
}