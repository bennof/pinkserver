// pinkdb.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Data.OleDb;
using System.Threading;
using System.Collections.Generic;

namespace Pink {
    public class DB {

        // default page size 
        public const int PAGE_SIZE = 4096;

        // XOR Buffer
        static byte[]   JET3_XOR = { 0x86,0xfb,0xec,0x37,0x5d,0x44,0x9c,0xfa,0xc6,
                                     0x5e,0x28,0xe6,0x13,0xb6,0x8a,0x60,0x54,0x94};
        static ushort[] JET4_XOR = { 0x6aba,0x37ec,0xd561,0xfa9c,0xcffa,
                                     0xe628,0x272f,0x608a,0x0568,0x367b,
                                     0xe3c9,0xb1df,0x654b,0x4313,0x3ef3,
                                     0x33b1,0xf008,0x5b79,0x24ae,0x2a7c};

        // Version ids
        const int VER_JET3 = 0;
        const int VER_JET4 = 1;
        const int VER_ACCDB2007 = 0x02;
        const int VER_ACCDB2010 = 0x0103;

        // auto create a buffer
        public static byte[] CreateBuffer()
        {
            return new byte[PAGE_SIZE];
        }

        // Alls in one ecoding extraction
        public static string GetEncoding(string filen)
        {
            int error = 0;
            string enc = "";
            byte[] buffer = CreateBuffer();
            error += ReadPage(filen,DB.PAGE_SIZE,buffer);
            error += ScanPage(buffer,ref enc);
            if(error != 0) {
                return "";
            } else {
                return enc;
            }
        }

        // Read the first page of a file into a bytebuffer 
        public static int ReadPage(string filen, int pageSize, byte[] buffer) 
        {
            try
            {
                using (System.IO.FileStream file = new System.IO.FileStream(filen, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    int len = buffer.Length;
                    int n = file.Read(buffer, 0, len);
                    if (n != len ) {
                        return len;
                    }
                }
                return 0;
            } catch ( System.Exception e) {
                Console.WriteLine(e.ToString());
                return 1;
            }
        }

        // the first page will be scanned for information
        public static int ScanPage(byte[] buffer, ref string enc) 
        {
            //page id
            if(buffer[0]!=0){ // first paage has id 0 
                Console.WriteLine("ERROR no vaild db");
                return 1;
            }

            // db type
            int version = System.BitConverter.ToInt32(buffer,0x14);
            switch(version){
            case VER_JET3:
                    Console.WriteLine("DB Version: JET 3");
                    break;
            case VER_JET4:
                    Console.WriteLine("DB Version: JET 4");
                    break;
            case VER_ACCDB2007:
                    Console.WriteLine("DB Version: AccessDB 2007");
                    break;
            case VER_ACCDB2010:
                    Console.WriteLine("DB Version: AccessDB 2010");
                    break;
            default:
                    Console.WriteLine("ERROR unkown version: {0}\n",version);
                    return 1;
            }

            // Check for encoding string
            //Password extract
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if( version==0 ) { // JET 3
                for(int i=0;i<18;i++){
                    sb.Append((byte)(buffer[i+0x42]^JET3_XOR[i]));
                }
            } else if ( version >=1 ) { // JET 4
                ushort magic = System.BitConverter.ToUInt16(buffer,0x66);
                magic = (ushort) (magic^JET4_XOR[18]);
                ushort b;
                for(int i=0;i<18;i++){
                    b = System.BitConverter.ToUInt16(buffer,2*i+0x42);
                    b = (ushort)(b^JET4_XOR[i]);
                    if(b>255){
                        b = (ushort)(b^magic);
                    }
                    //Console.WriteLine("{0}: {1}",i,Convert.ToChar(b));
                    if (b == 0 ) {
                        break;
                    }  
                    sb.Append(Convert.ToChar(b));
                }   
            }
            Console.WriteLine("Encoding: {0}",sb.ToString());
            enc = sb.ToString();
            return 0;
        }

        // list all OLEDB Providers to CLI
        public static void ShowProviders()
        {
            OleDbDataReader reader = OleDbEnumerator.GetRootEnumerator();
            //OleDbDataReader reader = enumerator.GetElements();
            for (int i = 0; i < reader.FieldCount; i++) {
                Console.Write("{0}\t",reader.GetName(i));
            }
            Console.WriteLine("");
            while (reader.Read()) {
                for (int i = 0; i < reader.FieldCount; i++) {
                    Console.Write("{0}\t",reader.GetValue(i));
                }
                Console.WriteLine("");
            }
        } 

        // Get a provider with filter string in name
        public static string GetProvider(string filter)
        {
            OleDbDataReader reader = OleDbEnumerator.GetRootEnumerator();
            int sel = 0;
            for (int i = 0; i < reader.FieldCount; i++) {
                if(reader.GetName(i)=="SOURCES_NAME"){
                    sel = i;
                    break;
                }
            }

            Console.WriteLine("Searching Providers ...");
            while (reader.Read()) {
                Console.WriteLine("\t ...{0}",reader.GetValue(sel));
                if(reader.GetValue(sel).ToString().Contains(filter)) {
                    Console.WriteLine("Provider: {0}",reader.GetValue(sel));
                    return reader.GetValue(sel).ToString();
                }
            }
            return null;
        }

        public static OleDbConnection Connect(string provider, string file) 
        {
            OleDbConnection c = new OleDbConnection("Provider="+provider+";Data Source=" + file + "; Jet OLEDB:Database Password="+ GetEncoding(file) +";");
            c.Open();
            return c;
        }
    }

