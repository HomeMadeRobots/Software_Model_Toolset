Imports rhapsody2
Imports System.Xml.Serialization
Imports System.Text.RegularExpressions

Public MustInherit Class Software_Element

    Public Name As String
    Public UUID As Guid = Nothing
    Public Description As String = Nothing

    Protected Children As List(Of Software_Element) = Nothing
    Protected Rpy_Element As RPModelElement = Nothing
    Protected Container As Software_Model_Container = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Function Get_Element_By_Uuid(element_uuid As Guid) As Software_Element
        ' Shall not be used for merge
        ' Shall be used only after Rpy model import (for check or metric computation)
        Return Me.Container.Get_Element(element_uuid)
    End Function

    Public Shared Function Is_Symbol_Valid(symbol As String) As Boolean
        Dim result As Boolean = False
        If Regex.IsMatch(symbol, "^[a-zA-Z][a-zA-Z0-9_]+$") Then
            result = True
        End If
        Return result
    End Function

    Public Overridable Function Get_Children() As List(Of Software_Element)
        Return Nothing
    End Function

    Public Function Get_Rpy_Element_Path() As String
        If IsNothing(Me.Rpy_Element) Then
            Return ""
        Else
            Return Me.Rpy_Element.getFullPathName
        End If
    End Function

    Public Function Get_Rpy_Element() As RPModelElement
        Return Me.Rpy_Element
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Sub Import_From_Rhapsody_Model(
        owner As Software_Element,
        rpy_mdl_element As RPModelElement)

        Me.Container = owner.Container
        Me.Rpy_Element = rpy_mdl_element

        Me.Get_Own_Data_From_Rhapsody_Model()

        Me.Container.Add_Element(Me)

        Me.Import_Children_From_Rhapsody_Model()

    End Sub

    Protected Overridable Sub Get_Own_Data_From_Rhapsody_Model()

        Me.Name = Rpy_Element.name

        If Rpy_Element.description <> "" Then
            Me.Description = Rpy_Element.description
        End If

        Me.UUID = Transform_Rpy_GUID_To_Guid(Rpy_Element.GUID)

    End Sub

    Protected Overridable Sub Import_Children_From_Rhapsody_Model()
        'No common child.
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected MustOverride Function Get_Rpy_Metaclass() As String

    Protected Overridable Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        Me.Rpy_Element = rpy_element
        If Me.Description <> "" Then
            Dim tmp_desc_1 As String = Me.Description.Replace(vbLf, String.Empty)
            Dim tmp_desc_2 As String
            tmp_desc_2 = rpy_element.description.Replace(Environment.NewLine, String.Empty)
            If tmp_desc_1 <> tmp_desc_2 Then
                rpy_element.getSaveUnit.setReadOnly(0)
                rpy_element.description = Me.Description
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Description merged.")
            End If
        End If
    End Sub

    Protected MustOverride Function Create_Rpy_Element(
        rpy_parent As RPModelElement) As RPModelElement

    Protected MustOverride Sub Set_Stereotype()

    Protected Overridable Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)
        Me.Rpy_Element = rpy_elmt
        Rpy_Element.description = Me.Description
        Rpy_Element.GUID = Transform_Guid_To_Rpy_GUID(Me.UUID)
        Me.Set_Stereotype()
        Me.Add_Export_Information_Item(report,
            Merge_Report_Item.E_Merge_Status.ELEMENT_CREATED,
            "")
    End Sub

    Public Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)

        Dim rpy_elmt As RPModelElement
        rpy_elmt = rpy_parent.findNestedElement(Me.Name, Me.Get_Rpy_Metaclass())

        If Not IsNothing(rpy_elmt) Then
            Me.Merge_Rpy_Element(rpy_elmt, report)
        Else
            rpy_elmt = Me.Create_Rpy_Element(rpy_parent)
            Me.Set_Rpy_Element_Attributes(rpy_elmt, report)
        End If

        Me.Export_Children(rpy_elmt, report)
    End Sub

    Protected Overridable Sub Export_Children(rpy_elmt As RPModelElement, report As Report)
        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Export_To_Rhapsody(rpy_elmt, report)
            Next
        End If
    End Sub

    Public Function Find_In_Rpy_Project(element_uuid As Guid) As RPModelElement
        Dim result As RPModelElement = Nothing
        Dim rpy_proj As RPProject = CType(Me.Rpy_Element.project, RPProject)
        result = rpy_proj.findElementByGUID(Transform_Guid_To_Rpy_GUID(element_uuid))
        Return result
    End Function

    Public Function Find_In_Rpy_Project(element_uuid As String) As RPModelElement
        Dim result As RPModelElement = Nothing
        Dim rpy_proj As RPProject = CType(Me.Rpy_Element.project, RPProject)
        result = rpy_proj.findElementByGUID(element_uuid)
        Return result
    End Function

    Public Sub Add_Export_Error_Item(
        report As Report,
        status As Merge_Report_Item.E_Merge_Status,
        message As String)

        Dim item As New Merge_Report_Item(
            Me,
            status,
            Report_Item.Item_Criticality.CRITICALITY_ERROR,
            message)
        report.Add_Report_Item(item)
    End Sub

    Public Sub Add_Export_Information_Item(
        report As Report,
        status As Merge_Report_Item.E_Merge_Status,
        message As String)

        Dim item As New Merge_Report_Item(
            Me,
            status,
            Report_Item.Item_Criticality.CRITICALITY_INFORMATION,
            message)
        report.Add_Report_Item(item)
    End Sub

    ' 4 useful methods to merge or set dependencies, i.e. Public attributes (As Guid or 
    ' As List(of Guid) ) of any class  that are modeled on Rhapsody using a dependency link.
    Protected Sub Merge_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid),
        is_of_stereotype As Func(Of RPModelElement, Boolean))

        Dim rpy_dep As RPDependency
        Dim ref_found As Boolean

        For Each id In element_list
            ref_found = False
            Dim rpy_element_guid As String = Transform_Guid_To_Rpy_GUID(id)
            For Each rpy_dep In Me.Rpy_Element.dependencies
                If is_of_stereotype(CType(rpy_dep, RPModelElement)) Then
                    Dim rpy_dep_elmt As RPModelElement
                    rpy_dep_elmt = rpy_dep.dependsOn
                    If rpy_dep_elmt.GUID = rpy_element_guid Then
                        ref_found = True
                        Exit For
                    Else
                        ' test only for RPPort
                        rpy_dep_elmt = rpy_dep.dependsOn.owner
                        Dim rpy_port_dep_elmnt As RPPort
                        Try
                            rpy_port_dep_elmnt = CType(rpy_dep_elmt, RPPort)
                            If rpy_dep_elmt.GUID = rpy_element_guid Then
                                ref_found = True
                                Exit For
                            End If
                        Catch
                            ' nothing
                        End Try
                    End If
                End If
            Next
            If ref_found = False Then
                Dim rpy_if As RPModelElement = Me.Find_In_Rpy_Project(rpy_element_guid)
                If IsNothing(rpy_if) Then
                    Me.Add_Export_Error_Item(report,
                        Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                        stereotype_str & " not found : " & id.ToString & ".")
                Else
                    Dim created_rpy_dep As RPDependency = Me.Rpy_Element.addDependencyTo(rpy_if)
                    created_rpy_dep.addStereotype(stereotype_str, "Dependency")
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        stereotype_str & " merged : " & id.ToString & ".")
                End If
            End If
        Next
    End Sub

    Protected Sub Set_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid))
        For Each elmt_ref In element_list
            Dim ref_rpy_elemt As RPModelElement = Me.Find_In_Rpy_Project(elmt_ref)
            If Not IsNothing(ref_rpy_elemt) Then
                Dim rpy_dep As RPDependency
                rpy_dep = Me.Rpy_Element.addDependencyTo(ref_rpy_elemt)
                rpy_dep.addStereotype(stereotype_str, "Dependency")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    stereotype_str & " not not found : " & elmt_ref.ToString & ".")
            End If
        Next
    End Sub

    Protected Sub Merge_Dependency(
        report As Report,
        stereotype_str As String,
        element As Guid,
        is_of_stereotype As Func(Of RPModelElement, Boolean))

        Dim temp_list As New List(Of Guid)
        temp_list.Add(element)
        Me.Merge_Dependencies(report, stereotype_str, temp_list, is_of_stereotype)

    End Sub

    Protected Sub Set_Dependency(report As Report, stereotype_str As String, element As Guid)
        Dim temp_list As New List(Of Guid)
        temp_list.Add(element)
        Me.Set_Dependencies(report, stereotype_str, temp_list)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Public Sub Check_Consistency(report As Report)

        Me.Check_Own_Consistency(report)

        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Check_Consistency(report)
            Next
        End If

    End Sub

    Protected Overridable Sub Check_Own_Consistency(report As Report)

        If Not Is_Symbol_Valid(Me.Name) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "ELMT_2",
                "Invalid symbol, expression shall match ^[a-zA-Z][a-zA-Z0-9_]+$.")
        End If

        If Me.Description = "" Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "ELMT_5",
                "Description is missing.")
        End If

    End Sub

    Public Sub Add_Consistency_Check_Error_Item(
        report As Report,
        id As String,
        message As String)

        Dim item As New Consistency_Check_Report_Item(
                Me,
                id,
                Report_Item.Item_Criticality.CRITICALITY_ERROR,
                message)
            report.Add_Report_Item(item)
    End Sub

    Public Sub Add_Consistency_Check_Warning_Item(
        report As Report,
        id As String,
        message As String)

        Dim item As New Consistency_Check_Report_Item(
                Me,
                id,
                Report_Item.Item_Criticality.CRITICALITY_WARNING,
                message)
            report.Add_Report_Item(item)
    End Sub

    Public Sub Add_Consistency_Check_Information_Item(
        report As Report,
        id As String,
        message As String)

        Dim item As New Consistency_Check_Report_Item(
                Me,
                id,
                Report_Item.Item_Criticality.CRITICALITY_INFORMATION,
                message)
            report.Add_Report_Item(item)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Protected Sub Compute_Documentation_Rate(
        ByRef nb_documentable_elements As Double,
        ByRef nb_documented_elements As Double)

        nb_documentable_elements = nb_documentable_elements + 1

        If Me.Description <> "" Then
            nb_documented_elements = nb_documented_elements + 1
        End If

        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Compute_Documentation_Rate(nb_documentable_elements, nb_documented_elements)
            Next
        End If

    End Sub

