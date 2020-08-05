Imports rhapsody2
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text

Public Class Rpy_Software_Model

    Private Soft_Mdl_Container As Software_Model_Container = Nothing
    Private Rpy_Project As RPProject = Nothing
    Private Consistency_Report As Report

    Public Sub Load_From_Rhapsody_Model(rpy_project As RPProject)

        Me.Rpy_Project = rpy_project
        Me.Soft_Mdl_Container = New Software_Model_Container

        Me.Soft_Mdl_Container.Import_All_From_Rhapsody_Model(rpy_project)

    End Sub

    Public Sub Create_Xml(xml_file_stream As FileStream)

        If Not IsNothing(Me.Soft_Mdl_Container) Then

            ' Initialize XML writer
            Dim writer As XmlTextWriter
            writer = New XmlTextWriter(xml_file_stream, Encoding.UTF8)
            writer.Indentation = 2
            writer.IndentChar = CChar(" ")
            writer.Formatting = Formatting.Indented

            ' Serialize model
            Dim serializer As New XmlSerializer(GetType(Software_Model_Container))
            serializer.Serialize(writer, Me.Soft_Mdl_Container)

            ' Close writter
            writer.Close()

        End If

    End Sub

    Public Sub Check_Consistency()
        Me.Consistency_Report = New Report
        Me.Soft_Mdl_Container.Check_Consistency(Me.Consistency_Report)
    End Sub

    Public Sub Generate_Consistency_Report(report_file_stream As StreamWriter)
        Me.Consistency_Report.Generate_Csv_Report(report_file_stream)
    End Sub

    Public Function Has_Error() As Boolean
        If Me.Consistency_Report.Get_Error_Number > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Compute_Metrics()
        Me.Soft_Mdl_Container.Compute_Metrics()
    End Sub

    Public Sub Generate_Metrics_Report(file_stream As StreamWriter)

        Add_Seperator(file_stream)
        file_stream.WriteLine("Metrics report : " & Me.Soft_Mdl_Container.Name)
        Add_Seperator(file_stream)


        For Each pkg In Me.Soft_Mdl_Container.PSWA_Packages

            file_stream.WriteLine()
            Add_Seperator(file_stream)

            file_stream.WriteLine("PSWA_Package : " & pkg.Name)
            file_stream.WriteLine()

            file_stream.WriteLine("Documentation rate : " &
                (pkg.Get_Documentation_Rate * 100).ToString("0") & "%")

            'file_stream.WriteLine("Needed PSWA_Packages : ")
            'Dim needed_pkg_list As List(Of Top_Level_PSWA_Package)
            'needed_pkg_list = pkg.Get_Needed_Top_Packages_List
            'For Each needed_pkg In needed_pkg_list
            '    file_stream.WriteLine("    " & needed_pkg.Name)
            'Next
            'file_stream.WriteLine()

            file_stream.WriteLine()
            file_stream.WriteLine("Interfaces : ")
            Dim pkg_list As List(Of PSWA_Package) = pkg.Get_All_Packages
            For Each current_pkg In pkg_list

                If Not IsNothing(current_pkg.Software_Interfaces) Then
                    For Each sw_if In current_pkg.Software_Interfaces
                        file_stream.WriteLine()
                        file_stream.WriteLine("    " & sw_if.Name)
                        file_stream.WriteLine("        WMC : " & sw_if.Compute_WMC())

                    Next
                End If
            Next

            file_stream.WriteLine()
            file_stream.WriteLine("Component_Types : ")
            For Each current_pkg In pkg_list
                If Not IsNothing(current_pkg.Component_Types) Then
                    For Each swct In current_pkg.Component_Types
                        file_stream.WriteLine()
                        file_stream.WriteLine("    " & swct.Name)
                        file_stream.WriteLine("        WMC : " & swct.Compute_WMC())
                    Next
                End If
            Next

        Next
    End Sub

    Private Sub Add_Seperator(file_stream As StreamWriter)
        file_stream.WriteLine("===============================================================")
    End Sub

End Class
