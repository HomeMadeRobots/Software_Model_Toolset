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

                Case "Merge_Rpy_Soft_Model"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Import_And_Merge_By_Name()

                Case "Check_Rpy_Soft_Model"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Check_Rpy_Soft_Model()

                Case "Compute_Rpy_Soft_Model_Metrics"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Compute_Rpy_Soft_Model_Metrics()

                Case "Generate_All"
                    Dim soft_mdl_app As Rpy_Software_Model_Controller
                    soft_mdl_app = New Rpy_Software_Model_Controller
                    soft_mdl_app.Generate_All()

            End Select
        Catch ex As Exception
            MsgBox("Software_Model_Tool : an error occured." & vbCrLf & vbCrLf & ex.Message)
        End Try
    End Sub

End Module
