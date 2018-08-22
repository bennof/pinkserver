// pinkmime.cs
// Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

// public static string Pink.MIME.<Type>
// public static string Pink.MIME.GetTypeFromExt(string ext)
// public static string Pink.MIME.GetTypeFromFilen(string filen)


using System;
using System.Collections.Generic;
using System.IO;


namespace Pink {
    public class MIME {
        public static string 
            AVI   = "video/x-msvideo",
            BMP   = "image/bmp",
            CSS   = "text/css; charset=utf-8",
            CSV   = "text/csv; charset=utf-8",
            DOCX  = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  	        FLV   = "video/x-flv",
            GIF   = "image/gif",
  	        HTML  = "text/html; charset=utf-8",
            ICO   = "image/vnd.microsoft.icon",
            ICS   = "text/calendar",
            JAR   = "application/java-archive",
  	        JPEG  = "image/jpeg",
  	        JS    = "application/javascript",
            JSON  = "application/json",
            JSONP = "application/javascript",
            MIDI  = "audio/midi",
            MP3   = "audio/mp3",
            MP4   = "video/mp4",
            MPEG  = "video/mpeg",
            OGG   = "video/ogg",
            OTF   = "font/opentype",
  	        PDF   = "application/pdf",
  	        PNG   = "image/png",
            PS    = "application/postscript",
            PPTX  = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            QT    = "video/quicktime",
  	        SVG   = "image/svg+xml",
            SWF   = "application/x-shockwave-flash",
            TIFF  = "image/tiff",
            TTF   = "font/ttf",
            TXT   = "text/plain",
            WAV   = "audio/wav",
            WOFF  = "font/woff",
            XLSX  = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  	        XML   = "text/xml; charset=utf-8",
            ZIP   = "application/zip";
              
        public static readonly Dictionary<string, string> TypesFromExt = new Dictionary<string, string> {
            {".avi",   AVI},
            {".bmp",   BMP},
            {".css",   CSS},
            {".csv",   CSV},
  	        {".gif",   GIF},
            {".docx",  DOCX},
  	        {".htm",   HTML},
  	        {".html",  HTML},
            {".ico",   ICO},
            {".ics",   ICS},
            {".jar",   JAR},
  	        {".jpg",   JPEG},
  	        {".js",    JS},
            {".json",  JSON},
            {".jsonp", JSONP},
            {".mid",   MIDI},
            {".midi",  MIDI},
            {".mp3",   MP3},
            {".mp4",   MP4},
            {".m4v",   MP4},
            {".f4v",   MP4},
            {".f4p",   MP4},
            {".mpeg",  MPEG},
            {".ogg",   OGG},
            {".otf",   OTF},
  	        {".pdf",   PDF},
  	        {".png",   PNG},
            {".pptx",  PPTX},
            {".ps",    PS},
            {".ai",    PS},
            {".qt",    QT},
            {".mov",   QT},
  	        {".svg",   SVG},
            {".swf",   SWF},
            {".tiff",  TIFF},
            {".tif",   TIFF},
            {".ttf",   TTF},
            {".txt",   TXT},
            {".wav",   WAV},
            {".woff",  WOFF},
  	        {".xml",   XML},
            {".zip",   ZIP},
        };

        public static string GetTypeFromExt(string ext){
            return TypesFromExt[ext.ToLower()];
        }

        public static string GetTypeFromFilen(string filen){
            return GetTypeFromExt(Path.GetExtension(filen));
        }
    }
}