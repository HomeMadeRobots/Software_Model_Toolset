Imports rhapsody2
Imports System.Xml.Serialization
Imports System.Text.RegularExpressions

Public MustInherit Class Software_Element

    Public Name As String
    Public UUID As Guid = Nothing
    Public Description As String = Nothing

    Protected Children As List(Of Software_Element) = Nothing
    Protected Rpy_Element As RPModelElement = Nothing
    Protected Top_Package As Top_Level_PSWA_Package = Nothing
    Protected Path As String = ""

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Function Get_Path() As String
        Return Me.Path
    End Function

    Public Function Get_Top_Package() As Top_Level_PSWA_Package
        Return Me.Top_Package
    End Function

    Public Function Get_Element_By_Uuid(element_uuid As Guid) As Software_Element
        Return Me.Top_Package.Container.Get_Element(element_uuid)
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Sub Import_From_Rhapsody_Model(
        owner As Software_Element,
        rpy_mdl_element As RPModelElement)

        Me.Top_Package = owner.Top_Package
        Me.Rpy_Element = rpy_mdl_element

        Me.Get_Own_Data_From_Rhapsody_Model()

        Me.Path = owner.Get_Path & "/" & Me.Name

        Me.Top_Package.Container.Add_Element(Me)

        Me.Import_Children_From_Rhapsody_Model()

    End Sub

    Protected Overridable Sub Get_Own_Data_From_Rhapsody_Model()

        Me.Name = Rpy_Element.name

        If Rpy_Element.description <> "" Then
            Me.Description = Rpy_Element.description
        End If

        Me.UUID = Transform_GUID_To_UUID(Rpy_Element.GUID)

    End Sub

    Protected Overridable Sub Import_Children_From_Rhapsody_Model()
        'No common child.
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public MustOverride Sub Export_To_Rhapsody(rpy_parent As RPModelElement)

    Public Overridable Sub Merge_Rpy_Element(rpy_element As RPModelElement)
        Me.Rpy_Element = rpy_element
        If Me.Description <> "" Then
            If Me.Description <> rpy_element.description Then
                rpy_element.getSaveUnit.setReadOnly(0)
                rpy_element.description = Me.Description
            End If
        End If
    End Sub

    Public Overridable Sub Set_Rpy_Common_Attributes(rpy_element As RPModelElement)
        Me.Rpy_Element = rpy_element
        rpy_element.description = Me.Description
        rpy_element.GUID = Transform_UUID_To_GUID(Me.UUID)
    End Sub

    Public Function Find_In_Rpy_Project(element_uuid As Guid) As RPModelElement
        Dim result As RPModelElement = Nothing
        Dim rpy_proj As RPProject = CType(Me.Rpy_Element.project, RPProject)
        result = rpy_proj.findElementByGUID(Transform_UUID_To_GUID(element_uuid))
        Return result
    End Function


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
                "TBD",
                "Invalid symbol, expression shall match ^[a-zA-Z][a-zA-Z0-9_]+$.")
        End If

        If Me.Description = "" Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
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
    Sub Compute_Documentation_Rate(
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

    Protected Needed_Elements As List(Of Classifier_Software_Element) = Nothing
    Protected Dependent_Elements As List(Of Classifier_Software_Element) = Nothing

    Public MustOverride Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
    Public MustOverride Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)

    Public Function Get_Dependent_Elements_Nb() As Double
        Return Me.Dependent_Elements.Count
    End Function

End Class


Public MustInherit Class Software_Class
    Inherits Classifier_Software_Element

    Protected Weighted_Methods_Per_Class As Double = 0

    Public MustOverride Function Compute_WMC() As Double

End Class


Public MustInherit Class Typed_Software_Element

    Inherits Software_Element

    Public Base_Data_Type_Ref As Guid = Guid.Empty


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected MustOverride Function Get_Rpy_Data_Type() As RPModelElement

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If Not IsNothing(rpy_type) Then
            If Is_Data_Type(rpy_type) Or Is_Physical_Data_Type(rpy_type) Then
                Me.Base_Data_Type_Ref = Transform_GUID_To_UUID(rpy_type.GUID)
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overridable Sub Set_Rpy_Data_Type(rpy_type As RPType)
        CType(Me.Rpy_Element, RPAttribute).type = CType(rpy_type, RPClassifier)
    End Sub

    Public Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement)
        MyBase.Merge_Rpy_Element(rpy_element)
        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If rpy_type.GUID <> Transform_UUID_To_GUID(Me.Base_Data_Type_Ref) Then
            rpy_element.getSaveUnit.setReadOnly(0)
            Dim arg_type As RPType
            arg_type = CType(Me.Find_In_Rpy_Project(Me.Base_Data_Type_Ref), RPType)
            Me.Set_Rpy_Data_Type(arg_type)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Base_Data_Type_Ref = Guid.Empty Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Referenced type shall be a Data_Type.")
        End If

    End Sub

End Class


Public MustInherit Class Stream_Typed_Software_Element

    Inherits Typed_Software_Element

    Public Stream As E_STREAM

    Public Enum E_STREAM
        INPUT
        OUTPUT
        INVALID
    End Enum

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Protected MustOverride Function Get_Rpy_Stream() As E_STREAM

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
                Return "In"
        End Select
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Me.Stream = Get_Rpy_Stream()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Rpy_Data_Type(rpy_type As RPType)
        CType(Me.Rpy_Element, RPArgument).type = CType(rpy_type, RPClassifier)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Stream = E_STREAM.INVALID Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Stream shall be In ou Out.")
        End If

    End Sub

End Class