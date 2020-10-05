Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Public Class Consistency_Check_Report
    Inherits Report

    Shared Attr_List As New List(Of String) From _
        {"Path", "Meta-class", "Rule ID", "Criticality", "Message", "Warning analysis"}

    Public Sub New()
        MyBase.New(Consistency_Check_Report.Attr_List)
    End Sub

    Public Function Load(report_full_path As String) As Boolean
        Dim is_valid As Boolean = True
        If File.Exists(report_full_path) Then
            Dim csv_reader As New TextFieldParser(report_full_path)
            csv_reader.SetDelimiters(";")
            Dim current_row As String()
            While Not csv_reader.EndOfData
                Try
                    current_row = csv_reader.ReadFields()
                    Dim new_item As New Consistency_Check_Report_Item(current_row)
                    Me.Add_Report_Item(new_item)
                Catch ex As MalformedLineException
                    is_valid = False
                End Try
            End While
        Else
            is_valid = False
        End If

        Return is_valid
    End Function

End Class


Public Class Consistency_Check_Report_Item

    Inherits Report_Item

    Private Rpy_Element_Path As String
    Private Rule_Id As String
    Private Sw_Element_Type As Type
    Private Analysis As String

    Public Sub New(
        sw_element As Software_Element,
        rule_id As String,
        criticality As Report_Item.Item_Criticality,
        message As String)

        Me.Rpy_Element_Path = sw_element.Get_Rpy_Element_Path
        Me.Sw_Element_Type = sw_element.GetType
        Me.Rule_Id = rule_id
        Me.Criticality = criticality
        Me.Message = message

    End Sub

    Public Sub New(field As String())
        Me.Rpy_Element_Path = field(0)
        Me.Message = field(4)
        Me.Analysis = field(5)
    End Sub

    Public Overrides Function Get_Item_Attribute_Value(attribute_idx As Integer) As String
        Select Case attribute_idx
            Case 1
                Return Me.Rpy_Element_Path
            Case 2
                Return Me.Sw_Element_Type.ToString.Split("."c).Last()
            Case 3
                Return Me.Rule_Id
            Case 4
                Return Me.Get_Criticality_String
            Case 5
                Return Me.Message
            Case 6
                Return Me.Analysis
            Case Else
                Return ""
        End Select
    End Function

    Public Function Get_Path() As String
        Return Me.Rpy_Element_Path
    End Function

    Public Function Get_Analysis() As String
        Return Me.Analysis
    End Function

    Public Sub Set_Analysis(analysis_str As String)
        Me.Analysis = analysis_str
    End Sub
End Class
