Imports System.IO
Imports System.Windows

Public Class CLOOF_Transformation_Form
    Inherits SMH_Form

    Private Output_Directory_TxtBx As New TextBox
    Private WithEvents Output_Directory_Button As New Button

    Private WithEvents Transform_Button As New Button

    Public Sub New(default_output_directory As String)

        Dim item_y_pos As Integer = Marge
        Dim inner_item_y_pos As Integer

        '------------------------------------------------------------------------------------------'
        ' Add output directory panel
        inner_item_y_pos = Marge
        Dim output_directory_panel As New Panel
        output_directory_panel.Location = New Point(Marge, item_y_pos)
        output_directory_panel.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(output_directory_panel)

        Dim output_directory_label As New Label
        output_directory_label.Text = "Output directory selection"
        output_directory_label.Location = New Point(Marge, inner_item_y_pos)
        output_directory_label.Size = Label_Size
        output_directory_panel.Controls.Add(output_directory_label)
        inner_item_y_pos += output_directory_label.Height

        Me.Output_Directory_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Output_Directory_TxtBx.Size = Path_Text_Size
        Me.Output_Directory_TxtBx.Text = default_output_directory
        output_directory_panel.Controls.Add(Me.Output_Directory_TxtBx)

        Me.Output_Directory_Button.Location = New Point(Path_Button_X_Pos, inner_item_y_pos)
        Me.Output_Directory_Button.Size = Path_Button_Size
        Me.Output_Directory_Button.Text = "..."
        output_directory_panel.Controls.Add(Me.Output_Directory_Button)
        inner_item_y_pos += Me.Output_Directory_Button.Height + Marge

        output_directory_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += output_directory_panel.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design transforamtion button
        Me.Transform_Button.Text = "Transform"
        Me.Controls.Add(Me.Transform_Button)
        Me.Transform_Button.Size = Button_Size
        Me.Transform_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Transform_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "C language object oriented framework transformation"
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_Output_Directory() As String
        Return Me.Output_Directory_TxtBx.Text
    End Function

    Private Sub Output_Directory_Button_Clicked() Handles Output_Directory_Button.Click
        SMH_Form.Select_Directory(
            "Select transformation output directory",
            Me.Output_Directory_TxtBx)
    End Sub

    Protected Overridable Sub Transformation_Started() Handles Transform_Button.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

End Class
