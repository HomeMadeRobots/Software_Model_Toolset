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
            End Select

        End If

    End Sub

End Class
