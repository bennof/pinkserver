// pinktoken.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

/* 
** Token is a map of string to object
** 
** string ToJSON(Token t)
** Token  FromJSON(string json)
** Token  FromJWT(string s) // missing validation
** string ToSecToken(RijndaelManaged AES, Token t)
** static Token FromSecToken(RijndaelManaged AES, string s)
*/

namespace Pink {

    public class Token :Dictionary<string, object> {
        
        
        // missing implementation
        public static string ToJSON(Token t){
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(t);
        }

        public static Token FromJSON(string json){
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<Token>(json);
        }

        public static Token FromJWT(string s) {
            string[] elem = s.Split('.');
            return FromJSON(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(elem[1])));
        }

        public static RijndaelManaged GetEncryption(string password){
            RijndaelManaged AES = new RijndaelManaged();
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(password), saltBytes, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CBC;
            return AES;
        }

        public static string ToSecToken(RijndaelManaged AES, Token t){
            MemoryStream ms = new MemoryStream();
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ToJSON(t));
            CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(b, 0, b.Length);
            cs.Close();
            string s = System.Convert.ToBase64String(ms.ToArray());
            ms.Close();
            return s;
        }

        public static Token FromSecToken(RijndaelManaged AES, string s){
            MemoryStream ms = new MemoryStream();
            byte[] b = System.Convert.FromBase64String(s);
            CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(b, 0, b.Length);
            cs.Close();
            Token t = FromJSON(System.Text.Encoding.UTF8.GetString(ms.ToArray()));
            ms.Close();
            return t;
        }

        public static void Test(){
            Token t = new Token();
            t.Add("test","ein test");
            t.Add("name","otto");

            RijndaelManaged AES = Token.GetEncryption("somekey");
            
            Console.WriteLine("PinkToken: {0}",t);
            foreach(KeyValuePair<string, object> entry in t){ 
                Console.WriteLine("{0}: {1}",entry.Key, entry.Value);
            }
            string s = Token.ToSecToken(AES,t);
            Console.WriteLine("JSON: {0}",s);
            
            Token tt = Token.FromSecToken(AES,s);
            foreach(KeyValuePair<string, object> entry in tt){ 
                Console.WriteLine("{0}: {1}",entry.Key, entry.Value);
            }


            string jwt ="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
            Token ttt = Token.FromJWT(jwt);
            foreach(KeyValuePair<string, object> entry in ttt){ 
                Console.WriteLine("{0}: {1}",entry.Key, entry.Value);
            }
        }
    }
}