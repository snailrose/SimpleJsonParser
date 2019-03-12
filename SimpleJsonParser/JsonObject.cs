/*
-------------------------------------------------------------------------------
    This file is part of SimpleJsonParser.

    Copyright (c) Charles Carley.

    Contributor(s): none yet.
-------------------------------------------------------------------------------
  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
-------------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;

namespace Json
{
    public class JObject : IDisposable
    {
        private List<object>        m_array;
        private static int          m_depth;

        public JObject()
        {
            Dictionary  = new Dictionary<string, object>();
            m_array     = new List<object>();
            Nodes       = new List<JObject>();
            m_depth     = 0;
            IsArray     = false;
        }


        public Dictionary<string, object>.KeyCollection Keys { get => Dictionary.Keys; }


        public void AddObject( JObject obj )
        {
            if( obj != null )
                Nodes.Add( obj );
            if( IsArray )
                m_array.Add( obj );
        }

        public void SetValue( string key, object value )
        {
            if( key != null && value != null ) {
                if( Dictionary.ContainsKey( key ) )
                    Dictionary[key] = value;
            }
        }


        public void AddValue( string key, object value )
        {
            if( key != null && value != null ) {
                if( Dictionary.ContainsKey( key ) )
                    Dictionary[key] = value;
                else
                    Dictionary.Add( key, value );
            }
        }

        public void AddValue( object value )
        {
            if( value != null )
                m_array.Add( value );
        }

        public bool HasKey( string key ) 
        { 
            return ( key != null && Dictionary.ContainsKey( key ) ); 
        }


        
        public string AsString( string key, string def )
        {
            object val = null;
            if( key != null && Dictionary.ContainsKey( key ) )
                val = Dictionary[key];
            if( val == null )
                return def;
            if( val.GetType() == typeof( string ) )
                return ( string )val;
            if( val.GetType() == typeof( int ) )
                return ( ( int )val ).ToString();
            if( val.GetType() == typeof( long ) )
                return ( ( long )val ).ToString();
            if( val.GetType() == typeof( double ) )
                return ( ( double )val ).ToString();

            throw new NotImplementedException();
        }

        
        public double AsDouble( string key, double def )
        {
            object val = null;
            if( key != null && Dictionary.ContainsKey( key ) )
                val = Dictionary[key];
            if( val == null )
                return def;
            if( val.GetType() == typeof( string ) ) {
                if( Double.TryParse( ( string )val, out double dval ) )
                    return dval;
                return 0;
            }
            if( val.GetType() == typeof( int ) )
                return ( ( int )val );
            if( val.GetType() == typeof( double ) )
                return ( ( double )val );

            throw new NotImplementedException();
        }

        public long AsLong( string key, long def  )
        {
            object val = null;
            if( key != null && Dictionary.ContainsKey( key ) )
                val = Dictionary[key];
            if( val == null )
                return def;
            if( val.GetType() == typeof( string ) ) {
                if( long.TryParse( ( string )val, out long dval ) )
                    return dval;
                return def;
            }
            if( val.GetType() == typeof( long ) )
                return ( ( long )val );
            if( val.GetType() == typeof( int ) )
                return ( ( long )val );
            if( val.GetType() == typeof( double ) )
                return ( long )( ( double )val );

            throw new NotImplementedException();
        }


        public int AsInt( string key, int def  )
        {
            object val = null;
            if( key != null && Dictionary.ContainsKey( key ) )
                val = Dictionary[key];
            if( val == null )
                return def;
            if( val.GetType() == typeof( string ) ) {
                if( Int32.TryParse( ( string )val, out int dval ) )
                    return dval;
                return def;
            }
            if( val.GetType() == typeof( int ) )
                return ( ( int )val );
            if( val.GetType() == typeof( double ) )
                return ( int )( ( double )val );

            throw new NotImplementedException();
        }


        public string AsString( string key )
        {
           return AsString(key, "null");
        }

        
        public double AsDouble( string key )
        {
           return AsDouble(key, 0);
        }

        public long AsLong( string key )
        {
           return AsLong(key, 0);
        }


        public int AsInt( string key )
        {
           return AsInt(key, 0);
        }


        static public JObject FromBin( string bin )
        {
            if( bin == null || bin.Length == 0 )
                return null;


            string[] array = bin.Split( ',' );
            string res = "";
            foreach( string str in array ) {
                if( Int32.TryParse( str, out int iv ) && iv < 256 ) {
                    char c = ( char )iv;
                    res += c;
                }
            }
            return Parse( res );
        }

        public string ToBin()
        {
            string result = "";
            string val = ToString();
            if( val.Length > 0 ) {
                int i = 0;
                foreach( char c in val ) {
                    int ci = c;
                    result += ci.ToString();
                    if( i + 1 < val.Length )
                        result += ",";
                    ++i;
                }
            }
            return result;
        }

        public static JObject Parse( string val )
        {
            JParser pse = new JParser();
            return pse.Parse( val );
        }

        public static JObject ParseBin( string val )
        {
            JParser pse = new JParser();
            return FromBin( val );
        }

        private string WriteNewLine()
        {
            return "\n";
        }

        private string WriteSpace()
        {
            string str = "";
            for( int i = 0; i < m_depth; ++i )
                str += " ";
            return str;
        }

        public string AsPrettyPrint()
        {
            string result = "";
            result += WriteSpace();
            result += IsArray ? "[" : "{";
            result += WriteNewLine();
            m_depth += 1;
            int count = 0;
            if( IsArray ) {
                count = 0;
                foreach( object obj in m_array ) {
                    if( obj.GetType() == typeof( string ) ) {
                        result += WriteSpace();
                        result += "\"" + obj + "\"";
                    } else if( obj.GetType() == typeof( bool ) ) {
                        result += WriteSpace();
                        result += ( ( ( bool )obj ) ? "true" : "false" );
                    } else {
                        result += WriteSpace();
                        result += obj;
                    }
                    count++;
                    if( count < m_array.Count )
                        result += ",";
                    result += WriteNewLine();
                }
            }
            count = 0;
            foreach( string key in Dictionary.Keys ) {
                object val = Dictionary[key];

                if( val.GetType() == typeof( string ) ) {
                    result += WriteSpace();
                    result += "\"" + key + "\": \"" + val + "\"";
                } else if( val.GetType() == typeof( bool ) ) {
                    result += WriteSpace();
                    result += "\"" + key + "\": " + ( ( ( bool )val ) ? "true" : "false" );
                } else {
                    result += WriteSpace();
                    result += "\"" + key + "\": " + val;
                }
                count++;
                if( count < Dictionary.Count )
                    result += ",";
                result += WriteNewLine();
            }
            m_depth -= 1;
            result += WriteSpace();
            result += IsArray ? "]" : "}";
            result += WriteNewLine();
            return result;
        }

        public string AsCompactPrint()
        {
            string result = "";
            result += IsArray ? "[" : "{";
            m_depth += 1;
            int count = 0;
            if( IsArray ) {
                count = 0;
                foreach( object obj in m_array ) {
                    if( obj.GetType() == typeof( string ) )
                        result += "\"" + obj + "\"";
                    else if( obj.GetType() == typeof( bool ) )
                        result += ( ( ( bool )obj ) ? "true" : "false" );
                    else
                        result += obj;
                    count++;
                    if( count < m_array.Count )
                        result += ",";
                }
            }
            count = 0;
            foreach( string key in Dictionary.Keys ) {
                object val = Dictionary[key];
                if( val.GetType() == typeof( string ) )
                    result += "\"" + key + "\":\"" + val + "\"";
                else if( val.GetType() == typeof( bool ) )
                    result += "\"" + key + "\":" + ( ( ( bool )val ) ? "true" : "false" );
                else
                    result += "\"" + key + "\":" + val;
                count++;
                if( count < Dictionary.Count )
                    result += ",";
            }
            result += IsArray ? "]" : "}";
            return result;
        }

        public void Dispose() 
        {
            Dictionary.Clear();
            m_array.Clear();
        }

        public override string ToString() 
        { 
            m_depth = 0;  
            return AsCompactPrint(); 
        }

        public bool IsArray                             { get; set; }
        public Dictionary<string, object> Dictionary    { get; private set; }
        public List<JObject> Nodes                      { get; private set; }
    }
}
