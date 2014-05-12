using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
//using System.Xml.Linq;

using SourceCode.SmartObjects.Services.ServiceSDK;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;

using K2Field.SmartObject.Services.PDFBox.Interfaces;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.interactive.form;
using System.Reflection;
using System.IO;

namespace K2Field.SmartObject.Services.PDFBox.Data
{
    /// <summary>
    /// A concrete implementation of IDataConnector responsible for interacting with an underlying system or technology. The purpose of this class it to expose and represent the underlying data and services as Service Objects for consumptions by K2 SmartObjects.
    /// </summary>
    class DataConnector : IDataConnector
    {
        #region Class Level Fields

        #region Constants
        /// <summary>
        /// Constant for the Type Mappings configuration lookup in the service instance.
        /// </summary>
        private static string __TypeMappings = "Type Mappings";
        #endregion

        #region Private Fields
        /// <summary>
        /// Local serviceBroker variable.
        /// </summary>
        private ServiceAssemblyBase serviceBroker = null;
        private string pdfUri = string.Empty;
        private string workingFolder = string.Empty;

        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a new DataConnector.
        /// </summary>
        /// <param name="serviceBroker">The ServiceBroker.</param>
        public DataConnector(ServiceAssemblyBase serviceBroker)
        {
            // Set local serviceBroker variable.
            this.serviceBroker = serviceBroker;
        }
        #endregion

        #region Methods

        #region void Dispose()
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Add any additional IDisposable implementation code here. Make sure to dispose of any data connections.
            // Clear references to serviceBroker.
            serviceBroker = null;
        }
        #endregion

        #region void GetConfiguration()
        /// <summary>
        /// Gets the configuration from the service instance and stores the retrieved configuration in local variables for later use.
        /// </summary>
        public void GetConfiguration()
        {
            pdfUri = serviceBroker.Service.ServiceConfiguration["pdfUri"].ToString();
            workingFolder = serviceBroker.Service.ServiceConfiguration["WorkingFolder"].ToString();

        }
        #endregion

        #region void SetupConfiguration()
        /// <summary>
        /// Sets up the required configuration parameters in the service instance. When a new service instance is registered for this ServiceBroker, the configuration parameters are surfaced to the appropriate tooling. The configuration parameters are provided by the person registering the service instance.
        /// </summary>
        public void SetupConfiguration()
        {
            serviceBroker.Service.ServiceConfiguration.Add("pdfUri", true, "");
            serviceBroker.Service.ServiceConfiguration.Add("WorkingFolder", true, "");

        }
        #endregion

        #region void SetupService()
        /// <summary>
        /// Sets up the service instance's default name, display name, and description.
        /// </summary>
        public void SetupService()
        {
            serviceBroker.Service.Name = "PDFBoxService";
            serviceBroker.Service.MetaData.DisplayName = "PDFBox Service";
            serviceBroker.Service.MetaData.Description = "PDFBox Service";
        }
        #endregion

        #region void DescribeSchema()
        /// <summary>
        /// Describes the schema of the underlying data and services to the K2 platform.
        /// </summary>
        public void DescribeSchema()
        {
            TypeMappings map = GetTypeMappings();
            PDFInfo info = new PDFInfo();
            Dictionary<string, PDFField> fields = new Dictionary<string,PDFField>();

            // get PDDocument
            try 
            {
                PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);

                // discover pdf doc
                info = GetPDFDoucmentInformation(doc);
                fields = DiscoverPDFFormFields(doc);

                doc.close();
                doc = null;
            }
            catch (Exception ex)
            {
                throw;
            }


            // create objects
            ServiceObject pdfServiceObject = new ServiceObject();
            pdfServiceObject.Name = info.Title.Replace(" ", "_");
            pdfServiceObject.MetaData.DisplayName = info.Title;
            pdfServiceObject.Active = true;



            List<Property> allprops = new List<Property>();
            List<Property> allfields = new List<Property>();
            List<Property> allmetadata = new List<Property>();


