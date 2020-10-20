Public Class Package_Selection_Form
    Inherits Form

    Private Package_Selection_Panel As New Panel

    Private Pkg_CheckBox_Panel As New Panel
    Protected WithEvents OK_Button As New Button
    Private WithEvents Select_All_Button As New Button
    Private WithEvents Unselect_All_Button As New Button
    Private Pkg_CheckBox_List As New List(Of CheckBox)

    Protected Const Form_Width As Integer = 500
    Protected Const Marge As Integer = 10

    Protected Const Panel_Width As Integer = Form_Width - 2 * Marge
    Protected Const Panel_Height As Integer = 500
    Protected Const Item_Width As Integer = Panel_Width - 2 * Marge

    Protected Const Text_Field_Height As Integer = 20
    Protected Shared Text_Field_Size As New Size(Item_Width, Text_Field_Height)

    Private Shared CheckBox_Size As New Size(Item_Width - 40, Text_Field_Height) ' -40 : scroll bar

    Protected Const Button_Width As Integer = 100
    Protected Const Button_Height As Integer = 2 * Marge
    Protected Shared Button_Size As New Size(Button_Width, Button_Height)

    Public Sub New(
        pkg_name_list As List(Of String),
        previous_pkg_selection_choices As Dictionary(Of String, String))

        Dim item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add package selection panel
        Me.Package_Selection_Panel.Location = New Point(Marge, Marge)
        Me.Package_Selection_Panel.BorderStyle = BorderStyle.FixedSingle
        Me.Package_Selection_Panel.Size = New Size(Panel_Width, Panel_Height)
        Me.Controls.Add(Me.Package_Selection_Panel)
        Dim inner_item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add package selection label
        Dim package_selection_label As New Label
        package_selection_label.Text = "Top level packages selection"
        package_selection_label.Location = New Point(Marge, inner_item_y_pos)
        package_selection_label.Size = Text_Field_Size
        Me.Package_Selection_Panel.Controls.Add(package_selection_label)
        inner_item_y_pos += package_selection_label.Height + Marge


        ' Add select/unselect all button
        Me.Select_All_Button.Text = "Select all"
        Me.Package_Selection_Panel.Controls.Add(Me.Select_All_Button)
        Me.Select_All_Button.Size = Button_Size
        Me.Select_All_Button.Location = New Point(Marge, inner_item_y_pos)

        Me.Unselect_All_Button.Text = "Unselect all"
        Me.Package_Selection_Panel.Controls.Add(Me.Unselect_All_Button)
        Me.Unselect_All_Button.Size = Button_Size
        Me.Unselect_All_Button.Location = New Point(Panel_Width - Button_Width - Marge,
            inner_item_y_pos)

        inner_item_y_pos += Button_Height + Marge

        ' Add package selection checkboxes panel
        Me.Pkg_CheckBox_Panel.AutoScroll = True
        Me.Pkg_CheckBox_Panel.Location = New Point(Marge, inner_item_y_pos)
        Me.Pkg_CheckBox_Panel.Size = New Size(Item_Width, Panel_Height - inner_item_y_pos)
        Me.Package_Selection_Panel.Controls.Add(Me.Pkg_CheckBox_Panel)

        ' Add a checkbox for each package
        Dim pkg_name As String
        Dim pkg_idx As Integer = 0
        For Each pkg_name In pkg_name_list
            Dim pkg_checkbox As New CheckBox
            Me.Pkg_CheckBox_Panel.Controls.Add(pkg_checkbox)
            Me.Pkg_CheckBox_List.Add(pkg_checkbox)
            pkg_checkbox.Location = New Point(0, pkg_idx * Text_Field_Height)
            pkg_checkbox.Size = CheckBox_Size
            pkg_checkbox.Text = pkg_name
            ' Check the check box if the package was previously transformed
            If previous_pkg_selection_choices.ContainsKey(pkg_name) Then
                pkg_checkbox.Checked = CBool(previous_pkg_selection_choices(pkg_name))
            End If
            pkg_idx = pkg_idx + 1
        Next

        item_y_pos += Panel_Height + Marge * 2

        '------------------------------------------------------------------------------------------'
        ' Add OK button
        Me.OK_Button.Text = "Selection done"
        Me.Controls.Add(OK_Button)
        OK_Button.Size = New Size(Button_Width, Button_Height * 2)
        OK_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Me.OK_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Sodftware package selection"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FormBorderStyle = FormBorderStyle.FixedDialog

    End Sub

    Private Sub Select_All() Handles Select_All_Button.Click
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            pkg_checkbox.Checked = True
        Next
    End Sub

    Private Sub Unselect_All() Handles Unselect_All_Button.Click
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            pkg_checkbox.Checked = False
        Next
    End Sub

    Protected Overridable Sub Selection_Done() Handles OK_Button.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Public Function Get_Selected_Package_Name_List() As List(Of String)
        Dim pkg_name_list As New List(Of String)
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            If pkg_checkbox.Checked = True Then
                pkg_name_list.Add(pkg_checkbox.Text)
            End If
        Next
        Return pkg_name_list
    End Function

    Public Function Get_Package_Selection_Choices() As Dictionary(Of String, String)
        Dim pkg_selection_choices As New Dictionary(Of String, String)
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            If pkg_checkbox.Checked = True Then
                pkg_selection_choices.Add(pkg_checkbox.Text, "True")
            Else
                pkg_selection_choices.Add(pkg_checkbox.Text, "False")
            End If
        Next
        Return pkg_selection_choices
    End Function

End Class
