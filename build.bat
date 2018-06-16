@ECHO OFF
ECHO Building PinkServer
ECHO Simple webserver framework in c#  
ECHO ===============================================================
ECHO Copyright 2018 Benjamin 'Benno' Falkner. All rights reserved.
ECHO Use of this source code is governed by a MIT-style
ECHO license that can be found in the LICENSE file.
ECHO ===============================================================
ECHO Building TestServer ... 
csc -out:.\bin\Test.exe   .\src\test.cs .\src\pinkserver.cs .\src\pinkmime.cs .\src\pinkstatic.cs .\src\pinktemplate.cs .\src\pinkdb.cs