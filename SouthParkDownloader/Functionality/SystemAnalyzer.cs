using System;

using SouthParkDownloader.Types;

namespace SouthParkDownloader.Functionality
{
  class SystemAnalyzer
  {

    private SystemInfo m_info;

    public SystemAnalyzer()
    {
      m_info = new SystemInfo();

      m_info.OS = Environment.OSVersion;
      m_info.CPUArchitecture = Environment.Is64BitOperatingSystem ? SystemInfo.Architecture.x86_64 : SystemInfo.Architecture.x86;
    }

    public SystemInfo GetInfo()
    {
      return m_info;
    }
  }
}
