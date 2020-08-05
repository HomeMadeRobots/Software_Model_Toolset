﻿Imports rhapsody2
Imports System.Xml.Serialization

Public Class PSWA_Package

    Inherits Software_Element

    Public PSWA_Packages As List(Of PSWA_Package)
    <XmlArrayItemAttribute(GetType(Enumerated_Data_Type)), _
     XmlArrayItemAttribute(GetType(Array_Data_Type)), _
     XmlArrayItemAttribute(GetType(Physical_Data_Type)), _
     XmlArrayItemAttribute(GetType(Structured_Data_Type)), _
     XmlArray("Data_Types")>
    Public Data_Types As List(Of Data_Type)

    <XmlArrayItemAttribute(GetType(Client_Server_Interface)), _
     XmlArrayItemAttribute(GetType(Event_Interface)), _
     XmlArray("Software_Interfaces")>
    Public Software_Interfaces As List(Of Software_Interface)

    Public Component_Types As List(Of Component_Type)
    Public Root_Software_Compositions As List(Of Root_Software_Composition)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.PSWA_Packages) Then
                children_list.AddRange(Me.PSWA_Packages)
            End If
            If Not IsNothing(Me.Data_Types) Then
                children_list.AddRange(Me.Data_Types)
            End If
            If Not IsNothing(Me.Software_Interfaces) Then
                children_list.AddRange(Me.Software_Interfaces)
            End If
            If Not IsNothing(Me.Component_Types) Then
                children_list.AddRange(Me.Component_Types)
            End If
            If Not IsNothing(Me.Root_Software_Compositions) Then
                children_list.AddRange(Me.Root_Software_Compositions)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Public Sub Get_All_Sub_Packages(ByRef pkg_list As List(Of PSWA_Package))
        If Not IsNothing(Me.PSWA_Packages) Then
            Dim pkg As PSWA_Package
            For Each pkg In Me.PSWA_Packages
                pkg_list.Add(pkg)
                pkg.Get_All_Sub_Packages(pkg_list)
            Next
        End If
    End Sub

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
                Select Case type_kind
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

        Me.Software_Interfaces = New List(Of Software_Interface)
        Me.Component_Types = New List(Of Component_Type)
        Me.Root_Software_Compositions = New List(Of Root_Software_Composition)
        Dim rpy_class As RPClass
        For Each rpy_class In CType(Me.Rpy_Element, RPPackage).classes
            If Is_Client_Server_Interface(CType(rpy_class, RPModelElement)) Then
                Dim cs_if As Client_Server_Interface = New Client_Server_Interface
                Me.Software_Interfaces.Add(cs_if)
                cs_if.Import_From_Rhapsody_Model(Me, CType(rpy_class, RPModelElement))
            ElseIf Is_Event_Interface(CType(rpy_class, RPModelElement)) Then
                Dim event_interface As Event_Interface = New Event_Interface
                Me.Software_Interfaces.Add(event_interface)
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
        If Me.Software_Interfaces.Count = 0 Then
            Me.Software_Interfaces = Nothing
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
            IsNothing(Me.Software_Interfaces) And
            IsNothing(Me.Data_Types) Then

            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "A Shall contain at least one element.")
        End If

    End Sub

End Class


Public Class Top_Level_PSWA_Package
    Inherits PSWA_Package

    <XmlIgnore()>
    Public Container As Software_Model_Container

    Private All_Packages As List(Of PSWA_Package) = Nothing

    Private Nb_Documented_Elements As Double = 0
    Private Nb_Documentable_Elements As Double = 1

    Private Needed_Top_Packages_List As List(Of Top_Level_PSWA_Package) = Nothing

    Public Function Get_All_Packages() As List(Of PSWA_Package)
        If IsNothing(Me.All_Packages) Then
            Me.All_Packages = New List(Of PSWA_Package)
            Me.Get_All_Sub_Packages(Me.All_Packages)
            Me.All_Packages.Add(Me)
        End If
        Return Me.All_Packages
    End Function

    Public Sub Compute_Package_Documentation_Level()
        If Me.Description <> "" Then
            Nb_Documented_Elements = 1
        End If

        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Compute_Documentation_Level(
                    Me.Nb_Documentable_Elements,
                    Me.Nb_Documented_Elements)
            Next
        End If
    End Sub

    Public Function Get_Documentation_Rate() As Double
        Return Nb_Documented_Elements / Nb_Documentable_Elements
    End Function

    Public Sub Find_Needed_Elements()

        Me.Needed_Top_Packages_List = New List(Of Top_Level_PSWA_Package)
        Dim needed_elements_list As New List(Of Classifier_Software_Element)

        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages

        ' Parse the list of sub packages + Me
        Dim pkg As PSWA_Package
        For Each pkg In pkg_list

            If Not IsNothing(pkg.Component_Types) Then
                Dim swct As Component_Type
                For Each swct In pkg.Component_Types
                    needed_elements_list.AddRange(swct.Find_Needed_Elements)
                Next
            End If

            If Not IsNothing(pkg.Software_Interfaces) Then
                For Each sw_if In pkg.Software_Interfaces
                    If Not IsNothing(sw_if.Find_Needed_Elements) Then
                        needed_elements_list.AddRange(sw_if.Find_Needed_Elements)
                    End If
                Next
            End If

            If Not IsNothing(pkg.Data_Types) Then
                Dim data_type As Data_Type
                For Each data_type In pkg.Data_Types
                    If Not IsNothing(data_type.Find_Needed_Elements) Then
                        needed_elements_list.AddRange(data_type.Find_Needed_Elements)
                    End If
                Next
            End If
        Next

        needed_elements_list = needed_elements_list.Distinct().ToList

        For Each element In needed_elements_list
            Dim owner_pkg As Top_Level_PSWA_Package = element.Get_Top_Package()
            If owner_pkg.UUID <> Me.UUID Then
                If Not Me.Needed_Top_Packages_List.Contains(owner_pkg) Then
                    Me.Needed_Top_Packages_List.Add(owner_pkg)
                End If
            End If
        Next
    End Sub

    Public Function Get_Needed_Top_Packages_List() As List(Of Top_Level_PSWA_Package)
        Return Me.Needed_Top_Packages_List
    End Function

    Public Sub Compute_Interfaces_WMC()
        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages
        For Each pkg In pkg_list
            If Not IsNothing(pkg.Software_Interfaces) Then
                For Each sw_if In pkg.Software_Interfaces
                    sw_if.Compute_WMC()
                Next
            End If
        Next
    End Sub

    Public Sub Compute_Component_Type_WMC()
        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages
        For Each pkg In pkg_list
            If Not IsNothing(pkg.Component_Types) Then
                For Each swct In pkg.Component_Types
                    swct.Compute_WMC()
                Next
            End If
        Next
    End Sub

End Class