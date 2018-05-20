// test.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;

class test{
    static void Main(string[] args) {
        Pink.Templates t = new Pink.Templates();
        Pink.Template tmpl = t.fromFile("Tryout", @"test\templ.html");


        Pink.Handlers routes = new Pink.Handlers();
        routes.Add("http://localhost:8080/test.html", tmpl);
        routes.Add("http://localhost:8080/", new Pink.DefaultHandler());

        Pink.Server s = new Pink.Server("http://localhost:8080/",routes);
        s.Start();
        Console.WriteLine("A simple webserver. Press a key to quit.");
        Console.ReadKey();
        s.Stop();
    }
}