            Property inputUriproperty = new Property();
            inputUriproperty.Name = "pdfuri";
            inputUriproperty.MetaData.DisplayName = "PDF Uri";
            inputUriproperty.Type = typeof(System.String).ToString();
            inputUriproperty.SoType = SoType.Text;

            pdfServiceObject.Properties.Add(inputUriproperty);

            // has signatures
            Property hasSigsProperty = new Property();
            hasSigsProperty.Name = "containssignatures";
            hasSigsProperty.MetaData.DisplayName = "Contains Signatures";
            hasSigsProperty.Type = typeof(System.Boolean).ToString();
            hasSigsProperty.SoType = SoType.YesNo;

            pdfServiceObject.Properties.Add(hasSigsProperty);

            // has unsigned signatures
            Property containsUnsignedSigsProperty = new Property();
            containsUnsignedSigsProperty.Name = "containsunsignedsignatures";
            containsUnsignedSigsProperty.MetaData.DisplayName = "Contains Unsigned Signatures";
            containsUnsignedSigsProperty.Type = typeof(System.Boolean).ToString();
            containsUnsignedSigsProperty.SoType = SoType.YesNo;

            pdfServiceObject.Properties.Add(containsUnsignedSigsProperty);


            // for update - return base64?
            Property includebase64Property = new Property();
            includebase64Property.Name = "returnbase64";
            includebase64Property.MetaData.DisplayName = "Return Base64";
            includebase64Property.Type = typeof(System.Boolean).ToString();
            includebase64Property.SoType = SoType.YesNo;

            pdfServiceObject.Properties.Add(includebase64Property);

            // for update - base64
            Property base64Property = new Property();
            base64Property.Name = "base64pdf";
            base64Property.MetaData.DisplayName = "Base64 PDF";
            base64Property.Type = typeof(System.String).ToString();
            base64Property.SoType = SoType.Memo;

            pdfServiceObject.Properties.Add(base64Property);

            // for update - return path
            Property returnpathProperty = new Property();
            returnpathProperty.Name = "returnpath";
            returnpathProperty.MetaData.DisplayName = "Return Path";
            returnpathProperty.Type = typeof(System.String).ToString();
            returnpathProperty.SoType = SoType.Text;

            pdfServiceObject.Properties.Add(returnpathProperty);


            Type type = typeof(PDFInfo);
            PropertyInfo[] props = type.GetProperties();
            foreach (var p in props)
            {
                //text += docinfo.GetType().GetProperty(field.Name).GetValue(docinfo, null);

                Property infoproperty = new Property();
                infoproperty.Name = p.Name;
                infoproperty.MetaData.DisplayName = p.Name;
                infoproperty.Type = p.PropertyType.ToString();

                // needs to be mapped properly
                infoproperty.SoType = SoType.Text;

                pdfServiceObject.Properties.Add(infoproperty);
                allmetadata.Add(infoproperty);
                allprops.Add(infoproperty);
            }


            foreach (KeyValuePair<string, PDFField> field in fields.OrderBy(p => p.Key))
            {
                Property property = new Property();
                property.Name = field.Value.FullName.Replace(" ", "_");
                property.MetaData.DisplayName = field.Value.FullName;
                property.Type = typeof(System.String).ToString();
                property.SoType = SoType.Text;
                property.MetaData.ServiceProperties.Add("pdffullname", field.Value.FullName);
                property.MetaData.ServiceProperties.Add("pdfalternativename", field.Value.AlternativeName);
                property.MetaData.ServiceProperties.Add("pdfisreadonly", field.Value.IsReadOnly);
                property.MetaData.ServiceProperties.Add("pdfisrequired", field.Value.IsRequired);
                property.MetaData.ServiceProperties.Add("pdfpartialname", field.Value.PartialName);
                property.MetaData.ServiceProperties.Add("pdftype", field.Value.Type);

                allfields.Add(property);
                allprops.Add(property);
                pdfServiceObject.Properties.Add(property);
            }


            // add methods
            Method GetAllFieldValues = new Method();
            GetAllFieldValues.Name = "getallfieldvalues";
            GetAllFieldValues.MetaData.DisplayName = "Get All Field Values";
            GetAllFieldValues.Type = MethodType.Read;

