Imports rhapsody2

Module Software_Metamodel

    '----------------------------------------------------------------------------------------------'
    Private Function Is_Element_Of_Sterotype(
        ByVal rpy_mdl_elmt As RPModelElement,
        ByVal stereotype_guid As String) _
        As Boolean

        ' Test 'main' stereotype
        If Not rpy_mdl_elmt.stereotype Is Nothing Then
            If rpy_mdl_elmt.stereotype.GUID = stereotype_guid Then
                Return True
            End If
        End If

        ' Test other stereotypes
        Dim rpy_stereotype As RPStereotype
        For Each rpy_stereotype In rpy_mdl_elmt.stereotypes
            If rpy_stereotype.GUID = stereotype_guid Then
                Return True
            End If
        Next

        ' No stereotype with given GUID found 
        Return False

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Structure
    Public Function Is_Software_Package(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID afb0e42d-a399-4819-bb39-46f0fd0f0fbf")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Shared
    Public Function Is_Operation_Argument(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a861ee16-1a2a-4570-98f4-5212221dd68d")
    End Function

    Public Function Is_Configuration_Parameter(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID c7a97ee0-e497-4c57-aaf1-4aefdce59a69")
    End Function

    Public Function Is_Private_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID f83e5855-ba0a-49b4-a839-2d1cddc54bd8")
    End Function

    Public Function Is_Variable_Attribute(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 1ab5b469-21ae-4135-9ec4-5adfcbba6a24")
    End Function

    Public Function Is_Sent_Event(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 8d3c01aa-c9f5-4df6-91f9-66e5fd75fd0e")
    End Function

    Public Function Is_Received_Event(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID bbc84abc-ada7-4082-98d4-c7ae4bd0d743")
    End Function

    Public Function Is_Connector_Prototype(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 99befb32-f0c3-413e-9f84-63c4ee447063")
    End Function

    Public Function Is_Operation_Delegation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID e7cfdec5-4b56-48d6-b81c-e078cb8c8158")
    End Function

    '----------------------------------------------------------------------------------------------'
    ' Data_Types
    Public Function Is_Data_Type(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID e059166f-c470-4144-ae7d-af8a82db546f")
    End Function

    Public Function Is_Physical_Data_Type(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 1a975650-21c7-4a3e-bdf1-c167b2560040")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Client_Server_Interfaces
    Public Function Is_Client_Server_Interface(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 74d15759-da33-4217-9115-4cad66849eff")
    End Function

    Public Function Is_Synchronous_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 739c3d19-e20b-4e24-bab1-ec51826db9b5")
    End Function

    Public Function Is_Asynchronous_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID cb10b39d-04db-49fa-8fc9-89760eff9508")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Event_Interfaces
    Public Function Is_Event_Interface(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a606fb06-732a-41c1-b270-a594f4ed47dd")
    End Function

    Public Function Is_Event_Argument(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 7d66c690-e491-496e-a465-ceae11d2b120")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Component_Types
    Public Function Is_Component_Type(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 9f000df0-3104-4bba-93b4-e2b6a64d3833")
    End Function

    Public Function Is_Requirer_Port(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID ab30ddc9-d03d-4ece-bea7-881e4753d8f7")
    End Function

    Public Function Is_Provider_Port(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 220d63ab-1b03-4cbb-83e1-4160a66b0389")
    End Function

    Public Function Is_OS_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 03e6f281-8593-48b0-a435-0fc8687783f6")
    End Function

    Public Function Is_Component_Type_Part(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 585f15bd-1f35-404f-8e57-1aa4bee9e1a5")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Root_Software_Composition
    Public Function Is_Root_Software_Composition(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a5bc8b23-148b-45cb-b210-3def4059fcd0")
    End Function

    Public Function Is_Component_Prototype(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 9ab1904a-0588-4eac-9f37-c6224bbd3de7")
    End Function

    Public Function Is_OS_Task(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID e4ca771f-e5e8-4770-97a8-5197730fb880")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Component_Design
    Public Function Is_Component_Design(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 5cef8605-f11b-45b0-9267-4db1d6e495d6")
    End Function

    Public Function Is_Component_Type_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 6a9f6eaa-cb72-4f60-98f9-0b0e21de73e0")
    End Function

    Public Function Is_Operation_Realization(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID d92413ce-606f-470d-927e-c71200c04793")
    End Function

    Public Function Is_Provider_Port_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a9743e19-c009-49cd-a592-1e4f0eb1be92")
    End Function

    Public Function Is_Operation_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 0cbb1e61-9891-48a6-8b90-26f789cc116a")
    End Function

    Public Function Is_Event_Reception_Realization(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID ce5b0aaa-6e4a-4df3-b351-42c8dad0b421")
    End Function

    Public Function Is_Requirer_Port_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 3c9476aa-db4c-4a17-9581-c47651b0862a")
    End Function

    Public Function Is_Internal_Design_Object(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 7318acee-3a17-4e05-959f-405011302abe")
    End Function

    Public Function Is_Callback_Realization(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 6906ea4f-d16f-41ee-b27e-ee87bb0fbbb2")
    End Function

    Public Function Is_Asynchronous_Operation_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 13fe3793-e4b9-4d17-943c-95086ec0d82f")
    End Function

    Public Function Is_OS_Operation_Realization(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 7b643216-acd0-469f-93ee-d2719e8f1dcf")
    End Function

    Public Function Is_OS_Operation_Ref(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID f1857119-9a2b-494f-96f5-8cf47bdb1daa")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Software_Class
    Public Function Is_Internal_Design_Class(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 9db356bc-6146-4703-a6f1-f05eb819dbfa")
    End Function

    Public Function Is_Public_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID ecaa2fe0-5d3b-4cc3-9862-9159c7ea16d8")
    End Function

    Public Function Is_Event_Reception(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 92c0d054-4885-4324-a809-168f50241810")
    End Function

    Public Function Is_Realized_Interface(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID b8d7b10a-4172-405e-b4b5-868172505c54")
    End Function

    Public Function Is_Needed_Interface(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID c4d77dea-4713-4da2-b474-305d676af411")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Implementation_File
    Public Function Is_Implementation_File(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 76068085-5ce8-416c-81a0-105fa38a041d")
    End Function

    Public Function Is_Implemented_Element(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID e1410ed5-8b64-43ee-a054-e77fd03a4de8")
    End Function

End Module
