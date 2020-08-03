Imports System.IO


Public Class Report

    Protected Report_Items_List As List(Of Report_Item) = New List(Of Report_Item)
    Protected Error_Number As UInteger = 0

    Public Overridable Sub Generate_Csv_Report(report_file_stream As StreamWriter)

        If Me.Report_Items_List.Count > 0 Then

            ' Write header
            Dim attribute_name As String
            Dim item_attribute_name_list As List(Of String)
            item_attribute_name_list = Me.Report_Items_List.First.Get_Item_Attribute_Name_List
            For Each attribute_name In item_attribute_name_list
                report_file_stream.Write(attribute_name & ";")
            Next
            report_file_stream.WriteLine()

            ' Write content
            Dim nb_attribute As Integer = item_attribute_name_list.Count
            Dim item As Report_Item
            For Each item In Me.Report_Items_List
                Dim attribute_idx As Integer
                For attribute_idx = 1 To nb_attribute
                    report_file_stream.Write(item.Get_Item_Attribute_Value(attribute_idx) & ";")
                Next
                report_file_stream.WriteLine()
            Next
        End If

    End Sub

    Public Sub Add_Report_Item(item As Report_Item)
        Me.Report_Items_List.Add(item)
        If item.Get_Criticality = Report_Item.Item_Criticality.CRITICALITY_ERROR Then
            Error_Number = CUInt(Error_Number + 1)
        End If
    End Sub

End Class


Public MustInherit Class Report_Item

    Protected Criticality As Item_Criticality = Item_Criticality.CRITICALITY_INFORMATION
    Protected Message As String
    Protected Analysis As String

    Public Enum Item_Criticality
        CRITICALITY_ERROR
        CRITICALITY_WARNING
        CRITICALITY_INFORMATION
    End Enum


    Public MustOverride Function Get_Item_Attribute_Name_List() As List(Of String)

    Public MustOverride Function Get_Item_Attribute_Value(attribute_idx As Integer) As String

    Public Function Get_Criticality() As Item_Criticality
        Return Me.Criticality
    End Function

    Public Sub Set_Analyse(analyse As String)
        Me.Analysis = analyse
    End Sub

    Public Sub Set_Error()
        Me.Criticality = Item_Criticality.CRITICALITY_ERROR
    End Sub

    Public Sub Set_Warning()
        Me.Criticality = Item_Criticality.CRITICALITY_WARNING
    End Sub

    Public Sub Set_Info()
        Me.Criticality = Item_Criticality.CRITICALITY_INFORMATION
    End Sub

    Public Sub Set_Message(message As String)
        Me.Message = message
    End Sub

    Public Sub Set_Error_Message(message As String)
        Me.Criticality = Item_Criticality.CRITICALITY_ERROR
        Me.Message = message
    End Sub

    Public Sub Set_Warning_Message(message As String)
        Me.Criticality = Item_Criticality.CRITICALITY_WARNING
        Me.Message = message
    End Sub

    Public Sub Set_Info_Message(message As String)
        Me.Criticality = Item_Criticality.CRITICALITY_INFORMATION
        Me.Message = message
    End Sub

    Shared Function Get_Criticality_String(criticality As Item_Criticality) As String
        Select Case criticality
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