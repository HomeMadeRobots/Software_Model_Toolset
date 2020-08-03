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

        Me.Soft_Mdl_Container.Import_From_Rhapsody_Model(
            Nothing,
            CType(rpy_project, RPModelElement))

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

    Sub Check_Consistency()
        Me.Consistency_Report = New Report
        Me.Soft_Mdl_Container.Check_Consistency(Me.Consistency_Report)
    End Sub

    Sub Generate_Consistency_Report(report_file_stream As StreamWriter)
        Me.Consistency_Report.Generate_Csv_Report(report_file_stream)
    End Sub

End Class
