Imports rhapsody2
Imports System.Windows
Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Public MustInherit Class Rpy_Controller
    Protected Rhapsody_App As RPApplication = Nothing

    Public Sub New()
        ' Get the opened Rhapsody application.
        Rhapsody_App = DirectCast(GetObject(Nothing, "Rhapsody2.Application"), RPApplication)
    End Sub

    Protected Function Get_Rhapsody_Project() As RPProject
        Dim selected_element As RPModelElement
        selected_element = Me.Rhapsody_App.getSelectedElement
        Dim rpy_sw_mdl As RPProject = Nothing
        If Not IsNothing(selected_element) Then
            If IsNothing(selected_element.owner) Then
                rpy_sw_mdl = CType(selected_element, RPProject)
            Else
                Me.Write_Csl_Line("A Rhapsody project shall be selected.")
            End If
        Else
            Me.Write_Csl_Line("A Rhapsody project shall be selected.")
        End If
        Return rpy_sw_mdl
    End Function

    Protected Shared Function Select_File(title As String, filter As String) As String
        Dim file_path As String = ""
        Dim dialog_box As OpenFileDialog
        dialog_box = New OpenFileDialog
        dialog_box.Title = title
        dialog_box.Filter = filter
        dialog_box.CheckFileExists = True
        Dim result As Forms.DialogResult = dialog_box.ShowDialog
        If result = Forms.DialogResult.OK Then
            file_path = dialog_box.FileName
        End If
        Return file_path
    End Function

    Protected Function Select_Directory(file_message As String) As String
        Dim output_directory As String = ""
        Me.Write_Csl("Select " & file_message & " directory...")
            Dim dialog_box As FolderBrowserDialog
            dialog_box = New FolderBrowserDialog
            dialog_box.Description = "Select " & file_message & " directory..."
            Dim result As Forms.DialogResult = dialog_box.ShowDialog()
            If result = Forms.DialogResult.OK Then
                output_directory = dialog_box.SelectedPath
                Me.Write_Csl_Line(" done.")
            End If
        Return output_directory
    End Function

    Protected Function Select_Directory(file_message As String, root As String) As String
        Dim output_directory As String = ""
        Me.Write_Csl("Select " & file_message & " directory...")
            Dim dialog_box As FolderBrowserDialog
            dialog_box = New FolderBrowserDialog
            dialog_box.Description = "Select " & file_message & " directory..."
            dialog_box.SelectedPath = root
            Dim result As Forms.DialogResult = dialog_box.ShowDialog()
            If result = Forms.DialogResult.OK Then
                output_directory = dialog_box.SelectedPath
                Me.Write_Csl_Line(" done.")
            End If
        Return output_directory
    End Function

    Protected Shared Function Get_Elapsed_Time(time As Stopwatch) As String
        Dim time_str As String
        time_str = "Elapsed time : " & _
                   time.Elapsed.Hours.ToString & "h " & _
                   time.Elapsed.Minutes.ToString & "m " & _
                   time.Elapsed.Seconds.ToString & "s."
        Return time_str
    End Function

    Protected Sub Clear_Window()
        Me.Rhapsody_App.clearOutputWindow("out")
    End Sub

    Protected Sub Write_Csl_Line(message As String)
        Me.Rhapsody_App.writeToOutputWindow("out", message & vbCrLf)
    End Sub

    Protected Sub Write_Csl(message As String)
        Me.Rhapsody_App.writeToOutputWindow("out", message)
    End Sub

    Shared Function Load_User_Record(file_name As String) As Dictionary(Of String, String)
        Dim user_record As New Dictionary(Of String, String)

        ' Get tool local folder 
        Dim tool_user_files_path As String
        tool_user_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        tool_user_files_path = tool_user_files_path & "\Software_Model_Toolset"

        ' Get record file full path
        Dim file_path As String
        file_path = tool_user_files_path & "\" & file_name

        ' Read data
        If File.Exists(file_path) = False Then
            'user_record = Nothing
        Else
            Dim stream_reader As FileIO.TextFieldParser
            stream_reader = New TextFieldParser(file_path)
            stream_reader.Delimiters = {";"}
            While stream_reader.EndOfData = False
                Dim line As String() = stream_reader.ReadFields
                user_record.Add(line(0), line(1))
            End While
        End If

        Return user_record

    End Function

    Shared Sub Save_User_Record(
        file_name As String,
        user_record As Dictionary(Of String, String))

        ' Get tool local folder 
        Dim tool_user_files_path As String
        tool_user_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        tool_user_files_path = tool_user_files_path & "\Software_Model_Toolset"
        If Not Directory.Exists(tool_user_files_path) Then
            Directory.CreateDirectory(tool_user_files_path)
        End If


        ' Get record file full path
        Dim file_path As String
        file_path = tool_user_files_path & "\" & file_name

        ' Create or replace file
        Dim stream_writer As StreamWriter
        stream_writer = New StreamWriter(file_path, False)

        For Each key In user_record.Keys
            stream_writer.WriteLine(key & ";" & user_record(key))
        Next

        stream_writer.Close()

    End Sub

End Class
