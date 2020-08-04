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
    Public Function Is_PSWA_Package(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID afb0e42d-a399-4819-bb39-46f0fd0f0fbf")
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

    Public Function Is_Operation_Argument(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a861ee16-1a2a-4570-98f4-5212221dd68d")
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

    Public Function Is_Component_Operation(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 03e6f281-8593-48b0-a435-0fc8687783f6")
    End Function

    Public Function Is_Component_Configuration(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID c7a97ee0-e497-4c57-aaf1-4aefdce59a69")
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Root_Software_Composition
    Public Function Is_Root_Software_Composition(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID a5bc8b23-148b-45cb-b210-3def4059fcd0")
    End Function

    Public Function Is_Component_Prototype(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 9ab1904a-0588-4eac-9f37-c6224bbd3de7")
    End Function

    Public Function Is_Assembly_Connector(ByVal model_element As RPModelElement) As Boolean
        Return Is_Element_Of_Sterotype(model_element, "GUID 99befb32-f0c3-413e-9f84-63c4ee447063")
    End Function


End Module
