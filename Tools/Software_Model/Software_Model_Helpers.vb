Imports rhapsody2
Imports System.IO

Module Software_Model_Helpers

    Public Sub main()

        Dim function_to_call As String
        function_to_call = My.Application.CommandLineArgs.Item(0)

        Try
            Select Case function_to_call

                Case "Export_Rpy_Soft_Model_To_Xml"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Export_Rpy_Soft_Model_To_Xml()

                Case "Check_Rpy_Soft_Model"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Check_Rpy_Soft_Model()

            End Select
        Catch ex As Exception
            MsgBox("Software_Model_Tool : an error occured." & vbCrLf & vbCrLf & ex.Message)
        End Try
    End Sub

End Module