            GetAllFieldValues.InputProperties.Add(inputUriproperty);
            GetAllFieldValues.Validation.RequiredProperties.Add(inputUriproperty);

            GetAllFieldValues.ReturnProperties.Add(inputUriproperty);
            foreach (Property prop in allprops)
            {
                GetAllFieldValues.ReturnProperties.Add(prop);
            }

            pdfServiceObject.Methods.Add(GetAllFieldValues);



            // contains signatures method
            Method HasSigs = new Method();
            HasSigs.Name = "containssignatures";
            HasSigs.MetaData.DisplayName = "Contains Signatures";
            HasSigs.Type = MethodType.Read;

            HasSigs.InputProperties.Add(inputUriproperty);
            HasSigs.Validation.RequiredProperties.Add(inputUriproperty);

            HasSigs.ReturnProperties.Add(inputUriproperty);
            HasSigs.ReturnProperties.Add(hasSigsProperty);

            pdfServiceObject.Methods.Add(HasSigs);

            // hanve't worked out how to implement with PDFBox
            // contains unsigned signatures method
            //Method ContainsUnsigned = new Method();
            //ContainsUnsigned.Name = "containsunsignedsignatures";
            //ContainsUnsigned.MetaData.DisplayName = "Contains Unsigned Signatures";
            //ContainsUnsigned.Type = MethodType.Read;

            //ContainsUnsigned.InputProperties.Add(inputUriproperty);
            //ContainsUnsigned.Validation.RequiredProperties.Add(inputUriproperty);

            //ContainsUnsigned.ReturnProperties.Add(inputUriproperty);
            //ContainsUnsigned.ReturnProperties.Add(containsUnsignedSigsProperty);

            //pdfServiceObject.Methods.Add(ContainsUnsigned);

            // add update
            Method UpdateMethod = new Method();
            UpdateMethod.Name = "updatepdffields";
            UpdateMethod.MetaData.DisplayName = "Update PDF Fields";
            UpdateMethod.Type = MethodType.Update;

            UpdateMethod.InputProperties.Add(inputUriproperty);
            UpdateMethod.Validation.RequiredProperties.Add(inputUriproperty);
            UpdateMethod.InputProperties.Add(includebase64Property);
            UpdateMethod.Validation.RequiredProperties.Add(includebase64Property);

            foreach (Property prop in allfields)
            {
                UpdateMethod.InputProperties.Add(prop);
            }

            UpdateMethod.ReturnProperties.Add(inputUriproperty);
            UpdateMethod.ReturnProperties.Add(includebase64Property);

            foreach (Property prop in allprops)
            {
                UpdateMethod.ReturnProperties.Add(prop);
            }
            UpdateMethod.ReturnProperties.Add(returnpathProperty);
            UpdateMethod.ReturnProperties.Add(base64Property);

            pdfServiceObject.Methods.Add(UpdateMethod);



            if (!serviceBroker.Service.ServiceObjects.Contains(pdfServiceObject))
            {
                serviceBroker.Service.ServiceObjects.Add(pdfServiceObject);
            }


            // admin object
            ServiceObject adminServiceObject = new ServiceObject();
            adminServiceObject.Name = "Functions" + info.Title.Replace(" ", "_");
            adminServiceObject.MetaData.DisplayName = "Functions - " + info.Title;
            adminServiceObject.Active = true;

            adminServiceObject.Properties.Add(inputUriproperty);


            Property sqlProperty = new Property();
            sqlProperty.Name = "generatedsql";
            sqlProperty.MetaData.DisplayName = "Generated SQL";
            sqlProperty.Type = typeof(System.String).ToString();
            sqlProperty.SoType = SoType.Memo;

            adminServiceObject.Properties.Add(sqlProperty);


            // generate create table sql
            Method generateCreateTable = new Method();
            generateCreateTable.Name = "generatecreatetablesql";
            generateCreateTable.MetaData.DisplayName = "Generate Create Table SQL";
            generateCreateTable.Type = SourceCode.SmartObjects.Services.ServiceSDK.Types.MethodType.Read;

