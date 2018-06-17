using System;

namespace Templates {
    public class Tryout :Pink.Template {
        public override void Render(Pink.Request req, object o){
            req.WriteString( "<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n            <meta charse" +
    "t=\'UTF-8\'>\r\n            <Title>Test</Title>\r\n            </head>\r\n            <b" +
    "ody>\r\n                <h1>Test Template Engine</h1>\r\n                <i>");
            req.WriteString(string.Format("{0}", DateTime.Now));
            req.WriteString( "</i>\r\n                <p>");
            req.WriteString(string.Format("{0}", req.Method));
            req.WriteString( "</p>\r\n                <p>");
            req.WriteString(string.Format("{0}", req.URL));
            req.WriteString( "</p>\r\n            </body>\r\n            </html>\r\n            ");
        }
    }
}
