// pinkean.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;

namespace Pink {
    /* European Article Number 13
    *  implementing basic functions to create 
    *  and check EAN 13 numbers. long int is 
    *  used to store the values
    */
    class EAN13 {

        // calculates the last digit of an ean and
        // returns the value
        static long getCheck(long ean){
            long h = ean / 10;
            long fac = 3;
            long sum = 0;
            for(int i=0; i<12; i++){
                sum = sum + (fac * (h%10));
                fac = (fac==3)? 1 : 3;
                h /= 10; 
            }
            long p9 = sum + 9;
            long n10 = p9 - (p9%10); 
            return n10 - sum;
        }

        // checks if an ean is valid by 
        // testing last digit 
        public static bool Check(long ean){
            long h = getCheck(ean);
            if (h == ean%10 ) 
                return true;
            else
                return false;
        } 

        // creates a new ean using a number num and 
        // a signiture sig, which is added
        public static long Create(long num, long sig){
            long r = sig + num*10;
            if( r%sig == num*10 ) // check signiture
                return r + getCheck(r);
            return 0;
        }

        // deprecated test function
        public static void Test() {
            Console.WriteLine("Test EAN");

            long h = 2700000000000;
            long hh = 1184;
            Console.WriteLine("Create EAN: {0} [{1}]",hh,h);
            //h = Create(hh, h);
            //Console.WriteLine("Restult: {0}",h);
            hh = 1185;
            Console.WriteLine("Create EAN: {0} [{1}]",hh,h);
            h = Create(hh, h);
            Console.WriteLine("Restult: {0}",h);
            Console.WriteLine("Check: {0}",Check(5449000096244));
        }
    }
}