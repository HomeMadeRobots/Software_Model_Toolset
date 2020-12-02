Imports rhapsody2
Imports System.Xml.Serialization
Imports System.IO


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
    Public Operations As New List(Of Client_Server_Operation)

    Private Has_Asynchronous_Operation As Boolean = False

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

    Public Function Is_My_Operation(operation_uuid As Guid) As Boolean
        Dim got_it As Boolean = False
        For Each ope In Me.Get_All_Operations()
            If ope.UUID = operation_uuid Then
                got_it = True
                Exit For
            End If
        Next
        Return got_it
    End Function

    ' Returns all the operations defined by an interface including the inherited operations.
    Public Function Get_All_Operations() As List(Of Operation_With_Arguments)
        Dim all_operations As New List(Of Operation_With_Arguments)
        all_operations.AddRange(Me.Operations)
        Dim base_ref As Guid = Me.Base_Class_Ref
        While base_ref <> Guid.Empty
            Dim base_cs_if As Client_Server_Interface
            base_cs_if = CType(Me.Get_Element_By_Uuid(base_ref), Client_Server_Interface)
            If Not IsNothing(base_cs_if) Then
                all_operations.AddRange(base_cs_if.Operations)
                base_ref = base_cs_if.Base_Class_Ref
            Else
                Exit While
            End If
        End While
        Return all_operations
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
                Me.Has_Asynchronous_Operation = True
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
                    If Not IsNothing(data_type) Then
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Transform_To_CLOOF(parent_folder_path As String)
        Dim file_stream As StreamWriter = Me.Create_C_Header_File_Stream_Writer(parent_folder_path)
        Me.Add_C_Multiple_Inclusion_Guard(file_stream)
        Me.Add_CLOOF_Inclusion_Directives(file_stream)
        If Me.Has_Asynchronous_Operation = True Then
            file_stream.WriteLine("#include ""Asynchronous_Operation_Manager.h""")
            file_stream.WriteLine("")
        End If
        Me.Add_C_Title(file_stream)
        file_stream.WriteLine("typedef struct {")
        For Each op In Me.Operations
            op.Create_CLOOF_Declaration(file_stream, "")
        Next
        file_stream.WriteLine("} " & Me.Name & ";")
        file_stream.WriteLine()
        Me.Finish_C_Multiple_Inclusion_Guard(file_stream)
        file_stream.Close()
    End Sub

End Class


Public MustInherit Class Client_Server_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation

End Class


Public Class Synchronous_Operation
    Inherits Client_Server_Operation

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Synchronous_Operation", "Operation")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
     Public Overrides Sub Create_CLOOF_Prototype(file_stream As StreamWriter, class_id As String)
        file_stream.Write("    void (*" & Me.Name & ") ( ")
        If Me.Arguments.Count = 0 Then
            file_stream.Write("void")
        Else
            file_stream.WriteLine("")
            Dim is_last As Boolean = False
            For Each arg In Me.Arguments
                If arg Is Me.Arguments.Last Then
                    is_last = True
                End If
                arg.Transform_To_CLOOF(file_stream, is_last, 2)
            Next
        End If
        file_stream.Write(" )")
    End Sub

End Class


Public Class Asynchronous_Operation
    Inherits Client_Server_Operation

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Asynchronous_Operation", "Operation")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Create_CLOOF_Prototype(file_stream As StreamWriter, class_id As String)
        file_stream.WriteLine("    void (*" & Me.Name & ") ( ")
        file_stream.Write("        const Asynchronous_Operation_Manager* async_op_mgr")
        If Me.Arguments.Count = 0 Then
            file_stream.Write(" )")
        Else
            file_stream.WriteLine(",")
            Dim is_last As Boolean = False
            For Each arg In Me.Arguments
                If arg Is Me.Arguments.Last Then
                    is_last = True
                End If
                arg.Transform_To_CLOOF(file_stream, is_last, 2)
            Next
            file_stream.Write(" )")
        End If
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
                If Not IsNothing(data_type) Then
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Transform_To_CLOOF(parent_folder_path As String)
        Dim file_stream As StreamWriter = Me.Create_C_Header_File_Stream_Writer(parent_folder_path)
        Me.Add_C_Multiple_Inclusion_Guard(file_stream)
        Me.Add_CLOOF_Inclusion_Directives(file_stream)
        Me.Add_C_Title(file_stream)
        file_stream.Write("typedef void (*" & Me.Name & ")")
        If Me.Arguments.Count = 0 Then
            file_stream.WriteLine("(void);")
        Else
            file_stream.WriteLine(" (")
            Dim is_last As Boolean = False
            For Each arg In Me.Arguments
                If arg Is Me.Arguments.Last Then
                    is_last = True
                End If
                arg.Transform_To_CLOOF(file_stream, is_last)
            Next
            file_stream.WriteLine(" );")
        End If
        file_stream.WriteLine()
        Me.Finish_C_Multiple_Inclusion_Guard(file_stream)
        file_stream.Close()
    End Sub

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


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Sub Transform_To_CLOOF(file_stream As StreamWriter, is_last As Boolean)
        Dim arg_dt As Data_Type
        arg_dt = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        file_stream.Write("    " & _
            arg_dt.Get_CLOOF_Arg_Type_Declaration(Operation_Argument.E_STREAM.INPUT))
        If is_last = True Then
            file_stream.Write(" " & Me.Name)
        Else
            file_stream.WriteLine(" " & Me.Name & ",")
        End If
    End Sub

End Class