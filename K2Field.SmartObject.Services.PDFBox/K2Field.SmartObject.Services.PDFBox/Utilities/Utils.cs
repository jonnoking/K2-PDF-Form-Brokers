using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.apache.pdfbox.pdmodel;

namespace K2Field.SmartObject.Services.PDFBox.Utilities
{
    public static class Utils
    {
        public static PDDocument GetPDDocument(string uri)
        {
             PDDocument doc;
             if (uri.StartsWith("http"))
             {
                 java.net.URL url = new java.net.URL(uri);
                 doc = PDDocument.load(url);
             }
             else
             {
                 doc = PDDocument.load(uri);
             }

             if (doc == null)
             {
                 throw new Exception(string.Format("Exception loading PDF document {0}.", uri));
             }
             return doc;
        }

        public static DateTime GetDateFromJava(java.util.Calendar javadate)
        {
            long TICKS_AT_EPOCH = 621355968000000000L;            
            DateTime dt = new DateTime((javadate.getTimeInMillis() * 10000) + TICKS_AT_EPOCH);
            return dt;
        }
    }
}
