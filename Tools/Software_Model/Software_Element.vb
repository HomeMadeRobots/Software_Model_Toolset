Imports rhapsody2
Imports System.Text.RegularExpressions

Public MustInherit Class Software_Element

    Public Name As String
    Public UUID As Guid = Nothing
    Public Description As String = Nothing

    Protected Rpy_Element As RPModelElement = Nothing
    Protected Parent As Software_Element = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Protected Function Get_Model_Container() As Software_Model_Container
        Dim container As Software_Model_Container = Nothing
        Dim parent As Software_Element
        parent = Me.Parent
        If IsNothing(parent) Then
            container = CType(Me, Software_Model_Container)
        Else
            While Not IsNothing(parent.Parent)
                parent = parent.Parent
            End While
            container = CType(parent, Software_Model_Container)
        End If
        Return container
    End Function

    Public Function Get_Path() As String
        Dim path As String = ""
        Dim sw_element As Software_Element
        sw_element = CType(Me, Software_Element)
        If Not IsNothing(sw_element.Parent) Then 'test if Me is a Software_Model_Container
            path = "/" & sw_element.Name
            sw_element = sw_element.Parent
            While Not IsNothing(sw_element.Parent)
                path = "/" & sw_element.Name & path
                sw_element = sw_element.Parent
            End While
        End If
        Return path
    End Function

    Public Sub Add_To_Model_Element_List()
        Dim container As Software_Model_Container
        container = Me.Get_Model_Container
        container.Add_Element(Me)
    End Sub

    Public Function Get_Element_By_Uuid(element_uuid As Guid) As Software_Element
        Dim container As Software_Model_Container
        container = Me.Get_Model_Container
        Return container.Get_Element(element_uuid)
    End Function

    Public Shared Function Is_Symbol_Valid(symbol As String) As Boolean
        Dim result As Boolean = False
        If Regex.IsMatch(symbol, "^[a-zA-Z][a-zA-Z0-9_]+$") Then
            result = True
        End If
        Return result
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Overridable Function Get_Children() As List(Of Software_Element)
        Return Nothing
    End Function

    Public Sub Import_From_Rhapsody_Model(
        owner As Software_Element,
        rpy_mdl_element As RPModelElement)

        Me.Parent = owner
        Me.Rpy_Element = rpy_mdl_element

        Me.Get_Own_Data_From_Rhapsody_Model()

        Me.Add_To_Model_Element_List()

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
    Public Sub Compute_Metrics()

        'Me.Compute_Own_Metrics()

        'Dim children As List(Of Software_Element) = Me.Get_Children
        'If Not IsNothing(children) Then
        '    For Each child In children
        '        child.Compute_Own_Metrics()
        '    Next
        'End If

    End Sub

    Protected Overridable Sub Compute_Own_Metrics()
        ' Nothing common
    End Sub

End Class


Public MustInherit Class Classifier_Software_Element
    Inherits Software_Element

    Private Needed_Elements As New List(Of Software_Element)
    Private Dependent_Elements As New List(Of Software_Element)

End Class


Public MustInherit Class Typed_Software_Element

    Inherits Software_Element

    Public Base_Data_Type_Ref As Guid = Guid.Empty

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

    Protected MustOverride Function Get_Rpy_Stream() As E_STREAM

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Me.Stream = Get_Rpy_Stream()
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Stream = E_STREAM.INVALID Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Stream shall be In ou Out.")
        End If

    End Sub

End Class