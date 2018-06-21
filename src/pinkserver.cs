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


namespace Pink {
    public class Router : Handler {
        Dictionary<string,Handler> routes = new Dictionary<string,Handler>();

        public void Add(string url, Handler h) {
            Uri uri = new Uri(url);
            Console.WriteLine("... path: " + uri.AbsolutePath);
            routes.Add(uri.AbsolutePath,h);
        }

        public override void handle(Request req){
            //get handler
            Handler h = null;
            string path = req.URL;
            foreach (var pair in routes)
            {
                if(Match(pair.Key,path)){
                    h = pair.Value;
                    break;
                }
            }

            if(h==null){
                req.StatusCode = 400;
                byte[] buf = Encoding.UTF8.GetBytes("Bad Request.");
                req.ContentLength = buf.Length;
                req.Write(buf,buf.Length);
            }

            //create context
            //Request req = new Request(ctx);
            h.handle(req);
        }

        public static bool Match(string pattern, string value) {
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

    public class Server{
        private HttpListener listener;
        private Handler router;

        public Server(string url, Handler routes){
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            listener = new HttpListener();
            listener.Prefixes.Add(url);
            router = routes;
        }

        public void Init(){}

        public void Start(){
            this.Init();

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
            //create context
            Request req = new Request(ctx);
            router.handle(req);

            // finalize
            req.finalize();
        }

        //test server
        public static void Test() {
            Router routes = new Router();
            routes.Add("http://localhost:8080/", new DefaultHandler());
            Server s = new Server("http://localhost:8080/",routes);
            s.Start();
            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();
            s.Stop();
        }
    }

    public class Request {
        private MemoryStream w;
        private HttpListenerRequest  req;
        private HttpListenerResponse res;

        public string              Method            {get => req.HttpMethod;}
        public string              URL               {get => req.Url.AbsolutePath;}
        public string              URI               {get => req.Url.LocalPath;}
        public CookieCollection    Cookies           {get => req.Cookies;}
        public Stream              Input             {get => req.InputStream ;}
        public NameValueCollection Query             {get => req.QueryString;}
        public string              ContentType       {get => req.ContentType;       set => res.ContentType=value;}
        public long                ContentLength     {get => req.ContentLength64;   set => res.ContentLength64=value;} 
        public bool                AcceptRange       {set => res.Headers.Add("Accept-Ranges", "bytes");}
        public bool                SendChunked       {set => res.SendChunked=value;}
        public int                 StatusCode        {set => res.StatusCode=value;}
        public string              StatusDescription {set => res.StatusDescription=value;}
        public string              RedirectLocation  {set => res.RedirectLocation=value;}

        
        public Request(HttpListenerContext ctx) {
            w = new MemoryStream();
            req = ctx.Request;
            res = ctx.Response;
        }

        public void Write(byte[] buf) {
            w.Write(buf, 0, buf.Length);
        }

        public void Write(byte[] buf, int len) {
            w.Write(buf, 0, len);
        }

        public void WriteString(string s) {
            byte[] buf = Encoding.UTF8.GetBytes(s);
            Write(buf);
        }

        public void finalize(){
            w.Flush();
            res.ContentLength64 = w.Length;
            w.WriteTo(res.OutputStream);
        }

        public void ContentRange(long start, long range) {
            res.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", start, range - 1, range));
        }


        public int Range () {
            string[] s = req.Headers.GetValues("Range"); 
            if (s!=null) {
                string[] start = s[0].Replace("bytes=", "").Split('-');;
                return int.Parse(start[0]);
            } else {return 0;} 
        }

    }


    public abstract class Handler{
        public abstract void handle(Request req);

        public static string StripPrefix(string s, string prefix){
            return s.StartsWith(prefix) ? s.Substring(prefix.Length) : s;
        }
    }

    public class DefaultHandler : Handler{
        public override void handle(Request req) {
            req.WriteString("<HTML><HEAD><TITLE>Testing PinkServer ... </TITLE><HEAD><BODY>");
            req.WriteString(string.Format("<h1>Pink Server</h1><i>{0}</i><br>", DateTime.Now));
            req.WriteString("see <a href=\"https://github.com/bennof/pinkserver\">GitHub</a>");
            req.WriteString(string.Format("<p>Method: {0}</p>", req.Method));
            req.WriteString(string.Format("<p>URL: {0}</p>", req.URL));
            req.WriteString("<p>Cookies:<ul>");
            foreach(Cookie c in req.Cookies){
                req.WriteString(string.Format("<li>{0}: {1}</li>", c.Name,c.Value));
            }
            req.WriteString("</ul></p>");
            req.WriteString("<p>Query:<ul>");
            foreach(string key in req.Query){
                req.WriteString(string.Format("<li>{0}: {1}</li>", key,req.Query[key]));
            }
            req.WriteString("</ul></p>");
            req.WriteString(string.Format("<p>ContentType: {0}</p>", req.ContentType));
            req.WriteString(string.Format("<p>ContentLength: {0}</p>", req.ContentLength));
            req.WriteString("<h1>Code: C#</h1><pre><code>" + 
                "using System;\n"+
                "class Example{\n"+
                "   static void Main(string[] args) {\n"+
                "       Pink.Handlers routes = new Pink.Handlers();\n"+
                "       routes.Add(\"http://localhost:8080/\", new Pink.DefaultHandler());\n"+
                "       Pink.Server s = new Pink.Server(\"http://localhost:8080/\",routes);\n"+
                "       s.Start();\n"+
                "       Console.WriteLine(\"A simple webserver. Press a key to quit.\");\n"+
                "       Console.ReadKey();\n"+
                "       s.Stop();\n"+
                "   }\n"+
                "}\n"+
                "</code></pre>");
            req.WriteString("</BODY></HTML>");
        }
    }
}