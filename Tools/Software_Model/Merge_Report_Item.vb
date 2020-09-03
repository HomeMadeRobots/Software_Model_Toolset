Imports rhapsody2

Public Class Merge_Report_Item
    Inherits Report_Item

    Public Enum E_Merge_Status
        ELEMENT_CREATED
        ELEMENT_ALREADY_EXISTS
        ELEMENT_ATTRIBUTE_MERGED
        MISSING_REFERENCED_ELEMENTS
    End Enum

    Private Shared Attribute_Name_List As List(Of String) = _
        New List(Of String)({"Path", "Kind", "Criticality", "Merge status", "Message"})

    Private Rpy_Element_Path As String
    Private Merge_Status As E_Merge_Status
    Private Sw_Element_Type As Type

    Public Sub New(
        sw_element As Software_Element,
        status As E_Merge_Status,
        criticality As Report_Item.Item_Criticality,
        message As String)

        Me.Rpy_Element_Path = sw_element.Get_Rpy_Element_Path
        Me.Sw_Element_Type = sw_element.GetType
        Me.Criticality = criticality
        Me.Merge_Status = status
        Me.Message = message

    End Sub

    Public Overrides Function Get_Item_Attribute_Name_List() As List(Of String)
        Return Merge_Report_Item.Attribute_Name_List
    End Function

    Public Overrides Function Get_Item_Attribute_Value(attribute_idx As Integer) As String
        Select Case attribute_idx
            Case 1
                Return Me.Rpy_Element_Path
            Case 2
                Return Me.Sw_Element_Type.ToString.Split("."c).Last()
            Case 3
                Return Me.Get_Criticality_String()
            Case 4
                Return Me.Get_Status_String()
            Case 5
                Return Me.Message
            Case Else
                Return ""
        End Select
    End Function

    Private Function Get_Status_String() As String
        Select Case Me.Merge_Status
            Case E_Merge_Status.ELEMENT_ALREADY_EXISTS
                Return "ELEMENT_ALREADY_EXISTS"
            Case E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED
                Return "ELEMENT_ATTRIBUTE_MERGED"
            Case E_Merge_Status.ELEMENT_CREATED
                Return "ELEMENT_CREATED"
            Case E_Merge_Status.MISSING_REFERENCED_ELEMENTS
                Return "MISSING_REFERENCED_ELEMENTS"
            Case Else
                Return ""
        End Select
    End Function
End Class
