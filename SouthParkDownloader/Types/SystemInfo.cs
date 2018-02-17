using System;

namespace SouthParkDownloader.Types
{
  class SystemInfo
  {
    public OperatingSystem OS;
    public Architecture CPUArchitecture;

    public enum Architecture
    {
      x86,
      x86_64
    }
  }
}
