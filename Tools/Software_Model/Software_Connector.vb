Imports rhapsody2

Public MustInherit Class Software_Connector
    Inherits Software_Element

    '----------------------------------------------------------------------------------------------'
    ' General methods


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Link"
    End Function

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Connector_Prototype", "Link")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        ' Do not call MyBase.Check_Own_Consistency to avoid to rise rule Elemt_5 for connector.

        If Not Is_Symbol_Valid(Me.Name) Then
            Me.Add_Consistency_Check_Error_Item(report, "ELMT_2",
                "Invalid symbol, expression shall match ^[a-zA-Z][a-zA-Z0-9_]+$.")
        End If
       
    End Sub

End Class

Public Class Assembly_Connector

    Inherits Software_Connector

    Public Provider_Component_Ref As Guid = Nothing
    Public Provider_Port_Ref As Guid = Nothing
    Public Requirer_Component_Ref As Guid = Nothing
    Public Requirer_Port_Ref As Guid = Nothing

    ' Used for model merge
    Private Rpy_Prov_Port As RPPort = Nothing
    Private Rpy_Req_Port As RPPort = Nothing
    Private Rpy_Prov_Comp As RPInstance = Nothing
    Private Rpy_Req_Comp As RPInstance = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Shared Function Is_Assembly_Connector(rpy_link As RPLink) As Boolean
        If Not IsNothing(rpy_link.fromPort) And Not IsNothing(rpy_link.toPort) _
           And Not IsNothing(rpy_link.from) And Not IsNothing(rpy_link.to) _
           And Not IsNothing(rpy_link.fromElement) And Not IsNothing(rpy_link.toElement) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function Compute_Automatic_Name(rpy_link As RPLink) As String
        Dim automatic_name As String
        Dim provider_port As RPPort = Nothing
        Dim requirer_port As RPPort = Nothing
        Dim provider_component As RPInstance = Nothing
        Dim requirer_component As RPInstance = Nothing
        Assembly_Connector.Get_Connector_Info(
            rpy_link,
            provider_port,
            requirer_port,
            provider_component,
            requirer_component)
        automatic_name = requirer_component.name & "__" & requirer_port.name & "__" &
                            provider_component.name & "__" & provider_port.name
        Return automatic_name
    End Function

    Public Shared Sub Get_Connector_Info(
        rpy_link As RPLink,
        ByRef provider_port As RPPort,
        ByRef requirer_port As RPPort,
        ByRef provider_component As RPInstance,
        ByRef requirer_component As RPInstance)
        provider_port = rpy_link.toPort
        If Is_Provider_Port(CType(provider_port, RPModelElement)) Then
            requirer_port = rpy_link.fromPort
            provider_component = rpy_link.to
            requirer_component = rpy_link.from
        Else
            provider_port = rpy_link.fromPort
            requirer_port = rpy_link.toPort
            provider_component = rpy_link.from
            requirer_component = rpy_link.to
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Get the UUID of ports and components
        Dim rpy_provider_port As RPPort = Nothing
        Dim rpy_requirer_port As RPPort = Nothing
        Dim rpy_provider_component As RPInstance = Nothing
        Dim rpy_requirer_component As RPInstance = Nothing

        Dim rpy_link As RPLink = CType(Me.Rpy_Element, RPLink)

        Assembly_Connector.Get_Connector_Info(
            rpy_link,
            rpy_provider_port,
            rpy_requirer_port,
            rpy_provider_component,
            rpy_requirer_component)
        Me.Provider_Component_Ref = Transform_Rpy_GUID_To_Guid(rpy_provider_component.GUID)
        Me.Provider_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_provider_port.GUID)
        Me.Requirer_Component_Ref = Transform_Rpy_GUID_To_Guid(rpy_requirer_component.GUID)
        Me.Requirer_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_requirer_port.GUID)

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_link As RPLink = Nothing

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Prov_Port = CType(Me.Find_In_Rpy_Project(Me.Provider_Port_Ref), RPPort)
        Me.Rpy_Req_Port = CType(Me.Find_In_Rpy_Project(Me.Requirer_Port_Ref), RPPort)
        Me.Rpy_Prov_Comp = CType(Me.Find_In_Rpy_Project(Me.Provider_Component_Ref), RPInstance)
        Me.Rpy_Req_Comp = CType(Me.Find_In_Rpy_Project(Me.Requirer_Component_Ref), RPInstance)

        If Not IsNothing(Me.Rpy_Prov_Port) And
            Not IsNothing(Me.Rpy_Req_Port) And
            Not IsNothing(Me.Rpy_Req_Comp) And
            Not IsNothing(Me.Rpy_Prov_Comp) Then
            rpy_link = rpy_parent_class.addLink(
                Me.Rpy_Prov_Comp,
                Me.Rpy_Req_Comp,
                Nothing,
                Me.Rpy_Prov_Port,
                Me.Rpy_Req_Port)
        End If
        Return CType(rpy_link, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
            rpy_elmt.name = Me.Name
        Else
            If IsNothing(Me.Rpy_Prov_Port) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Provider_Port not found : " & Me.Provider_Port_Ref.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Req_Port) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Requirer_Port not found : " & Me.Requirer_Port_Ref.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Req_Comp) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Provider_Component not found : " _
                    & Me.Provider_Component_Ref.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Req_Comp) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Requirer_Component not found : " _
                    & Me.Requirer_Component_Ref.ToString & ".")
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Provider_Component_Ref = Me.Requirer_Component_Ref Then
            Me.Add_Consistency_Check_Error_Item(report, "ASSBL_1",
                "Shall link two different objects.")
        Else
            Dim rport As Port = CType(Get_Element_By_Uuid(Me.Requirer_Port_Ref), Port)
            Dim pport As Port = CType(Get_Element_By_Uuid(Me.Provider_Port_Ref), Port)
            If Not (IsNothing(rport) And IsNothing(pport)) Then
                If rport.GetType = pport.GetType Then
                    Me.Add_Consistency_Check_Error_Item(report, "ASSBL_2",
                        "Linked ports are of the same kind.")
                Else
                    If rport.Contract_Ref <> pport.Contract_Ref Then
                        Me.Add_Consistency_Check_Error_Item(report, "ASSBL_3",
                            "Linked ports do not reference the same interface.")
                    End If
                End If
            End If
        End If
    End Sub

End Class