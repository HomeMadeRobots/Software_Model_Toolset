Imports System.Windows
Imports System.IO

Public Class Rhapsody_Project_Configuration_Form

    Inherits Form

    Private Controller As Rpy_Project_Controller

    Private Class Configured_Element_View

        Private Name As String
        Private WithEvents To_Be_Configured_Check_Box As New CheckBox
        Private WithEvents Status_Button As New Button
        Private Status_Message As String

        Public Sub New(name As String, vertical_pos As Integer, owner As Form)
            Me.Name = name
            Me.To_Be_Configured_Check_Box.Location = New Point(Marge, vertical_pos)
            Me.To_Be_Configured_Check_Box.Size = ChckBx_Size
            Me.To_Be_Configured_Check_Box.Text = name
            owner.Controls.Add(Me.To_Be_Configured_Check_Box)

            Me.Status_Button.Location = New Point(Button_X_Pos, vertical_pos)
            Me.Status_Button.Size = Bttn_Size
            Me.Status_Button.Visible = False
            owner.Controls.Add(Me.Status_Button)
        End Sub

        Public Sub Update_Status_View(
            status As String,
            message As String)
            Me.Status_Message = message
            Me.Update_Status_Button(status)
        End Sub

        Public Sub Reset_Status()
            Me.Status_Button.Visible = False
            Me.Status_Message = ""
        End Sub

        Public Function Get_Configuration_Request() As Boolean
            Return Me.To_Be_Configured_Check_Box.Checked
        End Function

        Public Sub Set_Configuration_Request(conf_is_requested As Boolean)
            Me.To_Be_Configured_Check_Box.Checked = conf_is_requested
        End Sub

        Public Function Get_Check_Box() As CheckBox
            Return Me.To_Be_Configured_Check_Box
        End Function

        Private Sub Status_Button_Clicked() Handles Status_Button.Click
            MsgBox(
                Me.Status_Message,
                MsgBoxStyle.Information,
                Me.Name & " configuration status")
        End Sub

        Private Sub Update_Status_Button(
            status As String)
            Me.Status_Button.Visible = True
            Select Case status
                Case "OK"
                    Me.Status_Button.Text = "OK"
                    Me.Status_Button.ForeColor = Color.Green
                Case "Error"
                    Me.Status_Button.Text = "Error"
                    Me.Status_Button.ForeColor = Color.Red
            End Select
        End Sub

    End Class

    Private WithEvents Toolset_Path_TxtBx As New TextBox
    Private WithEvents Toolset_Path_Button As New Button

    Private Profiles_Configuration As Configured_Element_View
    Private Helpers_Configuration As Configured_Element_View
    Private Activity_Diagrams_Configuration As Configured_Element_View
    Private Packages_Are_Not_Unit_Configuration As Configured_Element_View
    Private Accessible_Types_Configuration As Configured_Element_View

    Private WithEvents Configure_Button As New Button

    Private Const Form_Width As Integer = 500
    Private Const Marge As Integer = 20
    Private Const Item_Height As Integer = 20
    Private Const Item_Width As Integer = Form_Width - 3 * Marge - Button_Width
    Private Const Button_Width As Integer = 2 * Marge
    Private Const Button_X_Pos As Integer = Item_Width + 2 * Marge
    Private Shared ChckBx_Size As New Size(Item_Width, Item_Height)
    Private Shared Bttn_Size As New Size(Button_Width, Item_Height)

    Public Sub New()
        Dim item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add Toolset path selection stuff
        Dim toolset_path_title As New Label
        toolset_path_title.Text = "Software_Model_Toolset path :"
        toolset_path_title.Location = New Point(Marge, item_y_pos)
        toolset_path_title.Size = ChckBx_Size
        Me.Controls.Add(toolset_path_title)
        item_y_pos += Item_Height

        Me.Toolset_Path_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Toolset_Path_TxtBx.Size = New Size(Item_Width, Item_Height)
        Me.Toolset_Path_TxtBx.Enabled = False
        Me.Controls.Add(Me.Toolset_Path_TxtBx)

        Me.Toolset_Path_Button.Location = New Point(Button_X_Pos, item_y_pos)
        Me.Toolset_Path_Button.Size = Bttn_Size
        Me.Toolset_Path_Button.Text = "..."
        Me.Toolset_Path_Button.Enabled = False
        Me.Controls.Add(Me.Toolset_Path_Button)

        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add items to configure
        Dim configuration_list_title As New Label
        configuration_list_title.Text = "Configurable items :"
        configuration_list_title.Location = New Point(Marge, item_y_pos)
        configuration_list_title.Size = ChckBx_Size
        Me.Controls.Add(configuration_list_title)
        item_y_pos += Item_Height

        ' Add profiles configuration stuff
        Me.Profiles_Configuration = New Configured_Element_View("Profiles", item_y_pos, Me)
        Add_Click_Handlers_For_Config_Needed_Toolset_Path(Me.Profiles_Configuration)
        item_y_pos += Item_Height

        ' Add Rhapsody helpers configuration stuff
        Me.Helpers_Configuration = New Configured_Element_View("Helpers", item_y_pos, Me)
        Add_Click_Handlers_For_Config_Needed_Toolset_Path(Me.Helpers_Configuration)
        item_y_pos += Item_Height

        ' Add activity diagram configuration stuff
        Me.Activity_Diagrams_Configuration = New Configured_Element_View(
            "Activity diagrams", item_y_pos, Me)
        item_y_pos += Item_Height

        ' Add Rhapsody project views configuration stuff
        Me.Packages_Are_Not_Unit_Configuration = New Configured_Element_View(
            "Packages are not unit",
            item_y_pos, Me)
        item_y_pos += Item_Height

        Me.Accessible_Types_Configuration = New Configured_Element_View(
            "Accessible types", item_y_pos, Me)
        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add main button
        Me.Configure_Button.Text = "Configure"
        Me.Controls.Add(Me.Configure_Button)
        Me.Configure_Button.Size = New Size(100, Item_Height * 2)
        Me.Configure_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Me.Configure_Button.Size.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Configure Rhapsody project for Software_Model_Toolset"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FontHeight = 40

    End Sub

    Public Shared Function Load_Form(
        ctrl As Rpy_Project_Controller,
        default_toolset_path As String,
        conf_profiles As Boolean,
        conf_helpers As Boolean,
        conf_activity_diagrams As Boolean,
        conf_package As Boolean,
        conf_accessible_types As Boolean) As Rhapsody_Project_Configuration_Form

        Dim conf_form As New Rhapsody_Project_Configuration_Form
        conf_form.Controller = ctrl
        conf_form.Toolset_Path_TxtBx.Text = default_toolset_path

        conf_form.Profiles_Configuration.Set_Configuration_Request(conf_profiles)
        conf_form.Helpers_Configuration.Set_Configuration_Request(conf_helpers)
        conf_form.Activity_Diagrams_Configuration.Set_Configuration_Request(conf_activity_diagrams)
        conf_form.Packages_Are_Not_Unit_Configuration.Set_Configuration_Request(conf_package)
        conf_form.Accessible_Types_Configuration.Set_Configuration_Request(conf_accessible_types)

        conf_form.Manage_Toolset_Path_Visibility()

        Return conf_form
    End Function

    Public Sub Update_Profiles_Configuration_Status(status As String, message As String)
        Me.Profiles_Configuration.Update_Status_View(status, message)
    End Sub

    Public Sub Update_Helpers_Configuration_Status(status As String, message As String)
        Me.Helpers_Configuration.Update_Status_View(status, message)
    End Sub

    Public Sub Update_Act_Diagrams_Configuration_Status(status As String, message As String)
        Me.Activity_Diagrams_Configuration.Update_Status_View(status, message)
    End Sub

    Public Sub Update_Pkg_Are_Not_Unit_Configuration_Status(status As String, message As String)
        Me.Packages_Are_Not_Unit_Configuration.Update_Status_View(status, message)
    End Sub

    Public Sub Update_Accessible_Types_Configuration_Status(status As String, message As String)
        Me.Accessible_Types_Configuration.Update_Status_View(status, message)
    End Sub

    Public Sub Reset_Status()
        Me.Profiles_Configuration.Reset_Status()
        Me.Helpers_Configuration.Reset_Status()
        Me.Activity_Diagrams_Configuration.Reset_Status()
        Me.Packages_Are_Not_Unit_Configuration.Reset_Status()
        Me.Accessible_Types_Configuration.Reset_Status()
    End Sub

    Private Sub Toolset_Path_Selection_Button_Clicked() Handles Toolset_Path_Button.Click
        Dim dialog_box As FolderBrowserDialog
        dialog_box = New FolderBrowserDialog
        If Directory.Exists(Me.Toolset_Path_TxtBx.Text) Then
            dialog_box.SelectedPath = Me.Toolset_Path_TxtBx.Text
        Else
            dialog_box.SelectedPath =
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        End If
        dialog_box.Description = "Select Software_Model_Toolset directory"
        Dim result As Forms.DialogResult = dialog_box.ShowDialog()
        If result = Forms.DialogResult.OK Then
            Me.Toolset_Path_TxtBx.Text = dialog_box.SelectedPath
        End If
    End Sub

    Private Sub Manage_Toolset_Path_Visibility()
        If Me.Profiles_Configuration.Get_Configuration_Request = False _
            And Me.Helpers_Configuration.Get_Configuration_Request = False Then
            Me.Toolset_Path_TxtBx.Enabled = False
            Me.Toolset_Path_Button.Enabled = False
        Else
            Me.Toolset_Path_TxtBx.Enabled = True
            Me.Toolset_Path_Button.Enabled = True
        End If
    End Sub

    Private Sub Configure_Clicked() Handles Configure_Button.Click
        Me.Controller.Configure(
            Me.Toolset_Path_TxtBx.Text,
            Me.Profiles_Configuration.Get_Configuration_Request,
            Me.Helpers_Configuration.Get_Configuration_Request,
            Me.Activity_Diagrams_Configuration.Get_Configuration_Request,
            Me.Packages_Are_Not_Unit_Configuration.Get_Configuration_Request,
            Me.Accessible_Types_Configuration.Get_Configuration_Request)
    End Sub

    Private Sub Add_Click_Handlers_For_Config_Needed_Toolset_Path(ctl As Configured_Element_View)
        AddHandler ctl.Get_Check_Box.Click, _
        AddressOf Click_Event_Handler_For_Config_Needed_Toolset_Path
    End Sub

    Private Sub Click_Event_Handler_For_Config_Needed_Toolset_Path(sender As Object, e As EventArgs)
        Me.Manage_Toolset_Path_Visibility()
    End Sub

End Class
