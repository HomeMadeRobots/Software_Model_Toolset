Public Class Configuration_Parameter

    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Configuration_Parameter", "Attribute")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Not Me.Base_Data_Type_Ref.Equals(Guid.Empty) Then

            Dim config_data_type As Data_Type
            config_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            Select Case config_data_type.GetType
                Case GetType(Structured_Data_Type)
                    Me.Add_Consistency_Check_Error_Item(report,
                        "PARAM_1",
                        "Type shall not be Structured_Data_Type.")
                Case GetType(Array_Data_Type)
                    Dim base_type As Software_Element
                    Dim array_type As Array_Data_Type = CType(config_data_type, Array_Data_Type)
                    base_type = Me.Get_Element_By_Uuid(array_type.Base_Data_Type_Ref)
                    If Not IsNothing(base_type) Then
                        Select Case base_type.GetType
                            Case GetType(Structured_Data_Type)
                                Me.Add_Consistency_Check_Error_Item(report,
                                    "PARAM_2",
                                    "Type shall not be an array of structure.")
                            Case GetType(Array_Data_Type)
                                Me.Add_Consistency_Check_Error_Item(report,
                                    "PARAM_3",
                                    "Type shall not be an array of array.")
                        End Select
                    End If
            End Select

        End If

    End Sub

End Class