End Class


Public MustInherit Class SMM_Classifier
    Inherits Software_Element

    Protected Needed_Elements As List(Of SMM_Classifier) = Nothing
    Protected Dependent_Elements As List(Of SMM_Classifier) = Nothing

    Public MustOverride Function Find_Needed_Elements() As List(Of SMM_Classifier)
    Public MustOverride Function Find_Dependent_Elements() As List(Of SMM_Classifier)

    Public Function Get_Top_Package() As Top_Level_Package
        Dim result As Top_Level_Package = Nothing

        ' Get the Rhapsody top level package
        Dim project_guid As String
        project_guid = Transform_Guid_To_Rpy_GUID(Me.Container.UUID)
        Dim rpy_top_pkg As RPModelElement = Me.Rpy_Element.owner
        While rpy_top_pkg.owner.GUID <> project_guid
            rpy_top_pkg = rpy_top_pkg.owner
        End While

        ' Find the corresponding Software_Package
        Dim top_level_package_uuid As Guid
        top_level_package_uuid = Transform_Rpy_GUID_To_Guid(rpy_top_pkg.GUID)
        result = CType(Me.Container.Get_Element_By_Uuid(top_level_package_uuid), Top_Level_Package)

        Return result
    End Function

    Public Function Get_Dependent_Elements_Nb() As Double
        Return Me.Dependent_Elements.Count
    End Function

