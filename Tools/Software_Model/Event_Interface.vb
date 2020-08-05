Imports rhapsody2

Public Class Event_Interface

    Inherits Software_Interface

    Public Arguments As List(Of Event_Argument)

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

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Arguments = New List(Of Event_Argument)

        Dim rpy_event_arg As RPAttribute
        For Each rpy_event_arg In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Event_Argument(CType(rpy_event_arg, RPModelElement)) Then
                Dim arg As Event_Argument = New Event_Argument
                Me.Arguments.Add(arg)
                arg.Import_From_Rhapsody_Model(Me, CType(rpy_event_arg, RPModelElement))
            End If
        Next

        If Me.Arguments.Count = 0 Then
            Me.Arguments = Nothing
        End If

    End Sub

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of Classifier_Software_Element)
            If Not IsNothing(Me.Arguments) Then
                For Each arg In Me.Arguments
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                    If Not Me.Needed_Elements.Contains(data_type) Then
                        Me.Needed_Elements.Add(data_type)
                    End If
                Next
            End If
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            Me.Weighted_Methods_Per_Class = 1
            If Not IsNothing(Me.Arguments) Then
                For Each arg In Me.Arguments
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                    Me.Weighted_Methods_Per_Class += data_type.Get_Complexity
                Next
            End If
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function

End Class


Public Class Event_Argument

    Inherits Typed_Software_Element

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function

End Class