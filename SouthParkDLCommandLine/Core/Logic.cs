using System;
using System.IO;
using System.Reflection;
using Figgle;

namespace SouthParkDLCommandLine.Core
{
    abstract class Logic
    {
        /* Arguments */
        protected string[] m_args;

        /* Application tick */
        protected Int64 m_deltaTime = 0;
        protected Int64 m_runTime = 0;

        /* Start & Stop */
        protected Boolean m_exit = false;

        /* Application Info */
        protected String m_applicationName;
        protected String m_applicationVersion;

        public String m_workingDirectory;

        public Logic(String name)
        {
            m_applicationName = name;
            m_applicationVersion = AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();
            
            Console.WriteLine(FiggleFonts.Standard.Render(m_applicationName + " v" + m_applicationVersion));


            m_workingDirectory = Directory.GetCurrentDirectory();

            Console.WriteLine(m_applicationName + " is ready!");
        }

        public void Run(string[] args)
        {
            m_args = args;

            BeforeRun();

            while (!m_exit)
            {
                /* Get current timestamp */
                Int64 now = DateTime.Now.Ticks;

                /* Calculate delta */
                if (m_runTime > 0)
                    m_deltaTime = now - m_runTime;

                /* Add elapsed time to runtime */
                m_runTime += m_deltaTime;

                /* Run application logic */
                Tick();
            }
        }

        protected abstract void BeforeRun();

        public abstract void Tick();
    }
}
