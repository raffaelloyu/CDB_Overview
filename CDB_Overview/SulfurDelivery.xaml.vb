
Imports Visifire.Charts
Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset
Imports Microsoft.Office.Interop
Imports System.IO

Public Class SulfurDelivery
    Private piservers As New PIServers
    Private pisystems As New PISystems
    Private PISystem As PISystem
    Private PIServer As PIServer
    Private srvAF As OSIsoft.AF.PI.PIServer
    Private sServerName As String

    Private sinusoid As OSIsoft.AF.PI.PIPoint
    ' Private timerange As AFTimeRange
    Private values As AFValues
    Private sFilter As String

    Private sLineColors(16) As Brush
    Private rValues_as(6, 0) As Double
    Private sDates_as(6, 0) As String
    Private iPoints_as(6) As String

    Private sDates(0) As String
    Private blnAddMarkers, blnAddLabels, blnAddChart As Boolean
    Private iops As Integer = 0

    'chart
    Private ileft, iTop, iWidth, iHeight As Integer
    Private nSeries, nSeries_from As Integer
    Private chatype As RenderAs
    Private sColors() As Brush
    Private sChartName As String
    Private sSeriesNames() As String
    Private sTYpes(8) As RenderAs
    Private cha As Chart
    Private sValueFormatString As String
    'Private config_x As New Xml.XmlDocument
    Private sPrevDate, sLastDate As Date

    Private sYLegend(2) As String
    Private blnZoomingEnabled As Boolean = False
    Private blnSetInterval As Boolean = False
    Private iSetInterval As Integer = 10
    Private iPoints As Integer
    Private iAxesY(30) As Integer

    Private rtotal, rtotal_bags, rtotal_bulk, rtotal_molten As Double
    Private rtarget As Double

    Private rtotal_m, rtotal_bags_m, rtotal_bulk_m, rtotal_molten_m As Double
    Private stotal_m, stotal_bags_m, stotal_bulk_m, stotal_molten_m As String
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        sLineColors(0) = Brushes.Blue 'Brushes.Violet 'Violat
        sLineColors(1) = Brushes.CornflowerBlue
        sLineColors(2) = Brushes.Indigo 'Brushes.DarkOrange 'Brushes.OliveDrab
        sLineColors(3) = Brushes.DarkOrange
        sLineColors(4) = Brushes.DarkMagenta
        sLineColors(5) = Brushes.Indigo
        sLineColors(6) = Brushes.DeepPink
        sLineColors(7) = Brushes.Beige
        sLineColors(8) = Brushes.Brown
        sLineColors(9) = Brushes.Gray
        sLineColors(10) = Brushes.Olive
        sLineColors(11) = Brushes.Blue
        sLineColors(12) = Brushes.Purple
        sLineColors(13) = Brushes.Salmon
        sLineColors(14) = Brushes.Blue
        sLineColors(15) = Brushes.Aquamarine

        mycanvas.Width = 1430 ' 1160
        mycanvas.Height = 720

        PIServer = piservers("pnwpappv003")
        srvAF = OSIsoft.AF.PI.PIServer.FindPIServer("pnwpappv003")
        sServerName = "pnwpappv003"
        Try
            Call createTanks()
            Call createSulfur()

            comYear.Items.Clear()
            comYear.Items.Add("2020")
            comYear.Items.Add("2021")
            comYear.Items.Add("2022")
            comYear.Items.Add("2023")
            comYear.SelectedIndex = 2

            lblYTDYear.Content = "YTD 2022"

        Catch ex As Exception

        End Try

        imgPrintSulfur.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf printSulfur))
    End Sub
    Private Sub printSulfur()
        Dim config_x As New Xml.XmlDocument
        Dim stags(9) As String
        Dim avalue As AFValue
        Dim rTankInv(9) As Single
        Dim rTankCap(9) As Single
        Dim rTankWCap(9) As Single
        Dim rTankRemCap(9) As Single
        Dim rTankRemTime(9) As Single

        Dim rTankRemTime_t(9) As Single

        Dim rTotalRemTime As String = 0
        Dim rTotalRemTime_t As String = 0
        Dim rTotalRemCap As String = 0
        Dim rTotalInv As String = 0
        Dim rWarCapacity As Single = 8176

        stags(1) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-1_L-1.PV"
        stags(2) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-2_L-1.PV"
        stags(3) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-3_L-1.PV"
        stags(4) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-4_L-1.PV"
        stags(5) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-5_L-1.PV"
        stags(6) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-6_L-1.PV"
        stags(7) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-7_L-1.PV"
        stags(8) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-8_L-1.PV"
        stags(9) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-9_L-1.PV"

        ReDim sDates_as(3, 30)
        ReDim rValues_as(3, 30)

        For i = 1 To 9

            If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, stags(i), sinusoid) Then
                avalue = sinusoid.CurrentValue

                If avalue.IsGood Then
                    If avalue.Value <= 0 Then
                        '              rValues_as(0, i - 1) = 0
                    Else
                        '              rValues_as(0, i - 1) = avalue.Value
                        rTankInv(i) = (11.4536 * avalue.Value / 100 + 0.275) * 176.71 * 1.78
                    End If

                    '        sDates_as(0, i - 1) = "Tank #" & i
                End If
            End If
            ' rValues_as(1, i - 1) = 30

            rTankCap(i) = (11.4536 + 0.275) * 176.7146 * 1.78
            '  rTankWCap(i) = (11.4536 * 80 / 100 + 0.275) * 176.7146 * 1.78
            rTankWCap(i) = (11.4536 * 85 / 100 + 0.275) * 176.7146 * 1.78
            If i <> 4 Then
                rTankRemCap(i) = rTankWCap(i) - rTankInv(i)
                rTankRemTime(i) = rTankRemCap(i) / 1200 ' dayls left
                rTankRemTime_t(i) = rTankRemCap(i) / rtarget
            Else
                rTankRemCap(i) = 0
                rTankRemTime(i) = 0
                rTankRemTime_t(i) = 0
            End If
            rTotalRemCap = rTotalRemCap + rTankRemCap(i)
            rTotalInv = rTotalInv + rTankInv(i)
            rTotalRemTime = rTotalRemTime + rTankRemTime(i)
            rTotalRemTime_t = rTotalRemTime_t + rTankRemTime_t(i)
        Next

        Dim xdocfilename As String = My.Settings.sworkfolder & "\NGP_sulfur.docx" ' 

        Dim xlApp As New Word.Application
        Dim xlWordDoc As Word.Document
        Dim bMark As Word.Bookmark
        Dim fileloca As String = AppDomain.CurrentDomain.BaseDirectory

        xlWordDoc = xlApp.Documents.Add(xdocfilename)

        Call calcTotals(CStr(Now.Year))

        Try
            bMark = xlWordDoc.Bookmarks.Item("total_inv")
            bMark.Range.Text = FormatNumber(rTotalInv, 0)

            bMark = xlWordDoc.Bookmarks.Item("total_cap")
            bMark.Range.Text = FormatNumber(rTotalRemCap, 0)

            bMark = xlWordDoc.Bookmarks.Item("total_days")
            bMark.Range.Text = FormatNumber(rTotalRemTime, 1)

            'rTotalRemTime_t
            bMark = xlWordDoc.Bookmarks.Item("total_dayst")
            bMark.Range.Text = FormatNumber(rTotalRemTime_t, 1)

            bMark = xlWordDoc.Bookmarks.Item("total_date")
            bMark.Range.Text = Now.ToString("MM/dd/yy HH:mm")



        Catch ex As Exception

        End Try

        Try
            Dim sDateTemp As String
            Dim sTemp As String
            Dim nnodes As Xml.XmlNodeList
            Dim sDateFrom As Date = CDate("01/01/2019")
            Dim rtotal_f As Single = 0
            Dim rtotal_m As Single = -1000
            Dim stotal_m As Date
            Dim reff As Single

            nnodes = config_x.DocumentElement.SelectNodes("date")
            Dim ip As Integer = 0

            For Each nnode In nnodes
                sDateTemp = nnode.Attributes.GetNamedItem("value").Value
                If CDate(sDateTemp).Date >= sDateFrom Then

                    If sDateTemp <> sLastDate Then
                        sTemp = nnode.SelectSingleNode("Forecast_Production").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_f = rtotal_f + CDbl(sTemp)
                        End If
                    Else
                        sTemp = nnode.SelectSingleNode("Forecast_Production").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_f = rtotal_f + CDbl(sTemp)
                        End If

                    End If
                End If
                If sDateTemp >= DateAdd(DateInterval.Day, -14, sLastDate) Then

                    sTemp = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                    If IsNumeric(sTemp) Then
                        rValues_as(0, ip) = FormatNumber(CDbl(sTemp), 0)
                    Else
                        rValues_as(0, ip) = 0
                    End If

                    sTemp = nnode.SelectSingleNode("Forecast_Production").Attributes.GetNamedItem("value").Value
                    If IsNumeric(sTemp) Then
                        rValues_as(1, ip) = FormatNumber(CDbl(sTemp), 0)
                    Else
                        rValues_as(1, ip) = 0
                    End If

                    sTemp = nnode.SelectSingleNode("Actual_Inventory").Attributes.GetNamedItem("value").Value
                    If IsNumeric(sTemp) Then
                        rValues_as(2, ip) = FormatNumber(CDbl(sTemp), 0)
                    Else
                        rValues_as(2, ip) = 0
                    End If

                    sDates_as(0, ip) = CDate(sDateTemp).ToString("MM/dd/yy")
                    sDates_as(1, ip) = CDate(sDateTemp).ToString("MM/dd/yy")
                    sDates_as(2, ip) = CDate(sDateTemp).ToString("MM/dd/yy")
                    iPoints_as(0) = ip
                    iPoints_as(1) = ip
                    iPoints_as(2) = ip
                    ip = ip + 1
                End If

                If sDateTemp > sLastDate Then
                    Exit For
                End If
            Next

            Try
                reff = rtotal / rtotal_f * 100

                bMark = xlWordDoc.Bookmarks.Item("reff")
                bMark.Range.Text = FormatNumber(reff, 1)


                bMark = xlWordDoc.Bookmarks.Item("rtotal")
                bMark.Range.Text = FormatNumber(rtotal, 0)

                bMark = xlWordDoc.Bookmarks.Item("total_target")
                bMark.Range.Text = FormatNumber(rtotal_f, 0)

                bMark = xlWordDoc.Bookmarks.Item("rtotal_bulk")
                bMark.Range.Text = FormatNumber(rtotal_bulk, 0)

                bMark = xlWordDoc.Bookmarks.Item("rtotal_molten")
                bMark.Range.Text = FormatNumber(rtotal_molten, 0)

            Catch ex As Exception

            End Try

        Catch ex As Exception

        End Try

        Try
            ' chart
            iops = 3

            sLineColors(0) = Brushes.Violet 'Violat
            sLineColors(1) = Brushes.CornflowerBlue
            sLineColors(2) = Brushes.OliveDrab
            sLineColors(3) = Brushes.DarkOrange
            sLineColors(4) = Brushes.DarkMagenta
            sLineColors(5) = Brushes.Indigo
            sLineColors(6) = Brushes.DeepPink

            sChartName = "chart_temp"
            'Height="236" Canvas.Left="90" Stroke="Black" Canvas.Top="183" Width="512"
            'Height="406" Canvas.Left="996" Stroke="Black" Canvas.Top="168" Width="896"
            '   iops = 5
            iTop = 0
            ileft = 0
            iWidth = 896
            iHeight = 380

            nSeries = iops
            nSeries_from = 0

            For ii1 = nSeries_from To iops - 1

                sTYpes(ii1) = RenderAs.Line

                sColors(ii1) = sLineColors(ii1 + 1)
                sSeriesNames(ii1) = "Name " & ii1
            Next

            sSeriesNames(0) = "Actual Delivery"
            sSeriesNames(1) = "Forecast"
            sSeriesNames(2) = "Inventory"

            sYLegend(0) = "Ton"
            sYLegend(1) = "Ton"

            blnAddMarkers = True
            blnAddLabels = True
            blnAddChart = True
            sValueFormatString = "MM/dd"

            blnZoomingEnabled = False
            blnSetInterval = True

            iSetInterval = 1

            Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, "Ton", sValueFormatString)

            cha.AxesY(0).AxisMaximum = 2000
            cha.AxesY(1).AxisMaximum = 20000
            cha.AxesY(1).AxisMinimum = 0


            cha.AnimationEnabled = False
            cha.UpdateLayout()

            Dim transform As Transform = cha.LayoutTransform
            cha.LayoutTransform = Nothing
            Dim renderBitmap As RenderTargetBitmap
            ' renderBitmap = New RenderTargetBitmap(cha.Width * 1.2, cha.Height * 1.2, 96D, 96D, PixelFormats.Pbgra32)
            renderBitmap = New RenderTargetBitmap(cha.Width, cha.Height, 96D, 96D, PixelFormats.Pbgra32)
            renderBitmap.Render(cha)

            Dim outStream As FileStream = New FileStream(fileloca & "\chart.png", FileMode.OpenOrCreate)
            '  Dim outStream As FileStream = New FileStream("c:\Projects\PSSParser\chart.png", FileMode.OpenOrCreate)
            Dim encoder As New PngBitmapEncoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap))
            encoder.Save(outStream)
            cha.LayoutTransform = transform
            mycanvas.Children.Remove(cha)
            outStream.Close()
            encoder = Nothing

            xlWordDoc.Bookmarks.Item("chart").Range.InlineShapes.AddPicture(fileloca & "\chart.png")
        Catch ex As Exception

        End Try

        Try
            Dim spdf As String = fileloca & "NGP_sulfur_" & Now.Date.DayOfYear & "_" & Now.Minute & Now.Millisecond & ".pdf" 'sEventID & ".pdf"
            xlWordDoc.SaveAs2(spdf, Word.WdSaveFormat.wdFormatPDF)
            System.Diagnostics.Process.Start(spdf)
            Call writeToLog(" printed Sulfur report " & "Sulfur_" & Now.Date.DayOfYear & "_" & Now.Minute & Now.Millisecond & ".pdf")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        xlWordDoc.Close(Word.WdSaveOptions.wdDoNotSaveChanges)
        xlApp.Quit()

    End Sub
    Public Shared Sub writeToLog(ByVal sDisplay As String)
        Dim fs1 As FileStream
        Dim s1 As StreamWriter

        '  If Environment.UserName <> "bzye" Then
        Try
            If File.Exists("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log_scribe.txt") Then
                fs1 = New FileStream("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log_scribe.txt", FileMode.Append, FileAccess.Write, FileShare.Write)
            Else
                My.Computer.FileSystem.CreateDirectory("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001")
                fs1 = New FileStream("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log_scribe.txt", FileMode.Create, FileAccess.Write, FileShare.Write)
                ' File.Create("O:\Dropbox\000 Cache001\log.txt")
            End If

            s1 = New StreamWriter(fs1)
            s1.Write("User " & Environment.UserName & " called display " & sDisplay & " " & Now & vbCrLf)
            s1.Flush()
            s1.Close()
            fs1.Close()
        Catch ex As Exception

        End Try
        ' End If
    End Sub
    Private Sub createTanks()
        Dim stags(9) As String
        Dim avalue As AFValue

        stags(1) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-1_L-1.PV"
        stags(2) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-2_L-1.PV"
        stags(3) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-3_L-1.PV"
        stags(4) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-4_L-1.PV"
        stags(5) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-5_L-1.PV"
        stags(6) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-6_L-1.PV"
        stags(7) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-7_L-1.PV"
        stags(8) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-8_L-1.PV"
        stags(9) = "XHN_SLFR-PLNT_LIQ-SLFR-TK-9_L-1.PV"

        ReDim sDates_as(8, 9)
        ReDim rValues_as(8, 9)

        For i = 1 To 9
            If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, stags(i), sinusoid) Then
                avalue = sinusoid.CurrentValue

                If avalue.IsGood Then
                    If avalue.Value <= 0 Then
                        rValues_as(0, i - 1) = 0
                    Else
                        rValues_as(0, i - 1) = avalue.Value
                    End If

                    sDates_as(0, i - 1) = "Tank #" & i
                End If
            End If
            rValues_as(1, i - 1) = 30
        Next

        iPoints_as(0) = 9
        '  iPoints_as(1) = 1
        ' Height="343" Canvas.Left="271"   Canvas.Top="329" Width="634"
        ' Height="287" Canvas.Left="771" Stroke="Black" Canvas.Top="366" Width="642"

        sChartName = "chart_tank"

        ileft = 771
        iTop = 366
        iWidth = 642
        iHeight = 287
        chatype = RenderAs.StackedColumn
        nSeries = 1
        nSeries_from = 0
        ReDim sColors(nSeries)
        ReDim sSeriesNames(nSeries)
        ReDim sTYpes(nSeries)
        ' iPoints = UBound(sDates) '- 2

        For ii1 = 0 To 1
            sTYpes(ii1) = RenderAs.Column
            sColors(ii1) = sLineColors(ii1)
            '  sSeriesNames(ii1) = "Total CMS Flow"
        Next
        '   sTYpes(1) = RenderAs.Line
        sSeriesNames(0) = "Tank Level (%)"
        '   sSeriesNames(1) = "Sulfur Delivery Forecast (ton)"
        blnAddMarkers = False
        blnAddLabels = False
        blnAddChart = True
        sValueFormatString = "MM/dd"

        Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, "%", sValueFormatString)

        Dim ser As New DataSeries
        Dim dp As DataPoint

        ser.RenderAs = RenderAs.StepLine
        ser.Name = "LOW Operating Limit (15%)"
        ser.Color = Brushes.Green
        ser.LightingEnabled = False
        cha.Series.Add(ser)

        For i = 1 To 9
            dp = New DataPoint
            '  dp.XValue = sDates_as(0, i - 1)
            dp.YValue = 15
            ser.DataPoints.Add(dp)
        Next

        ser = New DataSeries

        ser.RenderAs = RenderAs.StepLine
        ser.Name = "HIGH Operating Limit (85%)"
        ser.Color = Brushes.Red
        ser.LightingEnabled = False
        cha.Series.Add(ser)

        For i = 1 To 9
            dp = New DataPoint
            '  dp.XValue = sDates_as(0, i - 1)
            dp.YValue = 85
            ser.DataPoints.Add(dp)

        Next

        ser = New DataSeries

        ser.RenderAs = RenderAs.StepLine
        ser.Name = "MAX Operating Limit (87%)"
        ser.Color = Brushes.Black
        ser.LightingEnabled = False
        cha.Series.Add(ser)

        For i = 1 To 9
            dp = New DataPoint
            '  dp.XValue = sDates_as(0, i - 1)
            dp.YValue = 87
            ser.DataPoints.Add(dp)

        Next

        cha.AxesY(0).AxisMinimum = 0
        cha.AxesY(0).AxisMaximum = 100
    End Sub
    Private Sub createSulfur()
        Dim config_x As New Xml.XmlDocument
        Dim itemp As Integer
        Dim sPrevDate, sLastDate As Date
        Dim nnode As Xml.XmlNode
        Dim mylbl As Label
        Dim rtemp As Double
        Dim sImage As String

        Dim newbitimg As New BitmapImage
        Dim imagePath As String
        Dim myimg As Image
        Dim nnode_last As Xml.XmlNode

        Try
            config_x.Load("\\pcwpfsv001\share\PI Process Book Folders\SCRIBE\Sulfur Inventory\sulfur.xml")
            'config_x.Load("O:\OPS Automation\Apps Folder\PROMISE\config\sulfur.xml")

            ' last day
            Try
                nnode_last = config_x.DocumentElement.SelectSingleNode("date/Actual_Inventory[@value='0']").ParentNode
                sLastDate = CDate(nnode_last.Attributes("value").Value)
                sLastDate = DateAdd(DateInterval.Day, -1, sLastDate)
            Catch ex As Exception
                itemp = CInt((Now - Now.Date).Hours)

                If itemp >= 8 Then
                    sLastDate = DateAdd(DateInterval.Day, 0, Now.Date)
                    sLastDate = DateAdd(DateInterval.Hour, 8, sLastDate)
                Else
                    sLastDate = DateAdd(DateInterval.Day, -1, Now.Date)
                    sLastDate = DateAdd(DateInterval.Hour, 8, sLastDate)
                End If
            End Try
            itemp = CInt((Now - Now.Date).Hours)

            If itemp >= 8 Then
                sLastDate = DateAdd(DateInterval.Day, 0, Now.Date)
                sLastDate = DateAdd(DateInterval.Hour, 8, sLastDate)
            Else
                sLastDate = DateAdd(DateInterval.Day, -1, Now.Date)
                sLastDate = DateAdd(DateInterval.Hour, 8, sLastDate)
            End If
            ' previous day
            sPrevDate = DateAdd(DateInterval.Day, -1, sLastDate)

            Dim rActual_inv_last, rActual_del_last, rActual_del_bulk, rActual_del_molten As Double
            Dim rActual_inv_prev, rActual_del_prev As Double

            Try
                nnode = config_x.DocumentElement.SelectSingleNode("date[@value='" & sLastDate & "']")
                Try
                    rActual_inv_last = nnode.SelectSingleNode("Actual_Inventory").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_inv_last = 0
                End Try

                Try
                    rActual_del_last = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_del_last = 0
                End Try

                Try
                    rActual_del_molten = nnode.SelectSingleNode("Actual_Delivery_Molten").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_del_molten = 0
                End Try

                Try
                    rActual_del_bulk = nnode.SelectSingleNode("Actual_Delivery_Bulk").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_del_bulk = 0
                End Try


                nnode = config_x.DocumentElement.SelectSingleNode("date[@value='" & sPrevDate & "']")
                Try
                    rActual_inv_prev = nnode.SelectSingleNode("Actual_Inventory").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_inv_prev = 0
                End Try

                Try
                    rActual_del_prev = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rActual_del_prev = 0
                End Try


            Catch ex As Exception

            End Try

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_molten")
            mylbl.Content = FormatNumber(rActual_del_molten, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_bulk")
            mylbl.Content = FormatNumber(rActual_del_bulk, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_inv_last")
            mylbl.Content = FormatNumber(rActual_inv_last, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_inv_prev")
            mylbl.Content = FormatNumber(rActual_inv_prev, 0)

            rtemp = rActual_inv_last - rActual_inv_prev

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_inv_diff")
            mylbl.Content = FormatNumber(rtemp, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_inv_diffP")
            If rActual_inv_prev > 0 Then
                rtemp = rtemp / rActual_inv_prev * 100
                mylbl.Content = FormatNumber(rtemp, 1)
            Else
                mylbl.Content = "n/a"
                rtemp = 0
            End If

            Dim myrec As Rectangle
            myrec = LogicalTreeHelper.FindLogicalNode(mycanvas, "recActual_inv")

            If rtemp < 0 Then
                sImage = "arrow_green_dn.png"
                If rtemp < -3 Then
                    myrec.Stroke = Brushes.Green
                End If
            ElseIf rtemp > 0 Then
                sImage = "arrow_red_up.png"
                If rtemp > 3 Then
                    myrec.Stroke = Brushes.Red
                End If
            Else
                sImage = "" '"Check-icon.png"
            End If

            myimg = LogicalTreeHelper.FindLogicalNode(mycanvas, "imgActual_inv")
            imagePath = "Images/" + sImage
            newbitimg.BeginInit()
            newbitimg.Rotation = Rotation.Rotate180
            newbitimg.UriSource = New Uri(imagePath, UriKind.RelativeOrAbsolute)
            newbitimg.EndInit()
            myimg.Source = newbitimg

            ' delivery

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_last")
            mylbl.Content = FormatNumber(rActual_del_last, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_prev")
            mylbl.Content = FormatNumber(rActual_del_prev, 0)

            rtemp = rActual_del_last - rActual_del_prev

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_diff")
            mylbl.Content = FormatNumber(rtemp, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "rActual_del_diffP")
            If rActual_del_prev > 0 Then
                rtemp = rtemp / rActual_del_prev * 100
                mylbl.Content = FormatNumber(rtemp, 1)
            Else
                mylbl.Content = "n/a"
                rtemp = 0
            End If

            myrec = LogicalTreeHelper.FindLogicalNode(mycanvas, "recActual_del")

            If rtemp > 0 Then
                sImage = "arrow_green_dn.png"
                If rtemp > 3 Then
                    myrec.Stroke = Brushes.Green
                End If
            ElseIf rtemp < 0 Then
                sImage = "arrow_red_up.png"
                If rtemp < -3 Then
                    myrec.Stroke = Brushes.Red
                End If
            Else
                sImage = "" '"Check-icon.png"
            End If

            newbitimg = New BitmapImage

            myimg = LogicalTreeHelper.FindLogicalNode(mycanvas, "imgActual_del")
            imagePath = "Images/" + sImage
            newbitimg.BeginInit()
            newbitimg.UriSource = New Uri(imagePath, UriKind.RelativeOrAbsolute)
            newbitimg.EndInit()
            myimg.Source = newbitimg

            ' column chart

            Dim nnodes As Xml.XmlNodeList
            Dim sDate As Date
            ReDim sDates_as(8, 60)
            ReDim rValues_as(8, 60)
            Dim ii As Integer = 0

            For i = 61 To 1 Step -1
                sDate = DateAdd(DateInterval.Day, -i + 1, sLastDate)
                nnode = config_x.DocumentElement.SelectSingleNode("date[@value='" & sDate & "']")

                sDates_as(0, ii) = sDate.Date
                Try
                    rValues_as(0, ii) = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rValues_as(0, ii) = 0
                End Try

                sDates_as(1, ii) = sDate.Date
                Try
                    rValues_as(1, ii) = nnode.SelectSingleNode("Forecast_Production").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rValues_as(1, ii) = 0
                End Try

                sDates_as(2, ii) = sDate.Date
                Try
                    rValues_as(2, ii) = nnode.SelectSingleNode("Actual_Inventory").Attributes.GetNamedItem("value").Value
                Catch ex As Exception
                    rValues_as(2, ii) = 0
                End Try

                ii = ii + 1
            Next

            iPoints_as(0) = 61
            iPoints_as(1) = 61
            iPoints_as(2) = 61
            ' Height="343" Canvas.Left="271"   Canvas.Top="329" Width="634"
            ' Height="287" Canvas.Left="446" Stroke="Black" Canvas.Top="60" Width="943"

            sChartName = "chart_week"

            ileft = 446
            iTop = 60
            iWidth = 943
            iHeight = 287
            chatype = RenderAs.StackedColumn
            nSeries = 3
            nSeries_from = 0
            ReDim sColors(nSeries)
            ReDim sSeriesNames(nSeries)
            ReDim sTYpes(nSeries)
            ' iPoints = UBound(sDates) '- 2

            For ii1 = 0 To 2
                sTYpes(ii1) = RenderAs.Line
                sColors(ii1) = sLineColors(ii1)
                '  sSeriesNames(ii1) = "Total CMS Flow"
            Next
            '   sTYpes(1) = RenderAs.Line
            sSeriesNames(0) = "Sulfur Delivery (ton)"
            sSeriesNames(1) = "Sulfur Delivery Forecast (ton)"
            sSeriesNames(2) = "Sulfur Inventory (ton)"
            blnAddMarkers = False
            blnAddLabels = False
            blnAddChart = True
            sValueFormatString = "MM/dd"

            Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, "Ton", sValueFormatString)

            'inventory chart
            ii = 0

            For i = 31 To 1 Step -1
                sDate = DateAdd(DateInterval.Day, -i + 1, sLastDate)
                nnode = config_x.DocumentElement.SelectSingleNode("date[@value='" & sDate & "']")

                sDates_as(0, ii) = sDate.Date
                rValues_as(0, ii) = FormatNumber(nnode.SelectSingleNode("Actual_Inventory").Attributes.GetNamedItem("value").Value / 23800 * 100, 1)

                ii = ii + 1
            Next

            iPoints_as(0) = 31
            ' iPoints_as(1) = 30
            ' Height="343" Canvas.Left="271"   Canvas.Top="329" Width="634"
            ' Height="287" Canvas.Left="771" Stroke="Black" Canvas.Top="366" Width="642"

            sChartName = "chart_inventory"

            ileft = 771
            iTop = 366
            iWidth = 642
            iHeight = 287
            chatype = RenderAs.StackedColumn
            nSeries = 1
            nSeries_from = 0
            ReDim sColors(nSeries)
            ReDim sSeriesNames(nSeries)
            ReDim sTYpes(nSeries)
            ' iPoints = UBound(sDates) '- 2

            For ii1 = 0 To 1
                sTYpes(ii1) = RenderAs.Line
                sColors(ii1) = sLineColors(ii1)
                '  sSeriesNames(ii1) = "Total CMS Flow"
            Next
            '   sTYpes(1) = RenderAs.Line
            sSeriesNames(0) = "% of Available Capacity"
            ' sSeriesNames(1) = "Sulfur Delivery Forecast (ton)"
            blnAddMarkers = True
            blnAddLabels = False
            blnAddChart = True
            sValueFormatString = "MM/dd"

            '    Call createNewChart(cha, sChartName, ileft, iTop,
            '     iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
            '  sTYpes, sColors, iPoints, "% of Available Capacity", sValueFormatString)


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
    Private Sub calcTotals(ByVal sYear As String)
        Dim nnodes As Xml.XmlNodeList
        Dim config_x As New Xml.XmlDocument
        'totals 

        Dim sDateFrom, sDateTo As Date
        sDateFrom = CDate("01/01/" & sYear)
        sDateTo = DateAdd(DateInterval.Year, 1, sDateFrom)

        rtotal = 0
        rtotal_bags = 0
        rtotal_bulk = 0
        rtotal_molten = 0

        rtotal_m = -1000
        rtotal_bags_m = -1000
        rtotal_bulk_m = -1000
        rtotal_molten_m = -1000

        stotal_m = ""
        stotal_bags_m = ""
        stotal_bulk_m = ""
        stotal_molten_m = ""

        Dim sDateTemp As String
        Dim sTemp As String
        config_x.Load("\\pcwpfsv001\share\PI Process Book Folders\SCRIBE\Sulfur Inventory\sulfur.xml")
        Try
            nnodes = config_x.DocumentElement.SelectNodes("date")
            For Each nnode In nnodes
                sDateTemp = nnode.Attributes.GetNamedItem("value").Value
                If CDate(sDateTemp).Date >= sDateFrom And CDate(sDateTemp).Date < sDateTo Then

                    If sDateTemp <> sLastDate Then

                        sTemp = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal = rtotal + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_m Then
                                rtotal_m = CDbl(sTemp)
                                stotal_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Bags").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_bags = rtotal_bags + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_bags_m Then
                                rtotal_bags_m = CDbl(sTemp)
                                stotal_bags_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Bulk").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_bulk = rtotal_bulk + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_bulk_m Then
                                rtotal_bulk_m = CDbl(sTemp)
                                stotal_bulk_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Molten").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_molten = rtotal_molten + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_molten_m Then
                                rtotal_molten_m = CDbl(sTemp)
                                stotal_molten_m = sDateTemp
                            End If
                        End If

                    Else
                        'rtarget
                        sTemp = nnode.SelectSingleNode("Forecast_Production").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtarget = CDbl(sTemp)
                        Else
                            rtarget = 1
                        End If


                        sTemp = nnode.SelectSingleNode("Actual_Devliery").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal = rtotal + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_m Then
                                rtotal_m = CDbl(sTemp)
                                stotal_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Bags").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_bags = rtotal_bags + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_bags_m Then
                                rtotal_bags_m = CDbl(sTemp)
                                stotal_bags_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Bulk").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_bulk = rtotal_bulk + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_bulk_m Then
                                rtotal_bulk_m = CDbl(sTemp)
                                stotal_bulk_m = sDateTemp
                            End If
                        End If

                        sTemp = nnode.SelectSingleNode("Actual_Delivery_Molten").Attributes.GetNamedItem("value").Value
                        If IsNumeric(sTemp) Then
                            rtotal_molten = rtotal_molten + CDbl(sTemp)
                            If CDbl(sTemp) > rtotal_molten_m Then
                                rtotal_molten_m = CDbl(sTemp)
                                stotal_molten_m = sDateTemp
                            End If
                        End If
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception

        End Try



    End Sub
    Private Sub createTotals(ByVal sYear As String)
        Dim nnodes As Xml.XmlNodeList
        'totals 


        Dim sDateFrom, sDateTo As Date

        Try
            Call calcTotals(sYear)

            Dim mylbl As Label

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_total")
            mylbl.Content = FormatNumber(rtotal, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bags")
            mylbl.Content = FormatNumber(rtotal_bags, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bulk")
            mylbl.Content = FormatNumber(rtotal_bulk, 0)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_molten")
            mylbl.Content = FormatNumber(rtotal_molten, 0)

            ' max values

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_total_maxv")
            mylbl.Content = FormatNumber(rtotal_m, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_total_maxd")
            mylbl.Content = CDate(stotal_m).Date

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bags_maxv")
            mylbl.Content = FormatNumber(rtotal_bags_m, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bags_maxd")
            mylbl.Content = CDate(stotal_bags_m).Date

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bulk_maxv")
            mylbl.Content = FormatNumber(rtotal_bulk_m, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_bulk_maxd")
            mylbl.Content = CDate(stotal_bulk_m).Date

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_molten_maxv")
            mylbl.Content = FormatNumber(rtotal_molten_m, 1)

            mylbl = LogicalTreeHelper.FindLogicalNode(mycanvas, "lblDel_molten_maxd")
            mylbl.Content = CDate(stotal_molten_m).Date

            ReDim sDates_as(8, 3)
            ReDim rValues_as(8, 3)

            sDates_as(0, 0) = "Actual Delivery Bags"
            sDates_as(0, 1) = "Actual Delivery Bulk"
            sDates_as(0, 2) = "Actual Delivery Molten"

            rValues_as(0, 0) = rtotal_bags
            rValues_as(0, 1) = rtotal_bulk
            rValues_as(0, 2) = rtotal_molten

            iPoints_as(0) = 3
            ' iPoints_as(1) = 30
            ' Height="343" Canvas.Left="271"   Canvas.Top="329" Width="634"
            ' Height="287" Canvas.Left="446" Stroke="Black" Canvas.Top="366" Width="481

            sChartName = "chart_pie"

            ileft = 446
            iTop = 330
            iWidth = 300
            iHeight = 340
            chatype = RenderAs.StackedColumn
            nSeries = 1
            nSeries_from = 0
            ReDim sColors(2)
            ReDim sSeriesNames(2)
            ReDim sTYpes(2)
            ' iPoints = UBound(sDates) '- 2

            For ii1 = 0 To 2
                sTYpes(ii1) = RenderAs.Pie
                sColors(ii1) = sLineColors(ii1)
                sSeriesNames(ii1) = sDates_as(0, ii1)
            Next
            '   sTYpes(1) = RenderAs.Line
            '  sSeriesNames(0) = "Actual Inventory (ton)"
            ' sSeriesNames(1) = "Sulfur Delivery Forecast (ton)"
            blnAddMarkers = True
            blnAddLabels = True
            blnAddChart = True
            sValueFormatString = "MM/dd"

            Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, "Ton", sValueFormatString)

        Catch ex As Exception

        End Try



    End Sub
    Private Sub createNewChart(ByRef cha As Chart, ByVal chaName As String, ByVal iLeft As Integer, ByVal iTop As Integer,
                               ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal nSeries As Integer, ByVal nSeries_from As Integer,
                               ByVal sSeriesNames() As String,
                               ByVal chatype() As RenderAs, ByVal sLineColor() As Brush, ByVal iPoints As Integer, ByVal sYTitle As String, ByVal sValueFormatString As String)

        Dim dp As DataPoint
        Dim stemp As String
        Dim itemp As Integer = 0


        '  Dim cha As New Chart

        ' check if chart exists

        ' check if chart exists
        If IsNothing(LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)) Then
            cha = New Chart
        Else
            cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)
            mycanvas.Children.Remove(cha)
            cha = Nothing
            cha = New Chart
        End If

        '  If blnAddChart Then
        cha.AnimationEnabled = True
        mycanvas.Children.Add(cha)
        '  End If

        Canvas.SetLeft(cha, iLeft)
        Canvas.SetTop(cha, iTop)

        cha.Width = iWidth
        cha.Height = iHeight

        cha.BorderThickness = New Thickness(0)
        cha.IndicatorEnabled = True
        cha.LightingEnabled = False
        cha.ShadowEnabled = False

        cha.Background = Brushes.Transparent

        Dim plt As New PlotArea
        plt.ShadowEnabled = False
        '  plt.Background = Brushes.Black

        cha.PlotArea = plt
        cha.Name = chaName
        cha.Theme = "Theme1"
        cha.ScrollingEnabled = False
        ' cha.ZoomingEnabled = True
        '  cha.ColorSet = "Visifire2"

        Dim lg As New Legend
        cha.Legends.Add(lg)
        lg.Background = Brushes.Transparent
        lg.LightingEnabled = False
        lg.BorderThickness = New Thickness(0)
        lg.ShadowEnabled = False



        ' create axis 
        Dim myXax As New Axis
        myXax.AxisType = AxisTypes.Primary
        ''   myXax.ValueFormatString = "MM/dd"

        Dim myaxL As AxisLabels = New AxisLabels
        myaxL.FontColor = Brushes.Black
        myXax.AxisLabels = myaxL

        Dim mygr As New ChartGrid
        mygr.LineStyle = LineStyles.Dashed
        mygr.LineThickness = 0.5
        mygr.LineColor = Brushes.Black
        myXax.Grids.Add(mygr)

        ''   myXax.AxisMinimum = FormatNumber(rDates.Average() * 0.8, 0)
        cha.AxesX.Add(myXax)

        Dim myYax As New Axis
        myYax.AxisType = AxisTypes.Primary

        Dim myaxLY As AxisLabels = New AxisLabels
        myaxLY.FontColor = Brushes.Black
        myYax.AxisLabels = myaxLY

        Dim mygr1 As New ChartGrid
        mygr1.LineStyle = LineStyles.Dashed
        mygr1.LineThickness = "0.50"
        mygr1.LineColor = Brushes.Black
        ''caa   myYax.Grids.Add(mygr1)
        myYax.Title = sYTitle
        cha.AxesY.Add(myYax)

        'title
        Dim myTitle As New Title
        cha.Titles.Add(myTitle)

        ' secondary Y
        Dim myYax1 As New Axis
        myYax1.AxisType = AxisTypes.Secondary
        cha.AxesY.Add(myYax1)

        '  cha.AxesX.ValueFormatString = "MM/dd"
        '   Else
        '   cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)
        '   cha.Series.Clear()
        '   End If

        For i = nSeries_from To nSeries_from + nSeries - 1
            Dim ser As New DataSeries
            ser.RenderAs = chatype(i)
            ser.LabelEnabled = False
            ser.MarkerEnabled = False
            ser.LightingEnabled = False

            If i = 2 Then
                ser.AxisYType = AxisTypes.Secondary
            End If

            If blnAddMarkers Then
                ser.MarkerEnabled = True
            End If

            If blnAddLabels Then
                ser.LabelEnabled = True
            End If

            If ser.RenderAs = RenderAs.Line Or ser.RenderAs = RenderAs.Spline Then
                '     ser.LabelEnabled = True
                If blnAddMarkers Then
                    ser.MarkerEnabled = True
                Else
                    ser.MarkerEnabled = False
                End If
                ser.XValueType = ChartValueTypes.DateTime
                ser.ToolTipText = "#YValue, #Series"
            End If

            ser.MarkerColor = Brushes.White
            ser.MarkerBorderColor = Brushes.Black
            ser.MarkerSize = 10
            If Not IsNothing(sLineColor(i)) Then
                If sLineColor(i).ToString <> Brushes.Transparent.ToString Then
                    ser.Color = sLineColor(i)
                End If
            End If
            ser.LineThickness = 3
            ser.ShadowEnabled = True
            ser.LightingEnabled = False
            ser.Name = sSeriesNames(i)
            ser.DataPoints.Clear()
            ''   AddHandler ser.MouseLeftButtonDown, AddressOf cha_details
            cha.Series.Add(ser)

            Dim itimes As Integer = 0
            stemp = ""
            Dim rtemp As Double = 100000000.0
            If iPoints = 0 Then iPoints = itimes - 1

            'For j = 0 To iPoints - 1
            For j = 0 To iPoints_as(i) - 1
                dp = New DataPoint
                Try
                    dp.YValue = FormatNumber(rValues_as(i, j), 2)
                    If ser.RenderAs = RenderAs.Column Or ser.RenderAs = RenderAs.StackedColumn Or ser.RenderAs = RenderAs.StackedColumn100 Then
                        '  dp.AxisXLabel = sDates(j)
                        dp.AxisXLabel = sDates_as(i, j)

                        ser.LabelEnabled = True
                        ser.MarkerEnabled = True
                    ElseIf ser.RenderAs = RenderAs.Pie Then
                        dp.AxisXLabel = sSeriesNames(j)
                        dp.Color = sLineColor(j)
                        ser.ShowInLegend = True
                        lg.DockInsidePlotArea = True
                        ser.LabelStyle = LabelStyles.Inside
                        ser.LabelText = "#YValue"
                        '   ser.LabelEnabled = True
                        '   ser.MarkerEnabled = True
                    Else
                        ' dp.XValue = sDates(j)
                        dp.XValue = sDates_as(i, j)
                        cha.AxesX(0).ValueFormatString = sValueFormatString '"h:mm"

                    End If

                    ser.DataPoints.Add(dp)
                    If rtemp > rValues_as(i + 1, j) Then
                        rtemp = rValues_as(i + 1, j)
                    End If
                Catch ex As Exception

                End Try
            Next
        Next

    End Sub

    Private Sub SulfurDelivery_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - 20) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub

    Private Sub comYear_SelectionChanged(sender As ComboBox, e As SelectionChangedEventArgs) Handles comYear.SelectionChanged
        Try
            Call createTotals(sender.SelectedValue)
            lblYTDYear.Content = "YTD " & sender.SelectedValue
        Catch ex As Exception

        End Try

    End Sub

End Class
