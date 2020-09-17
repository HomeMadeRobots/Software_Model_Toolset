Imports rhapsody2


Public MustInherit Class SMM_Operation
    Inherits Software_Element

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Operation"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return CType(rpy_parent_class.addOperation(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        CType(rpy_elmt, RPOperation).setReturnTypeDeclaration("")
    End Sub

End Class


Public MustInherit Class Operation_With_Arguments

    Inherits SMM_Operation

    Public Arguments As List(Of Operation_Argument)


    '----------------------------------------------------------------------------------------------'
    ' General methods 
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
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


Public Class Operation_Argument

    Inherits Typed_Software_Element

    Public Stream As E_STREAM

    Public Enum E_STREAM
        INPUT
        OUTPUT
        INVALID
    End Enum

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Private Function Get_Rpy_Stream() As E_STREAM

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

    Protected Shared Function Transform_Rpy_Stream_To_SMT_Stream(rpy_stream As String) As E_STREAM
        Select Case rpy_stream
            Case "In"
                Return E_STREAM.INPUT
            Case "Out"
                Return E_STREAM.OUTPUT
            Case Else
                Return E_STREAM.INVALID
        End Select
    End Function

    Protected Shared Function Transform_SMT_Stream_To_Rpy_Stream(smt_stream As E_STREAM) As String
        Select Case smt_stream
            Case E_STREAM.INPUT
                Return "In"
            Case E_STREAM.OUTPUT
                Return "Out"
            Case Else
                Return "InOut"
        End Select
    End Function

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Return CType(CType(Me.Rpy_Element, RPArgument).type, RPModelElement)
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Me.Stream = Get_Rpy_Stream()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Rpy_Data_Type(rpy_type As RPType)
        CType(Me.Rpy_Element, RPArgument).type = CType(rpy_type, RPClassifier)
    End Sub

    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Argument"
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_arg As RPArgument = CType(rpy_element, RPArgument)
        Dim arg_rpy_stream As String = Transform_SMT_Stream_To_Rpy_Stream(Me.Stream)
        If rpy_arg.argumentDirection <> arg_rpy_stream Then
            rpy_arg.getSaveUnit.setReadOnly(0)
            rpy_arg.argumentDirection = arg_rpy_stream
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Stream merged")
        End If
    End Sub

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_ope As RPOperation = CType(rpy_parent, RPOperation)
        Dim rpy_arg As RPArgument = rpy_parent_ope.addArgument(Me.Name)
        Return CType(rpy_arg, RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        ' No stereotype for Operation_Argument
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_arg As RPArgument = CType(rpy_elmt, RPArgument)
        rpy_arg.argumentDirection = Transform_SMT_Stream_To_Rpy_Stream(Me.Stream)
    End Sub

End Class
