Imports System
Imports System.Windows
Imports System.Data
Imports Visifire.Charts
Imports System.Windows.Threading
Imports System.Threading

Imports System.Globalization

Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset
Imports OSIsoft.AF.Search
Imports OSIsoft.AF.Time
Imports OSIsoft.AF.Data

Imports Excel = Microsoft.Office.Interop.Excel

Public Class ADDHOC
    Inherits Window

    Private Delegate Sub SubChartPrimeDelegate(ByRef kida As Chart)

    Private piservers As New PIServers
    Private pisystems As New PISystems
    Private PISystem As PISystem
    Private PIServer As PIServer

    Private cafops As AFDatabase
    Private sinusoid As OSIsoft.AF.PI.PIPoint
    Private timerange As AFTimeRange
    Private values As AFValues

    ' Private mySDK As New PISDK.PISDK
    '  Private srv As Server
    Private srvname As String = "cpi1-t"

    Private srvAF As OSIsoft.AF.PI.PIServer

    Public Delegate Sub NextPrimeDelegate()
    Private sTagName As String
    Private strConnectionString As String
    Private strSQL_a As String
    Private strSQL_t As String
    Private strSQL_v As String


    Private strName As String
    Private config_aa As New Xml.XmlDocument
    Private WithEvents timer1 As New DispatcherTimer

    ' Private chart As New Visifire.Charts.Chart

    Private dataSeries As New DataSeries
    Private iCount As Integer = 0
    Private blnInit As Boolean = True
    Private _value As Single
    Private _time As Date
    Private blnReady As Boolean = False

    'Private ds As New DataSeries
    'Dim dpcol As New DataPointCollection

    Private myXax As New Axis
    Private myYax As New Axis
    Private myYax_r As New Axis

    Private strXML As String = ""
    Private sConsole As String
    Private sTitle As String
    Private sLineColors(20) As Brush
    Private iType As Integer
    Private sfilename As String

    Private sVBS As String
    Private sTime_start As Date
    Private sTime_end As Date

    Private sTags() As String
    Private seQus() As String
    Private sScales() As String

    Private iTags As Integer = 0
    Private iseries As Integer = -1
    Private pi_col As New Collection
    Private pi_col_xls_tags As String()
    Private pi_col_xls_tags_UOM As String()
    Private pi_col_xls_tags_desc As String()

    Private iShift As Integer
    Private _sServerName As String
    Private foundPoints As IEnumerable(Of OSIsoft.AF.PI.PIPoint)
    Private pts As New OSIsoft.AF.PI.PIPointList
    Private pts1 As New OSIsoft.AF.PI.PIPointList

    Private iops, iPoints As Integer
    Private rValues_as(6, 0) As Double
    Private sDates_as(6, 0) As String
    Private iPoints_as(6) As Integer

    Private sDates(0) As String
    Private blnAddMarkers, blnAddLabels, blnAddChart As Boolean
    Private blnEqu() As Boolean
    Private cha As Chart
    Private xlnIn As New Xml.XmlDocument
    Private _xmlfile As String

    Public Property xmlfile As String
        Get
            Return _xmlfile
        End Get

        Set(ByVal value As String)
            '   If Not (value = testpuName) Then
            _xmlfile = value
            'NotifyPropertyChanged("ElementName")
            ' End If
        End Set
    End Property

    Public Sub New(ByVal _strXML As String, ByVal sServerName As String)


        '  Dim rmin, rmax As String
        '  Dim flag_fg As String
        ' This call is required by the Windows Form Designer.
        InitializeComponent()


        _sServerName = sServerName
        srvAF = OSIsoft.AF.PI.PIServer.FindPIServer(sServerName)

        Dim stemp As String
        For i = 0 To 23
            If i < 10 Then
                stemp = "0" & i & ":00"
            Else
                stemp = i & ":00"
            End If
            comStartHours.Items.Add(stemp)
            comEndHours.Items.Add(stemp)
            comDeltaHours.Items.Add(i + 1)
        Next
        comDeltaHours.SelectedIndex = 7

        If Now.Hour < 10 Then
            stemp = "0" & Now.Hour
        Else
            stemp = Now.Hour
        End If

        If Now.Minute < 10 Then
            stemp = stemp & ":" & "0" & Now.Minute
        Else
            stemp = stemp & ":" & Now.Minute
        End If

        mycanvas.Width = 1300
        mycanvas.Height = 700


        comStartHours.Text = stemp
        comEndHours.Text = stemp

        iShift = 6

        dpEnd.Text = Now.Date.ToString
        dpStart.Text = DateAdd(DateInterval.Day, -7, Now.Date).Date.ToString

        sTime_start = DateAdd(DateInterval.Hour, -iShift, DateAdd(DateInterval.Minute, 2, Now))
        sTime_end = Now ' DateAdd(DateInterval.Hour, 2, sTimes(ii))
        timerange = New AFTimeRange(sTime_start, sTime_end, CultureInfo.CurrentCulture)

        sLineColors(0) = Brushes.DarkBlue 'Violat
        sLineColors(1) = Brushes.DarkBlue
        sLineColors(2) = Brushes.Green
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

        ReDim rValues_as(8, 5000)
        ReDim sDates_as(8, 5000)
        ReDim iPoints_as(8)

        strXML = _strXML

        '   For i = 1 To 10
        ' sTags(i) = ""
        ' sScales(i) = "1"
        ' Next

        config_aa.LoadXml(strXML)
        stemp = ""
        Dim ii1 As Integer = 0

        For Each nnode In config_aa.SelectNodes("//input")
            stemp = stemp & " " & UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
            ReDim Preserve sTags(ii1)
            ReDim Preserve sScales(ii1)
            ReDim Preserve blnEqu(ii1)
            ReDim Preserve seQus(ii1)
            Try
                sScales(ii1) = nnode.attributes.getnameditem("scale").value.ToString
            Catch ex As Exception
                sScales(ii1) = 1
            End Try
            Try
                If nnode.attributes.getnameditem("equ").value <> "" Then
                    blnEqu(ii1) = True
                    sTags(ii1) = nnode.attributes.getnameditem("var").value.ToString
                    seQus(ii1) = nnode.attributes.getnameditem("equ").value.ToString
                    stemp = nnode.attributes.getnameditem("tag").value.ToString
                Else
                    sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                    blnEqu(ii1) = False
                End If
            Catch ex As Exception

                sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                blnEqu(ii1) = False

            End Try
            ii1 = ii1 + 1
        Next

        Try
            sTitle = config_aa.DocumentElement.Attributes("title").Value
        Catch ex As Exception
            sTitle = stemp
        End Try

        'foundPoints = OSIsoft.AF.PI.PIPoint.FindPIPoints(srvAF, sTags)
        ''     pts = New OSIsoft.AF.PI.PIPointList(foundPoints)

        ''    ii1 = 0

        ''    For Each myp In foundPoints
        ''   ReDim Preserve pi_col_xls_tags(ii1)
        ''   ReDim Preserve pi_col_xls_tags_UOM(ii1)
        ''  pi_col_xls_tags(ii1) = myp.Name
        ''  pi_col_xls_tags_UOM(ii1) = myp.GetAttributes("engUnits").Values(0).ToString
        ''  ii1 = ii1 + 1
        ''  Next
        iops = ii1


        If iops = 1 Then
            txtTag.Text = sTitle
        Else
            txtTag.Text = ""
        End If

        myYax.ViewportRangeEnabled = True
        blnInit = True

        exportXL.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf sendXL))
        recLeft.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf slideLeft))
        recRight.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf slideLeft))
        imgShowList.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf showList))

        chkEnableZoom.Visibility = Visibility.Hidden


        'lbxPLMEvents.DataContext = xlnIn.DocumentElement.SelectSingleNode("input").Attributes("var")

        blnAddMarkers = True
        If iops > 1 Then
            blnAddMarkers = False
        End If

        blnAddLabels = False
        blnAddChart = True

        Call CreateChart()

        timer1.IsEnabled = True
        timer1.Interval = TimeSpan.FromSeconds(10)
        'timer1.Start()
        '   Try
        '      chart_flow.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, New SubChartPrimeDelegate(AddressOf UpdateLiveChart), chart_flow)
        '   Catch ex As Exception

        '  End Try
        '  chart_flow.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, New SubChartPrimeDelegate(AddressOf UpdateLiveChart), chart_flow)
    End Sub
    Private Sub showList()
        Canvas.SetTop(stkPanel, 200)
        Canvas.SetZIndex(stkPanel, 10)

    End Sub
    Private Sub slideLeft(sender As Object, e As RoutedEventArgs)
        '   Dim iHour, iminutes As Integer
        Dim chart_flow As Chart
        Dim dtime As Integer
        Dim sTag As String = sender.Tag
        Dim itemp As Integer

        If sTag = "left" Then
            itemp = -1
        Else
            itemp = 1
        End If

        ' dtime = (sTime_end - sTime_start).TotalSeconds
        dtime = comDeltaHours.SelectedValue * 3600

        sTime_end = DateAdd(DateInterval.Second, itemp * dtime, sTime_end)
        sTime_start = DateAdd(DateInterval.Second, itemp * dtime, sTime_start)

        If sTime_end >= Now Then
            sTime_end = Now
            sTime_start = DateAdd(DateInterval.Second, -dtime, sTime_end)
        End If

        timerange = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)
        '    dpStart.SelectedDate = sTime_start.Date
        '   dpEnd.SelectedDate = sTime_end.Date
        blnInit = True
        timer1.IsEnabled = False

        Call CreateChart()

        chart_flow = LogicalTreeHelper.FindLogicalNode(mycanvas, "chart_addhoc")

        chart_flow.AxesX(0).AxisMinimum = sTime_start
        chart_flow.AxesX(0).AxisMaximum = sTime_end

        If timerange.Span.TotalHours < 2 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Minutes
            chart_flow.AxesX(0).Interval = 10
            chart_flow.AxesX(0).ValueFormatString = "H:mm"
        ElseIf timerange.Span.TotalHours < 24 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Hours
            chart_flow.AxesX(0).Interval = 1
            chart_flow.AxesX(0).ValueFormatString = "H:mm"
        ElseIf timerange.Span.TotalDays < 30 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Hours
            chart_flow.AxesX(0).Interval = 12
            chart_flow.AxesX(0).ValueFormatString = "MM/dd H:mm"
        ElseIf timerange.Span.TotalDays < 180 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Days
            chart_flow.AxesX(0).Interval = 7
            chart_flow.AxesX(0).ValueFormatString = "MM/dd H:mm"
        Else
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Months
            chart_flow.AxesX(0).Interval = 1
            chart_flow.AxesX(0).ValueFormatString = "MM/dd/yy"
        End If
        chart_flow.AxesX(0).ValueFormatString = "MM/dd H:mm"
    End Sub
    Private Sub CreateChart()

        Call getChartData(0, strXML, iPoints, pts)

        Dim ileft, iTop, iWidth, iHeight As Integer
        Dim nSeries, nSeries_from As Integer
        Dim chatype As RenderAs
        Dim sColors() As Brush
        Dim sSeriesNames() As String
        Dim sChartName As String = "chart_addhoc"

        '    Dim iPoints As Integer

        Dim sTYpes(8) As RenderAs
        ' Dim cha As Chart


        ' Height="517" Canvas.Left="5" Stroke="Black" Canvas.Top="52" Width="781"
        'Height="437" Canvas.Left="7" Stroke="Black" Canvas.Top="57" Width="1297"
        'Height="577" Canvas.Left="18" Stroke="Black" Canvas.Top="65" Width="1258"

        ileft = 18
        iTop = 55
        iWidth = 1260
        iHeight = 600
        chatype = RenderAs.StackedColumn
        nSeries = iops
        nSeries_from = 0
        ReDim sColors(nSeries)
        ReDim sSeriesNames(nSeries)
        ReDim sTYpes(nSeries)
        iPoints = UBound(sDates) '- 2

        For ii1 = 1 To iops
            sTYpes(ii1) = RenderAs.QuickLine
            sColors(ii1) = sLineColors(ii1)
            sSeriesNames(ii1) = sTags(ii1 - 1)
            'pi_col_xls_tags_UOM
        Next

        Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, pi_col_xls_tags_UOM(0))

        ' check if hihi or lolo limits are available

        Try
            Dim nnode As Xml.XmlNode
            nnode = config_aa.SelectSingleNode("//input[@hihi!='none']")

            If IsNothing(nnode) Then
                ' do nothing
            Else
                If IsNumeric(nnode.Attributes("hihi").Value) Then
                    Dim ser As New DataSeries
                    Dim dp As DataPoint

                    ser.RenderAs = RenderAs.QuickLine
                    ser.Name = "HIGH Reporting Limit"
                    ser.Color = Brushes.Red
                    ser.LightingEnabled = False
                    ser.XValueType = ChartValueTypes.DateTime
                    'ser.RenderAs = RenderAs.Line

                    For i = 1 To iPoints_as(0)
                        dp = New DataPoint
                        dp.XValue = sDates_as(0, i - 1)
                        dp.YValue = nnode.Attributes("hihi").Value
                        ser.DataPoints.Add(dp)
                    Next
                    cha.Series.Add(ser)

                    If nnode.Attributes("hihi").Value > cha.AxesY(0).AxisMaximum Then
                        cha.AxesY(0).AxisMaximum = nnode.Attributes("hihi").Value * 1.2
                    End If

                    If nnode.Attributes("hihi").Value < cha.AxesY(0).AxisMinimum Then
                        cha.AxesY(0).AxisMinimum = nnode.Attributes("hihi").Value * 0.8
                    End If

                End If
            End If

        Catch ex As Exception

        End Try

        Try
            Dim nnode As Xml.XmlNode
            nnode = config_aa.SelectSingleNode("//input[@lolo!='none']")

            If IsNothing(nnode) Then
                ' do nothing
            Else
                If IsNumeric(nnode.Attributes("lolo").Value) Then
                    Dim ser As New DataSeries
                    Dim dp As DataPoint

                    ser.RenderAs = RenderAs.QuickLine
                    ser.Name = "LOW Reporting Limit"
                    ser.Color = Brushes.DarkOrange
                    ser.LightingEnabled = False
                    ser.ShadowEnabled = False
                    ser.XValueType = ChartValueTypes.DateTime
                    ser.LineThickness = 2
                    ser.LineStyle = LineStyles.Dashed
                    'ser.RenderAs = RenderAs.Line

                    For i = 1 To iPoints_as(0)
                        dp = New DataPoint
                        dp.XValue = sDates_as(0, i - 1)
                        dp.YValue = nnode.Attributes("lolo").Value
                        ser.DataPoints.Add(dp)
                    Next
                    cha.Series.Add(ser)

                    If nnode.Attributes("lolo").Value < cha.AxesY(0).AxisMinimum Then
                        cha.AxesY(0).AxisMinimum = nnode.Attributes("lolo").Value * 0.9
                    End If

                    If nnode.Attributes("lolo").Value > cha.AxesY(0).AxisMaximum Then
                        cha.AxesY(0).AxisMaximum = nnode.Attributes("lolo").Value * 1.2
                    End If

                End If
            End If

        Catch ex As Exception

        End Try

    End Sub
    Private Sub createNewChart(ByRef cha As Chart, ByVal chaName As String, ByVal iLeft As Integer, ByVal iTop As Integer,
                               ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal nSeries As Integer, ByVal nSeries_from As Integer,
                               ByVal sSeriesNames() As String,
                               ByVal chatype() As RenderAs, ByVal sLineColor() As Brush, ByVal iPoints As Integer, ByVal sYTitle As String)

        Dim dp As DataPoint
        Dim stemp As String
        Dim itemp As Integer = 0
        Dim ymin, ymax As Integer

        ymin = 1000000.0
        ymax = -1000000.0

        '  Dim cha As New Chart

        ' check if chart exists

        ' check if chart exists
        If IsNothing(LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)) Then
            cha = New Chart
        Else
            cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)
            mycanvas.Children.Remove(cha)
            cha = New Chart
        End If

        Try
            Dim trl As New TrendLine
            trl.Orientation = Orientation.Vertical
            trl.StartValue = CDate("07/29/2017")
            trl.ToolTipText = "2017 Turnaround"
            trl.EndValue = CDate("09/01/2017")
            trl.LineColor = Brushes.Red
            trl.LineThickness = 2
            trl.Opacity = 0.2
            cha.TrendLines.Add(trl)
        Catch ex As Exception

        End Try

        '  If blnAddChart Then
        cha.AnimationEnabled = True
        mycanvas.Children.Add(cha)
        '  End If

        Canvas.SetLeft(cha, iLeft)
        Canvas.SetTop(cha, iTop)

        cha.Width = iWidth
        cha.Height = iHeight

        cha.BorderThickness = New Thickness(1)
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

        cha.ZoomingEnabled = False
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
        myYax.Grids.Add(mygr1)
        myYax.Title = sYTitle
        cha.AxesY.Add(myYax)

        'title
        Dim myTitle As New Title
        cha.Titles.Add(myTitle)
        myTitle.Text = sTitle

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
            ser.RenderAs = chatype(i + 1)
            ser.LabelEnabled = False
            ser.MarkerEnabled = False
            ser.LightingEnabled = True


            If blnAddMarkers Then
                ser.MarkerEnabled = True
            End If

            If blnAddLabels Then
                ser.LabelEnabled = True
            End If

            If ser.RenderAs = RenderAs.Line Or ser.RenderAs = RenderAs.Spline Or ser.RenderAs = RenderAs.QuickLine Then
                '     ser.LabelEnabled = True
                If blnAddMarkers Then
                    ser.MarkerEnabled = True
                Else
                    ser.MarkerEnabled = False
                End If
                ser.XValueType = ChartValueTypes.DateTime
                '   ser.ToolTipText = "#YValue, #Series"
                ser.ToolTipText = "#YValue"
            End If

            ser.MarkerColor = Brushes.LightYellow
            ser.MarkerBorderColor = Brushes.Crimson
            ser.MarkerSize = 4
            If Not IsNothing(sLineColor(i + 1)) Then
                If sLineColor(i + 1).ToString <> Brushes.Transparent.ToString Then
                    ser.Color = sLineColor(i + 1)
                End If
            End If
            ser.LineThickness = 2
            ser.ShadowEnabled = True
            ser.LightingEnabled = False
            ser.Name = sSeriesNames(i + 1)
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
                    If ymin > FormatNumber(rValues_as(i, j), 2) Then
                        ymin = FormatNumber(rValues_as(i, j), 2)
                    End If

                    If ymax < FormatNumber(rValues_as(i, j), 2) Then
                        ymax = FormatNumber(rValues_as(i, j), 2)
                    End If

                    If ser.RenderAs = RenderAs.Column Then
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
                        cha.AxesX(0).ValueFormatString = "MM/dd h:mm"
                    End If

                    ser.DataPoints.Add(dp)
                    If rtemp > rValues_as(i + 1, j) Then
                        rtemp = rValues_as(i + 1, j)
                    End If
                Catch ex As Exception

                End Try

            Next
        Next


        If ymax > 10 Then
            cha.AxesY(0).AxisMinimum = ymin * 0.8
            cha.AxesY(0).AxisMaximum = ymax * 1.2
        End If
    End Sub
    Private Sub getChartData(ByVal iGatherType As Integer, ByVal strXML As String, ByRef inum As Integer, ByVal pts As OSIsoft.AF.PI.PIPointList)
        Dim sEx(4) As String
        Dim avalues As AFValues

        Dim avalue As AFValue
        Dim ii As Integer
        Dim config_x As New Xml.XmlDocument
        Dim timerange1 As AFTimeRange

        Try
            If iGatherType = 0 Then

                timerange1 = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)

                inum = 0
                For i = 0 To iops - 1
                    If blnEqu(i) Then
                        Call processEquArray(i, timerange1)
                        ReDim Preserve pi_col_xls_tags(i)
                        ReDim Preserve pi_col_xls_tags_UOM(i)
                        ReDim Preserve pi_col_xls_tags_desc(i)
                        pi_col_xls_tags_UOM(i) = ""
                        pi_col_xls_tags(i) = "Calculated"
                        pi_col_xls_tags_desc(i) = "Calculated"
                    Else
                        If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, sTags(i), sinusoid) Then
                            ReDim Preserve pi_col_xls_tags(i)
                            ReDim Preserve pi_col_xls_tags_UOM(i)
                            ReDim Preserve pi_col_xls_tags_desc(i)
                            pi_col_xls_tags_UOM(i) = sinusoid.GetAttributes("engUnits").Values(0).ToString
                            pi_col_xls_tags(i) = sinusoid.Name
                            ' pi_col_xls_tags_desc(i) = sinusoid.GetAttributes("descriptor").Values(0).ToString
                            pts.Add(sinusoid)

                            avalues = sinusoid.InterpolatedValuesByCount(timerange1, 500, "", False)
                            '  avalues = pts(i).RecordedValues(timerange1, Data.AFBoundaryType.Outside, "", False) ' 500, "", False)
                            '  avalues = sinusoid.InterpolatedValuesByCount(timerange1, 500, "", False)
                            ii = 0
                            For Each avalue In avalues
                                If avalue.IsGood Then
                                    rValues_as(i, ii) = avalue.Value * CDbl(sScales(i))

                                Else
                                    rValues_as(i, ii) = -9
                                End If

                                sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                                ii = ii + 1
                            Next
                            iPoints_as(i) = ii - 1
                        End If
                    End If


                    ' End If
                Next
            Else

                inum = 0
                For i = 0 To iops - 1
                    '  If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, pi_col_xls_tags(i + 1), sinusoid) Then
                    ' avalues = sinusoid.InterpolatedValuesAtTimes(times, "", False)
                    '  avalues = sinusoid.PlotValues(timerange1, 500)
                    'avalues = pts(i).InterpolatedValuesByCount(timerange1, 500, "", False)
                    ii = 0
                    For Each avalue In avalues
                        rValues_as(i, ii) = avalue.Value
                        sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                        ii = ii + 1
                    Next
                    iPoints_as(i) = ii - 1
                    ' End If
                Next
            End If
        Catch ex As Exception

        End Try
        Try
            avalues.Clear()
            avalues = Nothing
            config_x = Nothing
            sinusoid = Nothing
        Catch ex As Exception

        End Try

    End Sub
    Private Sub processEquArray(ByVal itemp As Integer, ByRef timerange1 As AFTimeRange)
        Dim blnBadValue As Boolean = False
        Dim atemp() As String
        ' Dim atemp1() As String
        Dim rtemp1 As Double = 1
        Dim stemp As String
        Dim sQue As String
        Dim avalues As AFValues
        Dim stimes(300) As Date

        Dim rtemp(10, 300) As Double
        Dim ii1 As Integer = 0

        atemp = Split(sTags(itemp), ";")
        '  atemp1 = Split(seQus(itemp), ";")
        sQue = seQus(itemp)
        If atemp.Count > 1 Then
            For i = 1 To atemp.Count
                If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, atemp(i - 1), sinusoid) Then
                    avalues = sinusoid.InterpolatedValuesByCount(timerange1, 300, "", False)
                    ii1 = 0

                    For j = 1 To 300
                        stimes(j - 1) = avalues(j - 1).Timestamp.LocalTime

                        ii1 = ii1 + 1
                        If avalues(j - 1).IsGood Then
                            rtemp(i - 1, j - 1) = avalues(j - 1).Value
                        Else
                            rtemp(i - 1, j - 1) = -9
                        End If
                    Next
                Else
                    '  blnBadValue = True
                End If
            Next
        Else

        End If

        Dim parser As New System.Parsers.MQ

        Try
            If blnBadValue Then
                ' kida.ElementXML.SelectSingleNode("PV").InnerText = "ERR"
            Else
                stemp = ""
                Dim rtemp3 As Double
                For j = 1 To ii1 - 1
                    sQue = seQus(itemp)
                    blnBadValue = False
                    For i = 1 To atemp.Count
                        rtemp3 = CDbl(FormatNumber(rtemp(i - 1, j - 1), 3))
                        If rtemp3 = -9 Then
                            blnBadValue = True
                        End If
                        sQue = Replace(sQue, "X" & i, rtemp3)
                    Next
                    sDates_as(itemp, j - 1) = stimes(j - 1)
                    If Not blnBadValue Then
                        Try
                            rValues_as(itemp, j - 1) = parser.Calculate(sQue)
                        Catch ex As Exception
                            rValues_as(itemp, j - 1) = -9
                        End Try
                    Else
                        rValues_as(itemp, j - 1) = -9
                    End If
                Next

                '  kida.ElementXML.SelectSingleNode("PV").InnerText = FormatNumber(rtemp2, 1)

            End If
            iPoints_as(itemp) = ii1 - 1
        Catch ex As Exception

        End Try

    End Sub
    Private Sub sendXL()
        Dim xlApp As Excel.Application = New Excel.Application

        Dim itemp_f, itemp_t, itemp_max As Integer
        Dim xlWorkBook_temp As Excel.Workbook

        Dim irow, icol As Integer

        Dim xlWorkSheet As Excel.Worksheet

        Dim pi_values_xls As AFValues
        Dim timerange1 As AFTimeRange

        Dim win As System.Security.Principal.WindowsIdentity
        win = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim _UserName = win.Name.Substring(win.Name.IndexOf("\") + 1)

        ' get data

        '        mypi.sTime_end = sTime_end
        '       mypi.sTime_start = sTime_start
        '     mypi.numPoints = 200

        '    mypi.GatherType = "InterpolatedValues"
        '    config_aa.LoadXml(mypi.getPIDataAsXML(strXML))

        xlWorkBook_temp = xlApp.Workbooks.Add()
        xlWorkSheet = xlWorkBook_temp.Worksheets("Sheet1")

        With xlWorkSheet
            icol = 1
            irow = 1
            .Cells(irow, icol) = "Created"
            .Cells(irow, icol + 1) = Now()

            irow = 2
            .Cells(irow, icol) = "Created by"
            .Cells(irow, icol + 1) = _UserName

            irow = 3
            .Cells(irow, icol) = "Tag"
            .Cells(irow, icol + 1) = ""

        End With

        irow = 5
        icol = 1

        itemp_t = pi_col_xls_tags.Count - 1
        itemp_f = 0
        itemp_max = 0

        timerange1 = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)


        '  For i = itemp_f To itemp_t
        '  xlWorkSheet.Cells(irow, icol + 1) = pi_col_xls_tags(i)
        ' icol = icol + 2
        ' If itemp_max < pi_col(i).count Then
        'itemp_max = pi_col(i).count
        'End If
        'Next

        icol = 1
        Dim ii1 As Integer = 0
        Dim tempArray_v As Object(,)

        For i = itemp_f To itemp_t
            ' pi_values_xls = pi_col(i)
            If blnEqu(i) Then
                itemp_max = iPoints_as(i)
                ii1 = ii1 + 1
                irow = 7
                tempArray_v = New Object(itemp_max, 2) {}
                For j = 1 To iPoints_as(i) - 1
                    Try
                        tempArray_v(j, 0) = sDates_as(i, j - 1)   ' pi_values_xls.Item(j).Timestamp.LocalTime.ToString
                        tempArray_v(j, 1) = rValues_as(i, j - 1) ' pi_values_xls.Item(j).Value.ToString
                    Catch ex As Exception

                    End Try
                Next
            Else
                pi_values_xls = pts(i).RecordedValues(timerange1, Data.AFBoundaryType.Outside, "", False)
                itemp_max = pi_values_xls.Count
                ii1 = ii1 + 1
                irow = 7
                tempArray_v = New Object(itemp_max, 2) {}
                For j = 1 To pi_values_xls.Count - 1
                    Try
                        tempArray_v(j, 0) = pi_values_xls.Item(j).Timestamp.LocalTime.ToString
                        tempArray_v(j, 1) = pi_values_xls.Item(j).Value.ToString
                    Catch ex As Exception

                    End Try
                Next
            End If
            xlWorkSheet.Cells(3, ii1) = "Tag"
            xlWorkSheet.Cells(3, ii1 + 1) = sTags(i) ' pts(i).Name

            Dim myrang As Excel.Range
            myrang = xlWorkSheet.Range(xlWorkSheet.Cells(irow, icol), xlWorkSheet.Cells(irow + itemp_max, icol + 1))
            myrang.Value2 = tempArray_v
            icol = icol + 2
        Next

        xlWorkSheet.Columns.AutoFit()
        xlApp.Visible = True

    End Sub
    Private Sub UpdateLiveChart(ByRef cha As Chart)
        ' get series names

        Dim i1 As Integer = 0
        Dim dp As DataPoint

        Dim results As OSIsoft.AF.AFListResults(Of OSIsoft.AF.PI.PIPoint, OSIsoft.AF.Asset.AFValue)
        results = pts.CurrentValue
        Try
            '   For Each ser In cha.Series

            If results(0).IsGood Then
                ' add new data point
                dp = New DataPoint
                dp.XValue = results(0).Timestamp.LocalTime
                dp.YValue = results(0).Value * CDbl(sScales(0))
                cha.Series(0).DataPoints.Add(dp)

                'remove last
                cha.Series(0).DataPoints.RemoveAt(0)
                i1 = i1 + 1
                ' End If
            End If
            ' Next
        Catch ex As Exception

        End Try
        cha.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, New SubChartPrimeDelegate(AddressOf UpdateLiveChart), cha)
        '   cha.AxesY(0).AxisMinimum = rmin * 0.9
        '   cha.AxesY(0).AxisMaximum = rmax * 1.1
    End Sub



    Private Function rand() As Random
        'Dim rand As Random
        rand = New Random(DateTime.Now.Millisecond)
    End Function

    Private Sub btnDismiss_Click(sender As Object, e As RoutedEventArgs) Handles btnDismiss.Click
        Me.Close()
    End Sub

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        ' Call UpdateChart()
        Call UpdateLiveChart("chart_addhoc")
    End Sub
    Private Sub UpdateLiveChart(ByRef chaName As String)
        ' get series names

        ' Dim ser As DataSeries
        Dim i1 As Integer = 0

        Dim dp As DataPoint
        Dim cha As Chart
        Dim stemp As String
        ' Dim stime As Date

        Try
            Dim results As OSIsoft.AF.AFListResults(Of OSIsoft.AF.PI.PIPoint, OSIsoft.AF.Asset.AFValue)

            cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)

            For i = 0 To iops - 1
                If blnEqu(i) Then
                    stemp = processEquSingle(i)
                    If stemp <> "ERR" Then
                        ' add new data point
                        dp = New DataPoint
                        dp.XValue = Now
                        dp.YValue = CDbl(stemp) * CDbl(sScales(i))
                        cha.Series(i).DataPoints.Add(dp)

                        'remove last
                        cha.Series(i).DataPoints.RemoveAt(0)
                        i1 = i1 + 1
                        ' End If
                    End If
                Else

                    If pts(i).CurrentValue().IsGood Then
                        ' add new data point
                        dp = New DataPoint
                        dp.XValue = pts(i).CurrentValue().Timestamp.LocalTime
                        dp.YValue = pts(i).CurrentValue().Value * CDbl(sScales(i))
                        cha.Series(i).DataPoints.Add(dp)

                        'remove last
                        cha.Series(i).DataPoints.RemoveAt(0)
                    End If

                End If
            Next
        Catch ex As Exception

        End Try


        Try
            Dim nnode As Xml.XmlNode
            nnode = config_aa.SelectSingleNode("//input[@hihi!='none']")
            If IsNothing(nnode) Then
                'do nothing
            Else
                ' add new data point
                dp = New DataPoint
                dp.XValue = Now
                dp.YValue = nnode.Attributes("hihi").Value
                cha.Series(iops).DataPoints.Add(dp)

                'remove last
                cha.Series(iops).DataPoints.RemoveAt(0)
            End If
        Catch ex As Exception

        End Try



        '   cha.AxesY(0).AxisMinimum = rmin * 0.9
        '   cha.AxesY(0).AxisMaximum = rmax * 1.1
    End Sub
    Private Function processEquSingle(ByVal itemp As Integer) As String
        Dim blnBadValue As Boolean = False
        Dim atemp() As String
        ' Dim atemp1() As String
        Dim rtemp1 As Double = 1
        Dim stemp As String
        Dim sQue As String

        Dim rtemp(10) As Double

        sQue = seQus(itemp)
        atemp = Split(sTags(itemp), ";")

        If atemp.Count >= 1 Then
            For i = 1 To atemp.Count
                If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, atemp(i - 1), sinusoid) Then
                    If sinusoid.CurrentValue().IsGood Then
                        rtemp(i - 1) = sinusoid.CurrentValue().Value
                    Else
                        blnBadValue = True
                    End If
                ElseIf IsNumeric(atemp(i - 1)) Then
                    Try
                        rtemp(i - 1) = CDbl(atemp(i - 1))
                    Catch ex As Exception
                        rtemp(i - 1) = 1
                    End Try
                Else
                    blnBadValue = True
                End If
            Next
            If blnBadValue Then
                processEquSingle = "ERR"
            Else
                stemp = ""
                Dim rtemp2, rtemp3 As Double

                For i = 1 To atemp.Count
                    rtemp3 = CDbl(FormatNumber(rtemp(i - 1), 3))
                    sQue = Replace(sQue, "X" & i, rtemp3)
                Next

                Dim parser As New System.Parsers.MQ


                rtemp2 = parser.Calculate(sQue)

                processEquSingle = FormatNumber(rtemp2, 1)

            End If

        End If
    End Function

    ''  Private Sub btnLeft_Click(sender As Object, e As RoutedEventArgs) Handles btnLeft.Click

    ''   blnInit = True
    ''  timer1.IsEnabled = False
    ''  sTime_end = DateAdd(DateInterval.Hour, -iShift * 0.75, sTime_end)

    ''  Call UpdateChart()
    '' End Sub

    '' Private Sub btnRight_Click(sender As Object, e As RoutedEventArgs) Handles btnRight.Click
    ''Dim tempD As Date

    ''     blnInit = True
    ''     timer1.IsEnabled = False
    ''     tempD = DateAdd(DateInterval.Hour, iShift * 0.75, sTime_end)
    ''    If tempD < Now Then
    ''        sTime_end = DateAdd(DateInterval.Hour, iShift * 0.75, sTime_end)
    ''        Call UpdateChart()
    ''    End If
    '' End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim iHour, iminutes As Integer
        Dim chart_flow As Chart

        sTime_start = dpStart.Text
        iHour = CDate(comStartHours.Text).Hour
        sTime_start = DateAdd(DateInterval.Hour, iHour, sTime_start)
        iminutes = CDate(comStartHours.Text).Minute
        sTime_start = DateAdd(DateInterval.Minute, iminutes, sTime_start)

        sTime_end = dpEnd.Text 'Now ' DateAdd(DateInterval.Hour, 2, sTimes(ii))
        iHour = CDate(comEndHours.Text).Hour
        sTime_end = DateAdd(DateInterval.Hour, iHour, sTime_end)
        iminutes = CDate(comEndHours.Text).Minute
        sTime_end = DateAdd(DateInterval.Minute, iminutes, sTime_end)
        timerange = New AFTimeRange(sTime_start, sTime_end, CultureInfo.CurrentCulture)

        blnInit = True
        timer1.IsEnabled = False

        Call CreateChart()

        chart_flow = LogicalTreeHelper.FindLogicalNode(mycanvas, "chart_addhoc")

        chart_flow.AxesX(0).AxisMinimum = sTime_start
        chart_flow.AxesX(0).AxisMaximum = sTime_end

        If timerange.Span.TotalHours < 2 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Minutes
            chart_flow.AxesX(0).Interval = 10
            chart_flow.AxesX(0).ValueFormatString = "H:mm"
        ElseIf timerange.Span.TotalHours < 24 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Hours
            chart_flow.AxesX(0).Interval = 1
            chart_flow.AxesX(0).ValueFormatString = "H:mm"
        ElseIf timerange.Span.TotalDays < 30 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Hours
            chart_flow.AxesX(0).Interval = 12
            chart_flow.AxesX(0).ValueFormatString = "MM/dd H:mm"
        ElseIf timerange.Span.TotalDays < 180 Then
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Days
            chart_flow.AxesX(0).Interval = 7
            chart_flow.AxesX(0).ValueFormatString = "MM/dd H:mm"
        Else
            chart_flow.AxesX(0).IntervalType = IntervalTypes.Months
            chart_flow.AxesX(0).Interval = 1
            chart_flow.AxesX(0).ValueFormatString = "MM/dd/yy"
        End If

    End Sub

    Private Sub ADDHOC_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - 30) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub

    Private Sub ADDHOC_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single
        Dim stemp As String
        Dim itema As Integer = 0
        Dim pcol As New Collection
        Dim sDesc As String
        Dim itemp As Integer
        Dim blnSelected As Boolean = False
        Dim sEx, sEqu As String
        Dim ii1 As Integer = 0
        Dim nnode As Xml.XmlNode
        Dim sTags_a() As String

        If xmlfile <> "" Then
            xlnIn.Load(AppDomain.CurrentDomain.BaseDirectory & "XML\" & xmlfile)
            'dgEvents
            ' find descriptions amd points
            ReDim sTags_a(0)
            For Each nnode In xlnIn.SelectNodes("//input[@ext='PV']")
                ReDim Preserve sTags_a(ii1)
                stemp = nnode.Attributes("var").Value & ".PV"
                sTags_a(ii1) = stemp
                ii1 = ii1 + 1
            Next

            foundPoints = OSIsoft.AF.PI.PIPoint.FindPIPoints(srvAF, sTags_a)
            pts1 = New OSIsoft.AF.PI.PIPointList(foundPoints)

            Dim apit As PIPoint

            For Each apit In pts1
                nnode = xlnIn.DocumentElement.SelectSingleNode("input[@var='" & Replace(apit.Name, ".PV", "") & "']")
                'sEx = nnode.Attributes("ext").Value
                Try
                    sEqu = nnode.Attributes("equ").Value
                Catch ex As Exception
                    sEqu = ""
                End Try

                sDesc = apit.GetAttributes("descriptor").Values(0).ToString
                itemp = InStr(sDesc, " ")
                blnSelected = False

                For i = 0 To iops - 1
                    If sTags(i) = apit.Name Then
                        blnSelected = True
                        Exit For
                    End If
                Next

                Dim anonymousCust = New With {.selected = blnSelected, .tag = Replace(apit.Name, ".PV", ""), .desc = Mid(sDesc, itemp), .ext = sEx, .equ = sEqu}
                pcol.Add(anonymousCust)
            Next

            'If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, stemp & ".PV", sinusoid) Then

            'End If
            ' Next

            ' Dim mysort As New System.ComponentModel.SortDescription()
            '  lbxPLMEvents.Items.SortDescriptions.Add(mysort)
            dgEvents.DataContext = pcol
        Else
            imgShowList.Visibility = Visibility.Hidden
        End If


        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - 30) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub
    Public Sub Quad0(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30
    End Sub

    Public Sub Quad1(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = SystemParameters.PrimaryScreenWidth / 2
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad2(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = (SystemParameters.WorkArea.Height) / 2
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad3(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = (SystemParameters.WorkArea.Height) / 2
        Me.Left = SystemParameters.PrimaryScreenWidth / 2
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad00(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth
        Me.Height = SystemParameters.WorkArea.Height 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As RoutedEventArgs) Handles btnAdd.Click
        Dim stemp As String
        Dim ii2 As Integer = 0

        For Each itema In dgEvents.Items
            If itema.selected Then
                ii2 = ii2 + 1
            End If
        Next

        If ii2 <= 4 Then

            pts.Clear()

            strXML = "<variables title='" & sTitle & "'>"
            For Each itema In dgEvents.Items
                If itema.selected Then
                    strXML = strXML & "<input trend='yes' var='" & itema.tag & "' equ='" & itema.equ & "' ext='" & itema.ext & "'><value/><time/></input>"
                    '   OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, itema.tag & "." itema.ext, sinusoid)
                    ' pts.Add(sinusoid)
                End If
            Next
            strXML = strXML & "</variables>"


            config_aa.LoadXml(strXML)
            stemp = ""
            Dim ii1 As Integer = 0

            For Each nnode In config_aa.SelectNodes("//input")
                stemp = stemp & " " & UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                ReDim Preserve sTags(ii1)
                ReDim Preserve sScales(ii1)
                ReDim Preserve blnEqu(ii1)
                ReDim Preserve seQus(ii1)
                Try
                    sScales(ii1) = nnode.attributes.getnameditem("scale").value.ToString
                Catch ex As Exception
                    sScales(ii1) = 1
                End Try
                Try
                    If nnode.attributes.getnameditem("equ").value <> "" Then
                        blnEqu(ii1) = True
                        sTags(ii1) = nnode.attributes.getnameditem("var").value.ToString
                        seQus(ii1) = nnode.attributes.getnameditem("equ").value.ToString
                        stemp = nnode.attributes.getnameditem("tag").value.ToString
                    Else
                        sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                        blnEqu(ii1) = False
                    End If
                Catch ex As Exception

                    sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                    blnEqu(ii1) = False

                End Try
                ii1 = ii1 + 1
            Next

            iops = ii1

            sTitle = stemp

            Call CreateChart()
            Canvas.SetTop(stkPanel, 710)
        Else
            MsgBox("Too many selected tags (up to 4)")
        End If


    End Sub

    Private Sub btnCloseZoom_Click(sender As Object, e As RoutedEventArgs) Handles btnCloseZoom.Click
        Canvas.SetTop(stkPanel, 710)
    End Sub

    Private Sub chkEnableZoom_Checked(sender As Object, e As RoutedEventArgs) Handles chkEnableZoom.Checked
        cha.ZoomingEnabled = True
    End Sub

    Private Sub chkEnableZoom_Unchecked(sender As Object, e As RoutedEventArgs) Handles chkEnableZoom.Unchecked
        cha.ZoomingEnabled = False

    End Sub


End Class

