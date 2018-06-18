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
            string uri;
            byte[] buffer = new byte[BUFFER_SIZE] ;

            uri = (prefix != "") ? req.URL.Substring(prefix.Length): req.URL;

            try {
                // get file
                if(isFile){
                    fs = new FileStream(rootPath, FileMode.Open);
                } else if(File.Exists(rootPath+uri)){
                    fs = new FileStream(rootPath+uri, FileMode.Open);
                } else {
                    Console.WriteLine("Error: {0}",rootPath+uri);
                    req.StatusCode  = 400;
                    req.StatusDescription = "File "+uri+" not found.";
                    req.ContentType = MIME.TXT;
                    req.WriteString("400 File "+uri+" not found.");
                    return;    
                }

                // write header
                //req.AcceptRange = true;
                req.ContentType = MIME.GetTypeFromFilen(uri);
                req.SendChunked = true;
                req.StatusCode  = 200;
                //req.StatusDescription = ;
                int len = (int)fs.Length;
                int start = 0;//req.Range()

                // write data
                for(int i=start; i<len; i+= BUFFER_SIZE) {
                    fs.Read(buffer, i, BUFFER_SIZE);
                    req.Write(buffer);
                }
                fs.Close();
            } catch {
                req.StatusCode  = 400;
                req.StatusDescription = "File "+uri+" not found.";
            } 
            
        }

        public static void Test(string path){
            Handlers routes = new Handlers();
            routes.Add("http://localhost:8080/", new StaticFileHandler(path));
            Server s = new Server("http://localhost:8080/",routes);
            s.Start();
            Console.WriteLine("A simple static webserver. Press a key to quit.");
            Console.ReadKey();
            s.Stop();
        }
    }
}