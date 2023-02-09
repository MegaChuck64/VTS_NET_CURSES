using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace VTS_LIB;

public static class Terminal
{
    [DllImport("kernel32")]
    private static extern IntPtr GetStdHandle(StdHandle index);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);


    [DllImport("kernel32.dll")]
    static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);


    //https://www.pinvoke.net/default.aspx/kernel32.CreateFile
    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern SafeFileHandle CreateFile(
        string fileName,
        uint fileAccess,
        uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        int flags,
        IntPtr template);


    private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    [Flags]
    private enum ConsoleModes : uint
    {
        ENABLE_PROCESSED_INPUT = 0x0001,
        ENABLE_LINE_INPUT = 0x0002,
        ENABLE_ECHO_INPUT = 0x0004,
        ENABLE_WINDOW_INPUT = 0x0008,
        ENABLE_MOUSE_INPUT = 0x0010,
        ENABLE_INSERT_MODE = 0x0020,
        ENABLE_QUICK_EDIT_MODE = 0x0040,
        ENABLE_EXTENDED_FLAGS = 0x0080,
        ENABLE_AUTO_POSITION = 0x0100,

        ENABLE_PROCESSED_OUTPUT = 0x0001,
        ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
        ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
        ENABLE_LVB_GRID_WORLDWIDE = 0x0010
    }

    private enum StdHandle
    {
        OutputHandle = -11
    }





    public static void Test()
    {
        SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
       
               
        IntPtr hOut = GetStdHandle(StdHandle.OutputHandle);
        if (hOut != INVALID_HANDLE_VALUE)
        {
            if (GetConsoleMode(hOut, out uint mode))
            {
                mode |= (uint)ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(hOut, mode);
            }
        }
        
        
        //https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
        if (!h.IsInvalid)
        {
            Print(
                "This text has a red foreground and bright white background", 
                hOut,
                TerminalColor.BrightBackgroundWhite, TerminalColor.ForegroundRed);


            Print(
                "This text has a red foreground and white background",
                hOut,
                TerminalColor.BackgroundWhite, TerminalColor.ForegroundRed);
        }

    }


    public static void Print(string msg, IntPtr hOut, params TerminalColor[] color)
    {        
        var cols = string.Empty;
        foreach (var col in color)
        {
            cols += ((int)col).ToString() + ";";
        }
        cols = cols.TrimEnd(';');
        cols += 'm';
        var line = "\x1b[" + cols + msg + "\r\n";
        bool goodWrite = WriteConsole(hOut, line, (uint)line.Length, out uint cchwritten, 0);

        if (!goodWrite)
        {
            var lastError = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"Error drawing fast console output: \t{lastError}");
        }
        
    }

}

public enum TerminalColor
{
    Default = 0,
    Bright = 1,
    NoBright = 22,
    Underline = 4,
    NoUnderline = 24,
    Negative = 7,
    Positive = 27,
    ForegroundBlack = 30,
    ForegroundRed = 31,
    ForegroundGreen = 32,
    ForegroundYellow = 33,
    ForegroundBlue = 34,
    ForegroundMagenta = 35,
    ForegroundCyan = 36,
    ForegroundWhite = 37,
    ForegroundExtended = 38,
    ForegroundDefault = 39,
    BackgroundBlack = 40,
    BackgroundRed = 41,
    BackgroundGreen = 42,
    BackgroundYellow = 43,
    BackgroundBlue = 44,
    BackgroundMagenta = 45,
    BackgroundCyan = 46,
    BackgroundWhite = 47,
    BackgroundExtended = 48,
    BackgroundDefault = 49,
    BrightForegroundBlack = 90,
    BrightForegroundRed = 91,
    BrightForegroundGreen = 92,
    BrightForegroundYellow = 93,
    BrightForegroundBlue = 94,
    BrightForegroundMagenta = 95,
    BrightForegroundCyan = 96,
    BrightForegroundWhite = 97,
    BrightBackgroundBlack = 100,
    BrightBackgroundRed = 101,
    BrightBackgroundGreen = 102,
    BrightBackgroundYellow = 103,
    BrightBackgroundBlue = 104,
    BrightBackgroundMagenta = 105,
    BrightBackgroundCyan = 106,
    BrightBackgroundWhite = 107
}

