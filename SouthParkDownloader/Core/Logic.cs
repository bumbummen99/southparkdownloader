using System;
using System.IO;
using System.Collections;

namespace SouthParkDownloader.Core
{
  class Logic
  {
    /* Application tick */
    protected Int64 m_deltaTime = 0;
    protected Int64 m_runTime = 0;

    /* Start & Stop */
    protected Boolean m_exit = false;

    /* Application Info */
    protected String m_applicationName;
    protected String m_applicationVersion;

    protected String m_workingDirectory;

    public Logic( String name, String version )
    {
      m_applicationName = name;
      m_applicationVersion = version;

      Console.WriteLine( "Starting " + m_applicationName + " Version " + version );

      m_workingDirectory = Directory.GetCurrentDirectory();

      Console.WriteLine( m_applicationName + " is ready!" );
    }

    protected void Run()
    {
      while ( !m_exit )
      {
        /* Get current timestamp */
        Int64 now = DateTime.Now.Ticks;

        /* Calculate delta */
        if ( m_runTime > 0 )
          m_deltaTime = now - m_runTime;

        /* Add elapsed time to runtime */
        m_runTime += m_deltaTime;

        /* Run application logic */
        Tick();
      }
    }

    public virtual void Tick()
    { }
  }
}