End Class


Public MustInherit Class SMM_Class
    Inherits SMM_Classifier

    Public Base_Class_Ref As Guid = Guid.Empty 'Optional, not all SMM_Class can be specialized
    Protected Nb_Base_Class_Ref As Integer = 0

    Protected All_Children As List(Of Software_Element) = Nothing

    Protected Weighted_Methods_Per_Class As Double = 0

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected MustOverride Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_gen As RPGeneralization
        For Each rpy_gen In CType(Me.Rpy_Element, RPClass).generalizations
            Dim referenced_rpy_elmt_guid As String
            referenced_rpy_elmt_guid = rpy_gen.baseClass.GUID
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(referenced_rpy_elmt_guid)
            Dim referenced_rpy_elmt As RPModelElement
            referenced_rpy_elmt = Me.Find_In_Rpy_Project(referenced_rpy_elmt_guid)
            If Me.Is_My_Metaclass(referenced_rpy_elmt) Then
                Me.Base_Class_Ref = ref_elmt_guid
            End If
            Me.Nb_Base_Class_Ref += 1
        Next
    End Sub

    Protected Function Get_Inherited_Children() As List(Of Software_Element)
        If IsNothing(Me.All_Children) Then
            Dim children_list As New List(Of Software_Element)
            Dim base_ref As Guid = Me.Base_Class_Ref
            While base_ref <> Guid.Empty
                Dim base_elmt As SMM_Class
                base_elmt = CType(Me.Get_Element_By_Uuid(base_ref), SMM_Class)
                children_list.AddRange(base_elmt.Get_Children)
                base_ref = base_elmt.Base_Class_Ref
            End While
            Me.All_Children = children_list
        End If
        Return Me.All_Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Class"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Return CType(rpy_parent_pkg.addClass(Me.Name), RPModelElement)
    End Function

    Public Function Is_Exportable(any_rpy_elmt As RPModelElement) As Boolean
        If Me.Base_Class_Ref = Guid.Empty Then
            Return True
        End If
        Dim referenced_rpy_class As RPClass
        Dim rpy_proj As RPProject = CType(any_rpy_elmt.project, RPProject)
        Dim base_class_guid As String = Transform_Guid_To_Rpy_GUID(Me.Base_Class_Ref)
        referenced_rpy_class = CType(rpy_proj.findElementByGUID(base_class_guid), RPClass)
        If Not IsNothing(referenced_rpy_class) Then
            Return True
        Else
            Return False
        End If
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        ' Merge Base_Class_Ref
        Dim rpy_gen As RPGeneralization = Nothing
        Dim reference_found As Boolean = False
        If Me.Base_Class_Ref <> Guid.Empty Then
            Dim referenced_rpy_class_guid As String
            referenced_rpy_class_guid = Transform_Guid_To_Rpy_GUID(Me.Base_Class_Ref)
            For Each rpy_gen In rpy_class.generalizations
                Dim current_rpy_class As RPClass = CType(rpy_gen.baseClass, RPClass)
                If current_rpy_class.GUID = referenced_rpy_class_guid Then
                    ' No change
                    reference_found = True
                End If
            Next
            If reference_found = False Then
                Dim referenced_rpy_class As RPClass
                referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Class_Ref), RPClass)
                If IsNothing(referenced_rpy_class) Then
                    Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Base_Class not found : " & Me.Base_Class_Ref.ToString & ".")
                Else
                    rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
                End If
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)

        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        Dim referenced_rpy_class As RPClass
        If Me.Base_Class_Ref <> Guid.Empty Then
            referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Class_Ref), RPClass)
            If IsNothing(referenced_rpy_class) Then
                Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Class not found : " & Me.Base_Class_Ref.ToString & ".")
            Else
                rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
            End If
        End If

    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Base_Class_Ref > 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "CLASS_1",
                "Shall specialize at most 1 class.")
        End If

        If Me.Nb_Base_Class_Ref <> 0 Then
            If Me.Base_Class_Ref = Guid.Empty Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "CLASS_2",
                    "Shall specialize a class of its own meta-class.")
            End If
        End If

        Me.Check_Children_Name(report)
        Me.Check_Children_Name_Against_Inherited(report)

    End Sub

    Protected Overridable Sub Check_Children_Name_Against_Inherited(report As Report)
        Dim inherited_children_name_list As New List(Of String)
        For Each child In Me.Get_Inherited_Children
            inherited_children_name_list.Add(child.Name)
        Next
        For Each child In Me.Get_Children
            If inherited_children_name_list.Contains(child.Name) Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "CLASS_3",
                    "Symbol '" & child.Name & "' is already used by a child of a base class.")
            End If
        Next
    End Sub

    Public Overridable Sub Check_Children_Name(report As Report)
        ' Overridable because for some SMM_Class, it is interresting to skip it.
        ' Indeed, Rhapsody ensure that child of the same metaclass have a different name.
        Dim children_name_list As New List(Of String)
        For Each child In Me.Get_Children
            If children_name_list.Contains(child.Name) Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "ELMT_6",
                    "Symbol '" & child.Name & "' (" & child.GetType.ToString.Split("."c).Last & _
                    ") is already used by an other child.")
            Else
                children_name_list.Add(child.Name)
            End If
        Next
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public MustOverride Function Compute_WMC() As Double

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            If Me.Base_Class_Ref <> Guid.Empty Then
                Dim base_class As SMM_Class
                base_class = CType(Me.Get_Element_By_Uuid(Me.Base_Class_Ref), SMM_Class)
                Me.Needed_Elements.Add(base_class)
            End If
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)

            Dim class_list As List(Of SMM_Class)
            class_list = Me.Container.Get_All_Specializable_Class
            For Each elmt In class_list
                If elmt.Base_Class_Ref = Me.UUID Then
                    Me.Dependent_Elements.Add(elmt)
                End If
            Next
        End If
        Return Me.Dependent_Elements
    End Function

