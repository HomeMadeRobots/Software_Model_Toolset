﻿Imports rhapsody2
Imports System.Xml.Serialization

Public Class Software_Package

    Inherits Software_Element

    <XmlArrayItem("Package")>
    Public Packages As List(Of Software_Package)

    <XmlArrayItemAttribute(GetType(Enumerated_Data_Type)), _
     XmlArrayItemAttribute(GetType(Array_Data_Type)), _
     XmlArrayItemAttribute(GetType(Physical_Data_Type)), _
     XmlArrayItemAttribute(GetType(Structured_Data_Type)), _
     XmlArray("Data_Types")>
    Public Data_Types As List(Of Data_Type)

    <XmlArrayItemAttribute(GetType(Client_Server_Interface)), _
     XmlArrayItemAttribute(GetType(Event_Interface)), _
     XmlArray("Interfaces")>
    Public Software_Interfaces As List(Of Software_Interface)

    Public Component_Types As List(Of Component_Type)
    Public Root_Software_Compositions As List(Of Root_Software_Composition)
    Public Component_Designs As List(Of Component_Design)
    Public Classes As List(Of SDD_Class)


    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Packages) Then
                children_list.AddRange(Me.Packages)
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
            If Not IsNothing(Me.Component_Designs) Then
                children_list.AddRange(Me.Component_Designs)
            End If
            If Not IsNothing(Me.Classes) Then
                children_list.AddRange(Me.Classes)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Protected Sub Get_All_Sub_Packages(ByRef pkg_list As List(Of Software_Package))
        If Not IsNothing(Me.Packages) Then
            Dim pkg As Software_Package
            For Each pkg In Me.Packages
                pkg_list.Add(pkg)
                pkg.Get_All_Sub_Packages(pkg_list)
            Next
        End If
    End Sub

    Public Shared Sub Remove_Empty_Packages(rpy_package As RPPackage)
        Dim child_pgk As RPPackage
        For Each child_pgk In rpy_package.packages
            Remove_Empty_Packages(child_pgk)
        Next
        If Software_Package.Is_Empty(rpy_package) Then
            rpy_package.deleteFromProject()
        End If
    End Sub

    Private Shared Function Is_Empty(rpy_pkg As RPPackage) As Boolean
        Dim result As Boolean = False
        If rpy_pkg.classes.Count = 0 _
            And rpy_pkg.types.Count = 0 _
            And rpy_pkg.modules.Count = 0 _
            And rpy_pkg.globalObjects.Count = 0 _
            And rpy_pkg.packages.Count = 0 Then
            result = True
        End If
        Return result
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Packages = New List(Of Software_Package)
        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPPackage).packages
            If Is_PSWA_Package(CType(rpy_pkg, RPModelElement)) Then
                Dim pkg As Software_Package = New Software_Package
                Me.Packages.Add(pkg)
                pkg.Import_From_Rhapsody_Model(Me, CType(rpy_pkg, RPModelElement))
            End If
        Next
        If Me.Packages.Count = 0 Then
            Me.Packages = Nothing
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
        Me.Component_Designs = New List(Of Component_Design)
        Me.Classes = New List(Of SDD_Class)
        Dim rpy_class As RPClass
        For Each rpy_class In CType(Me.Rpy_Element, RPPackage).classes
            Dim rpy_element As RPModelElement = CType(rpy_class, RPModelElement)
            If Is_Client_Server_Interface(rpy_element) Then
                Dim cs_if As Client_Server_Interface = New Client_Server_Interface
                Me.Software_Interfaces.Add(cs_if)
                cs_if.Import_From_Rhapsody_Model(Me, rpy_element)
            ElseIf Is_Event_Interface(rpy_element) Then
                Dim event_interface As Event_Interface = New Event_Interface
                Me.Software_Interfaces.Add(event_interface)
                event_interface.Import_From_Rhapsody_Model(Me, rpy_element)
            ElseIf Is_Component_Type(rpy_element) Then
                Dim comp_type As New Component_Type
                Me.Component_Types.Add(comp_type)
                comp_type.Import_From_Rhapsody_Model(Me, rpy_element)
            ElseIf Is_Root_Software_Composition(rpy_element) Then
                Dim compo As New Root_Software_Composition
                Me.Root_Software_Compositions.Add(compo)
                compo.Import_From_Rhapsody_Model(Me, rpy_element)
            ElseIf Is_Component_Design(rpy_element) Then
                Dim comp_design As New Component_Design
                Me.Component_Designs.Add(comp_design)
                comp_design.Import_From_Rhapsody_Model(Me, rpy_element)
            ElseIf Is_SDD_Class(rpy_element) Then
                Dim sdd_class As New SDD_Class
                Me.Classes.Add(sdd_class)
                sdd_class.Import_From_Rhapsody_Model(Me, rpy_element)
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
        If Me.Component_Designs.Count = 0 Then
            Me.Component_Designs = Nothing
        End If
        If Me.Classes.Count = 0 Then
            Me.Classes = Nothing
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Package"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Return CType(rpy_parent_pkg.addNestedPackage(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Software_Package", "Package")
    End Sub

    Protected Overrides Sub Export_Children(rpy_elmt As RPModelElement, report As Report)
        For Each pkg In Me.Packages
            pkg.Export_To_Rhapsody(rpy_elmt, report)
        Next
    End Sub

    Public Sub Export_Independent_Data_Types_To_Rhapsody(report As Report)
        For Each dt In Me.Data_Types
            Select Case dt.GetType
                Case GetType(Enumerated_Data_Type)
                    CType(dt, Enumerated_Data_Type).Export_To_Rhapsody(Me.Rpy_Element, report)
                Case GetType(Physical_Data_Type)
                    CType(dt, Physical_Data_Type).Export_To_Rhapsody(Me.Rpy_Element, report)
            End Select
        Next

        For Each pkg In Me.Packages
            pkg.Export_Independent_Data_Types_To_Rhapsody(report)
        Next

    End Sub

    Public Sub Export_Dependent_Data_Types_To_Rhapsody(
        ByRef exported_dt_list As List(Of Data_Type),
        report As Report,
        force As Boolean)
        For Each dt In Me.Data_Types
            Select Case dt.GetType
                Case GetType(Array_Data_Type)
                    If force = True Or
                        CType(dt, Array_Data_Type).Is_Exportable(Me.Rpy_Element) = True Then
                        CType(dt, Array_Data_Type).Export_To_Rhapsody(Me.Rpy_Element, report)
                        exported_dt_list.Add(dt)
                    End If
                Case GetType(Structured_Data_Type)
                    If force = True Or
                        CType(dt, Structured_Data_Type).Is_Exportable(Me.Rpy_Element) = True Then
                        CType(dt, Structured_Data_Type).Export_To_Rhapsody(Me.Rpy_Element, report)
                        exported_dt_list.Add(dt)
                    End If
            End Select
        Next

        For Each pkg In Me.Packages
            pkg.Export_Dependent_Data_Types_To_Rhapsody(exported_dt_list, report, force)
        Next
    End Sub

    Public Sub Export_Interfaces_To_Rhapsody(report As Report)
        For Each sw_if In Me.Software_Interfaces
            sw_if.Export_To_Rhapsody(Me.Rpy_Element, report)
        Next

        For Each pkg In Me.Packages
            pkg.Export_Interfaces_To_Rhapsody(report)
        Next
    End Sub

    Public Sub Export_Component_Types_To_Rhapsody(report As Report)
        For Each swct In Me.Component_Types
            swct.Export_To_Rhapsody(Me.Rpy_Element, report)
        Next

        For Each pkg In Me.Packages
            pkg.Export_Component_Types_To_Rhapsody(report)
        Next
    End Sub

    Public Sub Export_Compositions_To_Rhapsody(report As Report)
        For Each compo In Me.Root_Software_Compositions
            compo.Export_To_Rhapsody(Me.Rpy_Element, report)
        Next

        For Each pkg In Me.Packages
            pkg.Export_Compositions_To_Rhapsody(report)
        Next
    End Sub

    Public Sub Export_Classes_To_Rhapsody(
        ByRef exported_classes_list As List(Of SDD_Class),
        report As Report,
        force As Boolean)

        For Each sdd_class In Me.Classes
            If force = True Or
                sdd_class.Is_Exportable(Me.Rpy_Element) = True Then
                sdd_class.Export_To_Rhapsody(Me.Rpy_Element, report)
                exported_classes_list.Add(sdd_class)
            End If
        Next

        For Each pkg In Me.Packages
            pkg.Export_Classes_To_Rhapsody(exported_classes_list, report, force)
        Next

    End Sub

    Public Sub Export_Component_Design_To_Rhapsody(report As Report)
        For Each swcd In Me.Component_Designs
            swcd.Export_To_Rhapsody(Me.Rpy_Element, report)
        Next

        For Each pkg In Me.Packages
            pkg.Export_Component_Design_To_Rhapsody(report)
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If IsNothing(Me.Packages) And
            IsNothing(Me.Component_Types) And
            IsNothing(Me.Root_Software_Compositions) And
            IsNothing(Me.Software_Interfaces) And
            IsNothing(Me.Data_Types) And
            IsNothing(Me.Classes) And
            IsNothing(Me.Component_Designs) Then

            Me.Add_Consistency_Check_Warning_Item(report,
                "PKG_1",
                "A Shall contain at least one element.")
        End If

    End Sub

