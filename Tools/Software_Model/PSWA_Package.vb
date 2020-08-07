Imports rhapsody2
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


    '----------------------------------------------------------------------------------------------'
    ' General methods 
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_pkg As RPPackage = Nothing
        rpy_pkg = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Package"), RPPackage)
        If Not IsNothing(rpy_pkg) Then
            Me.Merge_Rpy_Element(CType(rpy_pkg, RPModelElement))
        Else
            rpy_pkg = rpy_parent_pkg.addNestedPackage(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_pkg, RPModelElement))
            rpy_pkg.addStereotype("PSWA_Package", "Package")
        End If

        For Each pkg In Me.PSWA_Packages
            pkg.Export_To_Rhapsody(CType(rpy_pkg, RPModelElement))
        Next

    End Sub

    Public Sub Export_Independent_Data_Types()
        For Each dt In Me.Data_Types
            Select Case dt.GetType
                Case GetType(Enumerated_Data_Type)
                    CType(dt, Enumerated_Data_Type).Export_To_Rhapsody(Me.Rpy_Element)
                Case GetType(Physical_Data_Type)
                    CType(dt, Physical_Data_Type).Export_To_Rhapsody(Me.Rpy_Element)
            End Select
        Next

        For Each pkg In Me.PSWA_Packages
            pkg.Export_Independent_Data_Types()
        Next

    End Sub

    Public Sub Export_Dependent_Data_Types(ByRef exported_dt_list As List(Of Data_Type))
        For Each dt In Me.Data_Types
            Select Case dt.GetType
                Case GetType(Array_Data_Type)
                    If CType(dt, Array_Data_Type).Is_Exportable(Me.Rpy_Element) = True Then
                        CType(dt, Array_Data_Type).Export_To_Rhapsody(Me.Rpy_Element)
                        exported_dt_list.Add(dt)
                    End If
                Case GetType(Structured_Data_Type)
                    If CType(dt, Structured_Data_Type).Is_Exportable(Me.Rpy_Element) = False Then
                        CType(dt, Structured_Data_Type).Export_To_Rhapsody(Me.Rpy_Element)
                        exported_dt_list.Add(dt)
                    End If
            End Select
        Next

        For Each pkg In Me.PSWA_Packages
            pkg.Export_Dependent_Data_Types(exported_dt_list)
        Next
    End Sub

    Public Sub Export_Interfaces()
        For Each sw_if In Me.Software_Interfaces
            sw_if.Export_To_Rhapsody(Me.Rpy_Element)
        Next

        For Each pkg In Me.PSWA_Packages
            pkg.Export_Interfaces()
        Next
    End Sub

    Public Sub Export_Component_Types()
        For Each swct In Me.Component_Types
            swct.Export_To_Rhapsody(Me.Rpy_Element)
        Next

        For Each pkg In Me.PSWA_Packages
            pkg.Export_Component_Types()
        Next
    End Sub

    Public Sub Export_Compositions()
        For Each compo In Me.Root_Software_Compositions
            compo.Export_To_Rhapsody(Me.Rpy_Element)
        Next

        For Each pkg In Me.PSWA_Packages
            pkg.Export_Compositions()
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
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


    Private Documentation_Rate As Double = -1

    Private Needed_Elements_List As List(Of Classifier_Software_Element) = Nothing
    Private Needed_Top_Packages_List As List(Of Top_Level_PSWA_Package) = Nothing

    Private Dependent_Elements_List As List(Of Classifier_Software_Element) = Nothing
    Private Dependent_Top_Packages_List As List(Of Top_Level_PSWA_Package) = Nothing

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
    Public Function Get_All_Packages() As List(Of PSWA_Package)
        If IsNothing(Me.All_Packages) Then
            Me.All_Packages = New List(Of PSWA_Package)
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
            Dim nb_documentable_elements As Double = 1

            If Me.Description <> "" Then
                nb_documented_elements = 1
            End If

            Dim children As List(Of Software_Element) = Me.Get_Children
            If Not IsNothing(children) Then
                For Each child In children
                    child.Compute_Documentation_Rate(
                        nb_documentable_elements,
                        nb_documented_elements)
                Next
            End If
            Me.Documentation_Rate = nb_documented_elements / nb_documentable_elements
        End If
        Return Me.Documentation_Rate
    End Function

    Public Sub Compute_Nb_Classifiers()
        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages
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

        Me.Needed_Top_Packages_List = New List(Of Top_Level_PSWA_Package)
        Me.Needed_Elements_List = New List(Of Classifier_Software_Element)

        Dim tmp_needed_elements_list = New List(Of Classifier_Software_Element)

        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages

        ' Parse the list of sub packages + Me
        Dim pkg As PSWA_Package
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

            If Not IsNothing(Me.Root_Software_Compositions) Then
                For Each compo In pkg.Root_Software_Compositions
                    tmp_needed_elements_list.AddRange(compo.Find_Needed_Elements)
                Next
            End If
        Next

        tmp_needed_elements_list = tmp_needed_elements_list.Distinct().ToList

        For Each element In tmp_needed_elements_list
            Dim owner_pkg As Top_Level_PSWA_Package = element.Get_Top_Package()
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

        Me.Dependent_Top_Packages_List = New List(Of Top_Level_PSWA_Package)
        Me.Dependent_Elements_List = New List(Of Classifier_Software_Element)

        Dim tmp_dependent_elements_list As New List(Of Classifier_Software_Element)

        Dim pkg_list As List(Of PSWA_Package) = Me.Get_All_Packages

        ' Parse the list of sub packages + Me
        Dim pkg As PSWA_Package
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
            Dim owner_pkg As Top_Level_PSWA_Package = element.Get_Top_Package()
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