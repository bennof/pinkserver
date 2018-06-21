// pinkstatic.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Net;
using System.IO;
using System.Text;


// missing range handler

namespace Pink {
    public class StaticFileHandler : Handler{
        private static int BUFFER_SIZE = 4096;
        
        private string rootPath;
        private bool isFile;
        private string prefix = "";

        public StaticFileHandler(string path){ init(path); }
        public StaticFileHandler(string path, string prefix){ this.prefix = prefix; init(path); }

        void init(string path){
            // checkn path
            if(File.Exists(path)) {
                isFile =  true;
                rootPath = path;
            } else if(Directory.Exists(path)) {
                isFile =  false;
                rootPath = path;
            } else {
                Console.WriteLine("ERROR: {0} is not a valid file or directory.", path);
                throw new FileNotFoundException("File or Directory not found",path);
            }  
        }

        public override void handle(Request req) {
            FileStream fs;
            string uri = "";
            byte[] buffer = new byte[BUFFER_SIZE] ;
            
            
            try {
                // get file
                if(isFile){
                    fs = new FileStream(rootPath, FileMode.Open);
                    uri= rootPath;
                } else {
                    uri = (prefix != "") ? req.URL.Substring(prefix.Length): req.URI;
                    uri = WebUtility.UrlDecode(uri);
                    uri = uri.Replace('/','\\');
                    Console.WriteLine("PATH: {0}",rootPath+uri);
                    if(File.Exists(rootPath+uri)){
                        fs = new FileStream(rootPath+uri, FileMode.Open);
                    } else {
                        Console.WriteLine("Error: {0}",rootPath+uri);
                        req.StatusCode  = 400;
                        req.StatusDescription = "File "+uri+" not found.";
                        req.ContentType = MIME.TXT;
                        req.WriteString("400 File "+uri+" not found.");
                        return;    
                    }
                }
                // write header
                //req.AcceptRange = true;
                req.ContentType = MIME.GetTypeFromFilen(uri);
                req.SendChunked = true;
                req.StatusCode  = 200;
                req.StatusDescription = "OK";
                int len = (int)fs.Length;
                req.ContentLength=len;
                int start = req.Range();
                fs.Seek(start, SeekOrigin.Begin);
                // write data
                int i;
                while(len > 0){
                    i = fs.Read(buffer, 0, BUFFER_SIZE);
                    req.Write(buffer,i);
                    len -= i;
                }
                fs.Close();
            } catch (Exception e) {
                Console.WriteLine(e);
                req.StatusCode  = 400;
                req.StatusDescription = "File "+uri+" not found.";
            } 
            
        }

        public static void Test(string path){
            Router routes = new Router();
            routes.Add("http://localhost:8080/", new StaticFileHandler(path));
            Server s = new Server("http://localhost:8080/",routes);
            s.Start();
            Console.WriteLine("A simple static webserver. Press a key to quit.");
            Console.ReadKey();
            s.Stop();
        }
    }
}