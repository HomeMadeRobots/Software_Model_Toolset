Imports System.IO
Imports System.Windows

Public Class Consistency_Check_Form
    Inherits Package_Selection_Form

    Private Previous_Report_Panel As New Panel
    Private WithEvents Use_Previous_Report_Checkbox As New CheckBox
    Private Previous_Report_TxtBx As New TextBox
    Private WithEvents Previous_Report_Button As New Button
    Private Report_Directory_TxtBx As New TextBox
    Private WithEvents Report_Directory_Button As New Button

    Private Const Path_Button_Width As Integer = 40

    Public Sub New(
        pkg_name_list As List(Of String),
        previous_pkg_selection_choices As Dictionary(Of String, String),
        use_previous_report As Boolean,
        default_prev_report_path As String,
        default_report_directory As String)

        MyBase.New(pkg_name_list, previous_pkg_selection_choices)

        Dim item_y_pos = Panel_Height + Marge * 2
        Dim inner_item_y_pos = Marge

        '------------------------------------------------------------------------------------------'
        ' Add previous report panel
        Me.Previous_Report_Panel.Location = New Point(Marge, item_y_pos)
        Me.Previous_Report_Panel.Size = New Size(Panel_Width, 100)
        Me.Previous_Report_Panel.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(Me.Previous_Report_Panel)

        item_y_pos += Me.Previous_Report_Panel.Height + Marge

        Dim previous_report_label As New Label
        previous_report_label.Text = "Previous report selection"
        previous_report_label.Location = New Point(Marge, inner_item_y_pos)
        previous_report_label.Size = Text_Field_Size
        Me.Previous_Report_Panel.Controls.Add(previous_report_label)
        inner_item_y_pos += previous_report_label.Height + Marge

        Me.Use_Previous_Report_Checkbox.Location = New Point(Marge, inner_item_y_pos)
        Me.Use_Previous_Report_Checkbox.Size = Text_Field_Size
        Me.Use_Previous_Report_Checkbox.Text = "Merge warnings analysis with previous report"
        Me.Previous_Report_Panel.Controls.Add(Me.Use_Previous_Report_Checkbox)
        inner_item_y_pos += Me.Use_Previous_Report_Checkbox.Height + Marge

        Me.Previous_Report_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Previous_Report_TxtBx.Size = New Size(Item_Width - Marge - Path_Button_Width,
            Text_Field_Height)
        Me.Previous_Report_TxtBx.Text = default_prev_report_path
        Me.Previous_Report_Panel.Controls.Add(Me.Previous_Report_TxtBx)

        Dim path_button_x_pos As Integer = Me.Previous_Report_TxtBx.Width + 2 * Marge
        Me.Previous_Report_Button.Location = New Point(path_button_x_pos, inner_item_y_pos)
        Me.Previous_Report_Button.Size = New Size(Path_Button_Width, 2 * Marge)
        Me.Previous_Report_Button.Text = "..."
        Me.Previous_Report_Panel.Controls.Add(Me.Previous_Report_Button)

        '------------------------------------------------------------------------------------------'
        ' Add report directory panel
        Dim report_directory_panel As New Panel
        report_directory_panel.Location = New Point(Marge, item_y_pos)
        report_directory_panel.Size = New Size(Panel_Width, 70)
        report_directory_panel.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(report_directory_panel)

        item_y_pos += report_directory_panel.Height + Marge
        inner_item_y_pos = Marge

        Dim report_directory_label As New Label
        report_directory_label.Text = "Report directory selection"
        report_directory_label.Location = New Point(Marge, inner_item_y_pos)
        report_directory_label.Size = Text_Field_Size
        report_directory_panel.Controls.Add(report_directory_label)
        inner_item_y_pos += report_directory_label.Height + Marge

        Me.Report_Directory_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Report_Directory_TxtBx.Size = New Size(Item_Width - Marge - Path_Button_Width,
            Text_Field_Height)
        Me.Report_Directory_TxtBx.Text = default_report_directory
        report_directory_panel.Controls.Add(Me.Report_Directory_TxtBx)

        Me.Report_Directory_Button.Location = New Point(path_button_x_pos, inner_item_y_pos)
        Me.Report_Directory_Button.Size = New Size(Path_Button_Width, 2 * Marge)
        Me.Report_Directory_Button.Text = "..."
        report_directory_panel.Controls.Add(Me.Report_Directory_Button)

        '------------------------------------------------------------------------------------------'
        ' Design OK button
        Me.OK_Button.Text = "Check"
        OK_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Me.OK_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Use_Previous_Report_Checkbox.Checked = use_previous_report
        Me.Previous_Report_TxtBx.Enabled = use_previous_report
        Me.Previous_Report_Button.Enabled = use_previous_report
        Me.Text = "Consistency check"
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Is_Merge_Requested() As Boolean
        Return Me.Use_Previous_Report_Checkbox.Checked
    End Function

    Public Function Get_Previous_Report_Path() As String
        Return Me.Previous_Report_TxtBx.Text
    End Function

    Public Function Get_Report_Directory() As String
        Return Me.Report_Directory_TxtBx.Text
    End Function

    Protected Overrides Sub Selection_Done() Handles OK_Button.Click
        If Directory.Exists(Me.Report_Directory_TxtBx.Text) Then
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        Else
            MsgBox("The directory to save the report shall exist !",
                MsgBoxStyle.Exclamation,
                "Wrong directory")
        End If
    End Sub

    Private Sub Previous_Report_Path_Button_Clicked() Handles Previous_Report_Button.Click
        Dim dialog_box As OpenFileDialog
        dialog_box = New OpenFileDialog
        If File.Exists(Me.Previous_Report_TxtBx.Text) Then
            dialog_box.FileName = Me.Previous_Report_TxtBx.Text
        Else
            dialog_box.InitialDirectory =
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        End If
        dialog_box.Title = "Select report to merge"
        Dim result As DialogResult = dialog_box.ShowDialog()
        If result = DialogResult.OK Then
            Me.Previous_Report_TxtBx.Text = dialog_box.FileName
        End If
    End Sub

    Private Sub Use_Previous_Report_Checkbox_Clicked() Handles _
        Use_Previous_Report_Checkbox.CheckedChanged
        If Me.Use_Previous_Report_Checkbox.Checked = True Then
            Me.Previous_Report_TxtBx.Enabled = True
            Me.Previous_Report_Button.Enabled = True
        Else
            Me.Previous_Report_TxtBx.Enabled = False
            Me.Previous_Report_Button.Enabled = False
        End If
    End Sub

    Private Sub Report_Directory_Button_Clicked() Handles Report_Directory_Button.Click
        Dim dialog_box As FolderBrowserDialog
        dialog_box = New FolderBrowserDialog
        If Directory.Exists(Me.Report_Directory_TxtBx.Text) Then
            dialog_box.SelectedPath = Me.Report_Directory_TxtBx.Text
        Else
            dialog_box.SelectedPath =
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        End If
        dialog_box.Description = "Select consistency check report directory"
        Dim result As Forms.DialogResult = dialog_box.ShowDialog()
        If result = Forms.DialogResult.OK Then
            Me.Report_Directory_TxtBx.Text = dialog_box.SelectedPath
        End If
    End Sub

End Class
