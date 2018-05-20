// pinkserver.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.Linq;


namespace Pink {
    public class Handlers : Dictionary<string,Handler>{}

    public class Server{
        private HttpListener listener;
        private Handlers router;

        public Server(Handlers routes){
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            listener = new HttpListener();

            router = new Handlers();
            foreach (var pair in routes)
            {
                listener.Prefixes.Add(pair.Key);
                Console.WriteLine("new route: " + pair.Key);
                Uri url = new Uri(pair.Key);
                Console.WriteLine("... path: " + url.AbsolutePath);
                router.Add(url.AbsolutePath,pair.Value);
            }
        }

        public void Init(){}

        public void Start(){
            //fork server
            ThreadPool.QueueUserWorkItem((o) =>
            {
                listener.Start();
                Console.WriteLine("listening ...  ");
                try {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) => {
                            var ctx = c as HttpListenerContext;
                            try {
                                handle(ctx);
                            } catch  (Exception e) { Console.WriteLine("ERROR: {0}",e.ToString()); }
                            finally {ctx.Response.OutputStream.Close();/*close the stream*/}
                        },listener.GetContext());
                    }
                } catch {}
            });
        }


        public void Stop(){
            listener.Stop();
            listener.Close();
        }

        private void handle(HttpListenerContext ctx){
            //get handler
            Handler h = null;
            string path = ctx.Request.Url.AbsolutePath;
            foreach (var pair in router)
            {
                if(match(pair.Key,path)){
                    h = pair.Value;
                    break;
                }
            }

            if(h==null){
                ctx.Response.StatusCode = 400;
                byte[] buf = Encoding.UTF8.GetBytes("Bad Request.");
                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            }

            //create context
            Request req = new Request(ctx);
            h.handle(req);

            // finalize
            req.finalize();
        }

        private static bool match(string pattern, string value) {
            int pl, vl;
            pl = pattern.Length;
            vl = value.Length;
            if (pl == vl ) {
                return (pattern == value);
            }
            else if ( pl < vl && pattern[pl-1]=='/'){
                for(int i=0; i<pl; i++){
                    if(pattern[i]!=value[i])
                        return false;
                }
                return true;
            }
            return false;
        }
    }

    public class Request {
        private MemoryStream w;
        private HttpListenerRequest  req;
        private HttpListenerResponse res;

        public string              Method        {get => req.HttpMethod;}
        public string              URL           {get => req.Url.AbsolutePath;}
        public CookieCollection    Cookies       {get => req.Cookies;}
        public Stream              Input         {get => req.InputStream ;}
        public NameValueCollection Query         {get => req.QueryString;}
        public string              ContentType   {get => req.ContentType;}
        public long                ContentLength {get => req.ContentLength64;}

        public Request(HttpListenerContext ctx) {
            w = new MemoryStream();
            req = ctx.Request;
            res = ctx.Response;
        }

        public void write(byte[] buf) {
            w.Write(buf, 0, buf.Length);
        }

        public void writeString(string s) {
            byte[] buf = Encoding.UTF8.GetBytes(s);
            write(buf);
        }

        public void finalize(){
            res.ContentLength64 = w.Length;
            w.WriteTo(res.OutputStream);
        }
    }


    public abstract class Handler{
        abstract public void handle(Request req);
    }

    public class DefaultHandler : Handler{
        public override void handle(Request req) {
            req.writeString("<HTML><HEAD><TITLE>Testing PinkServer ... </TITLE><HEAD><BODY>");
            req.writeString(string.Format("<h1>Pink Server</h1><i>{0}</i>", DateTime.Now));
            req.writeString(string.Format("<p>Method: {0}</p>", req.Method));
            req.writeString(string.Format("<p>URL: {0}</p>", req.URL));
            req.writeString("<p>Cookies:<ul>");
            foreach(Cookie c in req.Cookies){
                req.writeString(string.Format("<li>{0}: {1}</li>", c.Name,c.Value));
            }
            req.writeString("</ul></p>");
            req.writeString("<p>Query:<ul>");
            foreach(string key in req.Query){
                req.writeString(string.Format("<li>{0}: {1}</li>", key,req.Query[key]));
            }
            req.writeString("</ul></p>");
            req.writeString(string.Format("<p>ContentType: {0}</p>", req.ContentType));
            req.writeString(string.Format("<p>ContentLength: {0}</p>", req.ContentLength));
            req.writeString("</BODY></HTML>");
        }
    }


    public class Templates : Dictionary<string,Template> {
        private static string[] start = new string[] {"{{"};
        private static string[] end = new string[] {"}}"};

        public Template fromFile(string name, string filen){
            try {
                return fromString(name,File.ReadAllText(filen));
            } catch(Exception e)    {
                Console.WriteLine("ERROR: {0}.", e.ToString());
            }
            return null; 
        }

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
                        sb.Append("            "+h[0]+";\r\n");
                        sb.Append("            req.writeString( "+ ToLiteral(h[1]) +");\r\n");
                    } else {
                        sb.Append("            req.writeString( "+ ToLiteral(s) +");\r\n");
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
    }

    // template prototype by default a Handler
    public abstract class Template : Handler{
        public abstract void Render(Request req, object o);

        public override void handle(Request req){
            Render(req,null);
        }
    }
}