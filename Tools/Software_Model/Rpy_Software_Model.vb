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
        file_stream.WriteLine()
        file_stream.WriteLine()

        Add_Seperator(file_stream)
        file_stream.WriteLine("Project metrics")
        file_stream.WriteLine()
        file_stream.WriteLine(
            "Number of PSWA_Packages : " &
            Me.Soft_Mdl_Container.PSWA_Packages.Count)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Nb_Data_Types_Series,
            "Number of Data_Types")
        file_stream.WriteLine("    tot : " & Me.Soft_Mdl_Container.Get_Nb_Data_Types_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Nb_Interfaces_Series,
            "Number of Interfaces")
        file_stream.WriteLine("    tot : " & Me.Soft_Mdl_Container.Get_Nb_Interfaces_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Nb_Component_Types_Series,
            "Number of Component_Types")
            file_stream.WriteLine(
                "    tot : " & Me.Soft_Mdl_Container.Get_Nb_Component_Types_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Documentation_Rate_Series,
            "Documentation rate")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Distance_Series,
            "Distance")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Component_Type_WMC_Series,
            "Component_Type WMC")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Soft_Mdl_Container.Get_Interfaces_WMC_Series,
            "Interfaces WMC")
        Add_Seperator(file_stream)
        file_stream.WriteLine()

        For Each pkg In Me.Soft_Mdl_Container.PSWA_Packages

            file_stream.WriteLine()
            Add_Seperator(file_stream)

            file_stream.WriteLine("PSWA_Package : " & pkg.Name)
            file_stream.WriteLine()

            file_stream.WriteLine("Documentation rate : " &
                pkg.Get_Package_Documentation_Rate.ToString("p0"))
            file_stream.WriteLine()

            file_stream.WriteLine("Number of Data_Types : " & pkg.Get_Nb_Data_Types)
            file_stream.WriteLine("Number of Interfaces : " & pkg.Get_Nb_Interfaces)
            file_stream.WriteLine("Number of Component_Types : " & pkg.Get_Nb_Component_Types)
            file_stream.WriteLine("Number of Compositions : " & pkg.Get_Nb_Compositions)
            file_stream.WriteLine("Abstraction level : " _
                & pkg.Get_Abstraction_Level.ToString("0.00"))
            file_stream.WriteLine()

            file_stream.WriteLine("Efferent coupling : " & pkg.Get_Efferent_Coupling)
            file_stream.WriteLine("Afferent coupling : " & pkg.Get_Afferent_Coupling)
            file_stream.WriteLine("Instability : " & pkg.Get_Instability.ToString("0.00"))
            file_stream.WriteLine()

            file_stream.WriteLine("Distance : " & pkg.Get_Distance.ToString("0.00"))

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

    Private Sub Write_Series_Metrics(
        file_stream As StreamWriter,
        series As Data_Series,
        series_name As String)
        file_stream.WriteLine(series_name & " : ")
        file_stream.WriteLine("    avg : " & series.Get_Average.ToString("0.00"))
        file_stream.WriteLine("    min : " & series.Get_Min.ToString("0.00"))
        file_stream.WriteLine("    max : " & series.Get_Max.ToString("0.00"))
        file_stream.WriteLine("    dev : " & series.Get_Standard_Deviation.ToString("0.00"))
    End Sub

End Class