End Class


Public MustInherit Class Typed_Software_Element

    Inherits Software_Element

    Public Base_Data_Type_Ref As Guid = Guid.Empty


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overridable Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If Not IsNothing(rpy_type) Then
            If Is_Data_Type(rpy_type) Or Is_Physical_Data_Type(rpy_type) Then
                Me.Base_Data_Type_Ref = Transform_Rpy_GUID_To_Guid(rpy_type.GUID)
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overridable Sub Set_Rpy_Data_Type(rpy_type As RPType)
        CType(Me.Rpy_Element, RPAttribute).type = CType(rpy_type, RPClassifier)
    End Sub

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If rpy_type.GUID <> Transform_Guid_To_Rpy_GUID(Me.Base_Data_Type_Ref) Then
            rpy_element.getSaveUnit.setReadOnly(0)
            Dim element_type As RPType
            element_type = CType(Me.Find_In_Rpy_Project(Me.Base_Data_Type_Ref), RPType)
            If Not IsNothing(element_type) Then
                Me.Set_Rpy_Data_Type(element_type)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Base_Data_Type merged.")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Base_Data_Type not found : " & Me.Base_Data_Type_Ref.ToString & ".")
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim element_type As RPType
        element_type = CType(Me.Find_In_Rpy_Project(Me.Base_Data_Type_Ref), RPType)
        If IsNothing(element_type) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Data_Type not found : " & Me.Base_Data_Type_Ref.ToString & ".")
        Else
            Me.Set_Rpy_Data_Type(element_type)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Base_Data_Type_Ref = Guid.Empty Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TYP_1",
                "Referenced type shall be a Data_Type.")
        End If

    End Sub

