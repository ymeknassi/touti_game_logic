using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
namespace touti_game_logic
{
    public static class ExecuteOtherGameInstances
    {
        // Importing required functions from user32.dll and kernel32.dll
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        const int SW_SHOWNORMAL = 1;
        const int SW_HIDE = 0;

        // Constants for SetWindowPos uFlags
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;

        public static void StartInstances(int instanceCount = 3)
        {
            int windowWidth = 800;  // Width of the window
            int windowHeight = 400; // Height of the window

            // Move the current instance window
            MoveCurrentInstance(10, 10, windowWidth, windowHeight);

            string executablePath = "C:\\Users\\ymekn\\OneDrive\\Documents\\Perso\\touti_console\\touti_game_logic\\touti_game_logic\\bin\\Debug\\touti_game_logic.exe"; // Path to your executable
            int[] windowXPositions = { 850, 10, 850 }; // X coordinates for the instances
            int[] windowYPositions = { 10, 450, 450 }; // Y coordinates for the instances

            Process[] processes = new Process[instanceCount];

            // Start the specified number of instances of the executable
            for (int i = 0; i < instanceCount; i++)
            {
                processes[i] = Process.Start(executablePath);
                Thread.Sleep(200); // Adjust as necessary
                IntPtr hWnd = FindWindow(null, processes[i].MainWindowTitle);
                if (hWnd != IntPtr.Zero)
                {
                    // Show the window (if hidden)
                    ShowWindow(hWnd, SW_SHOWNORMAL);

                    // Move the window to the specified position and resize it
                    MoveWindow(hWnd, windowXPositions[i], windowYPositions[i], windowWidth, windowHeight, true);
                }
            }

            Console.WriteLine("Windows are resized and moved!");
        }

        public static void MoveCurrentInstance(int x, int y, int width, int height)
        {
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                // Show the window (if hidden)
                ShowWindow(hWnd, SW_SHOWNORMAL);

                // Move the window to the specified position and resize it
                MoveWindow(hWnd, x, y, width, height, true);
            }
            else
            {
                Console.WriteLine("Failed to get the handle of the current process window.");
            }
        }
    }
}