            generateCreateTable.ReturnProperties.Add(sqlProperty);

            adminServiceObject.Methods.Add(generateCreateTable);



            Property smoProperty = new Property();
            smoProperty.Name = "formstoresmartobjectname";
            smoProperty.MetaData.DisplayName = "Form Store SmartObject Name";
            smoProperty.Type = typeof(System.String).ToString();
            smoProperty.SoType = SoType.Text;

            adminServiceObject.Properties.Add(smoProperty);

            Property smoMethodProperty = new Property();
            smoMethodProperty.Name = "formstoremethodname";
            smoMethodProperty.MetaData.DisplayName = "Form Store Method Name";
            smoMethodProperty.Type = typeof(System.String).ToString();
            smoMethodProperty.SoType = SoType.Text;

            adminServiceObject.Properties.Add(smoMethodProperty);

            Property smoIdProperty = new Property();
            smoIdProperty.Name = "returnidpropertyname";
            smoIdProperty.MetaData.DisplayName = "Return Id Property Name";
            smoIdProperty.Type = typeof(System.String).ToString();
            smoIdProperty.SoType = SoType.Text;

            adminServiceObject.Properties.Add(smoIdProperty);

            Property returnIdProperty = new Property();
            returnIdProperty.Name = "returnid";
            returnIdProperty.MetaData.DisplayName = "Return Id";
            returnIdProperty.Type = typeof(System.String).ToString();
            returnIdProperty.SoType = SoType.Text;

            adminServiceObject.Properties.Add(returnIdProperty);

            // copy form data to smartobject
            Method copyFormData = new Method();
            copyFormData.Name = "copyformdatatosmartobject";
            copyFormData.MetaData.DisplayName = "Copy Form Data to SmartObject";
            copyFormData.Type = SourceCode.SmartObjects.Services.ServiceSDK.Types.MethodType.Read;

            copyFormData.InputProperties.Add(inputUriproperty);
            copyFormData.Validation.RequiredProperties.Add(inputUriproperty);
            copyFormData.ReturnProperties.Add(inputUriproperty);

            copyFormData.InputProperties.Add(smoProperty);
            copyFormData.Validation.RequiredProperties.Add(smoProperty);
            copyFormData.ReturnProperties.Add(smoProperty);

            copyFormData.InputProperties.Add(smoMethodProperty);
            copyFormData.Validation.RequiredProperties.Add(smoMethodProperty);
            copyFormData.ReturnProperties.Add(smoMethodProperty);

            copyFormData.InputProperties.Add(smoIdProperty);
            copyFormData.Validation.RequiredProperties.Add(smoIdProperty);
            copyFormData.ReturnProperties.Add(smoIdProperty);

            copyFormData.ReturnProperties.Add(returnIdProperty);

            adminServiceObject.Methods.Add(copyFormData);


