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


namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var jobj = new Json.JObject();
            jobj.AddValue("ID", 123);
            jobj.AddValue("Name", "ABC");


            Console.WriteLine("--------------------------------");
            Console.WriteLine("Writing ");
            Console.WriteLine("");
            string val = jobj.AsPrettyPrint();
            Console.WriteLine(val);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Parsing ");
            Console.WriteLine("");

            jobj = Json.JObject.Parse(val);

            val = jobj.AsPrettyPrint();
            Console.WriteLine(val);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("As Byte Array ");
            Console.WriteLine("");

            val = jobj.ToBin();
            Console.WriteLine(val);


            Console.WriteLine("--------------------------------");
            Console.WriteLine("From Byte Array ");
            Console.WriteLine("");
            jobj = Json.JObject.ParseBin(val);

            val = jobj.AsPrettyPrint();
            Console.WriteLine(val);


            Console.WriteLine("--------------------------------");
            Console.WriteLine("As Compact");
            Console.WriteLine("");
            jobj = Json.JObject.Parse(val);

            val = jobj.AsCompactPrint();
            Console.WriteLine(val);
        }
    }
}
