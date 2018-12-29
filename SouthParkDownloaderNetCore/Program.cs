using System;

using SouthParkDownloaderNetCore.Logic;

namespace SouthParkDownloaderNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationLogic logic = ApplicationLogic.Instance;
            logic.Run();
        }
    }
}
