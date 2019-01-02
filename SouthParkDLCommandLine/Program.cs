using SouthParkDLCommandLine.Logic;

namespace SouthParkDLCommandLine
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
