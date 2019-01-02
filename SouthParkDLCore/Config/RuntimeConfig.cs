using SouthParkDLCore.Abstract;
using System;
using System.IO;

namespace SouthParkDLCore.Config
{
    public class RuntimeConfig : Singleton<RuntimeConfig>
    {
        // Folders
        public String m_workingDirectory;
        public String m_dependencyDirectory
        {
            get
            {
                return Directory.CreateDirectory(m_workingDirectory + "/dep").FullName;
            }
        }
        public String m_dataDirectory
        {
            get
            {
                return Directory.CreateDirectory(m_workingDirectory + "/data").FullName;
            }
        }
        public String m_tempDiretory
        {
            get
            {
                return Directory.CreateDirectory(m_workingDirectory + "/tmp").FullName;
            }
        }

        // Database
        public String m_indexFile
        {
            get
            {
                return m_dataDirectory + "/data.db";
            }
        }


        // Executables
        public String m_youtubeDL
        {
            get
            {
                return m_dependencyDirectory + "/youtube-dl.exe";
            }
        }

        public String m_ffmpeg
        {
            get
            {
                return m_dependencyDirectory + "/ffmpeg.exe";
            }
        }

        public RuntimeConfig()
        {
            m_workingDirectory = Directory.GetCurrentDirectory();
        }
    }
}
