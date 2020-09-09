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


Public MustInherit Class Classifier_Software_Element
    Inherits Software_Element

    Protected Top_Package As Top_Level_Package = Nothing

    Protected Needed_Elements As List(Of Classifier_Software_Element) = Nothing
    Protected Dependent_Elements As List(Of Classifier_Software_Element) = Nothing

    Public MustOverride Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
    Public MustOverride Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)

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


Public MustInherit Class Software_Class
    Inherits Classifier_Software_Element

    Protected Weighted_Methods_Per_Class As Double = 0

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Class"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Return CType(rpy_parent_pkg.addClass(Me.Name), RPModelElement)
    End Function

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public MustOverride Function Compute_WMC() As Double

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
        Me.Default_Value = CType(rpy_elmt, RPAttribute).defaultValue
    End Sub

End Class


Public MustInherit Class Operation
    Inherits Software_Element

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Operation"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return CType(rpy_parent_class.addOperation(Me.Name), RPModelElement)
    End Function

End Class


Public MustInherit Class Operation_With_Arguments

    Inherits Operation

    Public Arguments As List(Of Operation_Argument)


    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Arguments) Then
                children_list.AddRange(Me.Arguments)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Arguments = New List(Of Operation_Argument)

        Dim rpy_arg As RPArgument
        For Each rpy_arg In CType(Me.Rpy_Element, RPOperation).arguments
            Dim argument As Operation_Argument = New Operation_Argument
            Me.Arguments.Add(argument)
            argument.Import_From_Rhapsody_Model(Me, CType(rpy_arg, RPModelElement))
        Next

        If Me.Arguments.Count = 0 Then
            Me.Arguments = Nothing
        End If

    End Sub


End Class


Public Class Operation_Argument

    Inherits Typed_Software_Element

    Public Stream As E_STREAM

    Public Enum E_STREAM
        INPUT
        OUTPUT
        INVALID
    End Enum

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Private Function Get_Rpy_Stream() As E_STREAM

        Dim result As E_STREAM = E_STREAM.INVALID

        Dim rpy_stream As String
        rpy_stream = CType(Me.Rpy_Element, RPArgument).argumentDirection

        Select Case rpy_stream
            Case "In"
                result = E_STREAM.INPUT
            Case "Out"
                result = E_STREAM.OUTPUT
        End Select

        Return result

    End Function

    Protected Shared Function Transform_Rpy_Stream_To_SMT_Stream(rpy_stream As String) As E_STREAM
        Select Case rpy_stream
            Case "In"
                Return E_STREAM.INPUT
            Case "Out"
                Return E_STREAM.OUTPUT
            Case Else
                Return E_STREAM.INVALID
        End Select
    End Function

    Protected Shared Function Transform_SMT_Stream_To_Rpy_Stream(smt_stream As E_STREAM) As String
        Select Case smt_stream
            Case E_STREAM.INPUT
                Return "In"
            Case E_STREAM.OUTPUT
                Return "Out"
            Case Else
                Return "InOut"
        End Select
    End Function

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Return CType(CType(Me.Rpy_Element, RPArgument).type, RPModelElement)
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Rpy_Data_Type(rpy_type As RPType)
        CType(Me.Rpy_Element, RPArgument).type = CType(rpy_type, RPClassifier)
    End Sub

    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Argument"
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_arg As RPArgument = CType(rpy_element, RPArgument)
        Dim arg_rpy_stream As String = Transform_SMT_Stream_To_Rpy_Stream(Me.Stream)
        If rpy_arg.argumentDirection <> arg_rpy_stream Then
            rpy_arg.getSaveUnit.setReadOnly(0)
            rpy_arg.argumentDirection = arg_rpy_stream
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Stream merged")
        End If
    End Sub

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_ope As RPOperation = CType(rpy_parent, RPOperation)
        Dim rpy_arg As RPArgument = rpy_parent_ope.addArgument(Me.Name)
        Return CType(rpy_arg, RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        ' No stereotype for Operation_Argument
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_arg As RPArgument = CType(rpy_elmt, RPArgument)
        rpy_arg.argumentDirection = Transform_SMT_Stream_To_Rpy_Stream(Me.Stream)
    End Sub

End Class