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
        routes.Add("http://localhost:8080/", tmpl.fromFile("INDEX",@".\priv\tmpl\index.html"));
    }

    Pink.Router GetRouter() {
        return routes;
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
