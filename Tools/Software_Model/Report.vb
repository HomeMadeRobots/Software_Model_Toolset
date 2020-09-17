Imports System.IO


Public Class Report

    Protected Report_Items_List As List(Of Report_Item) = New List(Of Report_Item)
    Protected Nb_Error As Integer = 0

    Public Sub New(item_list As List(Of String))
        Report_Item.Set_Item_Attribute_Name_List(item_list)
    End Sub

    Public Overridable Sub Generate_Csv_Report(report_file_stream As StreamWriter)
        Me.Write_Header(report_file_stream)
        If Me.Report_Items_List.Count > 0 Then
            Me.Write_Content(report_file_stream)
        Else
            report_file_stream.Write("No item to report.;")
        End If
    End Sub

    Protected Sub Write_Header(report_file_stream As StreamWriter)
        Dim attribute_name As String
        Dim item_attribute_name_list As List(Of String)
        item_attribute_name_list = Report_Item.Get_Item_Attribute_Name_List
        For Each attribute_name In item_attribute_name_list
            report_file_stream.Write(attribute_name & ";")
        Next
        report_file_stream.WriteLine()
    End Sub

    Protected Overridable Sub Write_Content(report_file_stream As StreamWriter)
        Dim item_attribute_name_list As List(Of String)
        item_attribute_name_list = Report_Item.Get_Item_Attribute_Name_List
        Dim nb_attribute As Integer = item_attribute_name_list.Count
        Dim item As Report_Item
        For Each item In Me.Report_Items_List
            Dim attribute_idx As Integer
            For attribute_idx = 1 To nb_attribute
                report_file_stream.Write(item.Get_Item_Attribute_Value(attribute_idx) & ";")
            Next
            report_file_stream.WriteLine()
        Next
    End Sub

    Public Sub Add_Report_Item(item As Report_Item)
        Me.Report_Items_List.Add(item)
        If item.Get_Criticality = Report_Item.Item_Criticality.CRITICALITY_ERROR Then
            Nb_Error = Nb_Error + 1
        End If
    End Sub

    Public Function Get_Error_Number() As Integer
        Return Me.Nb_Error
    End Function

End Class


Public MustInherit Class Report_Item

    Protected Shared Attribute_Name_List As List(Of String)
    Protected Criticality As Item_Criticality = Item_Criticality.CRITICALITY_INFORMATION
    Protected Message As String
    Protected Analysis As String

    Public Enum Item_Criticality
        CRITICALITY_ERROR
        CRITICALITY_WARNING
        CRITICALITY_INFORMATION
    End Enum

    Public Shared Function Get_Item_Attribute_Name_List() As List(Of String)
        Return Attribute_Name_List
    End Function

    Public Shared Sub Set_Item_Attribute_Name_List(item_list As List(Of String))
        Report_Item.Attribute_Name_List = item_list
    End Sub

    Public MustOverride Function Get_Item_Attribute_Value(attribute_idx As Integer) As String

    Public Function Get_Criticality() As Item_Criticality
        Return Me.Criticality
    End Function

    Public Function Get_Criticality_String() As String
        Select Case Me.Criticality
        Case Item_Criticality.CRITICALITY_ERROR
            Return "ERROR"
        Case Item_Criticality.CRITICALITY_WARNING
            Return "WARNING"
        Case Item_Criticality.CRITICALITY_INFORMATION
            Return "INFO"
        Case Else
            Return ""
        End Select
    End Function

End Class