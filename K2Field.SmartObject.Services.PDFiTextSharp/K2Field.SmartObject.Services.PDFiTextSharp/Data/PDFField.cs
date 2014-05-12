using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K2Field.SmartObject.Services.PDFiTextSharp.Data
{
    public class PDFField
    {
        public int TypeId { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public string FullName { get; set; }
        public string AlternativeName { get; set; }
        public string PartialName { get; set; }

        // children

        // allowable values from children?

        public string FieldValue { get; set; }
    }
}