    public class JSON {
        public static void Write(OleDbDataReader reader, System.IO.StreamWriter file, string[] header, long ean)
        {
            if(header == null){
                header = new string[reader.FieldCount];
                for (int i=0; i<reader.FieldCount; i++) {
                    header[i] = reader.GetName(i);
                }
            }

            file.Write("[\r\n");
            while (reader.Read())  
            {    
                for (int i=0; i<reader.FieldCount; i++) 
                {
                    if (i != 0) {
                        file.Write(",\"");
                        file.Write(header[i]);
                        file.Write("\": \"");
                        file.Write(reader[i]);
                        file.Write("\"");
                    } else {
                        file.Write("{\"");
                        file.Write(header[i]);

                        if (ean > 0){
                            file.Write("\": ");
                            int h = (int) reader[i];
                            long hh = EAN13.Create(h,ean);// 2700000000000
                            file.Write(hh);
                        } else {
                            file.Write("\": \"");
                            file.Write(reader[i]);
                            file.Write("\"");
                        };
                    }
                }
                file.Write("},\r\n");
            }
            file.Write("{}]\r\n");
            file.Flush();
        }
        
    }

    public class CSV {
        public static void Write(OleDbDataReader reader, System.IO.StreamWriter file, string sep,string[] header,  long ean)
        {
            
            if(header!=null) {
                for (int i=0; i<reader.FieldCount; i++) 
                {      
                    if (i!=0) {
                        file.Write(sep);
                    }
                    file.Write(header[i]);
                }
            } else {
                for (int i=0; i<reader.FieldCount; i++) 
                {      
                    if (i!=0) {
                        file.Write(sep);
                    }
                    file.Write(reader.GetName(i));
                }
            }
            
            file.Write("\r\n");
            
            while (reader.Read())  
            {    
                for (int i=0; i<reader.FieldCount; i++) 
                {
                    if (i != 0) {
                        file.Write(sep);
                        file.Write(reader[i]);
                    } else {
                        if (ean > 0){
                            int h = (int) reader[i];
                            long hh = EAN13.Create(h,ean);// 2700000000000
                            file.Write(hh);
                        } else {
                            file.Write(reader[i]);
                        };
                    }
                }
                file.Write("\r\n");
            }
            file.Flush();
        }

