using System;
using System.Collections.Generic;
using System.Transactions;
using System.Text;

using SourceCode.SmartObjects.Services.ServiceSDK;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;

using K2Field.SmartObject.Services.PDFiTextSharp.Data;
using K2Field.SmartObject.Services.PDFiTextSharp.Interfaces;

namespace K2Field.SmartObject.Services.PDFiTextSharp.ServiceBrokers
{
    /// <summary>
    /// A ServiceBroker responsible for brokering communications between the K2 platform and an underlying system or technology.
    /// </summary>
    class ServiceBroker : ServiceAssemblyBase
    {
        #region Default Constructor
        /// <summary>
        /// Instantiates a new ServiceBroker.
        /// </summary>
        public ServiceBroker()
        {
            // No implementation necessary.
        }
        #endregion

        #region Methods

        #region Service Interaction Methods

        #region override string DescribeSchema()
        /// <summary>
        /// Describes the schema of the underlying data and services to the K2 platform.
        /// </summary>
        /// <returns>A string containing the schema XML.</returns>
        public override string DescribeSchema()
        {
            try
            {
                // Makes better use of resources and avoids any unnecessary open connections to data sources.
                using (DataConnector connector = new DataConnector(this))
                {
                    // Get the configuration from the service instance.
                    connector.GetConfiguration();
                    // Set the type mappings.
                    connector.SetTypeMappings();
                    // Describe the schema.
                    connector.DescribeSchema();
                    // Set up the service instance.
                    connector.SetupService();
                }

                // Indicate that the operation was successful.
                ServicePackage.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                // Record the exception message and indicate that this was an error.
                ServicePackage.ServiceMessages.Add(ex.Message, MessageSeverity.Error);
                // Indicate that the operation was unsuccessful.
                ServicePackage.IsSuccessful = false;
            }

            return base.DescribeSchema();
        }
        #endregion

        #region override string GetConfigSection()
        /// <summary>
        /// Sets up the required configuration parameters in the service instance. When a new service instance is registered for this ServiceBroker, the configuration parameters are surfaced to the appropriate tooling. The configuration parameters are provided by the person registering the service instance.
        /// </summary>
        /// <returns>A string containing the configuration XML.</returns>
        public override string GetConfigSection()
        {
            try
            {
                // Makes better use of resources and avoids any unnecessary open connections to data sources.
                using (DataConnector connector = new DataConnector(this))
                {
                    // Set up the required parameters in the service instance.
                    connector.SetupConfiguration();
                }
            }
            catch (Exception ex)
            {
                // Record the exception message and indicate that this was an error.
                ServicePackage.ServiceMessages.Add(ex.Message, MessageSeverity.Error);
            }

            return base.GetConfigSection();
        }
        #endregion

        #region override void Execute()
        /// <summary>
        /// Executes the Service Object method and returns any data.
        /// </summary>
        public override void Execute()
        {
            try
            {
                // Makes better use of resources and avoids any unnecessary open connections to data sources.
                using (DataConnector connector = new DataConnector(this))
                {
                    // Get the configuration from the service instance.
                    connector.GetConfiguration();

                    // Get the populated Service Object definition.
                    ServiceObject serviceObject = Service.ServiceObjects[0];
                    // Get the method to execute.
                    Method method = serviceObject.Methods[0];

                    // InputProperties and ReturnProperties are string collections, create property collections for later ease-of-use.
                    Property[] inputs = new Property[method.InputProperties.Count];
                    Property[] returns = new Property[method.ReturnProperties.Count];

                    for (int i = 0; i < method.InputProperties.Count; i++)
                    {
                        inputs[i] = serviceObject.Properties[method.InputProperties[i]];
                    }

                    for (int i = 0; i < method.ReturnProperties.Count; i++)
                    {
                        returns[i] = serviceObject.Properties[method.ReturnProperties[i]];
                    }

                    // Execute the Service Object method and return any data.
                    connector.Execute(inputs, method.Validation.RequiredProperties, returns, method.Type, serviceObject);
                }

                // Indicate that the operation was successful.
                ServicePackage.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                // Record the exception message and indicate that this was an error.
                ServicePackage.ServiceMessages.Add(ex.Message, MessageSeverity.Error);
                // Indicate that the operation was unsuccessful.
                ServicePackage.IsSuccessful = false;
            }
        }
        #endregion

        #region override void Extend()
        /// <summary>
        /// Extends the underlying system or technology's schema. Only implemented for K2 SmartBox.
        /// </summary>
        public override void Extend()
        {
            try
            {
                throw new NotImplementedException("Service Object \"Extend()\" is not implemented.");
            }
            catch (Exception ex)
            {
                // Record the exception message and indicate that this was an error.
                ServicePackage.ServiceMessages.Add(ex.Message, MessageSeverity.Error);
                // Indicate that the operation was unsuccessful.
                ServicePackage.IsSuccessful = false;
            }
        }
        #endregion

        #endregion

        #region Transaction Support Methods

        #region void Commit(Enlistment enlistment)
        /// <summary>
        /// Responds to the Commit notification.
        /// </summary>
        /// <param name="enlistment">An Enlistment that facilitates communication between the enlisted transaction participant and the transaction manager during the final phase of the transaction.</param>
        public override void Commit(Enlistment enlistment)
        {
            if (enlistment != null)
            {
                // Indicate that the transaction participant has completed its work.
                enlistment.Done();
            }
        }
        #endregion

        #region void InDoubt(Enlistment enlistment)
        /// <summary>
        /// Responds to the InDoubt notification.
        /// </summary>
        /// <param name="enlistment">An Enlistment that facilitates communication between the enlisted transaction participant and the transaction manager during the final phase of the transaction.</param>
        public override void InDoubt(Enlistment enlistment)
        {
            if (enlistment != null)
            {
                // Indicate that the transaction participant has completed its work.
                enlistment.Done();
            }
        }
        #endregion

        #region void Prepare(PreparingEnlistment preparingEnlistment)
        /// <summary>
        /// Responds to the Prepare notification.
        /// </summary>
        /// <param name="preparingEnlistment">An Enlistment that facilitates communication between the enlisted transaction participant and the transaction manager during the Prepare phase of the transaction.</param>
        public override void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Allow the base class to handle the Prepare notification.
            base.Prepare(preparingEnlistment);
        }
        #endregion

        #region void Rollback(Enlistment enlistment)
        /// <summary>
        /// Responds to the Rollback notification.
        /// </summary>
        /// <param name="enlistment">An Enlistment that facilitates communication between the enlisted transaction participant and the transaction manager during the final phase of the transaction.</param>
        public override void Rollback(Enlistment enlistment)
        {
            if (enlistment != null)
            {
                // Indicate that the transaction participant has completed its work.
                enlistment.Done();
            }
        }
        #endregion

        #endregion

        #endregion
    }
}