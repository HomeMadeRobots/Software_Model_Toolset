Imports rhapsody2

Public Class Event_Interface

    Inherits Software_Element

    Public Arguments As List(Of Event_Argument)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As List(Of Software_Element) = Nothing
        If Not IsNothing(Me.Arguments) Then
            children = New List(Of Software_Element)
            For Each arg In Me.Arguments
                children.Add(arg)
            Next
        End If
        Return children
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

End Class


Public Class Event_Argument

    Inherits Typed_Software_Element

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function

End Class