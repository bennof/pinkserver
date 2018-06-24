using System;

namespace Templates {
    public class INDEX :Pink.Template {
        public override void Render(Pink.Request req, object o){
            req.WriteString( "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<title>Blog Index</title" +
    ">\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"/css/style.css\">\r\n</head>\r\n<body" +
    ">\r\n<img src=\"/img/pinkserver.png\" style=\"text-align: center;\">\r\n<h1>Pink Server " +
    "Blog</h1>\r\n<i>");
            req.WriteString(string.Format("{0}", DateTime.Now));
            req.WriteString( "</i>\r\n<p>This the front page of the Pink Blog.</p>\r\n<table>\r\n");
             System.Data.OleDb.OleDbDataReader reader = Blog.DB.Query("SELECT ID, Title, Author, Date FROM Pages;",10);;
            req.WriteString( "\r\n<tr>\r\n    ");
             for (int i=0; i<reader.FieldCount; i++) { ;
            req.WriteString( " \r\n        <th> ");
            req.WriteString(string.Format("{0}", reader.GetName(i)));
            req.WriteString( " </th>\r\n    ");
             } ;
            req.WriteString( "\r\n    <th>link</th>\r\n</tr>\r\n");
             while (reader.Read()) { ;
            req.WriteString( "\r\n<tr>\r\n    ");
             for (int i=0; i<reader.FieldCount; i++) { ;
            req.WriteString( "\r\n        <td> ");
            req.WriteString(string.Format("{0}", reader[i]));
            req.WriteString( " </td>\r\n    ");
             } ;
            req.WriteString( "\r\n    <td><a href=\"/article/");
            req.WriteString(string.Format("{0}", reader[0]));
            req.WriteString( "\">");
            req.WriteString(string.Format("{0}", reader[1]));
            req.WriteString( "</a></td>\r\n</tr>\r\n");
             } reader.Close(); ;
            req.WriteString( "\r\n</table>\r\n<p><a href=\"/editor/-1\">new article</a></p>\r\n<p>");
            req.WriteString(string.Format("{0}", req.Method));
            req.WriteString( "</p>\r\n<p>");
            req.WriteString(string.Format("{0}", req.URL));
            req.WriteString( "</p>\r\n</body>\r\n</html>");
        }
    }
}
