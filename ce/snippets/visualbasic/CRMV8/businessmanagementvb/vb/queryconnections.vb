' <snippetqueryconnections>


Imports System.ServiceModel
Imports System.ServiceModel.Description

' These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
' found in the SDK\bin folder.
Imports Microsoft.Xrm.Sdk
Imports Microsoft.Xrm.Sdk.Client
Imports Microsoft.Xrm.Sdk.Query
Imports Microsoft.Xrm.Sdk.Discovery
Imports Microsoft.Xrm.Sdk.Messages

' This namespace is found in Microsoft.Crm.Sdk.Proxy.dll assembly
' found in the SDK\bin folder.
Imports Microsoft.Crm.Sdk.Messages

Namespace Microsoft.Crm.Sdk.Samples
    ''' <summary>
    ''' This Sample shows how to query connections.
    ''' </summary>
    Public Class QueryConnections
        #Region "Class Level Members"

        ''' <summary>
        ''' Stores the organization service proxy.
        ''' </summary>
        Private _serviceProxy As OrganizationServiceProxy

        ' Define the IDs needed for this sample.
        Public _connectionRoleId As Guid
        Public _account1Id As Guid
        Public _account2Id As Guid
        Public _contactId As Guid
        Public _connection1Id As Guid
        Public _connection2Id As Guid

        #End Region ' Class Level Members

        #Region "How To Sample Code"
        ''' <summary>
        ''' Create and configure the organization service proxy.
        ''' Call the method to create any data that this sample requires.
        ''' Query the connections.
        ''' Optionally delete any entity records that were created for this sample.
        ''' </summary>
                ''' <param name="serverConfig">Contains server connection information.</param>
        ''' <param name="promptforDelete">When True, the user will be prompted to delete all
        ''' created entities.</param>
        Public Sub Run(ByVal serverConfig As ServerConnection.Configuration, ByVal promptForDelete As Boolean)
            Try
                ' Connect to the Organization service. 
                ' The using statement assures that the service proxy will be properly disposed.
                _serviceProxy = ServerConnection.GetOrganizationProxy(serverConfig)
                Using _serviceProxy
                    ' This statement is required to enable early-bound type support.
                    _serviceProxy.EnableProxyTypes()


                    ' Call the method to create any data that this sample requires.
                    CreateRequiredRecords()

                    ' This query retrieves all connections this contact is part of.
                    Dim query As QueryExpression = New QueryExpression With {.EntityName = Connection.EntityLogicalName, _
                                                                             .ColumnSet = New ColumnSet("connectionid")}
                    query.Criteria = New FilterExpression()
                    query.Criteria.AddCondition("record1id", ConditionOperator.Equal, {_contactId})
                    query.Criteria.FilterOperator = LogicalOperator.And
                            ' You can safely query against only record1id or
                            ' record2id - CRM will find all connections this 
                            ' entity is a part of either way.

                    Dim results As EntityCollection = _serviceProxy.RetrieveMultiple(query)

                    ' TODO: Here you could do a variety of tasks with the 
                    ' connections retrieved, such as listing the connected entities,
                    ' finding reciprocal connections, etc.

                    Console.WriteLine("Retrieved {0} connectionrole instances.", results.Entities.Count)

                    DeleteRequiredRecords(promptForDelete)

                End Using
            ' Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            Catch fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                ' You can handle an exception here or pass it back to the calling method.
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' This method creates any entity records that this sample requires.
        ''' Create a new connectionrole instance. 
        ''' </summary>
        Public Sub CreateRequiredRecords()
            ' Define some anonymous types to define the range 
            ' of possible connection property values.
            Dim Categories = New With {Key .Business = 1, Key .Family = 2, Key .Social = 3, Key .Sales = 4, Key .Other = 5}

            ' Create a Connection Role
            Dim setupConnectionRole As ConnectionRole = New ConnectionRole With { _
                .Name = "Example Connection Role", .Description = "This is an example one sided connection role.", _
                .Category = New OptionSetValue(Categories.Business)}

            _connectionRoleId = _serviceProxy.Create(setupConnectionRole)
            Console.WriteLine("Created {0}.", setupConnectionRole.Name)

            ' Create a related Connection Role Object Type Code record for Account
            Dim newAccountConnectionRoleTypeCode As ConnectionRoleObjectTypeCode = _
                New ConnectionRoleObjectTypeCode With {.ConnectionRoleId = _
                    New EntityReference(ConnectionRole.EntityLogicalName, _connectionRoleId), _
                                                       .AssociatedObjectTypeCode = Account.EntityLogicalName}

            _serviceProxy.Create(newAccountConnectionRoleTypeCode)
            Console.WriteLine("Created a related Connection Role Object Type Code record for Account.")

            ' Create a related Connection Role Object Type Code record for Contact
            Dim newContactConnectionRoleTypeCode As ConnectionRoleObjectTypeCode = _
                New ConnectionRoleObjectTypeCode With {.ConnectionRoleId = _
                    New EntityReference(ConnectionRole.EntityLogicalName, _connectionRoleId), _
                                                       .AssociatedObjectTypeCode = Contact.EntityLogicalName}

            _serviceProxy.Create(newContactConnectionRoleTypeCode)
            Console.WriteLine("Created a related Connection Role Object Type Code record for Contact.")

            'Create a few account records for use in the connections.
            Dim setupAccount1 As Account = New Account With {.Name = "Example Account 1"}
            _account1Id = _serviceProxy.Create(setupAccount1)
            Console.WriteLine("Created {0}.", setupAccount1.Name)

            Dim setupAccount2 As Account = New Account With {.Name = "Example Account 2"}
            _account2Id = _serviceProxy.Create(setupAccount2)
            Console.WriteLine("Created {0}.", setupAccount2.Name)

            'Creates a contact used in the connection.
            Dim setupContact As Contact = New Contact With {.LastName = "Example Contact"}
            _contactId = _serviceProxy.Create(setupContact)
            Console.WriteLine("Created {0}.", setupContact.LastName)

            ' Create a new connection between Account 1 and the contact record.
            Dim newConnection1 As Connection = New Connection With {.Record1Id = _
                New EntityReference(Account.EntityLogicalName, _account1Id), _
                    .Record1RoleId = New EntityReference(ConnectionRole.EntityLogicalName, _connectionRoleId), _
                    .Record2Id = New EntityReference(Contact.EntityLogicalName, _contactId)}

            _connection1Id = _serviceProxy.Create(newConnection1)

            Console.WriteLine("Created a connection between the account 1 and the contact.")

            ' Create a new connection between the contact and Account 2 record
            Dim newConnection2 As Connection = New Connection With { _
                .Record1Id = New EntityReference(Contact.EntityLogicalName, _contactId), _
                .Record1RoleId = New EntityReference(ConnectionRole.EntityLogicalName, _connectionRoleId), _
                .Record2Id = New EntityReference(Account.EntityLogicalName, _account2Id)}

            _connection2Id = _serviceProxy.Create(newConnection2)

            Console.WriteLine("Created a connection between the contact and the account 2.")

            Return
        End Sub

        ''' <summary>
        ''' Deletes any entity records that were created for this sample.
        ''' <param name="prompt">Indicates whether to prompt the user 
        ''' to delete the records created in this sample.</param>
        ''' </summary>
        Public Sub DeleteRequiredRecords(ByVal prompt As Boolean)
            Dim deleteRecords As Boolean = True

            If prompt Then
                Console.WriteLine(vbLf &amp; "Do you want these entity records deleted? (y/n)")
                Dim answer As String = Console.ReadLine()

                deleteRecords = (answer.StartsWith("y") OrElse answer.StartsWith("Y"))
            End If

            If deleteRecords Then
                _serviceProxy.Delete(Connection.EntityLogicalName, _connection1Id)
                _serviceProxy.Delete(Connection.EntityLogicalName, _connection2Id)
                _serviceProxy.Delete(Account.EntityLogicalName, _account1Id)
                _serviceProxy.Delete(Account.EntityLogicalName, _account2Id)
                _serviceProxy.Delete(Contact.EntityLogicalName, _contactId)
                _serviceProxy.Delete(ConnectionRole.EntityLogicalName, _connectionRoleId)

                Console.WriteLine("Entity records have been deleted.")
            End If
        End Sub

        #End Region ' How To Sample Code

        #Region "Main"
        ''' <summary>
        ''' Standard Main() method used by most SDK samples.
        ''' </summary>
        ''' <param name="args"></param>
        Public Shared Sub Main(ByVal args() As String)
            Try
                ' Obtain the target organization's Web address and client logon 
                ' credentials from the user.
                Dim serverConnect As New ServerConnection()
                Dim config As ServerConnection.Configuration = serverConnect.GetServerConfiguration()

                Dim app As New QueryConnections()
                app.Run(config, True)

            Catch ex As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp)
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode)
                Console.WriteLine("Message: {0}", ex.Detail.Message)
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
            Catch ex As TimeoutException
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Message: {0}", ex.Message)
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.InnerException.Message, "No Inner Fault", ex.InnerException.Message))
            Catch ex As Exception
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine(ex.Message)

                ' Display the details of the inner exception.
                If ex.InnerException IsNot Nothing Then
                    Console.WriteLine(ex.InnerException.Message)

                    Dim fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault) = _
                        TryCast(ex.InnerException, FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault))
                    If fe IsNot Nothing Then
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp)
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode)
                        Console.WriteLine("Message: {0}", fe.Detail.Message)
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText)
                        Console.WriteLine("Inner Fault: {0}", If(Nothing Is fe.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
                    End If
                End If
            ' Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            ' SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            Finally

                Console.WriteLine("Press <Enter> to exit.")
                Console.ReadLine()
            End Try

        End Sub
        #End Region ' Main
    End Class
End Namespace

' </snippetqueryconnections>