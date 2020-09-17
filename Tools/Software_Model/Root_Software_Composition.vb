Imports rhapsody2

Public Class Root_Software_Composition

    Inherits SMM_Class

    Public Component_Prototypes As List(Of Component_Prototype)
    Public Assembly_Connectors As List(Of Assembly_Connector)

    Private Nb_Conn_By_PPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))
    Private Nb_Conn_By_RPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Component_Prototypes) Then
                children_list.AddRange(Me.Component_Prototypes)
            End If
            If Not IsNothing(Me.Assembly_Connectors) Then
                children_list.AddRange(Me.Assembly_Connectors)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Component_Prototypes = New List(Of Component_Prototype)

        Dim rpy_component As RPInstance

        For Each rpy_component In CType(Me.Rpy_Element, RPClass).relations
            If Is_Component_Prototype(CType(rpy_component, RPModelElement)) Then
                Dim component As New Component_Prototype
                Me.Component_Prototypes.Add(component)
                component.Import_From_Rhapsody_Model(Me, CType(rpy_component, RPModelElement))
                component.Set_Owner(Me)
            End If
        Next

        Me.Assembly_Connectors = New List(Of Assembly_Connector)

        Dim rpy_link As RPLink
        For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
            If Is_Assembly_Connector(CType(rpy_link, RPModelElement)) Then
                Dim connector As New Assembly_Connector
                Me.Assembly_Connectors.Add(connector)
                connector.Import_From_Rhapsody_Model(Me, CType(rpy_link, RPModelElement))
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Root_Software_Composition", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Component_Prototypes.Count < 2 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "COMP_1",
                "Should aggregate at least two Component_Prototypes.")
        End If

        For Each conn In Me.Assembly_Connectors
            Dim p_swc_port As Port = CType(Me.Get_Element_By_Uuid(conn.Provider_Port_Ref), Port)
            Dim r_swc_port As Port = CType(Me.Get_Element_By_Uuid(conn.Requirer_Port_Ref), Port)

            If IsNothing(p_swc_port) Or IsNothing(r_swc_port) Then
                Exit For
            End If

            ' Check connected ports contract.
            If p_swc_port.Contract_Ref <> r_swc_port.Contract_Ref Then
                Dim r_swc As Component_Prototype
                r_swc = CType(
                            Me.Get_Element_By_Uuid(conn.Requirer_Component_Ref), 
                            Component_Prototype)
                r_swc.Add_Consistency_Check_Error_Item(report,
                    "SWC_4",
                    r_swc_port.Name & " shall be linked to a Provider_Port with the same contract.")
            End If

            ' Build dictionaries of assembly connections
            ' These dictionaries will be used for Component_Prototype the consistency check.
            Dim nb_conn As Integer
            Dim nb_conn_by_port As Dictionary(Of Guid, Integer)
            ' Treat the provider Component_Prototype
            If Nb_Conn_By_PPort_By_Component.ContainsKey(conn.Provider_Component_Ref) Then
                nb_conn_by_port = Nb_Conn_By_PPort_By_Component.Item(conn.Provider_Component_Ref)
                If nb_conn_by_port.ContainsKey(p_swc_port.UUID) Then
                    nb_conn = nb_conn_by_port.Item(p_swc_port.UUID)
                    nb_conn_by_port.Item(p_swc_port.UUID) = nb_conn + 1
                Else
                    nb_conn_by_port.Add(p_swc_port.UUID, 1)
                End If
            Else
                nb_conn_by_port = New Dictionary(Of Guid, Integer)
                nb_conn_by_port.Add(p_swc_port.UUID, 1)
                Nb_Conn_By_PPort_By_Component.Add(conn.Provider_Component_Ref, nb_conn_by_port)
            End If
            ' Treat the requirer Component_Prototype
            If Nb_Conn_By_RPort_By_Component.ContainsKey(conn.Requirer_Component_Ref) Then
                nb_conn_by_port = Nb_Conn_By_RPort_By_Component.Item(conn.Requirer_Component_Ref)
                If nb_conn_by_port.ContainsKey(r_swc_port.UUID) Then
                    nb_conn = nb_conn_by_port.Item(r_swc_port.UUID)
                    nb_conn_by_port.Item(r_swc_port.UUID) = nb_conn + 1
                Else
                    nb_conn_by_port.Add(r_swc_port.UUID, 1)
                End If
            Else
                nb_conn_by_port = New Dictionary(Of Guid, Integer)
                nb_conn_by_port.Add(r_swc_port.UUID, 1)
                Nb_Conn_By_RPort_By_Component.Add(conn.Requirer_Component_Ref, nb_conn_by_port)
            End If

        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Function Get_PPort_Nb_Connection(component_uuid As Guid, port_uuid As Guid) As Integer
        Dim nb_conn As Integer = 0
        If Nb_Conn_By_PPort_By_Component.ContainsKey(component_uuid) Then
            Dim nb_conn_by_port = Nb_Conn_By_PPort_By_Component.Item(component_uuid)
            If nb_conn_by_port.ContainsKey(port_uuid) Then
                nb_conn = nb_conn_by_port.Item(port_uuid)
            End If
        End If
        Return nb_conn
    End Function

    Public Function Get_RPort_Nb_Connection(component_uuid As Guid, port_uuid As Guid) As Integer
        Dim nb_conn As Integer = 0
        If Nb_Conn_By_RPort_By_Component.ContainsKey(component_uuid) Then
            Dim nb_conn_by_port = Nb_Conn_By_RPort_By_Component.Item(component_uuid)
            If nb_conn_by_port.ContainsKey(port_uuid) Then
                nb_conn = nb_conn_by_port.Item(port_uuid)
            End If
        End If
        Return nb_conn
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            If Not IsNothing(Me.Component_Prototypes) Then
                For Each swc In Me.Component_Prototypes
                    Dim swct As Component_Type
                    swct = CType(Me.Get_Element_By_Uuid(swc.Type_Ref), Component_Type)
                    If Not Me.Needed_Elements.Contains(swct) Then
                        Me.Needed_Elements.Add(swct)
                    End If
                Next
            End If
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)
            ' The list remains empty because nothing can depend on a Root_Software_Composition.
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        Return 0 ' to be implemented when OS_Task will be modeled.
    End Function

