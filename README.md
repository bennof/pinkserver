![Image of Yaktocat](https://bennof.github.com/pinkserver/priv/static/pinkserver.png)

# pinkserver
A small c# webserver named after Pinksteren (Dutch: Pentecost) the time of the core development.

## Server
Minimalistic  HTTP server using the default debugging Handler.
```C#
// create router and set handler
Handlers routes = new Handlers();
routes.Add("http://localhost:8080/", new DefaultHandler());

// create and start server
Server s = new Server("http://localhost:8080/",routes);
s.Start();

// stop server            
s.Stop();
```

## Router
Uses first partial hit to route requests.

## Template Engine
Simple converter to cs with embedded code.

## Static File Handler
Static file handler uses the path of the URL and appends it to a given 
path. If a prefix is defined, the prefix is removed for URL before added
to the local path. Following code will return `C:\some\path\to\files\file.html` 
if `http://localhost:8080/to/be/removed/file.html` is requested.
```C#
new StaticFileHandler("C:\some\path\to\files","to/be/removed")
```

## DB Server (Access)

## TODO

 * write a documentation
 * create example files
 * impove templates

## License

Copyright 2018, [Benjamin Falkner](http://bennof.github.io/).

### Code

MIT License: [the `LICENSE` file](https://github.com/bennof/pinkserver/blob/master/LICENSE).