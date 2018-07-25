// pinktoken.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;

namespace Pink {
    public class Session : Token {
        public static Session Read(Request req, string name){
            c = req.Cookies[name];
            if (c!=null){
                return (Session) Token.FromSecToken(c.Value);
            }
            return null;
        }
        public static void Write(Request req, Session s) {
            
        }
    } 
}