End Class


Public MustInherit Class Attribute_Software_Element

    Inherits Typed_Software_Element

    Public Default_Value As String = Nothing


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim default_val_raw As String = CType(Me.Rpy_Element, RPAttribute).defaultValue
        If default_val_raw <> "" Then
            Me.Default_Value = default_val_raw
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Attribute"
    End Function

    Protected Overrides Function Create_Rpy_Element(
        rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return CType(rpy_parent_class.addAttribute(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim current_default_val As String
        current_default_val = CType(rpy_element, RPAttribute).defaultValue
        If Me.Default_Value <> current_default_val Then
            CType(rpy_element, RPAttribute).defaultValue = Me.Default_Value
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Default_Value merged.")
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        CType(rpy_elmt, RPAttribute).defaultValue = Me.Default_Value
    End Sub

End Class


Public MustInherit Class SMM_Object
    ' Note : a SMM_Object is not an instance of a SMM_Class (only true for Internal_Design_)

    Inherits Software_Element

    Public Type_Ref As Guid = Nothing
    Public Configuration_Values As List(Of Configuration_Value)

    Public Class Configuration_Value

        Public Configuration_Ref As Guid = Nothing
        Public Value As String

    End Class

    '----------------------------------------------------------------------------------------------'
    ' General methods


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_component As RPInstance
        rpy_component = CType(Me.Rpy_Element, RPInstance)
        If IsNothing(rpy_component.ObjectAsObjectType) Then
            Dim rpy_base_class As RPClass
            rpy_base_class = CType(rpy_component.otherClass, RPClass)
            If Not IsNothing(rpy_base_class) Then

                Me.Type_Ref = Transform_Rpy_GUID_To_Guid(rpy_base_class.GUID)

                Me.Configuration_Values = New List(Of Configuration_Value)
                Dim rpy_attribute As RPAttribute
                For Each rpy_attribute In rpy_base_class.attributes
                    If Is_Configuration_Parameter(CType(rpy_attribute, RPModelElement)) Then
                        Dim conf_val As New Configuration_Value
                        conf_val.Configuration_Ref = Transform_Rpy_GUID_To_Guid(rpy_attribute.GUID)
                        conf_val.Value = rpy_component.getAttributeValue(rpy_attribute.name)
                        Me.Configuration_Values.Add(conf_val)
                    End If
                Next

            End If
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Instance"
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_inst As RPInstance = CType(rpy_element, RPInstance)
        If rpy_inst.otherClass.GUID <> Transform_Guid_To_Rpy_GUID(Me.Type_Ref) Then
            rpy_inst.getSaveUnit.setReadOnly(0)
            Dim referenced_rpy_class As RPClass
            referenced_rpy_class = CType(Me.Find_In_Rpy_Project(Me.Type_Ref), RPClass)
            If IsNothing(referenced_rpy_class) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Type not found : " & Me.Type_Ref.ToString & ".")
            Else
                rpy_inst.otherClass = CType(referenced_rpy_class, RPClassifier)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Type_Ref merged.")
            End If
        End If
        If Not IsNothing(Me.Configuration_Values) Then
            For Each conf In Me.Configuration_Values
                Dim rpy_conf_attr As RPAttribute = Nothing
                rpy_conf_attr = CType(Me.Find_In_Rpy_Project(conf.Configuration_Ref), 
                                RPAttribute)
                If rpy_inst.getAttributeValue(rpy_conf_attr.name) <> conf.Value Then
                    rpy_inst.getSaveUnit.setReadOnly(0)
                    rpy_inst.setAttributeValue(rpy_conf_attr.name, conf.Value)
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Configuration_Value of " & rpy_conf_attr.name & " merged.")
                End If
            Next
        End If
    End Sub

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return rpy_parent_class.addNewAggr("Instance", Me.Name)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_inst As RPInstance = CType(rpy_elmt, RPInstance)

        ' Set Type_Ref
        Dim referenced_rpy_class As RPClass
        referenced_rpy_class = CType(Me.Find_In_Rpy_Project(Me.Type_Ref), RPClass)
        If IsNothing(referenced_rpy_class) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Type not found : " & Me.Type_Ref.ToString & ".")
        Else
            rpy_inst.otherClass = CType(referenced_rpy_class, RPClassifier)
        End If

        ' Set Configuration_Values
        For Each conf In Me.Configuration_Values
            Dim rpy_conf_attr As RPAttribute = Nothing
            rpy_conf_attr = CType(Me.Find_In_Rpy_Project(conf.Configuration_Ref), 
                            RPAttribute)
            rpy_inst.setAttributeValue(rpy_conf_attr.name, conf.Value)
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Type_Ref.Equals(Guid.Empty) Then
            Me.Add_Consistency_Check_Error_Item(report, "OBJ_1", "Shall reference a Class.")
        End If
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation

End Class