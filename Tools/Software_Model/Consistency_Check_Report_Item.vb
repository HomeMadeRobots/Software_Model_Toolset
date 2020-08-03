Public Class Consistency_Check_Report_Item

    Inherits Report_Item

    Private Shared Attribute_Name_List As List(Of String) = _
        New List(Of String)({"Path", "Meta-class", "Rule ID", "Criticality", "Message"})

    Private Rpy_Element_Path As String
    Private Rule_Id As String
    Private Sw_Element_Type As Type

    Public Sub New(
        sw_element As Software_Element,
        rule_id As String,
        criticality As Report_Item.Item_Criticality,
        message As String)

        Me.Rpy_Element_Path = sw_element.Get_Path
        Me.Sw_Element_Type = sw_element.GetType
        Me.Rule_Id = rule_id
        Me.Criticality = criticality
        Me.Message = message

    End Sub

    Public Overrides Function Get_Item_Attribute_Name_List() As List(Of String)
        Return Consistency_Check_Report_Item.Attribute_Name_List
    End Function

    Public Overrides Function Get_Item_Attribute_Value(attribute_idx As Integer) As String
        Select Case attribute_idx
            Case 1
                Return Me.Rpy_Element_Path
            Case 2
                Return Me.Sw_Element_Type.ToString.Split("."c).Last()
            Case 3
                Return Me.Rule_Id
            Case 4
                Return Report_Item.Get_Criticality_String(Me.Criticality)
            Case 5
                Return Me.Message
            Case Else
                Return ""
        End Select
    End Function

End Class
