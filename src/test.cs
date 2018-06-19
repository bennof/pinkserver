// test.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;



class Test{
    static void Main(string[] args) {
        // Bulding a Server
        // Prepare Templates and Handlers
        Pink.Handler Index = new Pink.DefaultHandler();
        //Pink.Handler Article;
        //Pink.Handler Editor;



        // Setting Routes
        Pink.Handlers routes = new Pink.Handlers();
        //routes.Add("http://localhost:8080/editor/", Editor);
        //routes.Add("http://localhost:8080/article/", Article);
        routes.Add("http://localhost:8080/", Index);

        /* Pink.Templates t = new Pink.Templates();
        Pink.Template tmpl = t.fromFile("Tryout", @"test\templ.html");*/

        // Start Pinkserver with routes on given address
        Pink.Server s = new Pink.Server("http://localhost:8080/",routes);
        s.Start();
        
        // keep running till key pressed
        Console.WriteLine("A simple webserver. Press a key to quit.");
        Console.ReadKey();
        s.Stop();

        //old Tests
        //Pink.Server.Test();
        //Pink.StaticFileHandler.Test(".\\priv\\static\\");
        //Pink.Templates.Test();
        //Pink.Token.Test();
        //Pink.DBServer.Test();
    }
}