            if (!serviceBroker.Service.ServiceObjects.Contains(adminServiceObject))
            {
                serviceBroker.Service.ServiceObjects.Add(adminServiceObject);
            }

        }

        #endregion

        #region PDF Discover

        public static PDFInfo GetPDFDoucmentInformation(PDDocument document)
        {
            PDFInfo i = new PDFInfo();
            PDDocumentInformation info = document.getDocumentInformation();
            i.Author = info.getAuthor();

            if (info.getCreationDate() != null)
            {
                DateTime dt = Utilities.Utils.GetDateFromJava(info.getCreationDate());
                i.CreationDate = dt.ToLongDateString() + " " + dt.ToLongTimeString();
            }

            i.Creator = info.getCreator();
            i.Keywords = info.getKeywords();

            if (info.getModificationDate() != null)
            {
                DateTime dt = Utilities.Utils.GetDateFromJava(info.getModificationDate());
                i.ModificationDate = dt.ToLongDateString() + " " + dt.ToLongTimeString();
            }

            i.Producer = info.getProducer();
            i.Subject = info.getSubject();
            i.Title = info.getTitle();
            i.Trapped = info.getTrapped();
            i.NumberOfPages = document.getNumberOfPages();
            return i;
        }

        public Dictionary<string, PDFField> DiscoverPDFFormFields(PDDocument document)
        {
            return GetPDFFormFields(document, false);
        }

        public Dictionary<string, PDFField> GetPDFFormFields(PDDocument document, bool includeValues)
        {
            Dictionary<string, PDFField> pdfFormFields = new Dictionary<string, PDFField>();

            PDDocumentCatalog docCat = document.getDocumentCatalog();
            PDAcroForm form = docCat.getAcroForm();

            string aa = string.Empty;
            var a = form.getFields();

            var iterator = a.iterator();

            while (iterator.hasNext())
            {
                try
                {
                    PDFField pdffield = new PDFField();

                    PDField f = (PDField)iterator.next();

                    pdffield.Type = f.getFieldType();
                    pdffield.IsRequired = f.isRequired();
                    pdffield.IsReadOnly = f.isReadonly();
                    pdffield.FullName = f.getFullyQualifiedName();
                    pdffield.AlternativeName = f.getAlternateFieldName();
                    pdffield.PartialName = f.getPartialName();

                    string fieldvalue = string.Empty;

                    // sig field throws not implemented in ver 1.2.1
                    if (includeValues)
                    {
                        try
                        {
                            fieldvalue = f.getValue();
                        }
                        catch (Exception e) { }

                    }

                    if (pdffield.Type == "Sig")
                    {
                        PDSignatureField sig = (PDSignatureField)f;
                        var x = sig.getSignature();
                        if (x != null)
                        {
                            fieldvalue = x.getName();
                        }
                    }

                    pdffield.FieldValue = fieldvalue;

                    pdfFormFields.Add(pdffield.FullName, pdffield);
                }
                catch (Exception e) { }
            }

            return pdfFormFields;
        }

        public bool ContainsSignatures(Dictionary<string, PDFField> fields)
        {
            int count = 0;
            count = fields.Values.Where(p => p.Type.Equals("Sig")).Count();
            if (count > 0)
            {
                return true;
            }
            return false;
        }

               public string GenerateCreateTableSQL(PDFInfo info, Dictionary<string, PDFField> fields)
       {
           string formname = info.Title.Replace(" ", "_");
           StringBuilder sb = new StringBuilder();

           sb.Append("USE [XXXX]");
           sb.Append("\n");
           sb.Append("GO");
           sb.Append("\n");
           sb.Append("SET ANSI_NULLS ON");
           sb.Append("\n");
           sb.Append("GO");
           sb.Append("\n");
           sb.Append("SET QUOTED_IDENTIFIER ON ");
           sb.Append("\n");
           sb.Append("GO ");
           sb.Append("\n");
           sb.Append("CREATE TABLE [dbo].[" + formname + "]( ");
           sb.Append("\n");
           sb.Append("[Id] [uniqueidentifier] NOT NULL, ");
           foreach (KeyValuePair<string, PDFField> field in fields.OrderBy(p => p.Key))
           {
               sb.Append("[" + field.Key.Replace(" ", "_") + "] [nvarchar](500) NULL, ");
               sb.Append("\n");
           }
           sb.Append("\n");
           sb.Append("CONSTRAINT [PK_" + formname + "] PRIMARY KEY CLUSTERED ");
           sb.Append("\n");
           sb.Append("( ");
           sb.Append("\n");
           sb.Append("[Id] ASC ");
           sb.Append("\n");
           sb.Append(")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ");
           sb.Append("\n");
           sb.Append(") ON [PRIMARY] ");
           sb.Append("\n");
           sb.Append("GO ");
           sb.Append("\n");
           sb.Append("ALTER TABLE [dbo].[" + formname + "] ADD  CONSTRAINT [DF_" + formname + "_Id]  DEFAULT (newid()) FOR [Id] ");
           sb.Append("\n");
           sb.Append("GO");
           sb.Append("\n");

           return sb.ToString();

       }

        #endregion PDF Discover

        #region XmlDocument DiscoverSchema()
        /// <summary>
        /// Discovers the schema of the underlying data and services, and then maps the schema into a structure and format which is compliant with the requirements of Service Objects.
        /// </summary>
        /// <returns>An XmlDocument containing the discovered schema in a structure which complies with the requirements of Service Objects.</returns>
        public XmlDocument DiscoverSchema()
        {            
            return null;
        }
        #endregion

        #region TypeMappings GetTypeMappings()
        /// <summary>
        /// Gets the type mappings used to map the underlying data's types to the appropriate K2 SmartObject types.
        /// </summary>
        /// <returns>A TypeMappings object containing the ServiceBroker's type mappings which were previously stored in the service instance configuration.</returns>
        public TypeMappings GetTypeMappings()
        {
            // Lookup and return the type mappings stored in the service instance.
            return (TypeMappings)serviceBroker.Service.ServiceConfiguration[__TypeMappings];
        }
        #endregion

        #region void SetTypeMappings()
        /// <summary>
        /// Sets the type mappings used to map the underlying data's types to the appropriate K2 SmartObject types.
        /// </summary>
        public void SetTypeMappings()
        {
            // Variable declaration.
            TypeMappings map = new TypeMappings();

            // Add type mappings.
            

            // Add the type mappings to the service instance.
            serviceBroker.Service.ServiceConfiguration.Add(__TypeMappings, map);
        }
        #endregion

        #region void Execute(Property[] inputs, RequiredProperties required, Property[] returns, MethodType methodType, ServiceObject serviceObject)
        /// <summary>
        /// Executes the Service Object method and returns any data.
        /// </summary>
        /// <param name="inputs">A Property[] array containing all the allowed input properties.</param>
        /// <param name="required">A RequiredProperties collection containing the required properties.</param>
        /// <param name="returns">A Property[] array containing all the allowed return properties.</param>
        /// <param name="methodType">A MethoType indicating what type of Service Object method was called.</param>
        /// <param name="serviceObject">A ServiceObject containing populated properties for use with the method call.</param>
        public void Execute(Property[] inputs, RequiredProperties required, Property[] returns, MethodType methodType, ServiceObject serviceObject)
        {
            #region Get All Field Values

            if (serviceObject.Methods[0].Name.Equals("getallfieldvalues"))
            {
                serviceObject.Properties.InitResultTable();
                
                // get input field value
                string pdfuri = inputs.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value.ToString();

                PDFInfo info = new PDFInfo();
                Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();
                
                try 
                {
                    PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);
                    info = GetPDFDoucmentInformation(doc);
                    fields = GetPDFFormFields(doc, true);
                    doc.close();
                    doc = null;
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("Error retrieving PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
                }

                returns.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value = pdfuri;

                foreach(Property prop in returns)
                {
                    PDFField fieldvalue = new PDFField();

                    string fullname = string.Empty;

                    object pfn = prop.MetaData.ServiceProperties["pdffullname"];
                    if (pfn == null)
                    {
                        fullname = prop.Name;
                    }
                    else
                    {
                        fullname = pfn.ToString();
                    }

                    if (fields.TryGetValue(fullname, out fieldvalue))
                    {
                        prop.Value = fieldvalue.FieldValue;
                    }
                }


                Type type = typeof(PDFInfo);
                PropertyInfo[] props = type.GetProperties();
                foreach (var p in props)
                {
                    object v = info.GetType().GetProperty(p.Name).GetValue(info, null);
                    if (v != null)
                    {
                        string value = v.ToString();
                        returns.Where(q => q.Name.Equals(p.Name)).First().Value = value;
                    }
                }

                serviceObject.Properties.BindPropertiesToResultTable();

            }

            #endregion Get All Field Values

            #region Contains Signatures

            if (serviceObject.Methods[0].Name.Equals("containssignatures"))
            {
                serviceObject.Properties.InitResultTable();

                // get input field value
                string pdfuri = inputs.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value.ToString();

                PDFInfo info = new PDFInfo();
                Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();

                bool containssigs = false;

                try
                {
                    PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);
                    fields = GetPDFFormFields(doc, true);
                    doc.close();
                    doc = null;
                    containssigs = ContainsSignatures(fields);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error retrieving PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
                }

                returns.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value = pdfuri;
                returns.Where(p => p.Name.Equals("containssignatures")).FirstOrDefault().Value = containssigs;

                serviceObject.Properties.BindPropertiesToResultTable();

            }

            #endregion Contains Signatures

            // not implemented with PDFBox
            #region Contains Unsigned Signatures

            //if (serviceObject.Methods[0].Name.Equals("containsunsignedsignatures"))
            //{
            //    serviceObject.Properties.InitResultTable();

            //    // get input field value
            //    string pdfuri = inputs.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value.ToString();


            //    PDFInfo info = new PDFInfo();
            //    Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();

            //    bool containsunsignedsigs = false;

            //    try
            //    {
            //        throw new Exception("Not implemented");
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(string.Format("Error retrieving PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
            //    }

            //    returns.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value = pdfuri;
            //    returns.Where(p => p.Name.Equals("containsunsignedsignatures")).FirstOrDefault().Value = containsunsignedsigs;

            //    serviceObject.Properties.BindPropertiesToResultTable();

            //}

            #endregion Contains Signatures

            #region Update PDF Field

            if (serviceObject.Methods[0].Name.Equals("updatepdffields"))
            {
                serviceObject.Properties.InitResultTable();

                // get input field value
                string pdfuri = inputs.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value.ToString();
                bool base64 = false;
                string base64temp = inputs.Where(p => p.Name.Equals("returnbase64")).FirstOrDefault().Value.ToString();
                bool.TryParse(base64temp, out base64);

                string workingFolder = serviceBroker.Service.ServiceConfiguration["WorkingFolder"].ToString();
                if (workingFolder.LastIndexOf(@"\") != workingFolder.Length - 1)
                {
                    workingFolder += @"\";
                }
                string filename = Guid.NewGuid().ToString() + ".pdf";
                string workingPath = workingFolder + filename;

                PDFInfo info = new PDFInfo();
                Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();

                Dictionary<string, string> updates = new Dictionary<string, string>();
                foreach (Property prop in inputs)
                {
                    object pfn = prop.MetaData.ServiceProperties["pdffullname"];
                    if (!prop.Name.Equals("pdfuri") && pfn != null)
                    {
                        if (prop.Value != null)
                        {
                            updates.Add(pfn.ToString(), prop.Value.ToString());
                        }
                    }
                }

                if (updates.Count > 0)
                {
                    // call update method
                    try
                    {
                        PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);
                        PDDocumentCatalog documentCatalog = doc.getDocumentCatalog();
                        PDAcroForm acroForm = documentCatalog.getAcroForm();
                        foreach (KeyValuePair<string, string> val in updates)
                        {
                            PDField field = acroForm.getField(val.Key);
                            if (field != null)
                            {
                                field.setValue(val.Value);
                            }
                        }
                        doc.save(workingPath);
                        doc.close();
                        doc = null;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error updateing PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
                    }

                    returns.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value = pdfuri;
                    returns.Where(p => p.Name.Equals("returnpath")).FirstOrDefault().Value = workingPath;
                    returns.Where(p => p.Name.Equals("returnbase64")).FirstOrDefault().Value = base64;

                    // read created doc
                    try
                    {
                        PDDocument doc = Utilities.Utils.GetPDDocument(workingPath);
                        info = GetPDFDoucmentInformation(doc);
                        fields = GetPDFFormFields(doc, true);
                        doc.close();
                        doc = null;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error retrieving PDF document from {0}. Exception: {1}", workingPath, ex.Message));
                    }

                    foreach (Property prop in returns)
                    {
                        PDFField fieldvalue = new PDFField();

                        string fullname = string.Empty;

                        object pfn = prop.MetaData.ServiceProperties["pdffullname"];
                        if (pfn == null)
                        {
                            fullname = prop.Name;
                        }
                        else
                        {
                            fullname = pfn.ToString();
                        }

                        if (fields.TryGetValue(fullname, out fieldvalue))
                        {
                            prop.Value = fieldvalue.FieldValue;
                        }
                    }


                    Type type = typeof(PDFInfo);
                    PropertyInfo[] props = type.GetProperties();
                    foreach (var p in props)
                    {
                        object v = info.GetType().GetProperty(p.Name).GetValue(info, null);
                        if (v != null)
                        {
                            string value = v.ToString();
                            returns.Where(q => q.Name.Equals(p.Name)).First().Value = value;
                        }
                    }

                    // get base64 of file
                    if (base64)
                    {
                        FileStream fs = new FileStream(workingPath, FileMode.Open, FileAccess.Read);
                        byte[] filebytes = new byte[fs.Length];
                        fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                        string encodedData = Convert.ToBase64String(filebytes);
                        returns.Where(p => p.Name.Equals("base64pdf")).FirstOrDefault().Value = encodedData;
                    }

                    serviceObject.Properties.BindPropertiesToResultTable();
                    return;
                }
            }
            
            #endregion Update PDF Field

            #region Generate Create Table SQL

            if (serviceObject.Methods[0].Name.Equals("generatecreatetablesql"))
            {
                serviceObject.Properties.InitResultTable();

                string pdfuri = serviceBroker.Service.ServiceConfiguration["pdfUri"].ToString();

                PDFInfo info = new PDFInfo();
                Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();

                string sql = string.Empty;

                // read created doc
                try
                {
                    PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);
                    info = GetPDFDoucmentInformation(doc);
                    fields = GetPDFFormFields(doc, true);
                    
                    sql = GenerateCreateTableSQL(info, fields);

                    doc.close();
                    doc = null;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error generating create table SQL from {0}. Exception: {1}", pdfuri, ex.Message));
                }

                returns.Where(p => p.Name.Equals("generatedsql")).FirstOrDefault().Value = sql;

                serviceObject.Properties.BindPropertiesToResultTable();

            }

            #endregion Generate Create Table SQL

            #region Copy Form Data to SmartObject

            if (serviceObject.Methods[0].Name.Equals("copyformdatatosmartobject"))
            {
                serviceObject.Properties.InitResultTable();

                string pdfuri = inputs.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value.ToString();
                string smoname = inputs.Where(p => p.Name.Equals("formstoresmartobjectname")).FirstOrDefault().Value.ToString();
                string smometh = inputs.Where(p => p.Name.Equals("formstoremethodname")).FirstOrDefault().Value.ToString();
                string returnprop = inputs.Where(p => p.Name.Equals("returnidpropertyname")).FirstOrDefault().Value.ToString();

                string returnvalue = string.Empty;

                PDFInfo info = new PDFInfo();
                Dictionary<string, PDFField> fields = new Dictionary<string, PDFField>();

                try
                {
                    PDDocument doc = Utilities.Utils.GetPDDocument(pdfUri);
                    info = GetPDFDoucmentInformation(doc);
                    fields = GetPDFFormFields(doc, true);
                    doc.close();
                    doc = null;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error retrieving PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
                }

                string returnId = string.Empty;
                try
                {
                    returnId = Utilities.SmartObjectUtils.CreateDataFromPDFForm(smoname, smometh, returnprop, info, fields);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error updated SmartObject from PDF document from {0}. Exception: {1}", pdfuri, ex.Message));
                }


                returns.Where(p => p.Name.Equals("pdfuri")).FirstOrDefault().Value = pdfuri;
                returns.Where(p => p.Name.Equals("formstoresmartobjectname")).FirstOrDefault().Value = smoname;
                returns.Where(p => p.Name.Equals("formstoremethodname")).FirstOrDefault().Value = smometh;
                returns.Where(p => p.Name.Equals("returnidpropertyname")).FirstOrDefault().Value = returnprop;
                returns.Where(p => p.Name.Equals("returnid")).FirstOrDefault().Value = returnId;

                serviceObject.Properties.BindPropertiesToResultTable();

            }

            #endregion Copy Form Data to SmartObject

           
        }
        #endregion

        #endregion
    }
}