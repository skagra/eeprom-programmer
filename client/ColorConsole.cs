namespace EEPROMProgrammer;

public static class ColorConsole
{
    public static void ConsoleWrite(string message, ConsoleColor color = ConsoleColor.White)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = oldColor;
    }

    public static void ConsoleWriteln(string message, ConsoleColor color = ConsoleColor.White)
    {
        ConsoleWrite(message, color);
        Console.WriteLine();
    }

    public static int HorizontalCentreCursor(int messageWidth)
    {
        return (Console.WindowWidth - messageWidth) / 2;
    }

    public static int HorizontalCentreCursor(string message)
    {
        return HorizontalCentreCursor(message.Length);
    }

    public static int VerticalCentreCursor(int messageHeight)
    {
        return (Console.WindowHeight - messageHeight) / 2;
    }

    public static void ConsoleCentreMessage(string message, ConsoleColor color = ConsoleColor.White)
    {
        Console.CursorLeft = HorizontalCentreCursor(message);
        Console.CursorTop = VerticalCentreCursor(0);
        ConsoleWrite(message, color);
    }

    public static void ConsoleClear()
    {
        Console.Clear();
    }
}