End Class


Public Class Top_Level_Package
    Inherits Software_Package

    Private All_Packages As List(Of Software_Package) = Nothing

    Private Documentation_Rate As Double = -1

    Private Needed_Elements_List As List(Of Classifier_Software_Element) = Nothing
    Private Needed_Top_Packages_List As List(Of Top_Level_Package) = Nothing

    Private Dependent_Elements_List As List(Of Classifier_Software_Element) = Nothing
    Private Dependent_Top_Packages_List As List(Of Top_Level_Package) = Nothing

    Private Nb_Data_Types As Double = 0
    Private Nb_Interfaces As Double = 0
    Private Nb_Component_Types As Double = 0
    Private Nb_Root_Software_Composition As Double = 0

    Private Efferent_Coupling As Double = 0
    Private Afferent_Coupling As Double = 0
    Private Instability As Double
    Private Abstraction_Level As Double = 0
    Private Distance As Double


    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Function Get_All_Packages() As List(Of Software_Package)
        If IsNothing(Me.All_Packages) Then
            Me.All_Packages = New List(Of Software_Package)
            Me.Get_All_Sub_Packages(Me.All_Packages)
            Me.All_Packages.Add(Me)
        End If
        Return Me.All_Packages
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Function Get_Package_Documentation_Rate() As Double
        If Me.Documentation_Rate = -1 Then
            Dim nb_documented_elements As Double = 0
            Dim nb_documentable_elements As Double = 0
            Me.Compute_Documentation_Rate(nb_documentable_elements, nb_documented_elements)
            Me.Documentation_Rate = nb_documented_elements / nb_documentable_elements
        End If
        Return Me.Documentation_Rate
    End Function

    Public Sub Compute_Nb_Classifiers()
        Dim pkg_list As List(Of Software_Package) = Me.Get_All_Packages
        For Each pkg In pkg_list
            If Not IsNothing(pkg.Component_Types) Then
                Me.Nb_Component_Types += pkg.Component_Types.Count
            End If
            If Not IsNothing(pkg.Software_Interfaces) Then
                Me.Nb_Interfaces += pkg.Software_Interfaces.Count
            End If
            If Not IsNothing(pkg.Data_Types) Then
                Me.Nb_Data_Types += pkg.Data_Types.Count
            End If
            If Not IsNothing(pkg.Root_Software_Compositions) Then
                Me.Nb_Root_Software_Composition += pkg.Root_Software_Compositions.Count
            End If
        Next
    End Sub

    Public Sub Find_Needed_Elements()

        Me.Needed_Top_Packages_List = New List(Of Top_Level_Package)
        Me.Needed_Elements_List = New List(Of Classifier_Software_Element)

        Dim tmp_needed_elements_list = New List(Of Classifier_Software_Element)

        Dim pkg_list As List(Of Software_Package) = Me.Get_All_Packages

        ' Parse the list of sub packages + Me
        Dim pkg As Software_Package
        For Each pkg In pkg_list

            If Not IsNothing(pkg.Component_Types) Then
                Dim swct As Component_Type
                For Each swct In pkg.Component_Types
                    tmp_needed_elements_list.AddRange(swct.Find_Needed_Elements)
                Next
            End If

            If Not IsNothing(pkg.Software_Interfaces) Then
                For Each sw_if In pkg.Software_Interfaces
                    If Not IsNothing(sw_if.Find_Needed_Elements) Then
                        tmp_needed_elements_list.AddRange(sw_if.Find_Needed_Elements)
                    End If
                Next
            End If

            If Not IsNothing(pkg.Data_Types) Then
                For Each data_type In pkg.Data_Types
                    If Not IsNothing(data_type.Find_Needed_Elements) Then
                        tmp_needed_elements_list.AddRange(data_type.Find_Needed_Elements)
                    End If
                Next
            End If

            If Not IsNothing(pkg.Root_Software_Compositions) Then
                For Each compo In pkg.Root_Software_Compositions
                    tmp_needed_elements_list.AddRange(compo.Find_Needed_Elements)
                Next
            End If
        Next

        tmp_needed_elements_list = tmp_needed_elements_list.Distinct().ToList

        For Each element In tmp_needed_elements_list
            Dim owner_pkg As Top_Level_Package = element.Get_Top_Package()
            If owner_pkg.UUID <> Me.UUID Then
                If Not Me.Needed_Top_Packages_List.Contains(owner_pkg) Then
                    Me.Needed_Top_Packages_List.Add(owner_pkg)
                End If
                Me.Needed_Elements_List.Add(element)
            End If
        Next
    End Sub

    Public Sub Find_Dependent_Elements()
        ' Find_Needed_Elements shall be called first.

        Me.Dependent_Top_Packages_List = New List(Of Top_Level_Package)
        Me.Dependent_Elements_List = New List(Of Classifier_Software_Element)

        Dim tmp_dependent_elements_list As New List(Of Classifier_Software_Element)

        Dim pkg_list As List(Of Software_Package) = Me.Get_All_Packages

        ' Parse the list of sub packages + Me
        Dim pkg As Software_Package
        For Each pkg In pkg_list

            If Not IsNothing(pkg.Component_Types) Then
                Dim swct As Component_Type
                For Each swct In pkg.Component_Types
                    If Not IsNothing(swct.Find_Dependent_Elements) Then
                        tmp_dependent_elements_list.AddRange(swct.Find_Dependent_Elements)
                    End If
                Next
            End If

            If Not IsNothing(pkg.Software_Interfaces) Then
                For Each sw_if In pkg.Software_Interfaces
                    If Not IsNothing(sw_if.Find_Dependent_Elements) Then
                        tmp_dependent_elements_list.AddRange(sw_if.Find_Dependent_Elements)
                    End If
                Next
            End If

            If Not IsNothing(pkg.Data_Types) Then
                For Each data_type In pkg.Data_Types
                    If Not IsNothing(data_type.Find_Dependent_Elements) Then
                        tmp_dependent_elements_list.AddRange(data_type.Find_Dependent_Elements)
                    End If
                Next
            End If

        Next

        tmp_dependent_elements_list = tmp_dependent_elements_list.Distinct().ToList

        For Each element In tmp_dependent_elements_list
            Dim owner_pkg As Top_Level_Package = element.Get_Top_Package()
            If owner_pkg.UUID <> Me.UUID Then
                If Not Me.Dependent_Top_Packages_List.Contains(owner_pkg) Then
                    Me.Dependent_Top_Packages_List.Add(owner_pkg)
                End If
                Me.Dependent_Elements_List.Add(element)
            End If
        Next

    End Sub

    Public Sub Compute_Coupling()
        ' Find_Needed_Elements and Find_Dependent_Elements shall be called first.

        ' Compute Afferent_Coupling
        Me.Afferent_Coupling = Me.Dependent_Elements_List.Count

        ' Compute Efferent_Coupling
        Me.Efferent_Coupling = Me.Needed_Elements_List.Count

        ' Compute Instability
        If (Me.Efferent_Coupling + Me.Afferent_Coupling) = 0 Then
            Me.Instability = 0
        Else
            Me.Instability = Me.Efferent_Coupling / (Me.Efferent_Coupling + Me.Afferent_Coupling)
        End If

        ' Compute Abstraction_Level
        Dim nb_concrete_classifiers As Double
        Dim nb_abstract_classifiers As Double
        nb_concrete_classifiers = Me.Nb_Component_Types + Me.Nb_Root_Software_Composition
        nb_abstract_classifiers = Me.Nb_Interfaces
        Dim nb_classifiers As Double = nb_concrete_classifiers + nb_abstract_classifiers
        If nb_classifiers = 0 Then
            Me.Abstraction_Level = 1
        Else
            Me.Abstraction_Level = nb_abstract_classifiers / nb_classifiers
        End If

        ' Compute Distance
        Me.Distance = Math.Abs(Me.Abstraction_Level + Me.Instability - 1)

    End Sub

    Public Function Get_Nb_Data_Types() As Double
        Return Me.Nb_Data_Types
    End Function

    Public Function Get_Nb_Interfaces() As Double
        Return Me.Nb_Interfaces
    End Function

    Public Function Get_Nb_Component_Types() As Double
        Return Me.Nb_Component_Types
    End Function

    Public Function Get_Nb_Compositions() As Double
        Return Me.Nb_Root_Software_Composition
    End Function

    Public Function Get_Efferent_Coupling() As Double
        Return Me.Efferent_Coupling
    End Function

    Public Function Get_Afferent_Coupling() As Double
        Return Me.Afferent_Coupling
    End Function

    Public Function Get_Instability() As Double
        Return Me.Instability
    End Function

    Public Function Get_Abstraction_Level() As Double
        Return Me.Abstraction_Level
    End Function

    Public Function Get_Distance() As Double
        Return Me.Distance
    End Function

End Class