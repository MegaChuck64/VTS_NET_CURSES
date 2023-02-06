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





    /// <summary>
    /// GRAPHICS TEST (prints grid of A - Z in every console color on keypress)
    /// </summary>
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
            PrintF(hOut, "\x1b[31mThis text has a red foreground using SGR.31.\r\n");
            PrintF(hOut, "\x1b[1mThis text has a bright (bold) red foreground using SGR.1 to affect the previous color setting.\r\n");
            PrintF(hOut, "\x1b[mThis text has returned to default colors using SGR.0 implicitly.\r\n");
            PrintF(hOut, "\x1b[34;46mThis text shows the foreground and background change at the same time.\r\n");
            PrintF(hOut, "\x1b[0mThis text has returned to default colors using SGR.0 explicitly.\r\n");
            PrintF(hOut, "\x1b[31;32;33;34;35;36;101;102;103;104;105;106;107mThis text attempts to apply many colors in the same command. Note the colors are applied from left to right so only the right-most option of foreground cyan (SGR.36) and background bright white (SGR.107) is effective.\r\n");
            PrintF(hOut, "\x1b[39mThis text has restored the foreground color only.\r\n");
            PrintF(hOut, "\x1b[49mThis text has restored the background color only.\r\n");
        }

    }


    public static void PrintF(IntPtr hOut, string msg)
    {
        bool goodWrite = WriteConsole(hOut, msg, (uint)msg.Length, out uint cchwritten, 0);

        if (!goodWrite)
        {
            var lastError = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"Error drawing fast console output: \t{lastError}");
        }

    }
}