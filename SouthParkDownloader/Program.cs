using System;

using SouthParkDownloader.Logic;

namespace SouthParkDownloader
{
    class Program
    {
        static void Main(String[] args)
        {
            ApplicationLogic logic = ApplicationLogic.Instance;
            logic.Run();
        }
    }
}
