Imports rhapsody2
Imports System.Xml.Serialization


Public MustInherit Class Software_Interface
    Inherits SMM_Class

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = MyBase.Find_Dependent_Elements()
            Dim swct_list As List(Of Component_Type)
            swct_list = Me.Container.Get_All_Component_Types
            For Each swct In swct_list
                For Each pport In swct.Provider_Ports
                    If pport.Contract_Ref = Me.UUID Then
                        If Not Me.Dependent_Elements.Contains(swct) Then
                            Me.Dependent_Elements.Add(swct)
                        End If
                    End If
                Next
                For Each rport In swct.Requirer_Ports
                    If rport.Contract_Ref = Me.UUID Then
                        If Not Me.Dependent_Elements.Contains(swct) Then
                            Me.Dependent_Elements.Add(swct)
                        End If
                    End If
                Next
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
    Public Operations As New List(Of Operation_With_Arguments)

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Operations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Client_Server_Interface(rpy_element)
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
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
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Client_Server_Interface", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Operations.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "CSIF_1",
                "Shall provide at least one operation.")
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = MyBase.Find_Needed_Elements()
            For Each current_ope In Me.Operations
                For Each arg In current_ope.Arguments
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
                    End If
                Next
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


Public Class Synchronous_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Synchronous_Operation", "Operation")
    End Sub

End Class


Public Class Asynchronous_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Asynchronous_Operation", "Operation")
    End Sub

End Class


Public Class Event_Interface

    Inherits Software_Interface

    Public Arguments As New List(Of Event_Argument)

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Arguments)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Event_Interface(rpy_element)
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_event_arg As RPAttribute
        For Each rpy_event_arg In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Event_Argument(CType(rpy_event_arg, RPModelElement)) Then
                Dim arg As Event_Argument = New Event_Argument
                Me.Arguments.Add(arg)
                arg.Import_From_Rhapsody_Model(Me, CType(rpy_event_arg, RPModelElement))
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Interface", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
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
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            Me.Weighted_Methods_Per_Class = 1
            For Each arg In Me.Arguments
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                Me.Weighted_Methods_Per_Class += data_type.Get_Complexity
            Next
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function

End Class


Public Class Event_Argument

    Inherits Typed_Software_Element

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Attribute"
    End Function

    Protected Overrides Function Create_Rpy_Element(
        rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return CType(rpy_parent_class.addAttribute(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Argument", "Attribute")
    End Sub

End Class