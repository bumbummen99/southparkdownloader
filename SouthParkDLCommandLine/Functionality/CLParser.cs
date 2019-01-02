using System;
using System.Collections;
using System.Linq;

namespace SouthParkDLCommandLine.Functionality
{
  class CLParser
  {

    /* Internals */
    private String m_input;
    private String[] m_parts = new String[1];
    private Hashtable m_arguments = new Hashtable();

    /* Access */
    public String Command
    {
      get
      {
        if ( this.m_parts[0] != null )
          return this.m_parts[0];
        return null;
      }
    }

    public Boolean HasArgument( String key )
    {
      return this.m_arguments.ContainsKey( key );
    }

    public String ArgumentValue( String key )
    {
      if ( !HasArgument( key ) )
        return null;

      return this.m_arguments[key].ToString();
    }

    public CLParser( String input )
    {
      this.m_input = input;
      this.m_parts = this.m_input.Split( null );

      /* Process input if there are parts seperated by whitespaces */
      if ( this.m_parts.Length > 0 )
      {
        for ( Int32 i = 1; i < this.m_parts.Length; i++ )
        {
          String[] keyval = this.m_parts[i].Split( '=', '"' );

          if ( keyval.Length > 1 && keyval[1].Contains( '"' ) )
            keyval[1] = keyval[1].Replace( "\"", "" );

          this.m_arguments.Add( keyval[0], keyval.Length > 1 ? keyval[1] : null );
        }
      }

    }

  }
}
