Imports rhapsody2
Imports System.Xml.Serialization


Public MustInherit Class Software_Interface
    Inherits Software_Class

    Public Overrides Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of Classifier_Software_Element)
            Dim swct_list As List(Of Component_Type)
            swct_list = Me.Top_Package.Container.Get_All_Component_Types
            For Each swct In swct_list
                If Not IsNothing(swct.Provider_Ports) Then
                    For Each pport In swct.Provider_Ports
                        If pport.Contract_Ref = Me.UUID Then
                            If Not Me.Dependent_Elements.Contains(swct) Then
                                Me.Dependent_Elements.Add(swct)
                            End If
                        End If
                    Next
                End If
                If Not IsNothing(swct.Requirer_Ports) Then
                    For Each rport In swct.Requirer_Ports
                        If rport.Contract_Ref = Me.UUID Then
                            If Not Me.Dependent_Elements.Contains(swct) Then
                                Me.Dependent_Elements.Add(swct)
                            End If
                        End If
                    Next
                End If
            Next
        End If
        Return Me.Dependent_Elements
    End Function

End Class


Public Class Client_Server_Interface

    Inherits Software_Interface

    <XmlArrayItemAttribute(GetType(Synchronous_Operation)), _
     XmlArrayItemAttribute(GetType(Asynchronous_Operation)), _
     XmlArray("Operations")>
    Public Operations As List(Of Operation)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Operations) Then
                children_list.AddRange(Me.Operations)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

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

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If IsNothing(Me.Operations) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall provide at least one operation.")
        End If

    End Sub

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of Classifier_Software_Element)
            For Each current_ope In Me.Operations
                If Not IsNothing(current_ope.Arguments) Then
                    For Each arg In current_ope.Arguments
                        Dim data_type As Data_Type
                        data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                        If Not data_type.Is_Basic_Type Then
                            If Not Me.Needed_Elements.Contains(data_type) Then
                                Me.Needed_Elements.Add(data_type)
                            End If
                        End If
                    Next
                End If
            Next
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            For Each ope In Me.Operations
                Me.Weighted_Methods_Per_Class += 1
                If Not IsNothing(ope.Arguments) Then
                    For Each arg In ope.Arguments
                        Dim data_type As Data_Type
                        data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                        Me.Weighted_Methods_Per_Class += data_type.Get_Complexity
                    Next
                End If
            Next
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function
End Class


Public MustInherit Class Operation

    Inherits Software_Element

    Public Arguments As List(Of Operation_Argument)

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

End Class


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
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
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