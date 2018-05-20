using System;

namespace Templates {
    public class Tryout :Pink.Template {
        public override void Render(Pink.Request req, object o){
            req.writeString( "<html>\r\n    <head>\r\n        <Title>\r\n            Test\r\n        </Title>\r\n    </he" +
    "ad>\r\n    <body>\r\n        <h1>Test</h1>\r\n        <i>");
            req.writeString(string.Format("{0}", DateTime.Now));;
            req.writeString( "</i>\r\n    </body>\r\n</html>");
        }
    }
}
