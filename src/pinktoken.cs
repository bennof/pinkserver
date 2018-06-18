// pinktoken.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace Pink {
    public class Token :Dictionary<string, object> {
        
        // missing implementation
        public string ToJSON(){
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>));
            return "";
        }

        public static void Test(){
            Token t = new Token();
            t.Add("test","ein test");
            t.Add("name","otto");
            
            Console.WriteLine("PinkToken: {0}",t);
            foreach(KeyValuePair<string, object> entry in t){ 
                Console.WriteLine("{0}: {1}",entry.Key, entry.Value);
            }

            

            Console.WriteLine("PinkToken: {0}",t);
            foreach(KeyValuePair<string, object> entry in t){ 
                Console.WriteLine("{0}: {1}",entry.Key, entry.Value);
            }

        }
    }
}