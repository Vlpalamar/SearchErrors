using System;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

namespace SearchErrors
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskDirScanStart taskDirScan= new TaskDirScanStart(@"D:\Новая папка\cs-coding-tasks\cs\bugs");
            taskDirScan.Run();

           

        }
    }
}
