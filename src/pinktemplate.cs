// pinktemplate.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.Linq;


namespace Pink {
    public class Templates : Dictionary<string,Template> {
        private static string[] start = new string[] {"{{"};
        private static string[] end = new string[] {"}}"};

        // use file input foe a file
        public Template fromFile(string name, string filen){
            try {
                return fromString(name,File.ReadAllText(filen));
            } catch(Exception e)    {
                Console.WriteLine("ERROR: {0}.", e.ToString());
            }
            return null; 
        }

        // use a string as input 
        public Template fromString(string name, string src) {
            try {
                string[] toks = src.Split(start, StringSplitOptions.None);
                StringBuilder sb = new StringBuilder();
                sb.Append("using System;\r\n\r\n" +
                "namespace Templates {\r\n" +
                "    public class " + name +" :Pink.Template {\r\n" +
                "        public override void Render(Pink.Request req, object o){\r\n");
                foreach(string s in toks){
                    string[] h = s.Split(end, StringSplitOptions.None);
                    if (h.Length==2) {
                        string hh = h[0].Trim();
                        if(hh[0]=='@'){
                            sb.Append("            req.WriteString(string.Format(\"{0}\", "+hh.Substring(1)+"));\r\n");
                        }else {
                            sb.Append("            "+h[0]+";\r\n");
                        }
                        sb.Append("            req.WriteString( "+ ToLiteral(h[1]) +");\r\n");
                    } else {
                        sb.Append("            req.WriteString( "+ ToLiteral(s) +");\r\n");
                    }
                }
                sb.Append("        }\r\n    }\r\n}\r\n");
                File.WriteAllText("debug.cs", sb.ToString());
                Assembly asm = compile(sb.ToString());
                Template tmpl = (Template)load(asm,"Templates."+name);
                //obj.GetType().InvokeMember("test",BindingFlags.InvokeMethod,null,obj,null); 
                Add(name,tmpl);
                return tmpl;
            } catch(Exception e)    {
                Console.WriteLine("ERROR: {0}.", e.ToString());
            }
            return null; 
        }

        // testing function
        public static void build(){
            string code =@"using System;
            using System.IO;
            namespace Templates {
                public class Template1: Pink.Template {
                    public override void Render(Pink.Request req, object o){
                        req.writeString(""<HTML><HEAD><TITLE>Testing PinkServer ... </TITLE><HEAD><BODY><H1>A Simple Template</H1><P>A simple test for the Template</P></BODY></HTML>"");
                    }
                    public void test(){
                        Console.WriteLine(""Test app"");
                    }
                }
            }
            ";
            try {  
                Assembly asm = compile(code);
                object obj = load(asm,"Templates.Template1");
                obj.GetType().InvokeMember("test",BindingFlags.InvokeMethod,null,obj,null); 
            } catch(Exception e)    {
                Console.WriteLine("ERROR: {0}.", e.ToString());
            }
        }
        public static Assembly compile(string code) {
            CodeDomProvider csc = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters param = new CompilerParameters();

            param.CompilerOptions = "/optimize";
            param.GenerateInMemory = true;

            // current assembly and deps
            Assembly curasm = System.Reflection.Assembly.GetExecutingAssembly();
            param.ReferencedAssemblies.Add(curasm.Location);
            param.ReferencedAssemblies.AddRange((from deps in curasm.GetReferencedAssemblies() select Assembly.ReflectionOnlyLoad(deps.FullName).Location).ToArray());
            //param.ReferencedAssemblies.Add("System.dll");

            CompilerResults result = csc.CompileAssemblyFromSource(param,code);
            if(result.Errors.HasErrors)    {
                string msg = "ERROR:";
                for (int i=0;i<result.Errors.Count;i++)
                    msg = msg  + "\r\nLine: " + result.Errors[i].Line.ToString() + " - " + result.Errors[i].ErrorText;     
                Console.WriteLine(msg);
                return null;
            }
            return result.CompiledAssembly;
        }

        public static object load(Assembly asm, string name) {
            object obj  = asm.CreateInstance(name);
            if (obj == null) {
                Console.WriteLine("ERROR: Could not load class");
                return null;
            }             
            return obj;
        }

        private static string ToLiteral(string input) {
            using (var writer = new StringWriter()) {
                using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        public static void Test(){
            string code =@"<!DOCTYPE html>
            <html>
            <head>
            <meta charset='UTF-8'>
            <Title>Test</Title>
            </head>
            <body>
                <h1>Test Template Engine</h1>
                <i>{{ @DateTime.Now }}</i>
                <p>{{ @req.Method}}</p>
                <p>{{ @req.URL}}</p>
            </body>
            </html>
            ";
            Pink.Templates t = new Pink.Templates();
            Pink.Template tmpl = t.fromString("Tryout",code);

            Router routes = new Router();
            routes.Add("http://localhost:8080/", tmpl);
            Server s = new Server("http://localhost:8080/",routes);
            s.Start();
            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();
            s.Stop();
        }
    }

    // template prototype by default a Handler
    public abstract class Template : Handler{
        public abstract void Render(Request req, object o);
        public override void handle(Request req){
            Render(req,null);
        }
    }
}