Imports rhapsody2

Public Class Root_Software_Composition

    Inherits SMM_Class

    Public Component_Prototypes As New List(Of Component_Prototype)
    Public Assembly_Connectors As New List(Of Assembly_Connector)

    Private Nb_Conn_By_PPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))
    Private Nb_Conn_By_RPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Component_Prototypes)
            children_list.AddRange(Me.Assembly_Connectors)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        ' Ignore generalizations added to a Root_Software_Composition
        Me.Nb_Base_Class_Ref = 0 ' Could have been set to 1 by SMM_Class
    End Sub


    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Root_Software_Composition(rpy_element)
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_component As RPInstance
        For Each rpy_component In CType(Me.Rpy_Element, RPClass).relations
            If Is_Component_Prototype(CType(rpy_component, RPModelElement)) Then
                Dim component As New Component_Prototype
                Me.Component_Prototypes.Add(component)
                component.Import_From_Rhapsody_Model(Me, CType(rpy_component, RPModelElement))
                component.Set_Owner(Me)
            End If
        Next

        Dim rpy_link As RPLink
        For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
            If Is_Connector_Prototype(CType(rpy_link, RPModelElement)) Then
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
            For Each swc In Me.Component_Prototypes
                Dim swct As Component_Type
                swct = CType(Me.Get_Element_By_Uuid(swc.Type_Ref), Component_Type)
                If Not Me.Needed_Elements.Contains(swct) Then
                    Me.Needed_Elements.Add(swct)
                End If
            Next
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

            For Each port In referenced_swct.Provider_Ports
                nb_conn = CType(Me.Owner, Root_Software_Composition). _
                            Get_PPort_Nb_Connection(Me.UUID, port.UUID)
                If nb_conn = 0 Then
                    Me.Add_Consistency_Check_Information_Item(report,
                        "SWC_3",
                        "Provider_Port " & port.Name & " not connected.")
                End If
            Next

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

    End Sub

End Class