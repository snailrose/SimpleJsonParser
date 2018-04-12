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
        Dictionary<string, object> m_dictionary;
        List<object>        m_array;
        List<JObject>       m_nodes;
        bool                m_isArray;

        private static int m_depth;

        public JObject()
        {
            m_dictionary = new Dictionary<string, object>();
            m_array = new List<object>();
            m_nodes = new List<JObject>();
            m_depth = 0;
            m_isArray = false;
        }

        public void AddObject(JObject obj)
        {
            if (obj != null)
                m_nodes.Add(obj);
            if (m_isArray)
                m_array.Add(obj);
        }

        public void AddValue(string key, object value)
        {
            if (key != null && value != null) {
                if (m_dictionary.ContainsKey(key))
                    m_dictionary[key] = value;
                else
                    m_dictionary.Add(key, value);
            }
        }

        public void AddValue(object value)
        {
            if (value != null)
                m_array.Add(value);
        }

        public bool HasKey(string key) { return (key != null && m_dictionary.ContainsKey(key)); }

        public string AsString(string key)
        {
            object val = null;
            if (key != null && m_dictionary.ContainsKey(key))
                val = m_dictionary[key];
            if (val == null)
                return "null";
            if (val.GetType() == typeof(string))
                return (string)val;
            if (val.GetType() == typeof(int))
                return ((int)val).ToString();
            if (val.GetType() == typeof(double))
                return ((double)val).ToString();

            throw new NotImplementedException();
        }


        public double AsDouble(string key)
        {
            object val = null;
            if (key != null && m_dictionary.ContainsKey(key))
                val = m_dictionary[key];
            if (val == null)
                return 0;
            if (val.GetType() == typeof(string))
            {
                if (Double.TryParse((string)val, out double dval))
                    return dval;
                return 0;
            }
            if (val.GetType() == typeof(int))
                return ((int)val);
            if (val.GetType() == typeof(double))
                return ((double)val);

            throw new NotImplementedException();
        }


        public int AsInt(string key)
        {
            object val = null;
            if (key != null && m_dictionary.ContainsKey(key))
                val = m_dictionary[key];
            if (val == null)
                return 0;
            if (val.GetType() == typeof(string))
            {
                if (Int32.TryParse((string)val, out int dval))
                    return dval;
                return 0;
            }
            if (val.GetType() == typeof(int))
                return ((int)val);
            if (val.GetType() == typeof(double))
                return (int)((double)val);

            throw new NotImplementedException();
        }


        static public JObject FromBin(string bin)
        {
            if (bin == null || bin.Length == 0)
                return null;

            string[] array = bin.Split(',');
            string res = "";
            foreach (string str in array)
            {
                if (Int32.TryParse(str, out int iv) && iv < 256)
                {
                    char c = (char)iv;
                    res += c;
                }
            }
            return JObject.Parse(res);
        }

        public string ToBin()
        {
            string result = "";
            string val = ToString();
            if (val.Length > 0)
            {
                int i = 0;
                foreach (char c in val)
                {
                    int ci = c;
                    result += ci.ToString();
                    if (i + 1 < val.Length)
                        result += ",";
                    ++i;
                }
            }
            return result;
        }


        public static JObject Parse(string val)
        {
            JParser pse = new JParser();
            return pse.Parse(val);
        }

        public static JObject ParseBin(string val)
        {
            JParser pse = new JParser();
            return FromBin(val);
        }

        private string WriteNewLine()
        {
            return "\n";
        }

        private string WriteSpace()
        {
            string str = "";
            for (int i = 0; i < m_depth; ++i)
                str += " ";
            return str;
        }

        public string AsPrettyPrint()
        {
            string result = "";
            result += WriteSpace();
            result += m_isArray ? "[" : "{";
            result += WriteNewLine();
            m_depth += 1;
            int count = 0;
            if (IsArray) {
                count = 0;
                foreach (object obj in m_array) {
                    if (obj.GetType() == typeof(string)) {
                        result += WriteSpace();
                        result += "\"" + obj + "\"";
                    } else if (obj.GetType() == typeof(bool)) {
                        result += WriteSpace();
                        result += (((bool)obj) ? "true" : "false");
                    } else {
                        result += WriteSpace();
                        result += obj;
                    }
                    count++;
                    if (count < m_array.Count)
                        result += ",";
                    result += WriteNewLine();
                }
            }
            count = 0;
            foreach (string key in m_dictionary.Keys) {
                object val = m_dictionary[key];

                if (val.GetType() == typeof(string)) {
                    result += WriteSpace();
                    result += "\"" + key + "\": \"" + val + "\"";
                } else if (val.GetType() == typeof(bool)) {
                    result += WriteSpace();
                    result += "\"" + key + "\": " + (((bool)val) ? "true" : "false");
                } else {
                    result += WriteSpace();
                    result += "\"" + key + "\": " + val;
                }
                count++;
                if (count < m_dictionary.Count)
                    result += ",";
                result += WriteNewLine();
            }
            m_depth -= 1;
            result += WriteSpace();
            result += m_isArray ? "]" : "}";
            result += WriteNewLine();
            return result;
        }

        public string AsCompactPrint()
        {
            string result = "";
            result += m_isArray ? "[" : "{";
            m_depth += 1;
            int count = 0;
            if (IsArray) {
                count = 0;
                foreach (object obj in m_array) {
                    if (obj.GetType() == typeof(string))
                        result += "\"" + obj + "\"";
                    else if (obj.GetType() == typeof(bool))
                        result += (((bool)obj) ? "true" : "false");
                    else
                        result += obj;
                    count++;
                    if (count < m_array.Count)
                        result += ",";
                }
            }
            count = 0;
            foreach (string key in m_dictionary.Keys) {
                object val = m_dictionary[key];
                if (val.GetType() == typeof(string))
                    result += "\"" + key + "\":\"" + val + "\"";
                else if (val.GetType() == typeof(bool))
                    result += "\"" + key + "\":" + (((bool)val) ? "true" : "false");
                else
                    result += "\"" + key + "\":" + val;
                count++;
                if (count < m_dictionary.Count)
                    result += ",";
            }
            result += m_isArray ? "]" : "}";
            return result;
        }
        public void Dispose() { }

        public override string ToString() { m_depth = 0;  return AsCompactPrint(); }
        public bool IsArray               { get => m_isArray; set => m_isArray = value; }

        public Dictionary<string, object> Dictionary => m_dictionary;
        public List<JObject> Nodes => m_nodes;
    }
}