End Class


Public Class Component_Prototype

    Inherits SMM_Object

    Private Owner As Root_Software_Composition = Nothing


    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Sub Set_Owner(parent As Root_Software_Composition)
        Me.Owner = parent
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Prototype", "Object")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Not Me.Type_Ref.Equals(Guid.Empty) Then
            ' Check Ports connections
            Dim port As Port
            Dim nb_conn As Integer

            Dim referenced_swct As Component_Type
            referenced_swct = CType(Me.Get_Element_By_Uuid(Me.Type_Ref), Component_Type)

            If Not IsNothing(referenced_swct.Provider_Ports) Then
                For Each port In referenced_swct.Provider_Ports
                    nb_conn = CType(Me.Owner, Root_Software_Composition). _
                                Get_PPort_Nb_Connection(Me.UUID, port.UUID)
                    If nb_conn = 0 Then
                        Me.Add_Consistency_Check_Information_Item(report,
                            "SWC_3",
                            "Provider_Port " & port.Name & " not connected.")
                    End If
                Next
            End If

            If Not IsNothing(referenced_swct.Requirer_Ports) Then
                For Each port In referenced_swct.Requirer_Ports
                    nb_conn = CType(Me.Owner, Root_Software_Composition). _
                                Get_RPort_Nb_Connection(Me.UUID, port.UUID)
                    If nb_conn = 0 Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "SWC_2",
                            "Requirer_Port " & port.Name & " shall be connected.")
                    ElseIf nb_conn > 1 Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "SWC_2",
                            "Requirer_Port " & port.Name & " shall be connected to only one port.")
                    End If
                Next
            End If

        End If

    End Sub

End Class


Public Class Assembly_Connector

    Inherits Software_Element

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
        automatic_name = requirer_component.name & "_" & requirer_port.name & "_" &
                            provider_component.name & "_" & provider_port.name
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

        rpy_provider_port = rpy_link.toPort
        rpy_requirer_port = rpy_link.fromPort
        If Not IsNothing(rpy_provider_port) And Not IsNothing(rpy_requirer_port) Then
            If Is_Provider_Port(CType(rpy_provider_port, RPModelElement)) Then
                rpy_requirer_port = rpy_link.fromPort
                rpy_provider_component = rpy_link.to
                rpy_requirer_component = rpy_link.from
            Else
                rpy_provider_port = rpy_link.fromPort
                rpy_requirer_port = rpy_link.toPort
                rpy_provider_component = rpy_link.from
                rpy_requirer_component = rpy_link.to
            End If

            Me.Provider_Component_Ref = Transform_Rpy_GUID_To_Guid(rpy_provider_component.GUID)
            Me.Provider_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_provider_port.GUID)
            Me.Requirer_Component_Ref = Transform_Rpy_GUID_To_Guid(rpy_requirer_component.GUID)
            Me.Requirer_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_requirer_port.GUID)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Link"
    End Function

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

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Assembly_Connector", "Link")
    End Sub

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
            Me.Add_Consistency_Check_Error_Item(report,
                "CONN_1",
                "Shall link two different components.")
        End If
    End Sub

End Class
