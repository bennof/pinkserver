// test.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;



public class Blog{
    Pink.Router routes;
    public static Pink.DBServer DB;
    //string URL = "http://localhost:8080";
    
    public Blog(){
        // create new router
        routes = new Pink.Router();

        // create templates
        Pink.Templates tmpl = new Pink.Templates();

        // DB connection
        string provider = Pink.DB.GetProvider("ACE");
        if(provider==null) { Console.WriteLine("Error: No ACE Provider"); return;}    
        DB = new Pink.DBServer(Pink.DB.Connect(provider,@".\priv\db\example.accdb"));
        DB.Start();

        // setup static handler
        Pink.StaticFileHandler staticHandler = new Pink.StaticFileHandler(@".\priv\static");
        routes.Add("http://localhost:8080/img/", staticHandler);
        routes.Add("http://localhost:8080/css/", staticHandler);
        routes.Add("http://localhost:8080/favicon.ico", staticHandler);
        routes.Add("http://localhost:8080/article/", tmpl.fromFile("ARTICLE",@".\priv\tmpl\article.html"));
        routes.Add("http://localhost:8080/editor/", tmpl.fromFile("EDITOR",@".\priv\tmpl\editor.html"));
        routes.Add("http://localhost:8080/upload", new Upload());
        routes.Add("http://localhost:8080/", tmpl.fromFile("INDEX",@".\priv\tmpl\index.html"));
    }

    Pink.Router GetRouter() {
        return routes;
    }

    class Upload :Pink.Handler{
        public override void handle(Pink.Request req){
            int r = DB.Execute("UPDATE Pages SET Title='"+req.GetPostQuery("title")+"', Body='"+req.GetPostQuery("body").Replace("'","''")+
                "', Author='"+req.GetPostQuery("author")+"' WHERE ID="+req.GetPostQuery("ID")+";",10);
            Console.WriteLine("Update: {0}",r);
            if (r!=1) { // try insert
                r = DB.Execute("INSERT INTO Pages (Title, Body, Author) VALUES ('"+req.GetPostQuery("title")+"', '"+req.GetPostQuery("body").Replace("'","''")+"','"+req.GetPostQuery("author")+"');",10);
                Console.WriteLine("Insert: {0}",r);
            }


            req.StatusCode = 200;
            req.WriteString("<HTML><HEAD><TITLE>Upload PinkServer ... </TITLE></HEAD><BODY>");
            req.WriteString(string.Format("<h1>Pink Server</h1><i>{0}</i><br>", DateTime.Now));
            req.WriteString(string.Format("<p>encoding: {0}</p>", req.ContentType));
            req.WriteString(string.Format("<p>length: {0}</p>", req.ContentLength));
            req.WriteString(string.Format("<hr>"));
            req.WriteString(req.GetPostQuery("title"));
            req.WriteString(string.Format("<hr>"));
            req.WriteString(req.GetPostQuery("author"));
            req.WriteString(string.Format("<hr>"));
            req.WriteString(req.GetPostQuery("body"));
            req.WriteString(string.Format("<hr>"));
            req.WriteString("</BODY></HTML>");
        }
    }

    static void Main(string[] args) {
        
        // Bulding a Blog Server
        Blog b = new Blog();
       
        // Start Pinkserver with routes on given address
        Pink.Server s = new Pink.Server("http://localhost:8080/",b.GetRouter());
        s.Start();
        
        // keep running till key pressed
        Console.WriteLine("A simple webserver. Press a key to quit.");
        Console.ReadKey();
        Console.WriteLine("Bye.");
        s.Stop();
        DB.Stop();
        
        //old Tests
        //Pink.Server.Test();
        //Pink.StaticFileHandler.Test(".\\priv\\static\\");
        //Pink.Templates.Test();
        //Pink.Token.Test();
        //Pink.DBServer.Test();
    }
}
