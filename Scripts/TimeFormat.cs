namespace GlobalGameJam.Scripts;

public class TimeFormat
{
    public static string Format(double time)
    {
        long minutes = (long)time / 60;
        long seconds = (long)time % 60;

        if (minutes > 99) minutes = 99;

        return $"{minutes:D2}:{seconds:D2}";
    }
}