        public static void Read(RowProcessor proc,System.IO.StreamReader file, string sep, bool hasHeader)
        {
            string[] header = null; 
            string[] read = null;
            string data;

            if(hasHeader) {
                if ((data = file.ReadLine()) != null ){
                    header = data.Split(sep[0]);
                }
            }

            while ((data = file.ReadLine()) != null) {
                read = data.Split(sep[0]);
                proc.ProcessRow(header, read);
            }
        }

        public interface RowProcessor {
            int ProcessRow(string[] header, string[] elements);
        }
    }


    public class DBServer {
        Thread self;
        EventWaitHandle  waitHandle = new EventWaitHandle (false,EventResetMode.ManualReset);
        bool isRunning = false;
        bool keepRunning = true;
        OleDbConnection db;
        Mutex mut = new Mutex();
        Queue<SQL> req;

        public DBServer(OleDbConnection db){
            this.db = db;
            this.req = new Queue<SQL>();
        }

        public void Start(){
            if(!isRunning) {
                self = new Thread(new ThreadStart(this.Run));
                self.Name = "DBServer";
                self.Start();
            }
        }

        public void Stop(){
            keepRunning = false;
            waitHandle.Set();
        }

        public void Run(){
            isRunning = true;
            SQL s;
            while(keepRunning) {
                waitHandle.WaitOne();
                if(mut.WaitOne()) {
                    if (req.Count > 0) {
                        s = req.Dequeue();
                        mut.ReleaseMutex();
                        OleDbCommand command = new OleDbCommand(s.Stm, db);
                        // process s 
                        if (s.Type == 1 ){
                            Console.WriteLine("Processing Query ({0}): {1}",s.Type, s.Stm);
                            s.R = command.ExecuteReader();
                        } else if (s.Type == 2) {
                            Console.WriteLine("Processing Execute ({0}): {1}",s.Type, s.Stm);
                            s.R = command.ExecuteNonQuery();
                        } else {
                            s.Type=0;
                        }
                        s.done = true;
                    } else {mut.ReleaseMutex();}    
                }

                waitHandle.Reset();
                if (req.Count > 0) {waitHandle.Set();}
            }
        }   

        public OleDbDataReader Query(string stm, int timeout){
            SQL q = new SQL(1,stm);
            mut.WaitOne();
            req.Enqueue(q);
            mut.ReleaseMutex();
            waitHandle.Set();
            while(!q.done){ Thread.Sleep(50); } // wait for result
            Console.WriteLine("Result statment ({0}): {1}",q.Type, q.Stm);
            if(q.Type == 1) {
                return (OleDbDataReader) q.R;
            }
            return null;
        }

        public int Execute(string stm, int timeout){
            SQL e = new SQL(2,stm);
            mut.WaitOne();
            req.Enqueue(e);
            mut.ReleaseMutex();
            waitHandle.Set();
            while(!e.done){ Thread.Sleep(50); } // wait for result
            Console.WriteLine("Result statment ({0}): {1}",e.Type, e.Stm);
            if(e.Type == 2) {
                return (int) e.R;
            }
            return -1;
        }


        public static void Test() {
            // Connect
            string provider = DB.GetProvider("ACE");
            if(provider==null) {
                Console.WriteLine("Error: No ACE Provider"); 
                return;
            }
            
            OleDbConnection db = DB.Connect(provider,@".\priv\db\example.accdb");
            db.Open();

            DBServer srv = new DBServer(db);
            srv.Start();
            //srv.Execute("Dies ist ein Test",10);
            OleDbDataReader r = srv.Query("SELECT Title, Author, Date FROM Pages;",10);
            System.IO.StreamWriter file = new System.IO.StreamWriter(Console.OpenStandardOutput());
            CSV.Write(r,file,";",null,-1);
            r.Close();
            file.Close();
            srv.Stop();
        }
    }

    class SQL {
        public SQL(int t, string stm){Type=t;Stm=stm;}
        public bool done = false;
        public int Type;
        public string Stm;
        public object R;
    }
}