Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel
Imports Microsoft.Office.Interop.Excel.XlBordersIndex
Imports Microsoft.Office.Interop.Excel.XlLineStyle
Imports Microsoft.Office.Interop.Excel.XlHAlign
Imports Microsoft.Office.Interop.Excel.XlVAlign

Public Class Metrics_Report_Generator

    Public Sub Generate_PSWA_Metrics_Report(
        file_path As String,
        top_package_list As List(Of Top_Level_Package),
        interfaces_list As List(Of Software_Interface),
        swct_list As List(Of Component_Type),
        doc_series As Data_Series,
        nb_data_types_series As Data_Series,
        nb_interfaces_series As Data_Series,
        nb_component_types_series As Data_Series,
        distance_series As Data_Series,
        if_WMC_series As Data_Series,
        swct_WMC_series As Data_Series)

        Dim excel_app As New Application
        excel_app.Visible = False
        Dim workbook As Workbook
        workbook = excel_app.Workbooks.Add


        '------------------------------------------------------------------------------------------'
        ' Treat packages (one specific sheet)
        Dim pkg_sheet As Worksheet
        pkg_sheet = CType(workbook.ActiveSheet, Worksheet)
        pkg_sheet.Name = "Packages"
        pkg_sheet.Cells.Interior.ThemeColor = XlThemeColor.xlThemeColorDark1

        ' Set titles
        pkg_sheet.Range("C2").Value = "Doc. rate"
        pkg_sheet.Range("C2:C4").Merge()

        pkg_sheet.Range("D2").Value = "Content"
        pkg_sheet.Range("D2:H2").Merge()
        pkg_sheet.Range("D3").Value = "Number of"
        pkg_sheet.Range("D3:G3").Merge()
        pkg_sheet.Range("H3").Value = "Abstraction"
        pkg_sheet.Range("H3:H4").Merge()
        pkg_sheet.Range("D4").Value = "Data_Types"
        pkg_sheet.Range("E4").Value = "Interfaces"
        pkg_sheet.Range("F4").Value = "Component_Types"
        pkg_sheet.Range("G4").Value = "Compositions"

        pkg_sheet.Range("I2").Value = "Coupling"
        pkg_sheet.Range("I2:K3").Merge()
        pkg_sheet.Range("I4").Value = "Ce"
        pkg_sheet.Range("J4").Value = "Ca"
        pkg_sheet.Range("K4").Value = "Instability"

        pkg_sheet.Range("L2").Value = "Distance"
        pkg_sheet.Range("L2:L4").Merge()

        Format_Title(pkg_sheet.Range("C2:L4"))

        pkg_sheet.Range("B5").Value = "Minimum"
        pkg_sheet.Range("B6").Value = "Maximum"
        pkg_sheet.Range("B7").Value = "Average"
        pkg_sheet.Range("B8").Value = "Deviation"

        ' Write statistics
        Write_Statistic(pkg_sheet, 5, 3, doc_series)
        Write_Statistic(pkg_sheet, 5, 4, nb_data_types_series)
        Write_Statistic(pkg_sheet, 5, 5, nb_interfaces_series)
        Write_Statistic(pkg_sheet, 5, 6, nb_component_types_series)
        For col = 7 To 11
            Write_No_Statistic(pkg_sheet, 5, col)
        Next
        Write_Statistic(pkg_sheet, 5, 12, distance_series)


        With pkg_sheet.Range("B5:L8").Interior
            .ThemeColor = XlThemeColor.xlThemeColorDark1
            .TintAndShade = -0.149998474074526
            .PatternTintAndShade = 0
        End With

        ' Write data
        Dim row_idx As Integer = 9
        For Each pkg In top_package_list
            CType(pkg_sheet.Cells(row_idx, 2), Range).Value = pkg.Name
            CType(pkg_sheet.Cells(row_idx, 3), Range).Value = pkg.Get_Package_Documentation_Rate()
            CType(pkg_sheet.Cells(row_idx, 4), Range).Value = pkg.Get_Nb_Data_Types()
            CType(pkg_sheet.Cells(row_idx, 5), Range).Value = pkg.Get_Nb_Interfaces()
            CType(pkg_sheet.Cells(row_idx, 6), Range).Value = pkg.Get_Nb_Component_Types()
            CType(pkg_sheet.Cells(row_idx, 7), Range).Value = pkg.Get_Nb_Compositions()
            CType(pkg_sheet.Cells(row_idx, 8), Range).Value = pkg.Get_Abstraction_Level()
            CType(pkg_sheet.Cells(row_idx, 9), Range).Value = pkg.Get_Efferent_Coupling()
            CType(pkg_sheet.Cells(row_idx, 10), Range).Value = pkg.Get_Afferent_Coupling()
            CType(pkg_sheet.Cells(row_idx, 11), Range).Value = pkg.Get_Instability()
            CType(pkg_sheet.Cells(row_idx, 12), Range).Value = pkg.Get_Distance()
            row_idx += 1
        Next

        Dim columns_idx_list(2, 5) As Integer
        columns_idx_list = {{3, 3}, {4, 8}, {9, 11}, {12, 12}}
        Dim cols_idx As Integer
        For cols_idx = 0 To columns_idx_list.GetLength(0) - 1
            Dim left As Integer = columns_idx_list(cols_idx, 0)
            Dim right As Integer = columns_idx_list(cols_idx, 1)
            With pkg_sheet.Range(pkg_sheet.Cells(2, left), pkg_sheet.Cells(row_idx - 1, right))
                .Borders(xlEdgeLeft).LineStyle = xlDouble
            End With
        Next

        ' Format columns
        pkg_sheet.Columns().EntireColumn.AutoFit()
        CType(pkg_sheet.Columns("C"), Range).Style = "Percent"
        CType(pkg_sheet.Columns("H"), Range).NumberFormat = "0.00"
        CType(pkg_sheet.Columns("K"), Range).NumberFormat = "0.00"
        CType(pkg_sheet.Columns("L"), Range).NumberFormat = "0.00"

        '------------------------------------------------------------------------------------------'
        ' Treat interfaces (one specific sheet)
        Dim if_sheet As Worksheet
        if_sheet = CType(workbook.Sheets.Add, Worksheet)
        if_sheet.Name = "Interfaces"
        if_sheet.Cells.Interior.ThemeColor = XlThemeColor.xlThemeColorDark1

        ' Write titles
        if_sheet.Range("C2").Value = "WMC"
        Format_Title(if_sheet.Range("C2:C2"))

        if_sheet.Range("B3").Value = "Minimum"
        if_sheet.Range("B4").Value = "Maximum"
        if_sheet.Range("B5").Value = "Average"
        if_sheet.Range("B6").Value = "Deviation"

        ' Write statistics
        Write_Statistic(if_sheet, 3, 3, if_WMC_series)

        With if_sheet.Range("B3:C6").Interior
            .ThemeColor = XlThemeColor.xlThemeColorDark1
            .TintAndShade = -0.149998474074526
            .PatternTintAndShade = 0
        End With

        row_idx = 7
        For Each sw_if In interfaces_list
            CType(if_sheet.Cells(row_idx, 2), Range).Value = sw_if.Get_Rpy_Element_Path()
            CType(if_sheet.Cells(row_idx, 3), Range).Value = sw_if.Compute_WMC()
            row_idx += 1
        Next

        With if_sheet.Range(if_sheet.Cells(2, 3), if_sheet.Cells(row_idx - 1, 3))
            .Borders(xlEdgeLeft).LineStyle = xlDouble
        End With

        if_sheet.Columns().EntireColumn.AutoFit()

        '------------------------------------------------------------------------------------------'
        ' Treat component types (one specific sheet)
        Dim swct_sheet As Worksheet
        swct_sheet = CType(workbook.Sheets.Add, Worksheet)
        swct_sheet.Name = "Component_Types"
        swct_sheet.Cells.Interior.ThemeColor = XlThemeColor.xlThemeColorDark1

        swct_sheet.Range("C2").Value = "WMC"
        swct_sheet.Range("D2").Value = "Nb Provider_Port"
        swct_sheet.Range("E2").Value = "Nb Requirer_Port"
        Format_Title(swct_sheet.Range("C2:E2"))

        swct_sheet.Range("B3").Value = "Minimum"
        swct_sheet.Range("B4").Value = "Maximum"
        swct_sheet.Range("B5").Value = "Average"
        swct_sheet.Range("B6").Value = "Deviation"

        Write_Statistic(swct_sheet, 3, 3, swct_WMC_series)
        Write_No_Statistic(swct_sheet, 3, 4)
        Write_No_Statistic(swct_sheet, 3, 5)

        With swct_sheet.Range("B3:E6").Interior
            .ThemeColor = XlThemeColor.xlThemeColorDark1
            .TintAndShade = -0.149998474074526
            .PatternTintAndShade = 0
        End With

        row_idx = 7
        For Each swct In swct_list
            CType(swct_sheet.Cells(row_idx, 2), Range).Value = swct.Get_Rpy_Element_Path()
            CType(swct_sheet.Cells(row_idx, 3), Range).Value = swct.Compute_WMC()
            CType(swct_sheet.Cells(row_idx, 4), Range).Value = swct.Provider_Ports.Count
            CType(swct_sheet.Cells(row_idx, 5), Range).Value = swct.Requirer_Ports.Count
            row_idx += 1
        Next

        With swct_sheet.Range(swct_sheet.Cells(2, 3), swct_sheet.Cells(row_idx - 1, 3))
            .Borders(xlEdgeLeft).LineStyle = xlDouble
            .Borders(xlEdgeRight).LineStyle = xlDouble
        End With
        With swct_sheet.Range(swct_sheet.Cells(2, 5), swct_sheet.Cells(row_idx - 1, 5))
            .Borders(xlEdgeLeft).LineStyle = xlDouble
            .Borders(xlEdgeRight).LineStyle = xlDouble
        End With

        swct_sheet.Columns().EntireColumn.AutoFit()

        workbook.SaveAs(file_path)
        pkg_sheet.Activate()
        excel_app.Visible = True

    End Sub

    Private Shared Sub Write_Statistic(
        sheet As Worksheet,
        row_idx As Integer,
        col_idx As Integer,
        serie As Data_Series)
        CType(sheet.Cells(row_idx, col_idx), Range).Value = serie.Get_Min()
        CType(sheet.Cells(row_idx + 1, col_idx), Range).Value = serie.Get_Max()
        CType(sheet.Cells(row_idx + 2, col_idx), Range).Value = serie.Get_Average()
        CType(sheet.Cells(row_idx + 3, col_idx), Range).Value = serie.Get_Standard_Deviation()
        With sheet.Range(sheet.Cells(row_idx + 2, col_idx), sheet.Cells(row_idx + 3, col_idx))
            .NumberFormat = "0.00"
        End With
    End Sub

    Private Shared Sub Write_No_Statistic(
        sheet As Worksheet,
        row_idx As Integer,
        col_idx As Integer)
        With sheet.Range(sheet.Cells(row_idx, col_idx), sheet.Cells(row_idx + 3, col_idx))
            .Value = "-"
            .HorizontalAlignment = xlHAlignRight
            .VerticalAlignment = xlVAlignCenter
        End With
    End Sub

    Private Shared Sub Format_Title(rg As Range)
        With rg
            .HorizontalAlignment = xlHAlignCenter
            .VerticalAlignment = xlVAlignCenter
            .Font.Bold = True
            .Interior.Color = 49407 'orange Conti
        End With
    End Sub

End Class
