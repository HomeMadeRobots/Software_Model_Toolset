Imports rhapsody2

Public Class PSWA_Package

    Inherits Software_Element

    Public PSWA_Packages As List(Of PSWA_Package)
    <Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Enumerated_Data_Type)), _
     Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Array_Data_Type)), _
     Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Physical_Data_Type)), _
     Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Structured_Data_Type)), _
     Global.System.Xml.Serialization.XmlArray("Data_Types")>
    Public Data_Types As List(Of Data_Type)
    Public Client_Server_Interfaces As List(Of Client_Server_Interface)
    Public Event_Interfaces As List(Of Event_Interface)
    Public Component_Types As List(Of Component_Type)
    Public Root_Software_Compositions As List(Of Root_Software_Composition)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As New List(Of Software_Element)
        If Not IsNothing(Me.PSWA_Packages) Then
            For Each pkg In Me.PSWA_Packages
                children.Add(pkg)
            Next
        End If
        If Not IsNothing(Me.Data_Types) Then
            For Each dt In Me.Data_Types
                children.Add(dt)
            Next
        End If
        If Not IsNothing(Me.Client_Server_Interfaces) Then
            For Each cs_if In Me.Client_Server_Interfaces
                children.Add(cs_if)
            Next
        End If
        If Not IsNothing(Me.Event_Interfaces) Then
            For Each ev_if In Me.Event_Interfaces
                children.Add(ev_if)
            Next
        End If
        If Not IsNothing(Me.Component_Types) Then
            For Each swct In Me.Component_Types
                children.Add(swct)
            Next
        End If
        If Not IsNothing(Me.Root_Software_Compositions) Then
            For Each compo In Me.Root_Software_Compositions
                children.Add(compo)
            Next
        End If
        Return children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.PSWA_Packages = New List(Of PSWA_Package)
        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPPackage).packages
            If Is_PSWA_Package(CType(rpy_pkg, RPModelElement)) Then
                Dim pswa_pkg As PSWA_Package = New PSWA_Package
                Me.PSWA_Packages.Add(pswa_pkg)
                pswa_pkg.Import_From_Rhapsody_Model(Me, CType(rpy_pkg, RPModelElement))
            End If
        Next
        If Me.PSWA_Packages.Count = 0 Then
            Me.PSWA_Packages = Nothing
        End If

        Me.Data_Types = New List(Of Data_Type)
        Dim rpy_type As RPType
        For Each rpy_type In CType(Me.Rpy_Element, RPPackage).types
            If Is_Data_Type(CType(rpy_type, RPModelElement)) Then
                Dim type_kind As String
                type_kind = rpy_type.kind
                Select type_kind
                Case "Enumeration"
                    Dim enumeration As Enumerated_Data_Type
                    enumeration = New Enumerated_Data_Type
                    Me.Data_Types.Add(enumeration)
                    enumeration.Import_From_Rhapsody_Model(Me, CType(rpy_type, RPModelElement))
                Case "Typedef"
                    Dim typedef As Array_Data_Type
                    typedef = New Array_Data_Type
                    Me.Data_Types.Add(typedef)
                    typedef.Import_From_Rhapsody_Model(Me, CType(rpy_type, RPModelElement))
                Case "Structure"
                    Dim struct As Structured_Data_Type
                    struct = New Structured_Data_Type
                    Me.Data_Types.Add(struct)
                    struct.Import_From_Rhapsody_Model(Me, CType(rpy_type, RPModelElement))
                End Select
            ElseIf Is_Physical_Data_Type(CType(rpy_type, RPModelElement)) Then
                Dim type_kind As String
                type_kind = rpy_type.kind
                Select Case type_kind
                Case "Typedef"
                    Dim phys_type As Physical_Data_Type
                    phys_type = New Physical_Data_Type
                    Me.Data_Types.Add(phys_type)
                    phys_type.Import_From_Rhapsody_Model(Me, CType(rpy_type, RPModelElement))
                End Select
            End If
        Next
        If Me.Data_Types.Count = 0 Then
            Me.Data_Types = Nothing
        End If

        Me.Client_Server_Interfaces = New List(Of Client_Server_Interface)
        Me.Event_Interfaces = New List(Of Event_Interface)
        Me.Component_Types = New List(Of Component_Type)
        Me.Root_Software_Compositions = New List(Of Root_Software_Composition)
        Dim rpy_class As RPClass
        For Each rpy_class In CType(Me.Rpy_Element, RPPackage).classes
            If Is_Client_Server_Interface(CType(rpy_class, RPModelElement)) Then
                Dim cs_if As Client_Server_Interface = New Client_Server_Interface
                Me.Client_Server_Interfaces.Add(cs_if)
                cs_if.Import_From_Rhapsody_Model(Me, CType(rpy_class, RPModelElement))
            ElseIf Is_Event_Interface(CType(rpy_class, RPModelElement)) Then
                Dim event_interface As Event_Interface = New Event_Interface
                Me.Event_Interfaces.Add(event_interface)
                event_interface.Import_From_Rhapsody_Model(Me, CType(rpy_class, RPModelElement))
            ElseIf Is_Component_Type(CType(rpy_class, RPModelElement)) Then
                Dim comp_type As New Component_Type
                Me.Component_Types.Add(comp_type)
                comp_type.Import_From_Rhapsody_Model(Me, CType(rpy_class, RPModelElement))
            ElseIf Is_Root_Software_Composition(CType(rpy_class, RPModelElement)) Then
                Dim compo As New Root_Software_Composition
                Me.Root_Software_Compositions.Add(compo)
                compo.Import_From_Rhapsody_Model(Me, CType(rpy_class, RPModelElement))
            End If

        Next
        If Me.Client_Server_Interfaces.Count = 0 Then
            Me.Client_Server_Interfaces = Nothing
        End If
        If Me.Event_Interfaces.Count = 0 Then
            Me.Event_Interfaces = Nothing
        End If
        If Me.Component_Types.Count = 0 Then
            Me.Component_Types = Nothing
        End If
        If Me.Root_Software_Compositions.Count = 0 Then
            Me.Root_Software_Compositions = Nothing
        End If

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If IsNothing(Me.PSWA_Packages) And
            IsNothing(Me.Component_Types) And
            IsNothing(Me.Root_Software_Compositions) And
            IsNothing(Me.Client_Server_Interfaces) And
            IsNothing(Me.Event_Interfaces) And
            IsNothing(Me.Data_Types) Then

            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "A Shall contain at least one element.")
        End If

    End Sub

End Class
