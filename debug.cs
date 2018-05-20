using System;

namespace Templates {
    public class Tryout :Pink.Template {
        public override void Render(Pink.Request req, object o){
            req.writeString( "<html>\r\n    <head>\r\n        <Title>\r\n            Test\r\n        </Title>\r\n    </he" +
    "ad>\r\n    <body>\r\n        <h1>Test Template Engine</h1>\r\n        <i>");
            req.writeString(string.Format("{0}", DateTime.Now));
            req.writeString( "</i>\r\n        <p>");
            req.writeString(string.Format("{0}", req.Method));
            req.writeString( "</p>\r\n        <p>");
            req.writeString(string.Format("{0}", req.URL));
            req.writeString( "</p>\r\n    </body>\r\n</html>");
        }
    }
}
