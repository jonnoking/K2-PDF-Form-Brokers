using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K2Field.SmartObject.Services.PDFBox.Data
{
    public class PDFInfo
    {
        public string Author { get; set; }
        public string CreationDate { get; set; }
        public string Creator { get; set; }
        public string Keywords { get; set; }
        public string ModificationDate { get; set; }
        public string Producer { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
        public string Trapped { get; set; }
        public int NumberOfPages { get; set; }

        public string PDFText { get; set; }

        public string PDFUri { get; set; }

    }
}
