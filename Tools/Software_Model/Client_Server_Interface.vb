Imports rhapsody2

Public Class Client_Server_Interface

    Inherits Software_Element

    <Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Synchronous_Operation)), _
     Global.System.Xml.Serialization.XmlArrayItemAttribute(GetType(Asynchronous_Operation)), _
     Global.System.Xml.Serialization.XmlArray("Operations")>
    Public Operations As List(Of Operation)

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Operations = New List(Of Operation)

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_Synchronous_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim operation As Synchronous_Operation = New Synchronous_Operation
                Me.Operations.Add(operation)
                operation.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            ElseIf Is_Asynchronous_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim operation As Asynchronous_Operation = New Asynchronous_Operation
                Me.Operations.Add(operation)
                operation.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            End If
        Next

        If Me.Operations.Count = 0 Then
            Me.Operations = Nothing
        End If

    End Sub

End Class


Public Class Operation

    Inherits Software_Element

    Public Arguments As List(Of Operation_Argument)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As Object = Me.Arguments
        Return TryCast(children, List(Of Software_Element))
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Arguments = New List(Of Operation_Argument)

        Dim rpy_arg As RPArgument
        For Each rpy_arg In CType(Me.Rpy_Element, RPOperation).arguments
            Dim argument As Operation_Argument = New Operation_Argument
            Me.Arguments.Add(argument)
            argument.Import_From_Rhapsody_Model(Me, CType(rpy_arg, RPModelElement))
        Next

        If Me.Arguments.Count = 0 Then
            Me.Arguments = Nothing
        End If

    End Sub

End Class


Public Class Synchronous_Operation
    Inherits Operation

End Class


Public Class Asynchronous_Operation
    Inherits Operation

End Class


Public Class Operation_Argument

    Inherits Stream_Typed_Software_Element

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Return CType(CType(Me.Rpy_Element, RPArgument).type, RPModelElement)
    End Function

    Protected Overrides Function Get_Rpy_Stream() As E_STREAM

        Dim result As E_STREAM = E_STREAM.INVALID

        Dim rpy_stream As String
        rpy_stream = CType(Me.Rpy_Element, RPArgument).argumentDirection

        Select Case rpy_stream
            Case "In"
                result = E_STREAM.INPUT
            Case "Out"
                result = E_STREAM.OUTPUT
        End Select

        Return result

    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        ' No child.
    End Sub

End Class