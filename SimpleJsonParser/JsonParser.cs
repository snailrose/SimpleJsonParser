/*
-------------------------------------------------------------------------------
    This file is part of SimpleJSONParser.

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
    class SyntaxError  : Exception {
        public SyntaxError(string message) : base(message) {}
    }

    public class JParser
    {
        JObject m_cur, m_root;
        Stack<JObject> m_stack;
        public JParser()
        {
            m_root = new JObject();
            m_cur = null;
            m_stack = new Stack<JObject>();
            m_stack.Push(m_root);
        }

        public JObject Parse(string buf)
        {
            if (buf == null)
                return m_root;

            JLex lex = new JLex(buf);
            JToken c = lex.Lex();

            while (c != null && c.Type != JTokenType.EOF &&  (m_stack.Count > 0))
            {
                if (c.Type == JTokenType.OpenBracket || c.Type == JTokenType.Comma) {
                    JToken str = lex.Lex();

                    if (str == null || str.Type != JTokenType.String)
                        throw new SyntaxError("Expecting  a string identifier");

                    JToken colon = lex.Lex();
                    if (colon == null || colon.Type != JTokenType.Colon)
                    {
                        // error: definition not well formed
                        // expecting a colon character after a string identifier 
                        throw new SyntaxError("Expecting a colon character after a string identifier");
                    }

                    JToken control = lex.Lex();
                    if (control == null)
                    {
                        // error: definition not well formed
                        throw new SyntaxError("not well formed");
                    }

                    if (control.Type == JTokenType.String       ||
                        control.Type == JTokenType.Numerical    ||
                        control.Type == JTokenType.Boolean      ||
                        control.Type == JTokenType.Null)
                    {
                        // Value
                        m_cur = m_stack.Peek();
                        if (m_cur != null)
                            m_cur.AddValue((string)str.Value, control.Value);
                        c = lex.Lex();
                    }
                    else if (control.Type == JTokenType.OpenBracket)
                    {
                        // Object
                        JObject nobj = new JObject();
                        m_cur = m_stack.Peek();
                        if (m_cur != null)
                            m_cur.AddValue((string)str.Value, nobj);
                        m_stack.Push(nobj);
                    }
                    else if (control.Type == JTokenType.OpenBrace)
                    {
                        JObject nobj = new JObject { IsArray = true };
                        m_cur = m_stack.Peek();
                        if (m_cur != null)
                            m_cur.AddValue((string)str.Value, nobj);
                        m_stack.Push(nobj);

                        c = lex.Lex();
                        if (c.Type == JTokenType.OpenBracket)
                            m_stack.Push(new JObject());
                    }
                    else
                        throw new SyntaxError("Expecting a Value, Object Or Array Assignemnt");
                }
                else if (c.Type == JTokenType.String    ||
                         c.Type == JTokenType.Numerical ||
                         c.Type == JTokenType.Boolean   ||
                         c.Type == JTokenType.Null)
                {
                    // Value
                    m_cur = m_stack.Peek();
                    if (m_cur != null && m_cur.IsArray)
                    {
                        // only an array
                        m_cur.AddValue(c.Value);
                    }

                    c = lex.Lex();
                    if (c.Type == JTokenType.Comma)
                        c = lex.Lex();
                }
                else if (c.Type == JTokenType.CloseBracket)
                {
                    JObject cur = (m_stack.Count > 0) ? m_stack.Peek() : null;
                    m_stack.Pop();
                    if (m_stack.Count > 0 && cur != null)
                        m_stack.Peek().AddObject(cur);
                    c = lex.Lex();
                    if (c.Type == JTokenType.Comma)
                        c = lex.Lex();
                    if (c.Type == JTokenType.OpenBracket)
                        m_stack.Push(new JObject());
                }
                else if (c.Type == JTokenType.CloseBrace)
                {
                    JObject cur = (m_stack.Count > 0) ? m_stack.Peek() : null;
                    m_stack.Pop();
                    if (m_stack.Count > 0 && cur != null)
                        m_stack.Peek().AddObject(cur);
                    c = lex.Lex();
                }
                else
                    break;
            }
            return m_root;
        }
    }
}
