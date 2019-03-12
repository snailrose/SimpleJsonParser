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

namespace Json
{
    public class JLex
    {
        private string  m_buffer;
        private int     m_cur;
        private int     m_len;

        public JLex(string cur)
        {
            m_buffer    = cur;
            m_cur       = 0;
            m_len       = cur.Length;
        }

        private static int GetValueI(string text)
        {
            if (Int32.TryParse(text, out int res))
                return res;
            return 0;
        }

        public static double GetValueD(string text)
        {
            if (Double.TryParse(text, out double res))
                return res;
            return 0;
        }


        public JToken Lex()
        {
            while (!EOF)
            {
                char c = m_buffer[m_cur];
                if (c == '\n' || c == '\r')
                {
                    ++m_cur;
                    continue;
                }
                else if (c == ' ' || c == '\t')
                {
                    while (!EOF)
                    {
                        c = m_buffer[m_cur];

                        if (c != ' ' && c != '\t')
                            break;
                        ++m_cur;
                    }
                    continue;
                }
                else if (c == '{')
                {
                    ++m_cur;
                    return new JToken(JTokenType.OpenBracket, "{");
                }
                else if (c == '}')
                {
                    ++m_cur;
                    return new JToken(JTokenType.CloseBracket, "}");
                }
                else if (c == '[')
                {
                    ++m_cur;
                    return new JToken(JTokenType.OpenBrace, "[");
                }
                else if (c == ']')
                {
                    ++m_cur;
                    return new JToken(JTokenType.CloseBrace, "]");
                }
                else if (c == ':')
                {
                    ++m_cur;
                    return new JToken(JTokenType.Colon, ":");
                }
                else if (c == ',')
                {
                    ++m_cur;
                    return new JToken(JTokenType.Comma, ",");
                }
                else if (c == '"')
                {
                    // some string decl
                    // [.] except ["|\] not including [\n\r\b\r\f\t]
                    c = m_buffer[++m_cur];
                    string val = "";
                    while (!EOF && c != '"')
                    {
                        val += c;
                        if (c == '\\')
                        {
                            c = m_buffer[++m_cur];
                            if (c == '"'  ||
                                    c == '\\' ||
                                    c == '\n' ||
                                    c == '\r' ||
                                    c == '\b' ||
                                    c == '\r' ||
                                    c == '\f' ||
                                    c == '\t')
                            {
                                val += c;
                            }
                            else
                                return new JToken(JTokenType.SyntaxError, "Undefined control character " + c);
                        }
                        c = m_buffer[++m_cur];
                    }
                    if (c == '"') m_cur++;

                    if (val == "null")
                        return new JToken(JTokenType.Null, null);
                    if (val == "true" || val == "false")
                        return new JToken(JTokenType.Boolean, val == "true" ? true : false);
                    if (IsNumber(val))
                        return new JToken(JTokenType.Numerical, GetValueD(val));

                    return new JToken(JTokenType.String, val);
                }
                else if (IsNumeric(c) || c == '-' || c == '.')
                {
                    string val  = "";
                    while (IsNumeric(c) || c == '.' || c == '-')
                    {
                        val += c;
                        ++m_cur;
                        if (EOF)
                            return new JToken(JTokenType.SyntaxError, "Undefined character" + c);
                        c = m_buffer[m_cur];
                    }
                    return new JToken(JTokenType.Numerical, GetValueD(val));
                }
                else if (IsAlpha(c))
                {
                    //
                    string val = "";
                    int i = 0;
                    while (IsAlpha(c) && i < 5)
                    {
                        val += c;
                        i += 1;
                        ++m_cur;
                        if (EOF)
                            return new JToken(JTokenType.SyntaxError, "Undefined character" + c);
                        c = m_buffer[m_cur];
                    }
                    if (val != "true" && val != "false" &&  val != "null")
                        return new JToken(JTokenType.SyntaxError, "expecting, true, falseor null");

                    if (val == "null")
                        return new JToken(JTokenType.Null, null);
                    return new JToken(JTokenType.Boolean, val == "true" ? true : false);
                }
                else
                    return new JToken(JTokenType.SyntaxError, "Undefined character" + c);
            }
            return new JToken(JTokenType.EOF, "EOF");
        }

        bool EOF => m_len == 0 || m_cur >= m_len;

        public static bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';

        }
        public static bool IsNumeric(char c)
        {
            return (c >= '1' && c <= '9') || c == '0';
        }

        public static bool IsNumber(string s)
        {
            if (s.Length == 0)
                return false;

            // 0123456789.+-feE
            foreach (char c in s)
            {
                if (!IsNumeric(c))
                {
                    if (!(c == '.' && c == '-' &&
                            c == 'f' && c == '+' &&
                            c == 'e' && c == 'E')
                       )
                        return false;
                }
            }
            return true;
        }
    }
}

