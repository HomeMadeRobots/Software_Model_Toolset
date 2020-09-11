Imports rhapsody2

Public Class SDD_Class

    Inherits Software_Class


    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        'If IsNothing(Me.Children) Then
        '    Dim children_list As New List(Of Software_Element)
        '    If Not IsNothing(Me.Variable_Attributes) Then
        '        children_list.AddRange(Me.Variable_Attributes)
        '    End If
        '    Me.Children = children_list
        'End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("SDD_Class", "Class")
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation (not yet implemented for SDD_Class)
    Public Overrides Function Compute_WMC() As Double
        Return 0
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
    End Function

End Class
